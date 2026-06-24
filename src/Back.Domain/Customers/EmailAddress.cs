using Back.Domain.Common;

namespace Back.Domain.Customers;

public sealed class EmailAddress : ValueObject
{
    private EmailAddress()
    {
    }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public string Value { get; private set; } = string.Empty;

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email is required.", nameof(value));
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!normalized.Contains('@') || normalized.Length > 256)
        {
            throw new ArgumentException("Email is not valid.", nameof(value));
        }

        return new EmailAddress(normalized);
    }

    public override string ToString()
    {
        return Value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
