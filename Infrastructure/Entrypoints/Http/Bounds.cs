using FluentValidation;

namespace Seatpicker.Infrastructure.Entrypoints.Http;

public record Bounds(double X, double Y, double Width, double Height)
{
    public static Bounds FromDomainBounds(Domain.Bounds bounds)
    {
        return new Bounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    public Domain.Bounds ToDomainBounds()
    {
        return new Domain.Bounds(X, Y, Width, Height);
    }
}

public class BoundsValidator : AbstractValidator<Bounds>
{
    public BoundsValidator()
    {
        RuleFor(x => x.Width).GreaterThan(0);

        RuleFor(x => x.Height).GreaterThan(0);
    }
}