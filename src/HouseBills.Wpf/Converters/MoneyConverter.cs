using System.Globalization;
using System.Windows.Data;

using HouseBills.Wpf.Localization;

namespace HouseBills.Wpf.Converters;

/// <summary>
/// Formats an amount as currency in the chosen language's formats ("1.234,56 RSD" for Serbian). Used instead of
/// StringFormat=C because WPF's element Language can only carry stock cultures (stock Serbian shows no decimals).
/// </summary>
[ValueConversion(typeof(decimal), typeof(string))]
public sealed class MoneyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is decimal amount ? amount.ToString("C", LocalizedStrings.FormattingCulture) : value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}