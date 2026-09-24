namespace HouseBills.Wpf.Localization;

/// <summary>A UI language the user can choose.</summary>
/// <param name="CultureName">UI culture, e.g. "en" or "sr-Latn-RS".</param>
/// <param name="DisplayName">Name in the language itself, e.g. "Srpski".</param>
public sealed record LanguageOption(string CultureName, string DisplayName)
{
    /// <summary>The visible name, which is also what screen readers announce for the list item.</summary>
    public override string ToString() => DisplayName;
}