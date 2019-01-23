using System;
using System.Collections.Generic;
using SharpBird.Models;

namespace SharpBird
{
    public interface IBirdSearch
    {
        IEnumerable<FlightModel> Search(string origin, string destination, DateTime startDate);
    }
}
