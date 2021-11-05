﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Utils.WinForm
{
    public class CustomTreeView : TreeView
    {
	    public const int NOIMAGE = -1;

        public CustomTreeView()
	    {
		    // .NET Bug: Unless LineColor is set, Win32 treeview returns -1 (default), .NET returns Color.Black!
		    base.LineColor = SystemColors.GrayText;
		    base.DrawMode = TreeViewDrawMode.OwnerDrawAll;
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            const int SPACE_IL = 3;  // space between Image and Label

            // we only do additional drawing
            e.DrawDefault = true;

            base.OnDrawNode(e);

            if (base.ShowLines && base.ImageList != null && e.Node.ImageIndex == NOIMAGE
                // exclude root nodes, if root lines are disabled
                //&& (base.ShowRootLines || e.Node.Level > 0))
                )
            {
                // Using lines & images, but this node has none: fill up missing treelines

                // Image size
                var imgW = base.ImageList.ImageSize.Width;
                var imgH = base.ImageList.ImageSize.Height;

                // Image center
                var xPos = e.Node.Bounds.Left - SPACE_IL - imgW / 2;
                var yPos = (e.Node.Bounds.Top + e.Node.Bounds.Bottom) / 2;

                // Image rect
                var imgRect = new Rectangle(xPos, yPos, 0, 0);
                imgRect.Inflate(imgW / 2, imgH / 2);

                using (var p = new Pen(base.LineColor, 1))
                {
                    p.DashStyle = DashStyle.Dot;

                    // account uneven Indent for both lines
                    p.DashOffset = base.Indent % 2;

                    // Horizontal treeline across width of image
                    // account uneven half of delta ItemHeight & ImageHeight
                    var yHor = yPos + ((base.ItemHeight - imgRect.Height) / 2) % 2;

                    //if (base.ShowRootLines || e.Node.Level > 0)
                    //{
                    //    e.Graphics.DrawLine(p, imgRect.Left, yHor, imgRect.Right, yHor);
                    //}
                    //else
                    //{
                    //    // for root nodes, if root lines are disabled, start at center
                    //    e.Graphics.DrawLine(p, xPos - (int)p.DashOffset, yHor, imgRect.Right, yHor);
                    //}

                    e.Graphics.DrawLine(p,
                        (base.ShowRootLines || e.Node.Level > 0) ? imgRect.Left : xPos - (int)p.DashOffset,
                        yHor, imgRect.Right, yHor);


                    if (!base.CheckBoxes && e.Node.IsExpanded)
                    {
                        // Vertical treeline , offspring from NodeImage center to e.Node.Bounds.Bottom
                        // yStartPos: account uneven Indent and uneven half of delta ItemHeight & ImageHeight
                        var yVer = yHor + (int)p.DashOffset;
                        e.Graphics.DrawLine(p, xPos, yVer, xPos, e.Node.Bounds.Bottom);
                    }
                }
            }
        }

        protected override void OnAfterCollapse(TreeViewEventArgs e)
        {
            base.OnAfterCollapse(e);

            if (!base.CheckBoxes && base.ImageList != null && e.Node.ImageIndex == NOIMAGE)
            {
                // DrawNode event not raised: redraw node with collapsed treeline
                base.Invalidate(e.Node.Bounds);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Правит баг когда ячейка выбрана, но визуально не обновляется 
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            // Suppress WM_LBUTTONDBLCLK
            if (m.Msg == 0x203)
            {
                m.Result = IntPtr.Zero;
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}