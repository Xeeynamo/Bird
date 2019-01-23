using System.Collections.Generic;

namespace SharpBird.Ryan.Models
{
    public class Trip
    {
        public string Origin { get; set; }

        public string OriginName { get; set; }

        public string Destination { get; set; }

        public string DestinationName { get; set; }

        public IEnumerable<TripDates> Dates { get; set; }
    }
}
