using Newtonsoft.Json;
using SharpBird.Ryan.Exceptions;
using SharpBird.Ryan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SharpBird.Ryan
{
    public class RyanUtil : IDisposable
    {
        private static readonly string BaseApi = "https://desktopapps.ryanair.com/v4/en-ie/";

        private readonly HttpClient httpClient;

        public RyanUtil()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }

        public IEnumerable<TripDates> GetFlights(string origin, string destination, DateTime dateOut)
        {
            List<TripDates> tripDates;
            do
            {
                tripDates = GetFlightsDates(origin, destination, dateOut)
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

        public async Task<Availability> GetFlightsAvailability(string origin, string destination, DateTime dateOut, int count = 6)
        {
            return await Get<Availability>("availability", new Dictionary<string, string>()
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

        private async Task<List<TripDates>> GetFlightsDates(string origin, string destination, DateTime dateOut, int count = 6)
        {
            return (await GetFlightsAvailability(origin, destination, dateOut, count))
                .Trips
                .FirstOrDefault()?
                .Dates?
                .ToList() ?? new List<TripDates>();
        }

        private async Task<T> Get<T>(string api, Dictionary<string, string> query = null)
        {
            var strQuery = query?.Count > 0 ? "?" + 
                string.Join("&", query.Select(x => $"{x.Key}={x.Value}")) : string.Empty;

            using (var response = await httpClient.GetAsync(BaseApi + api + strQuery))
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        throw new BlacklistedException();
                    case HttpStatusCode.ProxyAuthenticationRequired:
                        throw new ProxyAuthenticationRequiredException();
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
        }
    }
}
