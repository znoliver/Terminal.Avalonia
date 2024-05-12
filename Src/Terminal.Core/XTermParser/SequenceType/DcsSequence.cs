namespace Terminal.Core.XTermParser.SequenceType
{
    public class DcsSequence : TerminalSequence
    {
        public override string ToString()
        {
            return "DCS - " + base.ToString();
        }
    }
}
