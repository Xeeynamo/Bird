using System.Collections.Generic;

namespace SharpBird.Ryan.Models
{
    public class Availability
    {
        public string TermsOfUse { get; set; }

        public string Currency { get; set; }

        public int CurrPrecision { get; set; }

        public IEnumerable<Trip> Trips { get; set; }
    }
}
