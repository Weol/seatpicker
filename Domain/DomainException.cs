namespace Seatpicker.Domain;

public abstract class DomainException : Exception
{
    protected DomainException()
    {
    }

    protected DomainException(string message) : base(message)
    {
    }
}