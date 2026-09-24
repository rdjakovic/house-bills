using System.Windows.Data;
using System.Windows.Markup;


namespace HouseBills.Wpf.Localization;

/// <summary>
/// Localized text in XAML that updates when the language changes: <c>Text="{loc:Tr Bills_New}"</c>.
/// Keys are the names in <c>HouseBills.Presentation.Resources/Strings.resx</c>.
/// </summary>
[MarkupExtensionReturnType(typeof(object))]
public sealed class TrExtension(string key) : MarkupExtension
{
    public string Key { get; } = key;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding($"[{Key}]")
        {
            Source = LocalizedStrings.Instance,
            Mode = BindingMode.OneWay,
        };
        return binding.ProvideValue(serviceProvider);
    }
}