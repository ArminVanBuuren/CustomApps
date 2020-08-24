using System.Windows.Forms;

namespace SPAMassageSaloon.Common
{
    public sealed class CustomDataGridView : DataGridView
    {
        public bool IsSuspendLayout { get; private set; }

        public bool DoubleBuffered2
        {
	        get => DoubleBuffered;
	        set => DoubleBuffered = value;
        }

        public CustomDataGridView()
        {
	        if (!SystemInformation.TerminalServerSession)
		        DoubleBuffered = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {

            switch (e.KeyData)
            {
                case Keys.Left:
                    base.OnKeyDown(new KeyEventArgs(Keys.PageUp));
                    break;
                case Keys.Right:
                    base.OnKeyDown(new KeyEventArgs(Keys.PageDown));
                    break;
                case Keys.Enter:
                    break;
                default:
                    base.OnKeyDown(e);
                    break;
            }
        }

        public new void SuspendLayout()
        {
            base.SuspendLayout();
            IsSuspendLayout = true;
        }

        public new void ResumeLayout()
        {
            base.ResumeLayout();
            IsSuspendLayout = false;
        }
    }
}