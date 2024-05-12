using Terminal.Core.VirtualTerminal.Enums;

namespace Terminal.Core.XTermParser.SequenceType
{
    using Core.VirtualTerminal.Enums;

    public class CharacterSizeSequence : TerminalSequence
    {
        public ECharacterSize Size { get; set; }
        public override string ToString()
        {
            return "Character size - " + Size.ToString();
        }
    }
}
