using System.Globalization;

using CommunityToolkit.Mvvm.Messaging;

using HouseBills.Application.Preferences;
using HouseBills.Presentation.Resources;
using HouseBills.Wpf.Localization;

using NSubstitute;

namespace HouseBills.Wpf.Tests.Localization;

[Collection(UiCultureCollection.Name)]
public sealed class LocalizationServiceTests : IDisposable
{
    private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;
    private readonly CultureInfo? _originalDefaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
    private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;
    private readonly CultureInfo? _originalDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
    private readonly IUserPreferencesStore _preferences = Substitute.For<IUserPreferencesStore>();
    private readonly StrongReferenceMessenger _messenger = new();
    private readonly LocalizationService _service;

    public LocalizationServiceTests()
    {
        _preferences.LoadAsync(Arg.Any<CancellationToken>()).Returns(UserPreferences.Default);
        _service = new LocalizationService(_preferences, _messenger);
    }

    public void Dispose()
    {
        CultureInfo.CurrentUICulture = _originalUiCulture;
        CultureInfo.DefaultThreadCurrentUICulture = _originalDefaultUiCulture;
        Strings.Culture = null;
        HouseBills.Application.Resources.Messages.Culture = null;
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.DefaultThreadCurrentCulture = _originalDefaultCulture;
        LocalizedStrings.FormattingCulture = _originalCulture;
    }

    [Fact]
    public async Task InitializeAsync_SerbianSaved_AppliesSerbian()
    {
        _preferences.LoadAsync(Arg.Any<CancellationToken>()).Returns(new UserPreferences("sr-Latn-RS"));

        await _service.InitializeAsync(TestContext.Current.CancellationToken);

        _service.Current.ShouldBe(LocalizationService.Serbian);
        CultureInfo.DefaultThreadCurrentUICulture!.Name.ShouldBe("sr-Latn-RS");
        Strings.Page_Bills.ShouldBe("Računi");
        LocalizedStrings.Instance["Common_Save"].ShouldBe("Sačuvaj");
    }

    [Theory]
    [InlineData("sr-Cyrl-RS", "sr-Latn-RS")]
    [InlineData("sr-Latn-BA", "sr-Latn-RS")]
    [InlineData("en-US", "en")]
    [InlineData("de-DE", "en")]
    public async Task InitializeAsync_NothingSaved_FollowsWindowsDisplayLanguage(string windowsLanguage, string expected)
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(windowsLanguage);

        await _service.InitializeAsync(TestContext.Current.CancellationToken);

        _service.Current.CultureName.ShouldBe(expected);
    }

    [Fact]
    public async Task InitializeAsync_UnknownLanguageSaved_FallsBackToDefault()
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
        _preferences.LoadAsync(Arg.Any<CancellationToken>()).Returns(new UserPreferences("xx-Unknown"));

        await _service.InitializeAsync(TestContext.Current.CancellationToken);

        _service.Current.ShouldBe(LocalizationService.English);
    }

    [Fact]
    public async Task SetLanguageAsync_DifferentLanguage_AppliesNotifiesAndSaves()
    {
        LanguageChangedMessage? received = null;
        _messenger.Register<LanguageChangedMessage>(this, (_, m) => received = m);

        await _service.SetLanguageAsync(LocalizationService.Serbian, TestContext.Current.CancellationToken);

        CultureInfo.DefaultThreadCurrentUICulture!.Name.ShouldBe("sr-Latn-RS");
        Strings.Common_Save.ShouldBe("Sačuvaj");
        received.ShouldNotBeNull().Language.ShouldBe(LocalizationService.Serbian);
        await _preferences.Received(1).SaveAsync(new UserPreferences("sr-Latn-RS"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetLanguageAsync_SameLanguage_DoesNothing()
    {
        await _service.SetLanguageAsync(LocalizationService.English, TestContext.Current.CancellationToken);

        await _preferences.DidNotReceiveWithAnyArgs().SaveAsync(default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SetLanguageAsync_Serbian_UsesSerbianFormatsWithTwoDecimalAmounts()
    {
        await _service.SetLanguageAsync(LocalizationService.Serbian, TestContext.Current.CancellationToken);

        1234.56m.ToString("C", _service.FormattingCulture).ShouldBe("1.234,56 RSD");
        new DateOnly(2026, 9, 24).ToString("d", _service.FormattingCulture).ShouldBe("24.9.2026.");
        LocalizedStrings.FormattingCulture.ShouldBe(_service.FormattingCulture);
    }

    [Fact]
    public async Task SetLanguageAsync_BackToEnglish_RestoresWindowsFormats()
    {
        // The test runner uses en-US as the "Windows" formats (see xunit.runner.json).
        await _service.SetLanguageAsync(LocalizationService.Serbian, TestContext.Current.CancellationToken);

        await _service.SetLanguageAsync(LocalizationService.English, TestContext.Current.CancellationToken);

        1234.56m.ToString("C", _service.FormattingCulture).ShouldBe("$1,234.56");
    }
}