using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

public sealed record SystemId
{
    private SystemId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ValidationException("System id must not be empty.");
        }

        Value = value;
    }

    public Guid Value { get; }

    public static SystemId New() => new(Guid.NewGuid());

    public static SystemId From(Guid value) => new(value);

    public static SystemId Parse(string value)
    {
        if (!Guid.TryParse(value, out var parsed))
        {
            throw new ValidationException("System id must be a valid GUID.");
        }

        return new SystemId(parsed);
    }

    public override string ToString() => Value.ToString("D");
}
