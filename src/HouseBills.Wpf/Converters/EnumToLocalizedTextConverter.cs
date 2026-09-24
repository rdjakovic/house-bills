using System.Globalization;
using System.Windows.Data;

using HouseBills.Wpf.Localization;

namespace HouseBills.Wpf.Converters;

/// <summary>Shows an enum value in the UI language, using the resource key <c>Enum_{Type}_{Value}</c>.</summary>
[ValueConversion(typeof(Enum), typeof(string))]
public sealed class EnumToLocalizedTextConverter : IValueConverter
{
    public static string KeyFor(Enum value) => $"Enum_{value.GetType().Name}_{value}";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Enum enumValue ? LocalizedStrings.Instance[KeyFor(enumValue)] : value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}