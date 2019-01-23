using System;
using System.Collections.Generic;

namespace SharpBird.Ryan.Models
{
    public class TripDates
    {
        public DateTime DateOut { get; set; }

        public List<TripFlight> Flights { get; set; }
    }
}
