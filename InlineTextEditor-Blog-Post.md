# Build an Inline Text Editor in Blazor WebAssembly

Sometimes a form is too much UI for a single value. A project name, purchase-request description, or dashboard label is easier to work with when it can be edited right where it appears.

In this example, clicking a displayed value opens an editor. The user can change a draft, then explicitly save or cancel. Pressing Escape cancels, and focus moves to the input when editing starts and back to the display button when editing ends.

## Keep the component reusable

The editor accepts a label, current value, maximum length, and an `EventCallback<string>` for saving:

```razor
<InlineTextEditor Label="Project name"
                  Value="@_projectName"
                  ValueChanged="SaveProjectName" />
```

The component keeps the draft separate from `Value`. Opening the editor copies the current value into that draft; typing does not change the parent’s value:

```csharp
private void BeginEdit()
{
    _draft = Value;
    _error = null;
    _editing = true;
    _focusPending = true;
}
```

This makes Cancel straightforward: close the editor without sending the draft to the parent. Escape uses the same cancel path.

## Save only when asked

When Save is clicked, the component trims surrounding whitespace and rejects an empty value or a value longer than `MaxLength`. While the parent callback is running, `_saving` disables the controls and prevents a second save from starting:

```csharp
if (_saving) return;

var candidate = _draft.Trim();
if (candidate.Length == 0 || candidate.Length > MaxLength)
{
    _error = $"Enter between 1 and {MaxLength} characters.";
    _focusPending = true;
    return;
}

_saving = true;
try
{
    await ValueChanged.InvokeAsync(candidate);
    _editing = false;
}
catch (Exception)
{
    _error = "Could not save. Your draft is still here; try again.";
}
finally
{
    _saving = false;
    _focusPending = true;
}
```

The parent owns persistence and updates the displayed value only after its save succeeds:

```csharp
private string _projectName = "Warehouse Expansion";

private async Task SaveProjectName(string candidate)
{
    await projectRepository.SaveNameAsync(candidate);
    _projectName = candidate;
}
```

If the repository call fails, the exception returns to the component, which keeps the editor open and preserves the draft for retry. In a production application, log the exception in the persistence layer and show an appropriate user-facing message; avoid exposing sensitive exception details in the UI.

## Restore focus after rendering

The input only exists while the component is editing, so focus it after Blazor has rendered that state. The same approach returns focus to the display button after saving, canceling, or validation:

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (!_focusPending) return;
    _focusPending = false;

    if (_editing)
        await _input.FocusAsync();
    else
        await _trigger.FocusAsync();
}
```

There is no custom JavaScript to maintain. `ElementReference.FocusAsync` provides the focus operation through Blazor.

## Make keyboard and error states clear

Use native `<button>` elements for the display action, Save, and Cancel. They are keyboard-operable with Enter and Space and participate in normal Tab navigation. The component handles Escape while editing. Associate the visible label with the input, set `aria-invalid` when validation or saving fails, and expose the message through a live status region:

```razor
<input id="@_id"
       @bind="_draft" @bind:event="oninput"
       maxlength="@MaxLength"
       disabled="@_saving"
       aria-invalid="@(_error is not null ? "true" : "false")"
       aria-describedby="@($"{_id}-message")" />

<span id="@($"{_id}-message")"
      role="status" aria-live="polite">
    @_error
</span>
```

Keeping an explicit Save button avoids surprising commits when the user tabs away. Place the component outside an enclosing form, as its actions are intended to update one value rather than submit that form.

## Try it with a few types of values

The sample home page uses the same component for a project name, a longer purchase-request description, and a dashboard label. Each instance can set its own maximum length. The test options simulate a slow save or a save failure, making it easy to check that controls disable during persistence and that a failed save leaves the draft available.

The result is a small, reusable editing interaction that keeps persistence in the parent, makes changes explicit, and remains usable with a keyboard.
