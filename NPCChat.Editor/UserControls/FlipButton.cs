using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace NPCChat.Editor.UserControls;

[ContentProperty(nameof(States))]
public class FlipButton : Button
{
    private ICommand? _subscribedCommand;

    static FlipButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlipButton),
            new FrameworkPropertyMetadata(typeof(FlipButton)));
    }

    public FlipButton()
    {
        States = new ObservableCollection<FlipButtonState>();
    }

    public ObservableCollection<FlipButtonState> States { get; }

    public static readonly DependencyProperty CurrentStateIndexProperty =
        DependencyProperty.Register(
            nameof(CurrentStateIndex),
            typeof(int),
            typeof(FlipButton),
            new PropertyMetadata(0, OnStateChanged));

    public int CurrentStateIndex
    {
        get => (int)GetValue(CurrentStateIndexProperty);
        set => SetValue(CurrentStateIndexProperty, value);
    }

    public FlipButtonState? State =>
        States.Count == 0 || CurrentStateIndex < 0 || CurrentStateIndex >= States.Count
            ? null
            : States[CurrentStateIndex];

    public static readonly DependencyProperty PrimaryCommandProperty =
        DependencyProperty.Register(
            nameof(PrimaryCommand),
            typeof(ICommand),
            typeof(FlipButton),
            new PropertyMetadata(null, OnCommandChanged));

    public ICommand? PrimaryCommand
    {
        get => (ICommand?)GetValue(PrimaryCommandProperty);
        set => SetValue(PrimaryCommandProperty, value);
    }

    public static readonly DependencyProperty SecondaryCommandProperty =
        DependencyProperty.Register(
            nameof(SecondaryCommand),
            typeof(ICommand),
            typeof(FlipButton),
            new PropertyMetadata(null, OnCommandChanged));

    public ICommand? SecondaryCommand
    {
        get => (ICommand?)GetValue(SecondaryCommandProperty);
        set => SetValue(SecondaryCommandProperty, value);
    }

    public static readonly DependencyProperty PrimaryCommandParameterProperty =
        DependencyProperty.Register(
            nameof(PrimaryCommandParameter),
            typeof(object),
            typeof(FlipButton),
            new PropertyMetadata(null));

    public object? PrimaryCommandParameter
    {
        get => GetValue(PrimaryCommandParameterProperty);
        set => SetValue(PrimaryCommandParameterProperty, value);
    }

    public static readonly DependencyProperty SecondaryCommandParameterProperty =
        DependencyProperty.Register(
            nameof(SecondaryCommandParameter),
            typeof(object),
            typeof(FlipButton),
            new PropertyMetadata(null));

    public object? SecondaryCommandParameter
    {
        get => GetValue(SecondaryCommandParameterProperty);
        set => SetValue(SecondaryCommandParameterProperty, value);
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var button = (FlipButton)d;
        button.RebindCommandSubscription();
        button.UpdateVisuals();
        button.UpdateCanExecute();
    }

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var button = (FlipButton)d;
        button.RebindCommandSubscription();
        button.UpdateCanExecute();
    }

    protected override void OnClick()
    {
        var state = State;
        if (state is null)
        {
            base.OnClick();
            return;
        }

        var command = GetCurrentCommand();
        var parameter = GetCurrentCommandParameter();
        System.Diagnostics.Debug.WriteLine($"State={state.Key}, Text={state.Text}, CommandIsNull={command is null}");
        if (command is not null)
        {
            if (!command.CanExecute(parameter))
                return;

            try
            {
                command.Execute(parameter);
            }
            catch
            {
                return;
            }
        }

        FlipToNextState();
        base.OnClick();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        RebindCommandSubscription();
        UpdateVisuals();
        UpdateCanExecute();
    }

    private ICommand? GetCurrentCommand()
    {
        return CurrentStateIndex switch
        {
            0 => PrimaryCommand,
            1 => SecondaryCommand,
            _ => null
        };
    }

    private object? GetCurrentCommandParameter()
    {
        return CurrentStateIndex switch
        {
            0 => PrimaryCommandParameter ?? this,
            1 => SecondaryCommandParameter ?? this,
            _ => this
        };
    }

    private void FlipToNextState()
    {
        if (States.Count == 0)
            return;

        CurrentStateIndex = (CurrentStateIndex + 1) % States.Count;
    }

    private void UpdateVisuals()
    {
        Content = State?.Text ?? string.Empty;
    }

    private void UpdateCanExecute()
    {
        var command = GetCurrentCommand();
        if (command is null)
        {
            IsEnabled = true;
            return;
        }

        IsEnabled = command.CanExecute(GetCurrentCommandParameter());
    }

    private void RebindCommandSubscription()
    {
        if (_subscribedCommand is not null)
        {
            _subscribedCommand.CanExecuteChanged -= CurrentCommand_CanExecuteChanged;
            _subscribedCommand = null;
        }

        var command = GetCurrentCommand();
        if (command is not null)
        {
            _subscribedCommand = command;
            _subscribedCommand.CanExecuteChanged += CurrentCommand_CanExecuteChanged;
        }
    }

    private void CurrentCommand_CanExecuteChanged(object? sender, EventArgs e)
    {
        UpdateCanExecute();
    }
}

public class FlipButtonState
{
    public string Key { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}