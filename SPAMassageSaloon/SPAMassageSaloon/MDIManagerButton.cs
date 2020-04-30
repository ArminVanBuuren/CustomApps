using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Utils;

namespace SPAMassageSaloon
{
    internal class MDIManagerButton : ToolStripStatusLabel
    {
        public Form mdiForm { get; private set; }
        public event EventHandler MClick;
        public event EventHandler Activated;

        private int btnTop, btnLeft;
        private Rectangle btnRectangle;
        private bool mouseOnCloseBtn = false;

        public MDIManagerButton(Form form)
        {
            mdiForm = form;
            mdiForm.FormClosed += (s, e) => { this.Owner.Items.Remove(this); };
            mdiForm.TextChanged += (s, e) => { this.Text = mdiForm.Text + @"    "; };
            mdiForm.Activated += (s, e) => { this.MDIManagerButton_Click(this, null); };

            base.Text = mdiForm.Text.RegexReplace(@"[0-9.]+", string.Empty).Trim() + @"    ";
            base.Padding = new Padding(6, 0, 0, 5);
            base.Margin = new Padding(0, 0, 0, 0);
            base.TextAlign = ContentAlignment.MiddleCenter;
            MouseUp += MDIManagerButton_MouseUp;
            MouseMove += MDIManagerButton_MouseMove;
            Paint += MDIManagerButton_Paint;
            Click += MDIManagerButton_Click;
            this.BorderSides = ToolStripStatusLabelBorderSides.All;
        }

        void MDIManagerButton_Click(object sender, EventArgs e)
        {
            if (this.Parent == null)
                return;

            try
            {
                foreach (MDIManagerButton btn in this.Parent.Items.OfType<MDIManagerButton>())
                {
                    btn.BorderStyle = Border3DStyle.Flat;
                    btn.BackColor = SystemColors.ButtonFace;
                }

                this.BorderStyle = Border3DStyle.Sunken;
                this.BackColor = Color.White;
            }
            catch (Exception ex)
            {
                // ignored
            }
            finally
            {
                Activated?.Invoke(this, null);
            }
        }

        void MDIManagerButton_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (mouseOnCloseBtn)
                        mdiForm.Close();
                    else
                        MClick?.Invoke(sender, e);
                    break;
            }
        }

        void MDIManagerButton_MouseMove(object sender, MouseEventArgs e)
        {
            mouseOnCloseBtn = btnRectangle.Contains(e.Location);
            this.Invalidate();
        }

        void MDIManagerButton_Paint(object sender, PaintEventArgs e)
        {
            btnLeft = this.Width - Properties.Resources.close.Size.Width - 2;
            btnTop = (this.Height - Properties.Resources.close.Size.Height) / 2;
            btnRectangle = new Rectangle(btnLeft, btnTop, Properties.Resources.close.Size.Width, Properties.Resources.close.Size.Height);

            if (mouseOnCloseBtn)
                e.Graphics.DrawImage(Properties.Resources.close_active, new Point(btnLeft, btnTop));
            else
                e.Graphics.DrawImage(Properties.Resources.close, new Point(btnLeft, btnTop));
        }
    }
}
