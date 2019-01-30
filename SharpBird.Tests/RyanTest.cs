using SharpBird.Ryan;
using System;
using System.Linq;
using Xunit;

namespace SharpBird.Tests
{
    public class RyanTest
    {
        private readonly IBirdSearch birdSearch;

        public RyanTest()
        {
            birdSearch = new RyanBird();
        }

        [Fact]
        public void GetFlightsTest()
        {
            const string origin = "STN";
            const string destination = "NYO";
            const int datesCount = 10;
            var startDate = DateTime.Today.AddDays(1);

            var flightDates = birdSearch.Search(origin, destination, startDate)
                .Take(datesCount)
                .ToList();

            Assert.Equal(datesCount, flightDates.Count);
        }
    }
}
