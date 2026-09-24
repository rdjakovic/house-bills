using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

using HouseBills.Domain;

namespace HouseBills.Wpf.Converters;

/// <summary>Maps a <see cref="BillStatus"/> to a brush resource named <c>Status{Status}Brush</c> (see Themes/Styles.xaml).</summary>
[ValueConversion(typeof(BillStatus), typeof(Brush))]
public sealed class BillStatusToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is BillStatus status
            ? System.Windows.Application.Current.TryFindResource($"Status{status}Brush") as Brush ?? DependencyProperty.UnsetValue
            : DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}