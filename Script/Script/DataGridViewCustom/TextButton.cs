using System;
using System.Windows.Forms;

namespace Script.DataGridViewCustom
{
    internal partial class TextButton : UserControl
    {
        public sealed class TextButtonEventArgs : EventArgs
        {
            public string Text;
            public bool Handled;

            public TextButtonEventArgs(string text)
            {
                Text = text;
            }
        }

        public event EventHandler<TextButtonEventArgs> ButtonClick;

        public TextButton()
        {
            InitializeComponent();
        }

        public override string Text
        {
            get { return InnerTextBox.Text; }
            set { InnerTextBox.Text = value; }
        }

        private void button_Click(object sender, EventArgs e)
        {
            if (ButtonClick != null)
            {
                TextButtonEventArgs args = new TextButtonEventArgs(InnerTextBox.Text);
                ButtonClick(this, args);
                if (args.Handled)
                    InnerTextBox.Text = args.Text;
                InnerTextBox.Focus();
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, Math.Max(40, width), 18, specified);
        }

        public void ClearUndo()
        {
            InnerTextBox.ClearUndo();
        }
    }
}
