using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using MongoDB.Bson;
using MongoDB.Driver;
using SharpBird.Exceptions;
using SharpBird.Models;

namespace SharpBird.Mongo
{
    public class MongoBirdSearch : IBirdSearch
    {
        private class RecordInfo
        {
            public ObjectId Id { get; set; }

            public DateTime RegisterDate { get; set; }

            public int FlightsFound { get; set; }
        }


        private readonly IBirdSearch _birdSearch;
        private readonly TimeSpan _expiration;
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<FlightModel> _flights;
        private readonly IMongoCollection<RecordInfo> _records;

        public MongoBirdSearch(IBirdSearch birdSearch, TimeSpan expiration) :
            this(birdSearch, expiration, $"test_{Guid.NewGuid()}")
        {
        }

        public MongoBirdSearch(IBirdSearch birdSearch, TimeSpan expiration, string databaseName)
        {
            _birdSearch = birdSearch;
            _expiration = expiration;

            var mongoClientSettings = MongoClientSettings.FromConnectionString("mongodb://localhost");
            mongoClientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);

            _client = new MongoClient(mongoClientSettings);
            _database = _client.GetDatabase(databaseName);
            _flights = _database.GetCollection<FlightModel>("flights");
            _records = _database.GetCollection<RecordInfo>("records");
        }


        public IEnumerable<FlightModel> Search(string origin, string destination, DateTime startDate)
        {
            if (IsCacheStillValid())
                return GetCachedItems(origin, destination, startDate);

            IEnumerable<FlightModel> result;

            try
            {
                result = GetUncachedItems(origin, destination, startDate)
                    .ToList();
                
                AddItemsToCache(result);
            }
            catch (BlacklistedException e)
            {
                Console.WriteLine(e);
                result = GetCachedItems(origin, destination, startDate);
            }

            return result;
        }

        public void Drop()
        {
            _client.DropDatabase(_database.DatabaseNamespace.DatabaseName);
        }

        private IEnumerable<FlightModel> GetCachedItems(string origin, string destination, DateTime startDate)
        {
            return _flights
                .AsQueryable()
                .Where(x => x.Origin == origin && x.Destination == destination && x.TimeDepartureUtc > startDate)
                .GroupBy(x => x.ProviderId)
                .Select(x => x.OrderByDescending(f => f.RegisteredDate).First())
                .OrderBy(x => x.TimeDepartureUtc);
        }

        private IEnumerable<FlightModel> GetUncachedItems(string origin, string destination, DateTime startDate)
        {
            return _birdSearch.Search(origin, destination, startDate);
        }

        private void AddItemsToCache(IEnumerable<FlightModel> items)
        {
            var list = items as ICollection<FlightModel> ?? items.ToList();
            _records.InsertOne(new RecordInfo
            {
                Id = ObjectId.GenerateNewId(),
                RegisterDate = DateTime.UtcNow,
                FlightsFound = list.Count
            });

            _flights.InsertMany(items);
        }

        private bool IsCacheStillValid()
        {
            var cacheExpirationDate = GetCacheExpirationDate();
            return _records.AsQueryable().Any(x => x.RegisterDate > cacheExpirationDate);
        }

        private DateTime GetCacheExpirationDate() => DateTime.UtcNow - _expiration;
    }
}
