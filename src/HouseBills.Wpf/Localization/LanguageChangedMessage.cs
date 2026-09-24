namespace HouseBills.Wpf.Localization;

/// <summary>Sent through <c>IMessenger</c> after the UI language changed, so view models can refresh their texts.</summary>
public sealed record LanguageChangedMessage(LanguageOption Language);