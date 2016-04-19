using System;

namespace SomeLibraries
{
    public class KnownException : Exception
    {
        public KnownException()
        {

        }
        public KnownException(string message)
            : base(message)
        { }

    }

}
