# form.io Playground (Blazor)

## Zweck
Prototyp zur Evaluation des form.io-Renderers in einem .NET/Blazor-Host. Wegwerf-Code, kein
Produktionsanspruch. Ablauf: Dashboard listet Formulare -> Klick rendert die Form via form.io ->
Submit schickt das Submission-JSON zurück ins .NET-Backend.

## Architektur (vorläufig, NICHT final)
- Renderer-only: form.io liefert JSON-Schema + clientseitiges Rendering. Schema ist Source of Truth.
- KEINE parallele Validierungsschicht im App-Code. Validierung kommt aus dem form.io-Schema.
- Submission wird per JS-Interop an eine [JSInvokable] C#-Methode zurueckgegeben (das "Backend").
- Forms-Quelle Phase 1: lokale Schema-Dateien unter wwwroot/forms/ + Index wwwroot/forms/index.json.
  Phase 2 (spaeter, NICHT jetzt): Formulare aus dem form.io-Enterprise-API auslesen.

## Tech-Stack
- Blazor Web App, Interactive Server. Aktuelles .NET-SDK.
- form.io-Renderer via CDN-Script (KEIN npm, KEIN JS-Bundler).
- Bootstrap- und Bootstrap-Icons-CSS via CDN (vom Renderer erwartet).
- Keine Datenbank, kein Auth, keine zusaetzliche UI-Library (kein Telerik) in diesem Prototyp.

## Wichtiger Stolperstein
- form.io kontrolliert seinen DOM-Bereich selbst. In ein <div> mounten, das Blazor NICHT
  re-rendert. Initialisierung in OnAfterRenderAsync. DotNetObjectReference am Ende disposen.

## Scope-Grenzen
- Erfinde KEINE eigene Validierung, KEIN State-Management ueber das Noetigste hinaus.
- Halte alles minimal und in wenigen Dateien. Kein Over-Engineering.
- Pruefe die aktuelle CDN-URL / den Paketstand des form.io-Renderers und kommentiere die Version.

## Arbeitsweise
- Kleine, reviewbare Diffs. Nach Aenderungen bauen.
- Geaenderte Dateien auflisten, offene manuelle Schritte benennen.

## Befehle
- Start:  dotnet run   (oder Run in Rider)
- Build:  dotnet build