﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;

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
            Random myrandom = new Random();
            Int32 p = 79, g = 7, ya, yb, xa, xb, Sa, Sb, k;

            xa = (Int32) myrandom.Next(p); // A
            ya = fun(p, g, xa);

            xb = (Int32) myrandom.Next(p); //B
            yb = fun(p, g, xb);

            Sa = fun(p, yb, xa); //A
            Sb = fun(p, ya, xb); //B

            //шифровка A
            DHValue result = new DHValue();
            
            str1 = textBox1;
            for (int i = 0; i < str1.Length; i++)
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
            for (int i = 0; i < str2.Length; i++)
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
            int s = 1;
            for (int i = 1; i <= b; i++)
            {
                s = (s * a) % p;
            }
            return s;
        }
    }
}