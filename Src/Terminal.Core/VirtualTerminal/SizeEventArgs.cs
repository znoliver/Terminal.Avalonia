using System;
using System.Collections.Generic;
using System.Text;

namespace Terminal.Core.VirtualTerminal
{
    public class SizeEventArgs : EventArgs
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
