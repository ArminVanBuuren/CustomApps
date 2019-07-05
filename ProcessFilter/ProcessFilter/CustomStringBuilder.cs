using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAFilter
{
    public class CustomStringBuilder : IDisposable
    {
        private readonly List<string> _strLines = new List<string>();
        public int Lines => _strLines.Count;
        public int Length { get; private set; } = 0;

        public void AppendLine(string line)
        {
            Length += line.Length;
            _strLines.Add(line);
        }

        public string ToString(int lines)
        {
            var builder = new StringBuilder();
            int i = 0;
            foreach (var line in _strLines)
            {
                builder.AppendLine(line);
                if (++i >= lines)
                    break;
            }
            return builder.ToString();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var line in _strLines)
            {
                builder.AppendLine(line);
            }
            return builder.ToString();
        }

        public void Dispose()
        {
            _strLines.Clear();
            Length = 0;
        }
    }
}
