using SWFC.Application.M800_Security.M802_ApplicationSecurity;

namespace SWFC.Web.Pages.M300_Presentation.M306_Localization.Services;

public sealed class LocalizationState
{
    private readonly ICurrentUserService _currentUserService;
    private readonly LocalizationTextProvider _textProvider;

    public LocalizationState(
        ICurrentUserService currentUserService,
        LocalizationTextProvider textProvider)
    {
        _currentUserService = currentUserService;
        _textProvider = textProvider;
    }

    public string CultureName { get; private set; } = LocalizationTextProvider.DefaultCultureName;

    public event Action? Changed;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        CultureName = string.IsNullOrWhiteSpace(securityContext.PreferredCultureName)
            ? LocalizationTextProvider.DefaultCultureName
            : securityContext.PreferredCultureName.Trim();

        Changed?.Invoke();
    }

    public async Task SetCultureAsync(string cultureName, CancellationToken cancellationToken = default)
    {
        CultureName = string.IsNullOrWhiteSpace(cultureName)
            ? LocalizationTextProvider.DefaultCultureName
            : cultureName.Trim();

        await Task.CompletedTask;
        Changed?.Invoke();
    }

    public Task<string> TextAsync(string key, CancellationToken cancellationToken = default)
    {
        return _textProvider.GetTextAsync(key, CultureName, cancellationToken);
    }
}