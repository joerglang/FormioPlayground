using FormioPlaygroundWeb.Components;
using FormioPlaygroundWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// form.io Portal-Zugangsdaten aus User Secrets / Umgebungsvariablen
builder.Services.Configure<FormIoSettings>(builder.Configuration.GetSection("FormIo"));
builder.Services.AddHttpClient<FormIoService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
