using System;
using System.Collections.Generic;

namespace SharpBird.Models
{
    public class FlightHistoryModel
    {
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

        public List<FlightHistoryEntryModel> History
        { get; set; }
    }
}
