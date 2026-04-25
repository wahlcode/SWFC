namespace SWFC.Web.Pages.M300_Presentation.M306_Localization.Services;

public sealed class LocalizationValidationMessageCatalog
{
    private static readonly string[] ValidationMessageKeys =
    [
        "Validation.Required",
        "Validation.InvalidValue"
    ];

    private readonly LocalizationTextProvider _textProvider;

    public LocalizationValidationMessageCatalog(LocalizationTextProvider textProvider)
    {
        _textProvider = textProvider;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetValidationMessagesAsync(
        string cultureName,
        CancellationToken cancellationToken = default)
    {
        var messages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var key in ValidationMessageKeys)
        {
            messages[key] = await _textProvider.GetTextAsync(key, cultureName, cancellationToken);
        }

        return messages;
    }
}
