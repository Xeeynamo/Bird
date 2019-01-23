using System.Collections.Generic;

namespace SharpBird.Api.ViewModels
{
    public class FlightResultViewModel
    {
        public string Origin { get; set; }

        public string Destination { get; set; }

        public IEnumerable<FlightViewModel> Flights { get; set; }
    }
}
