using System;

namespace CommonX.Storage.Exceptions
{
    public class ChunkCreateException : Exception
    {
        public ChunkCreateException(string message) : base(message) { }
    }
}
