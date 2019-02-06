using System;
using System.Linq;

namespace SharpBird.Utilities
{
    public static class PseudoRandomUtil
    {

        public static int RandomInt(int seed, int randomization)
        {
            var n = new Random(seed).Next();
            return randomization == 0 ? n : RandomInt(n, randomization - 1);
        }

        public static double RandomDouble(int seed)
        {
            return new Random(seed).NextDouble();
        }

        public static Guid RandomGuid(int seed)
        {
            var rand = new Random(seed);
            var guid = new byte[16];
            rand.NextBytes(guid);

            return new Guid(guid);
        }

        public static string RandomString(int seed, int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var rand = new Random(RandomInt(seed, length));
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[rand.Next(s.Length)]).ToArray());
        }
    }
}
