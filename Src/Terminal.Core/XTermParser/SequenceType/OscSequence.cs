namespace Terminal.Core.XTermParser.SequenceType
{
    public class OscSequence : TerminalSequence
    {
        public override string ToString()
        {
            return "OSC - " + base.ToString();
        }
    }
}
