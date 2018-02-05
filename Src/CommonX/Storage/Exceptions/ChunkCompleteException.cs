using System;

namespace CommonX.Storage.Exceptions
{
    public class ChunkCompleteException : Exception
    {
        public ChunkCompleteException(string message) : base(message) { }
    }
}
