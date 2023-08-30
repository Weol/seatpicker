namespace Seatpicker.Application;

public abstract class ApplicationException : Exception
{
    protected ApplicationException()
    {
    }

    protected ApplicationException(string message) : base(message)
    {
    }

    protected abstract string ErrorMessage { get; }

    public override string Message => ErrorMessage;
}