using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASOTCutter
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

            System.Text.EncodingInfo[] encs = System.Text.Encoding.GetEncodings();

            foreach (EncodingInfo encIn in encs)
            {
                //Dictionary<int, string> result2 = new Dictionary<int, string>();

                System.Text.Encoding encGetBytes = System.Text.Encoding.GetEncoding(encIn.CodePage);
                foreach (EncodingInfo encOut in encs)
                {
                    System.Text.Encoding encGetString = System.Text.Encoding.GetEncoding(encOut.CodePage);

                    byte[] encoded = encGetBytes.GetBytes(sample);
                    string corrected = encGetString.GetString(encoded);

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

            byte[] encoded = EncodingIn.GetBytes(sample);
            string corrected = EncodingOut.GetString(encoded);

            return corrected;
        }
    }
}
