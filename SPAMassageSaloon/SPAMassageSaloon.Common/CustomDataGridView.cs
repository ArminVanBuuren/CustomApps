using System.Windows.Forms;

namespace SPAMassageSaloon.Common
{
    public class CustomDataGridView : DataGridView
    {
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
    }
}