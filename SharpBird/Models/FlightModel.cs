using System;

namespace SharpBird.Models
{
    public class FlightModel
    {
        public Guid Id { get; set; }

        public string ProviderId { get; set; }

        public string Provider { get; set; }

        public string Origin { get; set; }

        public string Destination { get; set; }

        public DateTime TimeDeparture { get; set; }

        public DateTime TimeArrival { get; set; }

        public DateTime TimeDepartureUtc { get; set; }

        public DateTime TimeArrivalUtc { get; set; }

        public string Duration { get; set; }

        public int Segments { get; set; }

        public double Price { get; set; }

        public int RemainingSeats { get; set; }

        public DateTime RegisteredDate { get; set; }
    }
}
