using System.Windows.Forms;

namespace SPAMassageSaloon.Common
{
    public class CustomDataGridView : DataGridView
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Left)
                base.OnKeyDown(new KeyEventArgs(Keys.PageUp));
            else if (e.KeyData == Keys.Right)
                base.OnKeyDown(new KeyEventArgs(Keys.PageDown));
            else
                base.OnKeyDown(e);
        }
    }
}