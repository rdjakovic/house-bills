namespace HouseBills.Wpf.Tests.Localization;

/// <summary>
/// Tests that change the process-wide UI language (<c>CultureInfo.DefaultThreadCurrentUICulture</c>) run alone,
/// so they can't change the language under other tests that assert English texts.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class UiCultureCollection
{
    public const string Name = "UI culture";
}