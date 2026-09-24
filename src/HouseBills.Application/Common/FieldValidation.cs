using System.Globalization;

using HouseBills.Application.Resources;
using HouseBills.Domain;

namespace HouseBills.Application.Common;

/// <summary>
/// Service-level input checks that mirror the domain invariants, producing user-facing messages in the current UI language.
/// </summary>
internal static class FieldValidation
{
    /// <param name="errors">Collected messages.</param>
    /// <param name="value">Input text.</param>
    /// <param name="maxLength">Maximum length after trimming.</param>
    /// <param name="field">Localized field name, e.g. <see cref="Messages.Field_Name"/>.</param>
    /// <param name="required">Whether an empty value is an error.</param>
    public static void Text(List<string> errors, string? value, int maxLength, string field, bool required)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (required && trimmed.Length == 0)
        {
            errors.Add(Format(Messages.Validation_Required, field));
            return;
        }

        if (trimmed.Length > maxLength)
        {
            errors.Add(Format(Messages.Validation_MaxLength, field, maxLength));
        }
    }

    public static void Amount(List<string> errors, decimal value, string field)
    {
        if (!MoneyRules.IsValidAmount(value))
        {
            errors.Add(Format(Messages.Validation_InvalidAmount, field));
        }
    }

    public static Error? ToError(List<string> errors)
    {
        return errors.Count == 0 ? null : Error.Validation(string.Join(Environment.NewLine, errors));
    }

    public static string Format(string format, params object[] args) => string.Format(CultureInfo.CurrentCulture, format, args);
}