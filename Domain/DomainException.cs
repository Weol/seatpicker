namespace Seatpicker.Domain;

public abstract class DomainException : Exception
{
    protected internal DomainException()
    {
    }

    protected internal DomainException(string message) : base(message)
    {
    }

    protected abstract string ErrorMessage { get; }

    public override string Message => ErrorMessage;
}