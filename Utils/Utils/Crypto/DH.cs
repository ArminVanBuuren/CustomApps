using System;

namespace Utils.Crypto
{
    public class DHValue
    {
        public string CryptoValue { get; internal set; } = string.Empty;
        public string EncryptoValue { get; internal set; } = string.Empty;
    }

    public class DH
    {
        public static DHValue Crypto(string textBox1)
        {
            string str1, str2;
            var myrandom = new Random();
            int p = 79, g = 7, ya, yb, xa, xb, Sa, Sb, k;

            xa = (int) myrandom.Next(p); // A
            ya = fun(p, g, xa);

            xb = myrandom.Next(p); //B
            yb = fun(p, g, xb);

            Sa = fun(p, yb, xa); //A
            Sb = fun(p, ya, xb); //B

            //шифровка A
            var result = new DHValue();
            
            str1 = textBox1;
            for (var i = 0; i < str1.Length; i++)
            {
                k = (int) str1[i];
                k = k + Sa;
                while (k > 122)
                {
                    k = k - 26;
                }
                result.CryptoValue = result.CryptoValue + (char) k;
            }

            //дешифровка B
            str2 = result.CryptoValue;
            for (var i = 0; i < str2.Length; i++)
            {
                k = (int) str2[i];
                k = k - Sb;
                while (k < 97)
                {
                    k = k + 26;
                }
                result.EncryptoValue = result.EncryptoValue + (char) k;
            }

            return result;
        }

        static int fun(int p, int a, int b)
        {
            var s = 1;
            for (var i = 1; i <= b; i++)
            {
                s = (s * a) % p;
            }
            return s;
        }
    }
}