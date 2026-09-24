using System.Globalization;
using System.Text.RegularExpressions;

using HouseBills.Application.Bills;
using HouseBills.Domain;
using HouseBills.Presentation.Resources;
using HouseBills.Wpf.Converters;

namespace HouseBills.Wpf.Tests.Localization;

public sealed partial class UiTextTranslationTests
{
    [Fact]
    public void Strings_SerbianTranslation_IsCompleteWithMatchingPlaceholders()
    {
        TranslationCompleteness.Problems(Strings.ResourceManager, "sr-Latn").ShouldBeEmpty();
    }

    [Fact]
    public void XamlViews_EveryTrKey_ExistsInStrings()
    {
        var english = TranslationCompleteness.Entries(Strings.ResourceManager, CultureInfo.InvariantCulture);
        var viewsFolder = Path.Combine(RepositoryRoot(), "src", "HouseBills.Wpf", "Views");
        var usedKeys = Directory.GetFiles(viewsFolder, "*.xaml")
            .SelectMany(file => TrKeyPattern().Matches(File.ReadAllText(file)).Select(m => (File: Path.GetFileName(file), Key: m.Groups[1].Value)))
            .ToList();

        usedKeys.ShouldNotBeEmpty();
        usedKeys.Where(u => !english.ContainsKey(u.Key)).Select(u => $"{u.File}: {u.Key}").ShouldBeEmpty();
    }

    [Fact]
    public void Enums_EveryDisplayedValue_HasTranslatedText()
    {
        var english = TranslationCompleteness.Entries(Strings.ResourceManager, CultureInfo.InvariantCulture);
        var values = Enum.GetValues<BillStatus>().Cast<Enum>()
            .Concat(Enum.GetValues<BillStatusFilter>().Cast<Enum>())
            .Concat(Enum.GetValues<BillFrequency>().Cast<Enum>());

        values.Select(EnumToLocalizedTextConverter.KeyFor).Where(key => !english.ContainsKey(key)).ShouldBeEmpty();
    }

    private static string RepositoryRoot([System.Runtime.CompilerServices.CallerFilePath] string thisFile = "")
    {
        var directory = new DirectoryInfo(Path.GetDirectoryName(thisFile)!);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "HouseBills.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }

    [GeneratedRegex(@"\{loc:Tr\s+(\w+)\}")]
    private static partial Regex TrKeyPattern();
}