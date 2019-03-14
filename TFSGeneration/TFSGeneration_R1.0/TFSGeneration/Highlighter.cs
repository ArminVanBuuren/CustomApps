using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace TFSAssist
{
    public class Highlighter
    {
        public static void Traces(Paragraph par, string message)
        {
            int startSmb = 0;
            StringBuilder highlightBuilder = new StringBuilder();
            StringBuilder notHighlighted = new StringBuilder();
            foreach (char ch in message)
            {
                if (ch == '[')
                {
                    startSmb++;
                    highlightBuilder.Append(ch);
                    AppentNotHghText(par, notHighlighted);
                    continue;
                }

                if (ch == ']')
                {
                    if (startSmb > 0)
                        startSmb--;
                    highlightBuilder.Append(ch);
                    if (startSmb == 0)
                        AppentHghText(par, highlightBuilder);
                    continue;
                }

                if (startSmb > 0)
                {
                    highlightBuilder.Append(ch);
                    continue;
                }

                notHighlighted.Append(ch);
            }

            AppentNotHghText(par, notHighlighted);
            AppentHghText(par, highlightBuilder);
        }

        static void AppentNotHghText(Paragraph par, StringBuilder builder)
        {
            if (builder.Length > 0)
            {
                par.Inlines.Add(builder.ToString());
                builder.Clear();
            }
        }

        static void AppentHghText(Paragraph par, StringBuilder builder)
        {
            if (builder.Length > 0)
            {
                Run r = new Run(builder.ToString())
                {
                    Foreground = Brushes.HotPink,
                    Background = Brushes.Black
                };
                par.Inlines.Add(r);
                builder.Clear();
            }
        }
    }
}
