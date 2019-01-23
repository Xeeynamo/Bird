using System.Collections.Generic;

namespace SharpBird.Ryan.Models
{
    public class TripFare
    {
        public string FareKey { get; set; }

        public string FareClass { get; set; }

        public IEnumerable<TripFareItem> Fares { get; set; }
    }

    public class TripFareItem
    {
        public string Type { get; set; }

        public double Amount { get; set; }

        public int Count { get; set; }

        public bool HasDiscount { get; set; }

        public double PublishedFare { get; set; }

        public double DiscountInPercent { get; set; }

        public bool HasPromoDiscount { get; set; }
    }
}
