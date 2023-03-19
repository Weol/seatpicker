namespace Seatpicker.Application;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}