namespace CognitiveServices.Db.Exceptions;
public class DuplicateKeyException : Exception
{
    public DuplicateKeyException() : base() { }
    public DuplicateKeyException(string message) : base(message) { }
    public DuplicateKeyException(string message, Exception innerException) : base(message, innerException) { }
}
