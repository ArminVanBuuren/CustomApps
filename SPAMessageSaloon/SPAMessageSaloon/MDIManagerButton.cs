using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SPAMessageSaloon
{
    class MDIManagerButton : ToolStripStatusLabel
    {
        public Form mdiForm { get; private set; }
        public event EventHandler MClick;

        private int btnTop, btnLeft;
        private Rectangle btnRectangle;
        private bool mouseOnCloseBtn = false;

        public MDIManagerButton(Form form)
        {
            mdiForm = form;
            mdiForm.FormClosed += (s, e) => { this.Owner.Items.Remove(this); };
            mdiForm.TextChanged += (s, e) => { this.Text = mdiForm.Text + "    "; };
            mdiForm.Activated += (s, e) => { this.MDIManagerButton_Click(this, null); };

            this.Text = mdiForm.Text + "    ";
            MouseUp += new MouseEventHandler(MDIManagerButton_MouseUp);
            MouseMove += new MouseEventHandler(MDIManagerButton_MouseMove);
            Paint += new PaintEventHandler(MDIManagerButton_Paint);
            Click += new EventHandler(MDIManagerButton_Click);
            this.BorderSides = ToolStripStatusLabelBorderSides.All;
        }

        void MDIManagerButton_Click(object sender, EventArgs e)
        {
            if (this.Parent == null) return;
            foreach (MDIManagerButton btn in this.Parent.Items.OfType<MDIManagerButton>())
            {
                btn.BorderStyle = Border3DStyle.Flat;
                btn.BackColor = SystemColors.ButtonFace;
            }
            this.BorderStyle = Border3DStyle.Sunken;
            this.BackColor = Color.White;
        }

        void MDIManagerButton_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {

                case MouseButtons.Left:
                    if (mouseOnCloseBtn)
                    {
                        mdiForm.Close();
                    }
                    else
                        if (MClick != null)
                        MClick(sender, e);
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
