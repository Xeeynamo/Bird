using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NSubstitute;
using NSubstitute.ClearExtensions;
using SharpBird.Models;
using SharpBird.Mongo;
using SharpBird.Utilities;
using Xunit;

namespace SharpBird.Tests
{
    public class MongoBirdTest : IDisposable
    {
        private MongoBirdSearch mongoBirdSearch;

        public void Dispose()
        {
            (mongoBirdSearch as IDisposable)?.Dispose();
            mongoBirdSearch?.Drop();
        }

        [Fact]
        public void CanConnectToMongoDb()
        {
            var birdSearch = Substitute.For<IBirdSearch>();
            mongoBirdSearch = new MongoBirdSearch(birdSearch, default);
        }

        [Fact]
        public void ShouldNotCallUnderlyingBirdTwiceWithHighExpirationTime()
        {
            var birdSearch = Substitute.For<IBirdSearch>();
            birdSearch.Search(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>())
                .Returns(GetRandomFlights(10));
            mongoBirdSearch = new MongoBirdSearch(birdSearch, TimeSpan.FromHours(1));

            Assert.Empty(birdSearch.ReceivedCalls());
            mongoBirdSearch.Search("mockSrc", "mockDst", DateTime.Now);
            Assert.Single(birdSearch.ReceivedCalls());
            mongoBirdSearch.Search("mockSrc", "mockDst", DateTime.Now);
            Assert.Single(birdSearch.ReceivedCalls());
        }

        [Fact]
        public void ShouldCallUnderlyingBirdTwiceWhenCacheExpires()
        {
            var birdSearch = Substitute.For<IBirdSearch>();
            birdSearch.Search(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>())
                .Returns(GetRandomFlights(10));
            mongoBirdSearch = new MongoBirdSearch(birdSearch, TimeSpan.FromMilliseconds(500));

            Assert.Empty(birdSearch.ReceivedCalls());
            mongoBirdSearch.Search("mockSrc", "mockDst", DateTime.Now);
            Assert.Single(birdSearch.ReceivedCalls());
            mongoBirdSearch.Search("mockSrc", "mockDst", DateTime.Now);
            Assert.Single(birdSearch.ReceivedCalls());

            Thread.Sleep(500);
            mongoBirdSearch.Search("mockSrc", "mockDst", DateTime.Now);
            Assert.Equal(2, birdSearch.ReceivedCalls().Count());
        }

        private IEnumerable<FlightModel> GetRandomFlights(int count) =>
            Enumerable.Range(0, count).Select(GetRandomFlight);

        private FlightModel GetRandomFlight(int seed)
        {
            var today = DateTime.Today.AddDays(seed);

            return new FlightModel
            {
                Id = Guid.NewGuid(),
                Provider = "testing",
                ProviderId = PseudoRandomUtil.RandomString(seed, 16),
                Origin = PseudoRandomUtil.RandomString(seed, 3),
                Destination = PseudoRandomUtil.RandomString(seed, 4),
                TimeDeparture = today,
                TimeArrival = today.AddMinutes(95),
                TimeDepartureUtc = today,
                TimeArrivalUtc = today.AddMinutes(95),
                Duration = "1:35",
                Segments = 1,
                Price = PseudoRandomUtil.RandomDouble(seed) * 100.0,
                RemainingSeats = 5,
                RegisteredDate = DateTime.Today
            };
        }
    }
}
