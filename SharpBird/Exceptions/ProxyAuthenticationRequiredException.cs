using System;

namespace SharpBird.Exceptions
{
    public class ProxyAuthenticationRequiredException : Exception
    {
        public ProxyAuthenticationRequiredException() :
            base("Proxy Authentication Required")
        { }
    }
}
