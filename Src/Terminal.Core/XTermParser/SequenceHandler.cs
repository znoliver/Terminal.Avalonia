using System;
using Terminal.Core.Models;
using Terminal.Core.VirtualTerminal;
using Terminal.Core.XTermParser.SequenceType;

namespace Terminal.Core.XTermParser
{
    public class SequenceHandler
    {
        public string Description { get; set; }
        public TerminalSequenceType SequenceType { get; set; }
        public int ExactParameterCount { get; set; } = -1;
        public int ExactParameterCountOrDefault { get; set; } = -1;
        public int DefaultParamValue { get; set; } = 1;
        public int MinimumParameterCount { get; set; } = 0;
        public bool Query { get; set; } = false;
        public bool Send { get; set; } = false;
        public bool Equal { get; set; } = false;
        public bool Bang { get; set; } = false;
        public int[] Param0 { get; set; } = new int[] { };
        public int[] ValidParams { get; set; } = new int[] { };
        public string CsiCommand { get; set; }
        public string OscText { get; set; }
        public Action<TerminalSequence, IVirtualTerminalController> Handler { get; set; }
        public Vt52Mode Vt52 { get; set; } = Vt52Mode.Irrelevent;
    }
}
