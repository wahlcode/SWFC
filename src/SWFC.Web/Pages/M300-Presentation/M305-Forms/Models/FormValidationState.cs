namespace SWFC.Web.Pages.M300_Presentation.M305_Forms.Models;

public sealed class FormValidationState
{
    private readonly Dictionary<string, string> _errors = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, string> Errors => _errors;
    public bool HasErrors => _errors.Count > 0;

    public void Add(string fieldName, string message)
    {
        if (string.IsNullOrWhiteSpace(fieldName) || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _errors[fieldName.Trim()] = message.Trim();
    }

    public string? GetError(string fieldName)
    {
        return _errors.TryGetValue(fieldName, out var message)
            ? message
            : null;
    }
}
