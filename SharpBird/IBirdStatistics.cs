using System;
using System.Collections.Generic;
using SharpBird.Models;

namespace SharpBird
{
    public interface IBirdStatistics
    {
        IEnumerable<FlightHistoryModel> GetHistory(string origin, string destination, DateTime startDate, DateTime endDate);
    }
}
