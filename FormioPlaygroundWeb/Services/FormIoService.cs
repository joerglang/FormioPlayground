using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace FormioPlaygroundWeb.Services;

public sealed class FormIoService(
    HttpClient http,
    IOptions<FormIoSettings> opts,
    ILogger<FormIoService> logger)
{
    private readonly FormIoSettings _cfg = opts.Value;

    public async Task<IReadOnlyList<PortalForm>> GetFormsAsync()
    {
        if (!_cfg.IsConfigured)
        {
            logger.LogInformation("form.io: keine Zugangsdaten konfiguriert – Portal-Formulare übersprungen.");
            return [];
        }

        var auth = await GetAuthHeaderAsync();
        if (auth is null) return [];

        var projectSegment = string.IsNullOrWhiteSpace(_cfg.ProjectPath)
            ? ""
            : $"/{_cfg.ProjectPath!.Trim('/')}";
        var url = $"{_cfg.BaseUrl!.TrimEnd('/')}{projectSegment}/form?type=form&select=title,name,path,type&limit=250";

        logger.LogDebug("form.io: GET {Url}", url);

        var resp = await SendAsync(url, auth.Value);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            logger.LogWarning("form.io: {Status} auf {Url} – Body: {Body}", (int)resp.StatusCode, url, body);
            return [];
        }

        return await resp.Content.ReadFromJsonAsync<List<PortalForm>>() ?? [];
    }

    /// <summary>Lädt das vollständige Schema eines Formulars vom Portal.</summary>
    public async Task<string?> GetFormSchemaAsync(string formPath)
    {
        if (!_cfg.IsConfigured)
        {
            logger.LogInformation("form.io: keine Zugangsdaten konfiguriert.");
            return null;
        }

        var auth = await GetAuthHeaderAsync();
        if (auth is null) return null;

        var url = $"{_cfg.BaseUrl!.TrimEnd('/')}/{formPath.TrimStart('/')}";
        logger.LogDebug("form.io: GET Schema {Url}", url);

        var resp = await SendAsync(url, auth.Value);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            logger.LogWarning("form.io GetSchema: {Status} auf {Url} – {Body}", (int)resp.StatusCode, url, body);
            return null;
        }

        return await resp.Content.ReadAsStringAsync();
    }

    private async Task<(string Name, string Value)?> GetAuthHeaderAsync()
    {
        if (!string.IsNullOrWhiteSpace(_cfg.ApiKey))
            return ("x-token", _cfg.ApiKey);

        var jwt = await LoginAsync();
        if (jwt is null)
        {
            logger.LogWarning("form.io: Authentifizierung fehlgeschlagen.");
            return null;
        }
        return ("x-jwt-token", jwt);
    }

    private async Task<HttpResponseMessage> SendAsync(string url, (string Name, string Value) auth)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.TryAddWithoutValidation(auth.Name, auth.Value);
        return await http.SendAsync(req);
    }

    private async Task<string?> LoginAsync()
    {
        var loginUrl = !string.IsNullOrWhiteSpace(_cfg.LoginUrl)
            ? _cfg.LoginUrl
            : $"{_cfg.BaseUrl!.TrimEnd('/')}/user/login";

        logger.LogDebug("form.io: POST {Url}", loginUrl);

        var resp = await http.PostAsJsonAsync(
            loginUrl,
            new { data = new { email = _cfg.Email, password = _cfg.Password } });

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            logger.LogWarning("form.io Login fehlgeschlagen: {Status} auf {Url} – {Body}",
                (int)resp.StatusCode, loginUrl, body);
            return null;
        }

        resp.Headers.TryGetValues("x-jwt-token", out var values);
        return values?.FirstOrDefault();
    }
}

public sealed record PortalForm(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("name")]  string Name,
    [property: JsonPropertyName("path")]  string Path
);
