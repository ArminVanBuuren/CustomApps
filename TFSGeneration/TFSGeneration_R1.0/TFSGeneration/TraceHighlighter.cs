using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Utils;

namespace TFSAssist
{
    [Serializable]
    public enum TraceBitType
    {
        DateTrace = 0,
        Text = 1,
        Higlight = 2
    }

    [Serializable]
    public class TraceBit
    {
        [NonSerialized]
        private Run _run;
        public int Index { get; }
        public string Value { get; }
        public TraceBitType Type { get; }

        public TraceBit(string text, TraceBitType type, int index = -1)
        {
            Index = index;
            Type = type;
            Value = text;
            Init(text, type, index);
        }

        public void Refresh()
        {
            Init(Value, Type, Index);
        }

        void Init(string text, TraceBitType type, int index = -1)
        {
            if (type == TraceBitType.Text)
            {
                _run = new Run(text)
                {
                    Name = $"TEXT_{index}"
                };
            }
            else if (type == TraceBitType.Higlight)
            {
                _run = new Run(text)
                {
                    Name = $"HGH_{index}",
                    Foreground = Brushes.HotPink,
                    Background = Brushes.Black
                };
            }
            else if (type == TraceBitType.DateTrace)
            {
                _run = new Run(text)
                {
                    Name = $"DATE",
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Aqua,
                    Background = Brushes.Black
                };
            }
        }

        public Run GetRun()
        {
            return _run;
        }
    }

    [Serializable]
    public class TraceHighlighter
    {
        [NonSerialized]
        private Paragraph _paragraph;

        public DateTime DateOfTrace { get; }
        public TraceBit TraceBitDate { get; }
        public List<TraceBit> TraceBitCollection { get; private set; }

        public TraceHighlighter(DateTime dateLog, string message)
        {
            _paragraph = new Paragraph();
            DateOfTrace = dateLog;
            TraceBitDate = new TraceBit($"[{dateLog:G}]:", TraceBitType.DateTrace);
            AddParagraphRun(message);
            _paragraph.LineHeight = 0.1;
        }

        /// <summary>
        /// Обновить параграф и runы
        /// </summary>
        public void Refresh()
        {
            _paragraph = new Paragraph();
            TraceBitDate.Refresh();
            _paragraph.Inlines.Add(TraceBitDate.GetRun());
            foreach (TraceBit trace in TraceBitCollection)
            {
                trace.Refresh();
                _paragraph.Inlines.Add(trace.GetRun());
            }
            _paragraph.LineHeight = 0.1;
        }

        /// <summary>
        /// Обновить сообщение
        /// </summary>
        /// <param name="message"></param>
        public void Refresh(string message)
        {
            _paragraph.Inlines.Clear();
            AddParagraphRun(message);
        }

        void AddParagraphRun(string message)
        {
            _paragraph.Inlines.Add(TraceBitDate.GetRun());
            TraceBitCollection = GetTraceBits(message.Trim());
            foreach (TraceBit trace in TraceBitCollection)
            {
                _paragraph.Inlines.Add(trace.GetRun());
            }
        }

        public Paragraph GetParagraph()
        {
            return _paragraph;
        }

        static List<TraceBit> GetTraceBits(string message)
        {
            List<TraceBit> traces = new List<TraceBit>();
            int index = 0;
            int startSmb = 0;
            StringBuilder highlightBuilder = new StringBuilder();
            StringBuilder notHighlighted = new StringBuilder();
            foreach (char ch in message)
            {
                if (ch == '[')
                {
                    startSmb++;
                    highlightBuilder.Append(ch);
                    AddToParagraph(traces, notHighlighted, TraceBitType.Text, ref index);
                    continue;
                }

                if (ch == ']')
                {
                    if (startSmb > 0)
                        startSmb--;
                    highlightBuilder.Append(ch);
                    if (startSmb == 0)
                        AddToParagraph(traces, highlightBuilder, TraceBitType.Higlight, ref index);
                    continue;
                }

                if (startSmb > 0)
                {
                    highlightBuilder.Append(ch);
                    continue;
                }

                notHighlighted.Append(ch);
            }

            AddToParagraph(traces, notHighlighted, TraceBitType.Text, ref index);
            AddToParagraph(traces, highlightBuilder, TraceBitType.Higlight, ref index);
            return traces;
        }

        static void AddToParagraph(List<TraceBit> bitCollection, StringBuilder builder, TraceBitType type, ref int index)
        {
            if (builder.Length <= 0)
                return;

            TraceBit traceBit = new TraceBit(builder.ToString(), type, index++);
            bitCollection.Add(traceBit);
            builder.Clear();
        }
    }
}
