using System;

namespace SharpBird.Ryan.Models
{
    public class Segment
    {
        public int SegmentNr { get; set; }

        public string Origin { get; set; }

        public string Destination { get; set; }

        public string FlightNumber { get; set; }

        public DateTime[] Time { get; set; }

        public DateTime[] TimeUTC { get; set; }

        public string Duration { get; set; }
    }
}
