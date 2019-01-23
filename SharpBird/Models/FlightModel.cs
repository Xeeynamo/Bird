using System;

namespace SharpBird.Models
{
    public class FlightModel
    {
        public string Id { get; set; }

        public string Provider { get; set; }

        public string Origin { get; set; }

        public string Destination { get; set; }

        public DateTime Date { get; set; }

        public double Price { get; set; }

        public string Duration { get; set; }

        public DateTime RegisteredDate { get; set; }
    }
}
