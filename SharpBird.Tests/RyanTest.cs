using SharpBird.Ryan;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SharpBird.Tests
{
    public class RyanTest
    {
        [Fact]
        public async Task GetFlightsAvailabilityTest()
        {
            using (var ryan = new RyanBird())
            {
                var origin = "STN";
                var destination = "NYO";
                int datesCount = 6;

                var availability = await ryan.GetFlightsAvailability(origin, destination, DateTime.Today.AddDays(1), datesCount);

                Assert.Equal("https://www.ryanair.com/ie/en/corporate/terms-of-use=AGREED", availability.TermsOfUse);
                Assert.Equal("GBP", availability.Currency);
                Assert.Equal(2, availability.CurrPrecision);
                Assert.NotEmpty(availability.Trips);

                var trip = availability.Trips.First();
                Assert.Equal(origin, trip.Origin);
                Assert.Equal(destination, trip.Destination);

                // Cannot really test much about an external API, mostly if no flights are found...

                Assert.Equal(datesCount + 1, trip.Dates.Count());
            }
        }

        [Fact]
        public void GetFlightsTest()
        {
            using (var ryan = new RyanBird())
            {
                var origin = "STN";
                var destination = "NYO";
                int datesCount = 10;
                var startDate = DateTime.Today.AddDays(1);

                var flightDates = ryan.GetFlights(origin, destination, startDate)
                    .Take(datesCount)
                    .ToList();

                Assert.Equal(datesCount, flightDates.Count);

                var expectedDate = new DateTime(startDate.Year, startDate.Month, startDate.Day);
                foreach (var item in flightDates)
                {
                    Assert.Equal(expectedDate, item.DateOut);
                    expectedDate = expectedDate.AddDays(1);
                }
            }
        }
    }
}
