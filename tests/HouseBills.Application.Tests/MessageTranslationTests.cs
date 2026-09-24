using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

using HouseBills.Application.Categories;
using HouseBills.Application.Common;
using HouseBills.Application.Persistence;
using HouseBills.Application.Resources;

using NSubstitute;

namespace HouseBills.Application.Tests;

public sealed partial class MessageTranslationTests
{
    [Fact]
    public void Messages_SerbianTranslation_IsCompleteWithMatchingPlaceholders()
    {
        var english = Entries(CultureInfo.InvariantCulture);
        var serbian = Entries(CultureInfo.GetCultureInfo("sr-Latn"));

        serbian.Keys.ShouldBe(english.Keys, ignoreOrder: true);
        foreach (var (key, text) in english)
        {
            serbian[key].ShouldNotBeNullOrWhiteSpace(key);
            Placeholders(serbian[key]).ShouldBe(Placeholders(text), ignoreOrder: true, customMessage: key);
        }
    }

    [Fact]
    public async Task SaveAsync_SerbianUiAndBlankName_ReturnsSerbianMessage()
    {
        // CurrentUICulture flows with this test's async context only; restore it anyway to be explicit.
        var original = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("sr-Latn-RS");
        try
        {
            var service = new CategoryService(Substitute.For<ICategoryRepository>());

            var result = await service.SaveAsync(new SaveCategoryRequest(null, " ", null), TestContext.Current.CancellationToken);

            result.Error!.Kind.ShouldBe(ErrorKind.Validation);
            result.Error.Message.ShouldBe("Polje „Naziv“ je obavezno.");
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }

    private static Dictionary<string, string> Entries(CultureInfo culture) =>
        Messages.ResourceManager.GetResourceSet(culture, createIfNotExists: true, tryParents: false)!
            .Cast<DictionaryEntry>()
            .ToDictionary(e => (string)e.Key, e => (string)e.Value!);

    private static List<string> Placeholders(string text) =>
        PlaceholderPattern().Matches(text).Select(m => m.Groups[1].Value).Distinct().ToList();

    [GeneratedRegex(@"\{(\d+)[^}]*\}")]
    private static partial Regex PlaceholderPattern();
}