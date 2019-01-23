using System;

namespace SharpBird.Ryan.Exceptions
{
    public class ProxyAuthenticationRequiredException : Exception
    {
        public ProxyAuthenticationRequiredException() :
            base("Proxy Authentication Required")
        { }
    }
}
