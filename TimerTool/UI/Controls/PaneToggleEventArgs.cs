using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TimerTool.UI.Controls
{

    public class PaneToggleEventArgs : EventArgs
    {
        public Visibility Visibility { get; }

        public PaneToggleEventArgs(Visibility visibility)
        {
            Visibility = visibility;
        }
    }
}
