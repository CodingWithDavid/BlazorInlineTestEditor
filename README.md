# InlineTextEditor

A .NET 10 Blazor WebAssembly sample featuring an accessible inline text editor in a responsive, modern blue landing page. Click a displayed value to edit it, then explicitly save or cancel. Press **Escape** to discard the draft and return focus to the displayed value.

## Requirements

- .NET 10 SDK

## Run the app

From this directory, run:

```powershell
dotnet run
```

Open the local URL printed by `dotnet run` in your browser.

## Try the demo

The home page presents three examples: a project name, purchase-request description, and dashboard label. Use the test options to simulate a slow save or a save failure. Values are kept in memory and reset when the page reloads; no backend persistence is configured.

The editor keeps changes in a draft until Save is clicked, rejects blank and overlength values, prevents duplicate saves, and retains the draft if the parent save callback fails. The parent owns persistence and supplies the updated value after a successful save. The component uses native buttons, keyboard interaction, accessible error announcements, and Blazor's focus API without custom JavaScript.

## What's included

- `Components/InlineTextEditor.razor` — reusable inline editor component.
- `Components/InlineTextEditor.razor.css` — component-scoped editor styles.
- `Pages/Home.razor` — landing page content and interactive examples.
- `Pages/Home.razor.css` — responsive page layout, hero, dashboard preview, demo cards, and feature sections.
- `Layout/MainLayout.razor` and `Layout/MainLayout.razor.css` — sticky header and responsive navigation.
- `wwwroot/css/app.css` — global typography and base styles.
- `tests/InputControl.Tests/` — bUnit tests for editor interactions and save behavior.
- `InlineTextEditor-Blog-Post.md` — a blog post explaining the implementation.

## Run tests

```powershell
dotnet test tests\InputControl.Tests\InputControl.Tests.csproj
```
