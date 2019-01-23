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
            using (var httpClient = NewHttpClient())
            {
                return GetFlights(httpClient, origin, destination, startDate)
                    .Where(x => x.Flights.Any())
                    .Select(x => new
                    {
                        Date = x.DateOut,
                        Flight = x.Flights.First()  // TODO not a nice hack. Do a SelectMany while maintaining the Date
                    })
                    .Where(x => x.Flight.RegularFare.Fares.Any())
                    .Select(x => new
                    {
                        x.Date,
                        x.Flight,
                        Fare = x.Flight.RegularFare.Fares
                            .OrderBy(fare => Math.Min(fare.Amount, fare.PublishedFare))
                            .FirstOrDefault()
                    })
                    .Select(x => new FlightModel
                    {
                        Id = string.Empty,
                        Provider = Provider,
                        Origin = origin,
                        Destination = destination,
                        Date = x.Date,
                        Price = x.Fare.Amount,
                        Duration = x.Flight.Duration,
                        RegisteredDate = DateTime.UtcNow,
                    });
            }
        }

        private IEnumerable<TripDates> GetFlights(HttpClient client, string origin, string destination, DateTime dateOut)
        {
            List<TripDates> tripDates;
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

        //public async IAsyncEnumerable<TripDates> GetFlightsAsync(string origin, string destination, DateTime dateOut)
        //{
        //    List<TripDates> tripDates;
        //    do
        //    {
        //        tripDates = await GetFlightsDates(origin, destination, dateOut);
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
