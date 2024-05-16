using Avalonia.Interactivity;

namespace Terminal.Avalonia.Models;

public class TerminalInputEventArgs : RoutedEventArgs
{
    public string InputString { get; }

    public TerminalInputEventArgs(RoutedEvent routedEvent, string inputString)
    {
        this.RoutedEvent = routedEvent;
        this.InputString = inputString;
    }
}