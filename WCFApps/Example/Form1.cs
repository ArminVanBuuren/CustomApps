using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;

using System.Windows.Forms;

namespace DH
{
    public partial class Form1 : Form
    {
        public int fun(int p, int a, int b)
        {
            int s = 1;
            for (int i = 1; i <= b; i++)
            {

                s = (s * a) % p;
            }
            return s;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string str1, str2;
            Random myrandom = new Random();
            Int32 p = 79, g = 7, ya, yb, xa, xb, Sa, Sb, k;
            
            xa = (Int32)myrandom.Next(p); // A
            ya = fun(p,g,xa);

            xb = (Int32)myrandom.Next(p); //B
            yb = fun(p, g, xb);

            Sa = fun(p, yb, xa); //A
            Sb = fun(p, ya, xb); //B
        
            //шифровка A
            textBox2.Text = "";
            textBox3.Text = "";
            str1 = textBox1.Text;
            for (int i = 0; i < str1.Length; i++)
            {
                k=(int)str1[i];
                k=k+Sa;
                while(k>122)
                {
                    k=k-26;
                }
                textBox2.Text = textBox2.Text + (char)k;
            }

            //дешифровка B
            str2 = textBox2.Text;
            for (int i = 0; i < str2.Length; i++)
            {
                k = (int)str2[i];
                k = k - Sb;
                while (k < 97)
                {
                    k = k + 26;
                }
                textBox3.Text = textBox3.Text + (char)k;
            }
        }
    }
}
