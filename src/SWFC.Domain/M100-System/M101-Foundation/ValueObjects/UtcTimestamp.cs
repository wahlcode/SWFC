using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

public sealed record UtcTimestamp
{
    private UtcTimestamp(DateTimeOffset value)
    {
        Value = value.ToUniversalTime();
    }

    public DateTimeOffset Value { get; }
    public DateTime UtcDateTime => Value.UtcDateTime;

    public static UtcTimestamp Now() => new(DateTimeOffset.UtcNow);

    public static UtcTimestamp From(DateTimeOffset value)
    {
        if (value == default)
        {
            throw new ValidationException("UTC timestamp must be provided.");
        }

        return new UtcTimestamp(value);
    }

    public static UtcTimestamp From(DateTime value)
    {
        if (value == default)
        {
            throw new ValidationException("UTC timestamp must be provided.");
        }

        var offset = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();

        return new UtcTimestamp(new DateTimeOffset(offset, TimeSpan.Zero));
    }

    public override string ToString() => Value.ToString("O");
}
