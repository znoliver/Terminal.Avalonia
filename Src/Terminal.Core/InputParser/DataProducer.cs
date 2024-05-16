using System;

namespace Terminal.Core.InputParser
{
    public static class DataProducer
    {
        public static char Produce(ConsoleKey key)
        {
            char escapeCharacter;
            switch (key)
            {
                case ConsoleKey.Backspace:
                    escapeCharacter = '\b';
                    break;
                case ConsoleKey.Tab:
                    escapeCharacter = '\t';
                    break;
                case ConsoleKey.Enter:
                    escapeCharacter = '\n';
                    break;
                case ConsoleKey.Escape:
                    escapeCharacter = '\u001B';
                    break;
                case ConsoleKey.Spacebar:
                    escapeCharacter = ' ';
                    break;
                default:
                    escapeCharacter = key.ToString()[0];
                    break;
            }
            return escapeCharacter;
        }
        
        public static char Produce(ConsoleKey key, ConsoleModifiers modifiers)
        {
            char escapeCharacter;
            switch (modifiers)
            {
                case ConsoleModifiers.Alt:
                    escapeCharacter = Produce(key);
                    break;
                case ConsoleModifiers.Control:
                    escapeCharacter = CtrlProduce(key);
                    break;
                case ConsoleModifiers.Shift:
                    escapeCharacter = ShiftProduce(key);
                    break;
#if NET8_0_OR_GREATER
                case ConsoleModifiers.None:
                    escapeCharacter = Produce(key);
                    break;
#endif
                default:
                    throw new ArgumentOutOfRangeException(nameof(modifiers), modifiers, null);
            }
            return escapeCharacter;
        }

        private static char ShiftProduce(ConsoleKey key)
        {
            char escapeCharacter;
            switch (key)
            {
                case ConsoleKey.Backspace:
                    escapeCharacter = '\b';
                    break;
                case ConsoleKey.Tab:
                    escapeCharacter = '\t';
                    break;
                case ConsoleKey.Enter:
                    escapeCharacter = '\n';
                    break;
                case ConsoleKey.Escape:
                    escapeCharacter = '\u001B';
                    break;
                case ConsoleKey.Spacebar:
                    escapeCharacter = ' ';
                    break;
                default:
                    escapeCharacter = key.ToString()[0];
                    break;
            }
            return escapeCharacter;
        }
        
        private static char CtrlProduce(ConsoleKey key)
        {
            char escapeCharacter;
            switch (key)
            {
                case ConsoleKey.Backspace:
                    escapeCharacter = '\b';
                    break;
                case ConsoleKey.Tab:
                    escapeCharacter = '\t';
                    break;
                case ConsoleKey.Enter:
                    escapeCharacter = '\n';
                    break;
                case ConsoleKey.Escape:
                    escapeCharacter = '\u001B';
                    break;
                case ConsoleKey.Spacebar:
                    escapeCharacter = ' ';
                    break;
                default:
                    escapeCharacter = key.ToString()[0];
                    break;
            }
            return escapeCharacter;
        }
    }
}