using System.Globalization;

namespace HouseBills.Wpf.Localization;

/// <summary>The UI language: which ones exist, which one is active, and switching between them.</summary>
public interface ILocalizationService
{
    IReadOnlyList<LanguageOption> Languages { get; }

    LanguageOption Current { get; }

    /// <summary>Formats that go with <see cref="Current"/>: Serbian formats for Serbian, Windows regional settings for English.</summary>
    CultureInfo FormattingCulture { get; }

    /// <summary>Applies the saved language, or the Windows display language if Serbian, otherwise English.</summary>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>Switches the UI language immediately, notifies view models and saves the choice.</summary>
    Task SetLanguageAsync(LanguageOption language, CancellationToken cancellationToken);
}