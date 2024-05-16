using System;
using System.Collections.Generic;
using Avalonia.Input;

namespace Terminal.Avalonia;

public static class KeyMaps
{
    private static readonly Dictionary<Key, ConsoleKey> KeyMap = new()
    {
        { Key.A, ConsoleKey.A },
        { Key.B, ConsoleKey.B },
        { Key.C, ConsoleKey.C },
        { Key.D, ConsoleKey.D },
        { Key.E, ConsoleKey.E },
        { Key.F, ConsoleKey.F },
        { Key.G, ConsoleKey.G },
        { Key.H, ConsoleKey.H },
        { Key.I, ConsoleKey.I },
        { Key.J, ConsoleKey.J },
        { Key.K, ConsoleKey.K },
        { Key.L, ConsoleKey.L },
        { Key.M, ConsoleKey.M },
        { Key.N, ConsoleKey.N },
        { Key.O, ConsoleKey.O },
        { Key.P, ConsoleKey.P },
        { Key.Q, ConsoleKey.Q },
        { Key.R, ConsoleKey.R },
        { Key.S, ConsoleKey.S },
        { Key.T, ConsoleKey.T },
        { Key.U, ConsoleKey.U },
        { Key.V, ConsoleKey.V },
        { Key.W, ConsoleKey.W },
        { Key.X, ConsoleKey.X },
        { Key.Y, ConsoleKey.Y },
        { Key.Z, ConsoleKey.Z },
        { Key.Space, ConsoleKey.Spacebar },
        { Key.Back, ConsoleKey.Backspace },
        { Key.Escape, ConsoleKey.Escape },
        { Key.Tab, ConsoleKey.Tab },
    };

    private static readonly Dictionary<KeyModifiers, ConsoleModifiers> ModifierMap = new()
    {
        { KeyModifiers.Shift, ConsoleModifiers.Shift },
        { KeyModifiers.Control, ConsoleModifiers.Control },
        { KeyModifiers.Alt, ConsoleModifiers.Alt },
        { KeyModifiers.None, ConsoleModifiers.None },
    };

    public static ConsoleKey GetKey(Key keyGesture) => KeyMap.GetValueOrDefault(keyGesture, ConsoleKey.None);
    
    public static ConsoleModifiers GetModifiers(KeyModifiers keyModifiers) => ModifierMap.GetValueOrDefault(keyModifiers, ConsoleModifiers.None);
}