using System;
using System.Collections.Generic;
using System.Text;

namespace Terminal.Core.VirtualTerminal
{
    public class TextEventArgs : EventArgs
    {
        public string Text { get; set; }
    }
}
