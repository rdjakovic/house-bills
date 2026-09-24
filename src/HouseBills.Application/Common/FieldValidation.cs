using HouseBills.Domain;

namespace HouseBills.Application.Common;

/// <summary>Service-level input checks that mirror the domain invariants, producing user-facing messages.</summary>
internal static class FieldValidation
{
    public static void Text(List<string> errors, string? value, int maxLength, string field, bool required)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (required && trimmed.Length == 0)
        {
            errors.Add($"{field} is required.");
            return;
        }

        if (trimmed.Length > maxLength)
        {
            errors.Add($"{field} must be at most {maxLength} characters.");
        }
    }

    public static void Amount(List<string> errors, decimal value, string field)
    {
        if (!MoneyRules.IsValidAmount(value))
        {
            errors.Add($"{field}: {MoneyRules.InvalidAmountMessage}");
        }
    }

    public static Error? ToError(List<string> errors)
    {
        return errors.Count == 0 ? null : Error.Validation(string.Join(Environment.NewLine, errors));
    }
}