using System;

namespace SharpBird.Exceptions
{
    public class BlacklistedException : Exception
    {
        public BlacklistedException() :
            base("All your dreams fade to nothing.")
        { }
    }
}
