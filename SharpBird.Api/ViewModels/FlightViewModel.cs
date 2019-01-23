using System;
using System.Collections.Generic;

namespace SharpBird.Api.ViewModels
{
    public class FlightViewModel
    {
        public double Fare { get; set; }

        public List<DateTime> Dates { get; set; }
    }
}
