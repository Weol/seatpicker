namespace Seatpicker.Infrastructure.Entrypoints.Controllers;

public class ControllerException : Exception
{
    public ControllerException(string message) : base(message)
    {

    }
}