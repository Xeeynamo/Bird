using System;
using System.Diagnostics;

namespace SharpBird.Api.Utils
{
    public static class Helpers
    {
        public static (T, double) EvaluateWithElapsed<T>(Func<T> func)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = func();
            return (func(), stopwatch.ElapsedMilliseconds / 1000.0);
        }
    }
}
