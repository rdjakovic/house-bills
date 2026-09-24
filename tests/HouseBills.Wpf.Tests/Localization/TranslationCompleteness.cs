using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;

namespace HouseBills.Wpf.Tests.Localization;

/// <summary>Compares the neutral (English) resources with a translation, without falling back to English.</summary>
internal static partial class TranslationCompleteness
{
    public static Dictionary<string, string> Entries(ResourceManager resources, CultureInfo culture)
    {
        var set = resources.GetResourceSet(culture, createIfNotExists: true, tryParents: false)
            ?? throw new InvalidOperationException($"No resources for {culture.Name}.");
        return set.Cast<DictionaryEntry>().ToDictionary(e => (string)e.Key, e => (string)e.Value!);
    }

    /// <summary>Returns a description of every problem: missing/extra/empty keys and mismatched placeholders.</summary>
    public static List<string> Problems(ResourceManager resources, string translationCulture)
    {
        var english = Entries(resources, CultureInfo.InvariantCulture);
        var translated = Entries(resources, CultureInfo.GetCultureInfo(translationCulture));
        var problems = new List<string>();
        problems.AddRange(english.Keys.Except(translated.Keys).Select(k => $"{k}: missing in {translationCulture}"));
        problems.AddRange(translated.Keys.Except(english.Keys).Select(k => $"{k}: only in {translationCulture}"));
        foreach (var (key, text) in translated.Where(e => english.ContainsKey(e.Key)))
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                problems.Add($"{key}: empty in {translationCulture}");
            }

            var expected = Placeholders(english[key]);
            var actual = Placeholders(text);
            if (!expected.SetEquals(actual))
            {
                problems.Add($"{key}: placeholders {{{string.Join(",", expected)}}} vs {{{string.Join(",", actual)}}}");
            }
        }

        return problems;
    }

    private static HashSet<string> Placeholders(string text) =>
        PlaceholderPattern().Matches(text).Select(m => m.Groups[1].Value).ToHashSet();

    [GeneratedRegex(@"\{(\d+)[^}]*\}")]
    private static partial Regex PlaceholderPattern();
}