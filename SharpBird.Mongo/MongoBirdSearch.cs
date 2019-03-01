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
    public class MongoBirdSearch : IBirdSearch, IBirdStatistics
    {
        private class RecordInfo
        {
            public ObjectId Id { get; set; }

            public DateTime RegisterDate { get; set; }

            public int FlightsFound { get; set; }

            public string SourceDestination { get; set; }
        }


        private readonly IBirdSearch _birdSearch;
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<FlightModel> _flights;
        private readonly IMongoCollection<RecordInfo> _records;

        public TimeSpan Expiration { get; set; }

        public MongoBirdSearch(IBirdSearch birdSearch, TimeSpan expiration) :
            this(birdSearch, expiration, $"test_{Guid.NewGuid()}")
        {
        }

        public MongoBirdSearch(IBirdSearch birdSearch, TimeSpan expiration, string databaseName)
        {
            _birdSearch = birdSearch;
            Expiration = expiration;

            var mongoClientSettings = MongoClientSettings.FromConnectionString("mongodb://localhost");
            mongoClientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);

            _client = new MongoClient(mongoClientSettings);
            _database = _client.GetDatabase(databaseName);
            _flights = _database.GetCollection<FlightModel>("flights");
            _records = _database.GetCollection<RecordInfo>("records");
        }


        public IEnumerable<FlightModel> Search(string origin, string destination, DateTime startDate)
        {
            if (IsCacheStillValid(origin, destination))
                return GetCachedItems(origin, destination, startDate);

            IEnumerable<FlightModel> result;

            try
            {
                result = GetUncachedItems(origin, destination, startDate)
                    .ToList();

                AddItemsToCache(result, origin, destination);
            }
            catch (BlacklistedException e)
            {
                Console.WriteLine(e);
                result = GetCachedItems(origin, destination, startDate);
            }

            return result;
        }

        public IEnumerable<FlightHistoryModel> GetHistory(string origin, string destination, DateTime startDate, DateTime endDate)
        {
            return _flights
                .AsQueryable()
                .Where(x => x.Origin == origin && x.Destination == destination && x.TimeDepartureUtc >= startDate && x.TimeDeparture < endDate.AddDays(1))
                .OrderBy(x => x.TimeDepartureUtc)
                .AsEnumerable()
                .GroupBy(x => x.ProviderId)
                .Where(x => x.Any())
                .Select(x => new FlightHistoryModel()
                {
                    ProviderId = x.First().ProviderId,
                    Provider = x.First().Provider,
                    Origin = x.First().Origin,
                    Destination = x.First().Destination,
                    TimeDeparture = x.First().TimeDeparture,
                    TimeArrival = x.First().TimeArrival,
                    TimeDepartureUtc = x.First().TimeDepartureUtc,
                    TimeArrivalUtc = x.First().TimeArrivalUtc,
                    Duration = x.First().Duration,
                    Segments = x.First().Segments,
                    History = x.OrderBy(h => h.RegisteredDate).Select(h => new FlightHistoryEntryModel()
                    {
                        Price = h.Price,
                        RemainingSeats = h.RemainingSeats,
                        RegisteredDate = h.RegisteredDate
                    }).ToList()
                });
        }

        public void Drop()
        {
            _client.DropDatabase(_database.DatabaseNamespace.DatabaseName);
        }

        private IEnumerable<FlightModel> GetCachedItems(string origin, string destination, DateTime startDate)
        {
            return _flights
                .AsQueryable()
                .Where(x => x.Origin == origin && x.Destination == destination && x.TimeDepartureUtc >= startDate)
                .OrderBy(x => x.TimeDepartureUtc)
                .AsEnumerable()
                .GroupBy(x => x.ProviderId)
                .Where(x => x.Any())
                .Select(x => x.OrderByDescending(y => y.RegisteredDate).First());
        }

        private IEnumerable<FlightModel> GetUncachedItems(string origin, string destination, DateTime startDate)
        {
            return _birdSearch.Search(origin, destination, startDate);
        }

        private void AddItemsToCache(IEnumerable<FlightModel> items, string origin, string destination)
        {
            var list = items as ICollection<FlightModel> ?? items.ToList();
            _records.InsertOne(new RecordInfo
            {
                Id = ObjectId.GenerateNewId(),
                RegisterDate = DateTime.UtcNow,
                FlightsFound = list.Count,
                SourceDestination = GetSrcDstKeyPair(origin, destination)
            });

            _flights.InsertMany(items);
        }

        private bool IsCacheStillValid(string origin, string destination)
        {
            var cacheExpirationDate = GetCacheExpirationDate();
            var keyPair = GetSrcDstKeyPair(origin, destination);

            return _records.AsQueryable()
                .Any(x => x.RegisterDate > cacheExpirationDate && x.SourceDestination == keyPair);
        }

        private DateTime GetCacheExpirationDate() => DateTime.UtcNow - Expiration;

        private static string GetSrcDstKeyPair(string origin, string destination)
        {
            return $"{origin}-{destination}";
        }
    }
}
