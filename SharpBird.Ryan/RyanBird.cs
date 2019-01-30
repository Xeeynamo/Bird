using SharpBird.Ryan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SharpBird.Models;
using SharpBird.Extensions;

namespace SharpBird.Ryan
{
    public class RyanBird : IBirdSearch
    {
        private static readonly string BaseApi = "https://desktopapps.ryanair.com/v4/en-ie/";
        private static readonly string Provider = "Ryanair";

        public IEnumerable<FlightModel> Search(string origin, string destination, DateTime startDate)
        {
            return GetFlights(origin, destination, startDate)
                .Where(x => x.Flights.Any())
                .SelectMany(x => x.Flights)
                .Where(x => x.RegularFare.Fares.Any())
                .Select(x => new
                {
                    Flight = x,
                    MinimumFare = x.RegularFare.Fares
                        .OrderBy(fare => Math.Min(fare.Amount, fare.PublishedFare))
                        .FirstOrDefault()
                })
                .Select(x => new FlightModel
                {
                    Id = x.Flight.FlightKey,
                    Provider = Provider,
                    Origin = origin,
                    Destination = destination,
                    TimeDeparture = x.Flight.Time.First(),
                    TimeArrival = x.Flight.Time.Last(),
                    TimeDepartureUtc = x.Flight.TimeUTC.First(),
                    TimeArrivalUtc = x.Flight.TimeUTC.Last(),
                    Duration = x.Flight.Duration,
                    Segments = x.Flight.Segments.Count,
                    Price = x.MinimumFare.Amount,
                    RemainingSeats = x.Flight.FaresLeft,
                    RegisteredDate = DateTime.UtcNow,
                });
        }

        private IEnumerable<TripDates> GetFlights(string origin, string destination, DateTime dateOut)
        {
            List<TripDates> tripDates;

            using var client = NewHttpClient();
            do
            {
                tripDates = GetFlightsDates(client, origin, destination, dateOut)
                    .Result
                    .Where(x => x.Flights.Any())
                    .ToList();

                foreach (var tripDate in tripDates)
                {
                    yield return tripDate;
                }
                dateOut = dateOut.AddDays(tripDates.Count);

            } while (tripDates?.Count > 0);
        }

        //public async IAsyncEnumerable<TripDates> GetFlightsAsync(HttpClient client, string origin, string destination, DateTime dateOut)
        //{
        //    List<TripDates> tripDates;
        //    do
        //    {
        //        tripDates = await GetFlightsDates(client, origin, destination, dateOut);
        //        foreach (var tripDate in tripDates)
        //        {
        //            yield return tripDate;
        //        }
        //    } while (tripDates?.Count > 0);
        //}

        private async Task<Availability> GetFlightsAvailability(HttpClient client, string origin, string destination, DateTime dateOut, int count = 6)
        {
            return await Get<Availability>(client, "availability", new Dictionary<string, string>()
            {
                ["ADT"] = 1.ToString(),
                ["CHD"] = 0.ToString(),
                ["DateOut"] = dateOut.ToString("yyyy-MM-dd"),
                ["Destination"] = destination,
                ["FlexDaysOut"] = count.ToString(),
                ["INF"] = 0.ToString(),
                ["IncludeConnectingFlights"] = true.ToString(),
                ["Origin"] = origin,
                ["RoundTrip"] = false.ToString(),
                ["TEEN"] = 0.ToString(),
                ["ToUs"] = "AGREED",
                ["exists"] = false.ToString(),
                ["promoCode"] = string.Empty
            });
        }

        private async Task<List<TripDates>> GetFlightsDates(HttpClient client, string origin, string destination, DateTime dateOut, int count = 6)
        {
            return (await GetFlightsAvailability(client, origin, destination, dateOut, count))
                .Trips
                .FirstOrDefault()?
                .Dates?
                .ToList() ?? new List<TripDates>();
        }

        private Task<T> Get<T>(HttpClient client, string api, Dictionary<string, string> query = null) =>
            client.Get<T>(BaseApi + api, query);

        private HttpClient NewHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }
    }
}
