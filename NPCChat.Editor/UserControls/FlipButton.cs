using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace NPCChat.Editor.UserControls;

[ContentProperty(nameof(States))] // Allows nesting states directly in XAML
public class FlipButton : Button
{
    static FlipButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FlipButton),
            new FrameworkPropertyMetadata(typeof(FlipButton)));
    }

    public FlipButton()
    {
        States = new List<FlipButtonState>();
    }

    // Stores the list of states
    public List<FlipButtonState> States { get; set; }

    // Tracks which index is currently active
    public static readonly DependencyProperty CurrentStateIndexProperty =
        DependencyProperty.Register("CurrentStateIndex", typeof(int), typeof(FlipButton), new PropertyMetadata(0, OnStateChanged));

    public int CurrentStateIndex
    {
        get => (int)GetValue(CurrentStateIndexProperty);
        set => SetValue(CurrentStateIndexProperty, value);
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var btn = (FlipButton)d;
        btn.UpdateVisuals();
    }

    // Logic to run the handler and flip to the next state
    protected override void OnClick()
    {
        base.OnClick();

        if (States != null && States.Count > 0)
        {
            // 1. Raise the event for the CURRENT state
            States[CurrentStateIndex].RaiseSelected(this);

            // 2. Increment index (looping back to 0)
            CurrentStateIndex = (CurrentStateIndex + 1) % States.Count;
        }
    }

    private void UpdateVisuals()
    {
        if (States != null && CurrentStateIndex < States.Count)
        {
            this.Content = States[CurrentStateIndex].Text;
        }
    }

    // Ensure the first state's text shows up immediately
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateVisuals();
    }
}

public class FlipButtonState
{
    public string Text { get; set; } = string.Empty;

    // This allows the XAML attribute syntax: Selected="MyMethodName"
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public event EventHandler Selected;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    // Internal helper to raise the event
    internal void RaiseSelected(object sender)
    {
        Selected?.Invoke(sender, EventArgs.Empty);
    }
}
