using System;

namespace CommonX.Storage.Exceptions
{
    public class ChunkBadDataException : Exception
    {
        public ChunkBadDataException(string message) : base(message)
        {
        }
    }
}
