using System;

namespace CommonX.Storage.Exceptions
{
    public class ChunkReadException : Exception
    {
        public ChunkReadException(string message) : base(message) { }
    }
}
