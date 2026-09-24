using System.Globalization;

namespace HouseBills.Wpf.Localization;

/// <summary>Formats (dates, numbers, currency) that go with each UI language.</summary>
internal static class FormattingCultures
{
    /// <summary>
    /// Serbian formats: "1.234,56 RSD", "24.9.2026.". Based on sr-Latn-RS, but with two decimals for amounts
    /// (the stock culture shows whole dinars, which would round away the para part of stored amounts).
    /// </summary>
    public static CultureInfo Serbian { get; } = CreateSerbian();

    private static CultureInfo CreateSerbian()
    {
        var culture = (CultureInfo)CultureInfo.GetCultureInfo("sr-Latn-RS").Clone();
        culture.NumberFormat.CurrencyDecimalDigits = 2;
        return CultureInfo.ReadOnly(culture);
    }
}