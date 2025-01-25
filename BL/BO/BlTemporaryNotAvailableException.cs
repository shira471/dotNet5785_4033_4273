
namespace BO
{
    [Serializable]
    internal class BlTemporaryNotAvailableException : Exception
    {
        public BlTemporaryNotAvailableException()
        {
        }

        public BlTemporaryNotAvailableException(string? message) : base(message)
        {
        }

        public BlTemporaryNotAvailableException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}