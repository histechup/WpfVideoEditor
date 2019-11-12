using System;
using WpfVideoEditor.Models;

namespace WpfVideoEditor.Controls
{
    public class ClipEventArgs : EventArgs
    {
        public Clip Clip { get; }
        public ClipEventArgs(Clip clip) { Clip = clip; }
    }
}
