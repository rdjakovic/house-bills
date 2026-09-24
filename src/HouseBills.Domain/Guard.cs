namespace HouseBills.Domain;

internal static class Guard
{
    public static string RequiredText(string? value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", paramName);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Value must be at most {maxLength} characters.", paramName);
        }

        return trimmed;
    }

    public static string? OptionalText(string? value, int maxLength, string paramName)
    {
        return string.IsNullOrWhiteSpace(value) ? null : RequiredText(value, maxLength, paramName);
    }

    public static decimal Money(decimal value, string paramName)
    {
        if (!MoneyRules.IsValidAmount(value))
        {
            throw new ArgumentOutOfRangeException(paramName, value, MoneyRules.InvalidAmountMessage);
        }

        return value;
    }

    public static int Id(int value, string paramName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Id must be positive.");
        }

        return value;
    }
}