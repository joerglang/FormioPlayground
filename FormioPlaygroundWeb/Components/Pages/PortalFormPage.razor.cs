using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FormioPlaygroundWeb.Components.Pages;

public partial class PortalFormPage
{
    [Parameter] public string FormPath { get; set; } = "";
    private string? _title;
    private string? _schemaJson;
    private string? _submissionJson;
    private bool _notFound;
    private bool _jsInitialized;
    private DotNetObjectReference<PortalFormPage>? _dotNetRef;

	protected override async Task OnInitializedAsync()
    {
		_schemaJson = await FormIo.GetFormSchemaAsync(FormPath);
        if (_schemaJson is null)
        {
            _notFound = true;
            return;
        }

        using var doc = JsonDocument.Parse(_schemaJson);
        if (doc.RootElement.TryGetProperty("title", out var t))
            _title = t.GetString();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
		// Nicht firstRender prüfen: bei async OnInitializedAsync feuert firstRender=true
		// bevor der HTTP-Call fertig ist → _schemaJson wäre null. Deshalb Flag-Ansatz.
		if (_jsInitialized || _schemaJson is null) return;
        _jsInitialized = true;
        _dotNetRef = DotNetObjectReference.Create(this);
        var schema = JsonDocument.Parse(_schemaJson).RootElement;
        var url = FormIo.GetFormUrl(FormPath);
        var jwtToken = await FormIo.LoginAsync();
		await JS.InvokeVoidAsync("formioInterop.setToken", jwtToken);
		await JS.InvokeVoidAsync("formioInterop.createFormByUrl", "formio-container", url, _dotNetRef);
	}

	[JSInvokable]
    public void OnFormSubmitted(string json)
    {
        // Breakpoint hier
        Logger.LogInformation("Portal-Form submitted: {Json}", json);
        //_submissionJson = json;
        StateHasChanged();
    }

	[JSInvokable]
	public void OnSubmitDone(string json)
	{
		// Breakpoint hier
		Logger.LogInformation("Portal-Form Submit done: {Json}", json);

		var element = JsonSerializer.Deserialize<JsonElement>(json);
		_submissionJson = JsonSerializer.Serialize(
			element,
	        new JsonSerializerOptions
	        {
		        WriteIndented = true
	        });

		StateHasChanged();
	}

	public async ValueTask DisposeAsync()
    {
        try
        {
			await JS.InvokeVoidAsync("formioInterop.destroyForm");
		}
		catch (JSDisconnectedException)
        {
        }

        _dotNetRef?.Dispose();
    }
}