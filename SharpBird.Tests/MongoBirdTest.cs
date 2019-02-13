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

        [Fact]
        public void CacheShouldWorkOnlyWithASpecificTrip()
        {
            var birdSearch = Substitute.For<IBirdSearch>();
            birdSearch.Search(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>())
                .Returns(GetRandomFlights(10));
            mongoBirdSearch = new MongoBirdSearch(birdSearch, TimeSpan.FromHours(1));

            Assert.Empty(birdSearch.ReceivedCalls());
            mongoBirdSearch.Search("mockSrc1", "mockDst", DateTime.Now);
            Assert.Single(birdSearch.ReceivedCalls());
            mongoBirdSearch.Search("mockSrc2", "mockDst", DateTime.Now);
            Assert.Equal(2, birdSearch.ReceivedCalls().Count());
            Assert.Equal("mockSrc1", birdSearch.ReceivedCalls().Skip(0).First().GetArguments().First());
            Assert.Equal("mockSrc2", birdSearch.ReceivedCalls().Skip(1).First().GetArguments().First());
        }

        [Fact]
        public void ShouldReturnOnlyOneKindOfSourceDestinationPair()
        {
            const string FlightSrc = "TST1";
            const string FlightDst = "TST2";
            var expected = GetRandomFlights(10, 1, FlightSrc, FlightDst).ToList();

            var birdSearch = Substitute.For<IBirdSearch>();
            birdSearch.Search(Arg.Is(FlightSrc), Arg.Is(FlightDst), Arg.Any<DateTime>())
                .Returns(expected);
            birdSearch.Search(Arg.Is(FlightDst), Arg.Is(FlightSrc), Arg.Any<DateTime>())
                .Returns(GetRandomFlights(10, 2, FlightDst, FlightSrc));
            mongoBirdSearch = new MongoBirdSearch(birdSearch, TimeSpan.FromHours(1));

            mongoBirdSearch.Search(FlightSrc, FlightDst, DateTime.Today);
            mongoBirdSearch.Search(FlightDst, FlightSrc, DateTime.Today);

            var actual = mongoBirdSearch.Search(FlightSrc, FlightDst, DateTime.Today)
                .ToList();

            Assert.All(actual, x =>
            {
                Assert.Equal(FlightSrc, x.Origin);
                Assert.Equal(FlightDst, x.Destination);
            });

            Assert.Equal(expected.Count, actual.Count);
            Assert.All(actual, act =>
            {
                Assert.True(expected.Any(exp => exp.Id == act.Id));
            });
        }

        private IEnumerable<FlightModel> GetRandomFlights(int count, int seed = 0, string origin = null, string destination = null) =>
            Enumerable.Range(0, count).Select(x => GetRandomFlight(count * 1000 + seed * 50 + x, origin, destination));

        private FlightModel GetRandomFlight(int seed, string origin, string destination)
        {
            var today = DateTime.Today.AddDays(seed);

            return new FlightModel
            {
                Id = Guid.NewGuid(),
                Provider = "testing",
                ProviderId = PseudoRandomUtil.RandomString(seed, 16),
                Origin = origin ?? PseudoRandomUtil.RandomString(seed, 3),
                Destination = destination ?? PseudoRandomUtil.RandomString(seed, 4),
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
