using System;

namespace SharpBird.Models
{
    public class FlightHistoryEntryModel
    {
        public double Price { get; set; }

        public int RemainingSeats { get; set; }

        public DateTime RegisteredDate { get; set; }
    }
}
