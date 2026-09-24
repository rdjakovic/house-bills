using System.Globalization;
using System.Windows.Data;

namespace HouseBills.Wpf.Converters;

/// <summary>Lets a <c>DatePicker.SelectedDate</c> (DateTime?) bind to a DateOnly? view-model property.</summary>
[ValueConversion(typeof(DateOnly?), typeof(DateTime?))]
public sealed class DateOnlyToDateTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is DateOnly date ? date.ToDateTime(TimeOnly.MinValue) : null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is DateTime dateTime ? DateOnly.FromDateTime(dateTime) : null;
    }
}