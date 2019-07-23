using System;
using System.Text;

namespace DjSetCutter
{
    public class IdentifyEncoding
    {
        public Encoding EncodingIn { get; private set; }
        public Encoding EncodingOut { get; private set; }
        public bool IsIdentified { get; private set; } = false;

        public IdentifyEncoding(string sample, string correct)
        {
            TryIdentify(sample, correct);
        }

        void TryIdentify(string sample, string correct)
        {
            //Dictionary<int, Dictionary<int, string>> result = new Dictionary<int, Dictionary<int, string>>();

            var encs = Encoding.GetEncodings();

            foreach (var encIn in encs)
            {
                //Dictionary<int, string> result2 = new Dictionary<int, string>();

                var encGetBytes = Encoding.GetEncoding(encIn.CodePage);
                foreach (var encOut in encs)
                {
                    var encGetString = Encoding.GetEncoding(encOut.CodePage);

                    var encoded = encGetBytes.GetBytes(sample);
                    var corrected = encGetString.GetString(encoded);

                    if (corrected.IndexOf(correct, StringComparison.Ordinal) != -1)
                    {
                        IsIdentified = true;
                        EncodingIn = encGetBytes;
                        EncodingOut = encGetString;
                        return;
                    }

                    //result2.Add(encs[j].CodePage, corrected);
                }

                //result.Add(encs[i].CodePage, result2);
            }
            IsIdentified = false;
        }

        public string GetCorrectString(string sample)
        {
            if (!IsIdentified)
                throw new Exception("Encoding not identified!");

            var encoded = EncodingIn.GetBytes(sample);
            var corrected = EncodingOut.GetString(encoded);

            return corrected;
        }
    }
}
