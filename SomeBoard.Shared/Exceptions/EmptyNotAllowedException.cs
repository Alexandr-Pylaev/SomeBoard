namespace SomeBoard.Shared.Exceptions;

public class EmptyNotAllowedException : Exception
{
    public EmptyNotAllowedException () : this("Empty is not allowed.")
    {}

    public EmptyNotAllowedException (string message) 
        : base(message)
    {}

    public EmptyNotAllowedException (string message, Exception innerException)
        : base (message, innerException)
    {}    
}