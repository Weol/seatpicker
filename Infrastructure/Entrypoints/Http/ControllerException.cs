namespace Seatpicker.Infrastructure.Entrypoints.Http;

public class ControllerException : Exception
{
    public ControllerException(string message) : base(message)
    {

    }
}