using System.Drawing;
using System.Windows.Forms;

namespace Utils.WinForm
{
    public class CustomRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (e.Item is ToolStripStatusLabel)
            {
                TextRenderer.DrawText(e.Graphics,
                    e.Text,
                    e.Item.Font,
                    e.TextRectangle,
                    e.TextColor,
                    e.Item.BackColor,
                    e.TextFormat | TextFormatFlags.EndEllipsis | TextFormatFlags.Left);
            }
            else
                base.OnRenderItemText(e);
        }

        public static Size FlipSize(Size size)
        {
            int width = size.Width;
            size.Width = size.Height;
            size.Height = width;
            return size;
        }
    }
}
