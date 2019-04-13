using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastColoredTextBoxNS;

namespace Script.ColoredStyle
{
    public class LogStyle
    {
        TextStyle infoStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);
        TextStyle warningStyle = new TextStyle(Brushes.BurlyWood, null, FontStyle.Regular);
        TextStyle errorStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);

        public LogStyle(FastColoredTextBox fctb)
        {
            
        }
    }
}
