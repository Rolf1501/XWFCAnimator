using System;

namespace XWFC
{
    public class NoMoreChoicesException : Exception
    {
        public NoMoreChoicesException(string message)
        {
            Console.WriteLine(message);
        }
    }
}
