using Bunit;
using InputControl.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace InputControl.Tests;

public class InlineTextEditorTests : BunitContext
{
    public InlineTextEditorTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void ClickingDisplayedValueOpensInputWithCurrentValue()
    {
        var cut = Render<InlineTextEditor>(parameters => parameters
            .Add(component => component.Label, "Project name")
            .Add(component => component.Value, "Warehouse Expansion"));

        cut.Find("button.display-value").Click();

        var input = cut.Find("input");
        Assert.Equal("Warehouse Expansion", input.GetAttribute("value"));
        Assert.Equal("Project name", cut.Find("label").TextContent);
    }

    [Fact]
    public async Task CancelDiscardsDraftAndDoesNotInvokeCallback()
    {
        var saveCalls = 0;
        var cut = Render<InlineTextEditor>(parameters => parameters
            .Add(component => component.Value, "Original")
            .Add(component => component.ValueChanged,
                EventCallback.Factory.Create<string>(this, _ => saveCalls++)));

        cut.Find("button.display-value").Click();
        cut.Find("input").Input("Draft");
        await FindButton(cut, "Cancel").ClickAsync();

        Assert.Equal(0, saveCalls);
        Assert.Contains("Original", cut.Find("button.display-value").TextContent);
        Assert.DoesNotContain("Draft", cut.Find("button.display-value").TextContent);
    }

    [Fact]
    public async Task EscapeDiscardsDraftAndRestoresDisplayedValue()
    {
        var cut = Render<InlineTextEditor>(parameters => parameters
            .Add(component => component.Value, "Original"));

        cut.Find("button.display-value").Click();
        cut.Find("input").Input("Draft");
        await cut.Find("input").KeyDownAsync(new KeyboardEventArgs { Key = "Escape" });

        Assert.Empty(cut.FindAll("input"));
        Assert.Contains("Original", cut.Find("button.display-value").TextContent);
    }

    [Fact]
    public async Task SaveTrimsDraftAndInvokesParentCallback()
    {
        string? savedValue = null;
        var cut = Render<InlineTextEditor>(parameters => parameters
            .Add(component => component.Value, "Original")
            .Add(component => component.ValueChanged,
                EventCallback.Factory.Create<string>(this, value => savedValue = value)));

        cut.Find("button.display-value").Click();
        cut.Find("input").Input("  Updated name  ");
        await FindButton(cut, "Save").ClickAsync();

        Assert.Equal("Updated name", savedValue);
        Assert.Empty(cut.FindAll("input"));
    }

    [Fact]
    public async Task BlankDraftShowsValidationAndDoesNotSave()
    {
        var saveCalls = 0;
        var cut = Render<InlineTextEditor>(parameters => parameters
            .Add(component => component.Value, "Original")
            .Add(component => component.ValueChanged,
                EventCallback.Factory.Create<string>(this, _ => saveCalls++)));

        cut.Find("button.display-value").Click();
        cut.Find("input").Input("   ");
        await FindButton(cut, "Save").ClickAsync();

        Assert.Equal(0, saveCalls);
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));
        Assert.Contains("Enter between 1 and 120 characters.", cut.Markup);
    }

    [Fact]
    public async Task FailedSaveKeepsDraftForRetry()
    {
        var attempts = 0;
        var cut = Render<InlineTextEditor>(parameters => parameters
            .Add(component => component.Value, "Original")
            .Add(component => component.ValueChanged,
                EventCallback.Factory.Create<string>(this, _ =>
                {
                    attempts++;
                    if (attempts == 1)
                        throw new InvalidOperationException("Save failed.");
                })));

        cut.Find("button.display-value").Click();
        cut.Find("input").Input("Draft");
        await FindButton(cut, "Save").ClickAsync();

        Assert.Equal("Draft", cut.Find("input").GetAttribute("value"));
        Assert.Contains("Could not save. Your draft is still here; try again.", cut.Markup);

        await FindButton(cut, "Save").ClickAsync();

        Assert.Equal(2, attempts);
        Assert.Empty(cut.FindAll("input"));
    }

    [Fact]
    public async Task PendingSaveDisablesControlsUntilCallbackCompletes()
    {
        var callbackStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var finishSave = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var saveCalls = 0;
        var cut = Render<InlineTextEditor>(parameters => parameters
            .Add(component => component.Value, "Original")
            .Add(component => component.ValueChanged,
                EventCallback.Factory.Create<string>(this, async _ =>
                {
                    saveCalls++;
                    callbackStarted.SetResult();
                    await finishSave.Task;
                })));

        cut.Find("button.display-value").Click();
        cut.Find("input").Input("Updated");
        var saveTask = FindButton(cut, "Save").ClickAsync();
        await callbackStarted.Task;

        Assert.True(FindButton(cut, "Saving…").HasAttribute("disabled"));

        finishSave.SetResult();
        await saveTask;
        Assert.Equal(1, saveCalls);
    }

    private static AngleSharp.Dom.IElement FindButton(
        IRenderedComponent<InlineTextEditor> cut, string text) =>
        cut.FindAll("button").Single(button => button.TextContent.Trim() == text);
}
