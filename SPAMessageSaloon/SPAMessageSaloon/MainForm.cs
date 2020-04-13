using SPAMessageSaloon.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace SPAMessageSaloon
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ToolStripManager.LoadSettings(this);
            InitializeToolbarsMenu();
        }

        private void InitializeToolbarsMenu()
        {
            foreach (Control ctrl in toolStrip.Controls)
            {
                if (ctrl is ToolStrip strip && !(ctrl is MenuStrip))
                {
                    strip.AllowItemReorder = true;
                }
            }
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
                childForm.Close();
        }

        private void russianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            englishToolStripMenuItem.Checked = true;
            russianToolStripMenuItem.Checked = false;

            CultureInfo info = this.GetCultureInfo();
            if (info != null)
            {
                Thread.CurrentThread.CurrentUICulture = info;
                ApplyResources();
            }
        }

        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            englishToolStripMenuItem.Checked = false;
            russianToolStripMenuItem.Checked = true;

            CultureInfo info = this.GetCultureInfo();
            if (info != null)
            {
                Thread.CurrentThread.CurrentUICulture = info;
                ApplyResources();
            }
        }

        private CultureInfo GetCultureInfo()
        {
            if (this.russianToolStripMenuItem.Checked)
                return new CultureInfo("ru-RU");
            return new CultureInfo("en-US");
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {

        }

        private void ApplyResources()
        {
            base.Text = $"SPA Message Saloon {ASSEMBLY.CurrentVersion}";
        }

        public T ShowMdiForm<T>(Func<T> formMaker, bool newInstance = false) where T : Form, ISPAMessageSaloonItems
        {
            if (!newInstance && ActivateMdiForm(out T form))
                return form;

            form = formMaker();
            if (form == null)
                return null;

            form.MdiParent = this;
            form.Load += MDIManagerButton_Load;
            form.Show();

            return form;
        }

        private bool ActivateMdiForm<T>(out T form) where T : Form
        {
            foreach (Form f in this.MdiChildren)
            {
                form = f as T;
                if (form != null)
                {
                    f.Activate();
                    return true;
                }
            }
            form = null;
            return false;
        }

        private void MDIManagerButton_Load(object sender, EventArgs e)
        {
            MDIManagerButton button = new MDIManagerButton(sender as Form) {BackColor = Color.White};
            button.Click += (S, E) => { ((MDIManagerButton) S).mdiForm.Activate(); };
            ((sender as Form)?.MdiParent as MainForm)?.statusStrip.Items.Add(button);
        }


    }
}
