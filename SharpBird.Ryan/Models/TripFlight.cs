using System;
using System.Collections.Generic;

namespace SharpBird.Ryan.Models
{
    public class TripFlight
    {
        public int FaresLeft { get; set; }

        public string FlightKey { get; set; }

        public int InfantsLeft { get; set; }

        public TripFare RegularFare { get; set; }

        public string OperatedBy { get; set; }

        public List<Segment> Segments { get; set; }

        public string FlightNumber { get; set; }

        public DateTime[] Time { get; set; }

        public DateTime[] TimeUTC { get; set; }

        public string Duration { get; set; }
    }
}
