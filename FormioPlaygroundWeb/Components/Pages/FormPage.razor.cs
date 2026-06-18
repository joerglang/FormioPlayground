using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FormioPlaygroundWeb.Components.Pages;

public partial class FormPage
{
    [Parameter] public string Name { get; set; } = "";
    private string? _title;
    private string? _schemaJson;
    private string? _submissionJson;
    private DotNetObjectReference<FormPage>? _dotNetRef;
	private bool _formCreated;

	protected override async Task OnInitializedAsync()
    {
        var path = Path.Combine(Env.WebRootPath, "forms", $"{Name}.json");
        _schemaJson = await File.ReadAllTextAsync(path);
        using var doc = JsonDocument.Parse(_schemaJson);
        if (doc.RootElement.TryGetProperty("title", out var t))
            _title = t.GetString();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        _dotNetRef = DotNetObjectReference.Create(this);
        var schema = JsonDocument.Parse(_schemaJson!).RootElement;
        await JS.InvokeVoidAsync("formioInterop.createForm", "formio-container", schema, _dotNetRef);

		_formCreated = true;
	}

    [JSInvokable]
    public void OnFormSubmitted(string json)
    {
        // Breakpoint hier
        Logger.LogInformation("Form submitted: {Json}", json);
        _submissionJson = json;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        if (_formCreated)
        {
            await JS.InvokeVoidAsync("formioInterop.destroyForm");
        }

        _dotNetRef?.Dispose();
    }
}