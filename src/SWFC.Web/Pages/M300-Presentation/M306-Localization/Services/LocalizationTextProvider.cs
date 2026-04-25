using System.Text.Json;

namespace SWFC.Web.Pages.M300_Presentation.M306_Localization.Services;

public sealed class LocalizationTextProvider
{
    public const string DefaultCultureName = "en-US";

    private readonly IWebHostEnvironment _environment;

    private readonly Dictionary<string, IReadOnlyDictionary<string, string>> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    public LocalizationTextProvider(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> GetTextAsync(
        string key,
        string cultureName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        var texts = await GetTextsAsync(cultureName, cancellationToken);

        return texts.TryGetValue(key, out var text) && !string.IsNullOrWhiteSpace(text)
            ? text
            : key;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetTextsAsync(
        string cultureName,
        CancellationToken cancellationToken = default)
    {
        var normalizedCulture = NormalizeCultureName(cultureName);

        if (_cache.TryGetValue(normalizedCulture, out var cached))
        {
            return cached;
        }

        var baseTexts = await LoadTextsAsync(DefaultCultureName, cancellationToken);

        if (string.Equals(normalizedCulture, DefaultCultureName, StringComparison.OrdinalIgnoreCase))
        {
            _cache[normalizedCulture] = baseTexts;
            return baseTexts;
        }

        var cultureTexts = await LoadTextsAsync(normalizedCulture, cancellationToken);
        var mergedTexts = new Dictionary<string, string>(baseTexts, StringComparer.OrdinalIgnoreCase);

        foreach (var item in cultureTexts)
        {
            if (!string.IsNullOrWhiteSpace(item.Value))
            {
                mergedTexts[item.Key] = item.Value;
            }
        }

        _cache[normalizedCulture] = mergedTexts;
        return mergedTexts;
    }

    private async Task<IReadOnlyDictionary<string, string>> LoadTextsAsync(
        string cultureName,
        CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(
            _environment.ContentRootPath,
            "Pages",
            "M300-Presentation",
            "M306-Localization",
            "Resources",
            $"{cultureName}.json");

        if (!File.Exists(filePath))
        {
            return new Dictionary<string, string>();
        }

        await using var stream = File.OpenRead(filePath);

        var texts = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(
            stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            },
            cancellationToken);

        return texts ?? new Dictionary<string, string>();
    }

    private static string NormalizeCultureName(string? cultureName)
    {
        return string.IsNullOrWhiteSpace(cultureName)
            ? DefaultCultureName
            : cultureName.Trim();
    }
}