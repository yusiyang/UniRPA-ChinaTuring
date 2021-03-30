using System;

namespace UniWorkforce.Exceptions
{
    public class AuthorizationExpiredException : Exception
    {
        public AuthorizationExpiredException(string message) : base(message)
        {
        }
    }
}
