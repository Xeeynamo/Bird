using System;

namespace SharpBird.Ryan.Exceptions
{
    public class BlacklistedException : Exception
    {
        public BlacklistedException() :
            base("All your dreams fade to nothing.")
        { }
    }
}
