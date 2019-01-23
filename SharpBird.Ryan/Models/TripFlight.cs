namespace SharpBird.Ryan.Models
{
    public class TripFlight
    {
        public int FaresLeft { get; set; }

        public string FlightKey { get; set; }

        public int InfantsLeft { get; set; }

        public TripFare RegularFare { get; set; }

        public string OperatedBy { get; set; }

        // Segments

        public string FlightNumber { get; set; }

        // Time

        // TimeUtc

        public string Duration { get; set; }
    }
}
