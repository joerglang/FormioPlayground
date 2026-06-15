namespace FormioPlaygroundWeb.Services;

/// <summary>
/// Konfiguration für das form.io Enterprise-Portal.
/// Werte kommen aus User Secrets (Entwicklung) oder Umgebungsvariablen (Prod).
/// Niemals in appsettings.json oder der Solution speichern.
/// </summary>
public sealed class FormIoSettings
{
    /// <summary>Projekt-Endpunkt, z.B. https://sxpymcwzalmfzyx.form.io</summary>
    public string? BaseUrl { get; init; }

    /// <summary>
    /// Optionaler Pfad zwischen BaseUrl und /form (meist leer wenn BaseUrl = Projekt-URL).
    /// </summary>
    public string? ProjectPath { get; init; }

    /// <summary>
    /// Login-URL wenn die Auth an einem anderen Server als BaseUrl stattfindet.
    /// Beispiel: https://portal.form.io/user/login
    /// Leer = Login-Versuch an {BaseUrl}/user/login.
    /// </summary>
    public string? LoginUrl { get; init; }

    /// <summary>Projekt-API-Key (x-token). Hat Vorrang vor Email/Passwort.</summary>
    public string? ApiKey { get; init; }

    /// <summary>E-Mail für Login via Portal oder Projekt.</summary>
    public string? Email { get; init; }

    /// <summary>Passwort für Login.</summary>
    public string? Password { get; init; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(BaseUrl) &&
        (!string.IsNullOrWhiteSpace(ApiKey) ||
         (!string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password)));
}
