namespace Seatpicker.Application;

public abstract class ApplicationException : Exception
{
    protected ApplicationException()
    {
    }

    protected ApplicationException(string message) : base(message)
    {
    }
}