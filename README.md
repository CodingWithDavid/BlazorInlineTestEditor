# InlineTextEditor

A .NET 10 Blazor WebAssembly sample featuring an accessible inline text editor. Click a displayed value to edit it, then explicitly save or cancel. Press **Escape** to discard the draft and return focus to the displayed value.

## Requirements

- .NET 10 SDK

## Run the app

From this directory, run:

```powershell
dotnet run
```

Open the local URL printed by `dotnet run` in your browser.

## What's included

- `Components/InlineTextEditor.razor` — reusable editor component.
- `Components/InlineTextEditor.razor.css` — component-scoped styles.
- `Pages/Home.razor` — examples for a project name, purchase-request description, and dashboard label.
- `InlineTextEditor-Blog-Post.md` — a blog post explaining the implementation.

The editor keeps changes in a draft until Save is clicked, rejects blank and overlength values, prevents duplicate saves, and retains the draft if the parent save callback fails. The parent owns persistence and supplies the updated value after a successful save.

The home page includes options to simulate a slow save or a save failure. These are for demonstrating the editor's loading and error states; the sample does not persist changes to a backend, so edits are lost when the page is reloaded.
