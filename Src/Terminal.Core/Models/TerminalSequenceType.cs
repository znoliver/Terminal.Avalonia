namespace Terminal.Core.Models
{
    public enum TerminalSequenceType
    {
        Character,
        CSI,            // Control Sequence Introducer
        OSC,            // Operating System Command
        DCS,            // Device Control String
        SS3,            // Signal Shift Select 3
        VT52mc,         // VT52 Move Cursor
        Compliance,     // Compliance
        CharacterSet,   // Character set
        Escape,
        CharacterSize,
        Unicode
    }
}