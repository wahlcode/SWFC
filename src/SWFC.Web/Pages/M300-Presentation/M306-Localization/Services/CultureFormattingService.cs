using System.Globalization;

namespace SWFC.Web.Pages.M300_Presentation.M306_Localization.Services;

public sealed class CultureFormattingService
{
    public string FormatDate(DateTime value, string cultureName)
    {
        return value.ToString("d", GetCulture(cultureName));
    }

    public string FormatDateTime(DateTime value, string cultureName)
    {
        return value.ToString("g", GetCulture(cultureName));
    }

    public string FormatNumber(decimal value, string cultureName)
    {
        return value.ToString("N2", GetCulture(cultureName));
    }

    public string FormatCurrency(decimal value, string cultureName)
    {
        return value.ToString("C", GetCulture(cultureName));
    }

    public CultureInfo GetCulture(string? cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            return CultureInfo.GetCultureInfo("de-DE");
        }

        try
        {
            return CultureInfo.GetCultureInfo(cultureName.Trim());
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.GetCultureInfo("de-DE");
        }
    }
}