using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

using HouseBills.Presentation.Resources;

namespace HouseBills.Wpf.Localization;

/// <summary>
/// WPF's DatePicker shows a built-in English "Select a date" placeholder (WPF has no Serbian translation of it).
/// This replaces it with <see cref="Strings.DatePicker_Watermark"/> for every DatePicker when it loads; views are
/// recreated on navigation, so a language change applies to the next page shown.
/// </summary>
internal static class DatePickerWatermark
{
    public static void Register()
    {
        EventManager.RegisterClassHandler(typeof(DatePicker), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnLoaded));
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (FindChild<DatePickerTextBox>((DatePicker)sender) is not { } textBox)
        {
            return;
        }

        textBox.ApplyTemplate();
        if (textBox.Template?.FindName("PART_Watermark", textBox) is ContentControl watermark)
        {
            watermark.Content = Strings.DatePicker_Watermark;
        }
    }

    private static T? FindChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match)
            {
                return match;
            }

            if (FindChild<T>(child) is { } nested)
            {
                return nested;
            }
        }

        return null;
    }
}