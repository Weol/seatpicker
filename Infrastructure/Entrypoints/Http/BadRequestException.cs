namespace Seatpicker.Infrastructure.Entrypoints.Http;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }
}