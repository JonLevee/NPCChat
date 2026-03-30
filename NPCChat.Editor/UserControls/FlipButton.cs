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

    public FlipButtonState State => States[CurrentStateIndex];

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var btn = (FlipButton)d;
        btn.UpdateVisuals();
    }

    // Logic to run the handler and flip to the next state
    protected override void OnClick()
    {
        try
        {
            base.OnClick();
        }
        catch (Exception)
        {
            return; // Don't flip state if handler throws an exception
        }

        CurrentStateIndex = (CurrentStateIndex + 1) % States.Count;
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
    public string Key { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
