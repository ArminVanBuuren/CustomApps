using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Utils.WinForm.Expander
{

	// ExpandButtonStyles
	/// <summary>
	/// Visual styles of the expand-collapse button.
	/// </summary>
	public enum ExpandButtonStyle
	{
		Classic,
		Circle,
		MagicArrow,
		Triangle,
		FatArrow
	}

	// ExpandButtonSizes
	/// <summary>
	/// Size presets of the expand-collapse button.
	/// </summary>
	public enum ExpandButtonSize
	{
		Small,
		Normal,
		Large
	}

    /// <summary>
    /// Button with two states: expanded/collapsed
    /// </summary>
    internal partial class ExpandCollapseButton : UserControl
    {
        /// <summary>
        /// Image displays expanded state of button
        /// </summary>
        private Image _expanded;
        /// <summary>
        /// Image displays collapsed state of button
        /// </summary>
        private Image _collapsed;

        private Color _headerBackColor = Color.Azure;
        private bool _isExpanded = true;
        private ExpandButtonSize _expandButtonSize = ExpandButtonSize.Small;
        private ExpandButtonStyle _expandButtonStyle = ExpandButtonStyle.Circle;

        /// <summary>
        /// Occurs when the button has expanded or collapsed
        /// </summary>
        [Category("ExpandCollapseButton")]
        [Description("Occurs when the button has expanded or collapsed.")]
        [Browsable(true)]
        public event EventHandler<ExpandCollapseEventArgs> ExpandCollapse;

        /// <summary>
        /// Occurs when the button has expanded or collapsed
        /// </summary>
        [Category("ExpandCollapseButton")]
        [Description("Occurs when the button has CheckBox check changed.")]
        [Browsable(true)]
        public event EventHandler<ExpandCollapseEventArgs> CheckedChanged;

        /// <summary>
        /// Set flag for expand or collapse button
        /// (true - expanded, false - collapsed)
        /// </summary>
        [Browsable(true)]
        [Category("ExpandCollapseButton")]
        [Description("Expand or collapse button.")]
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnExpandCollapse();
            }
        }

        /// <summary>
        /// Header
        /// </summary>
        [Category("ExpandCollapseButton")]
        [Description("Header")]
        [Browsable(true)]
        public override string Text
        {
            get => lblHeader.Text;
            set => lblHeader.Text = value;
        }

        /// <summary>
        /// CheckBox
        /// </summary>
        [Category("ExpandCollapseButton")]
        [Description("CheckBoxShown")]
        [Browsable(true)]
        public bool CheckBoxShown
        {
	        get => checkBox.Visible;
	        set => checkBox.Visible = value;
        }

        /// <summary>
        /// CheckBox
        /// </summary>
        [Category("ExpandCollapseButton")]
        [Description("CheckBoxEnabled")]
        [Browsable(true)]
        public bool CheckBoxEnabled
        {
	        get => checkBox.Enabled;
	        set => checkBox.Enabled = value;
        }

        /// <summary>
        /// CheckBox
        /// </summary>
        [Category("ExpandCollapseButton")]
        [Description("IsChecked")]
        [Browsable(true)]
        public bool IsChecked
        {
	        get => checkBox.Checked;
	        set => checkBox.Checked = value;
        }

        /// <summary>
        /// Font used for displays header text
        /// </summary>
        public override Font Font
        {
            get => lblHeader.Font;
            set => lblHeader.Font = value;
        }

        /// <summary>
        /// Foreground color used for displays header text
        /// </summary>
        public override Color ForeColor
        {
            get => lblHeader.ForeColor;
            set => lblHeader.ForeColor = value;
        }

        /// <summary>
        /// HeaderBackColor
        /// </summary>
        [Category("ExpandCollapseButton")]
        [Description("HeaderBackColor")]
        [Browsable(true)]
        public Color HeaderBackColor
        {
	        get => _headerBackColor;
	        set
	        {
		        _headerBackColor = value;
		        panel.BackColor = _headerBackColor;
		        checkBox.BackColor = _headerBackColor;
	        }
        }

        /// <summary>
        /// HeaderLineColor
        /// </summary>
        [Category("ExpandCollapseButton")]
        [Description("HeaderLineColor")]
        [Browsable(true)]
        public Color HeaderLineColor
        {
	        get => lblLine.BackColor;
	        set => lblLine.BackColor = value;
        }

        /// <summary>
        /// Visual style of the expand-collapse button.
        /// </summary>
        [Category("ExpandCollapseButton")]
        [Description("Visual style of the expand-collapse button.")]
        [Browsable(true)]
        public ExpandButtonStyle ButtonStyle
        {
            get => _expandButtonStyle;
            set
            {
                if (_expandButtonStyle != value)
                {
                    InitButtonStyle(value);
                }
            }
        }

        /// <summary>
        /// Size preset of the expand-collapse button.
        /// </summary>
        [Category("ExpandCollapseButton")]
        [Description("Size preset of the expand-collapse button.")]
        [Browsable(true)]
        public ExpandButtonSize ButtonSize
        {
	        get => _expandButtonSize;
	        set
	        {
		        if (_expandButtonSize != value)
		        {
			        InitButtonSize(value);
		        }
	        }
        }

        public ExpandCollapseButton()
        {
	        InitializeComponent();

	        panel.BackColor = _headerBackColor;
	        checkBox.BackColor = _headerBackColor;

            // initialize expanded/collapsed state bitmaps:
            InitButtonStyle(_expandButtonStyle);
	        InitButtonSize(_expandButtonSize);

            checkBox.CheckedChanged += (sender, args) => CheckedChanged?.Invoke(this, new ExpandCollapseEventArgs(IsExpanded, IsChecked));
        }

        private void InitButtonStyle(ExpandButtonStyle style)
        {
            _expandButtonStyle = style;

            switch (_expandButtonStyle)
            {
                case ExpandButtonStyle.MagicArrow:
                    var bmp = Properties.Resources.expander_Upload;
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    pictureBox.Image = bmp;
                    break;
                case ExpandButtonStyle.Circle:
                    bmp = Properties.Resources.expander_icon_expand;
                    pictureBox.Image = bmp;
                    break;
                case ExpandButtonStyle.Triangle:
                    pictureBox.Image = Properties.Resources.expander_downarrow;
                    break;
                case ExpandButtonStyle.FatArrow:
                    bmp = Properties.Resources.expander_up_256;
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    pictureBox.Image = bmp;
                    break;
                case ExpandButtonStyle.Classic:
                    bmp = Properties.Resources.expander_icon_struct_hide_collapsed;
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    pictureBox.Image = bmp;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(style));
            }

            // collapsed bitmap:
            _collapsed = pictureBox.Image;

            // expanded bitmap is rotated collapsed bitmap:
            _expanded = MakeGrayscale3(pictureBox.Image);
            _expanded.RotateFlip(RotateFlipType.Rotate180FlipNone);


            // finally set appropriate bitmap for current state
            pictureBox.Image = _isExpanded ? _expanded : _collapsed;
        }

        /// <summary>
        /// Resize and arrange child controls according to ButtonSize preset
        /// </summary>
        /// <param name="size">ButtonSize preset</param>
        private void InitButtonSize(ExpandButtonSize size)
        {
            _expandButtonSize = size;

            switch (_expandButtonSize)
            {
                case ExpandButtonSize.Small:
                    pictureBox.Location = new Point(2, 1);
                    pictureBox.Size = new Size(16, 16);
                    lblLine.Location = new Point(20, 18);
                    lblHeader.Location = new Point(20, 1);
                    break;
                case ExpandButtonSize.Normal:
                    pictureBox.Location = new Point(2, 1);
                    pictureBox.Size = new Size(24, 24);
                    lblLine.Location = new Point(30, 22);
                    lblHeader.Location = new Point(30, 3);
                    break;
                case ExpandButtonSize.Large:
                    pictureBox.Location = new Point(2, 1);
                    pictureBox.Size = new Size(35, 35);
                    lblLine.Location = new Point(41, 28);
                    lblHeader.Location = new Point(41, 3);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // after resize all child controls - do resize for entire ExpandCollapseButton control:
            Height = pictureBox.Location.Y + pictureBox.Height + 2;
        }

        /// <summary>
        /// Handle clicks from PictureBox and Header
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnMouseDown(object sender, EventArgs e)
        {
            // just invert current state
            IsExpanded = !IsExpanded;
        }

        /// <summary>
        /// Handle state changing
        /// </summary>
        protected virtual void OnExpandCollapse()
        {
            // set appropriate bitmap
            pictureBox.Image = _isExpanded ? _expanded : _collapsed;
            //lblHeader.ForeColor = _isExpanded ? Color.DarkGray : Color.SteelBlue;

            // and fire the event:
            var handler = ExpandCollapse;
            handler?.Invoke(this, new ExpandCollapseEventArgs(IsExpanded, IsChecked));
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            //using (var g = CreateGraphics())
            //{
            // var size = g.MeasureString(lblHeader.Text, lblHeader.Font);
            //}

            //var test = this.Size;

            panel.MaximumSize = new Size(Size.Width - (checkBox.Visible ? checkBox.Size.Width : 0), 0);
            base.OnSizeChanged(e);
        }

        /// <summary>
        /// Utillity method for createing a grayscale copy of image
        /// </summary>
        /// <param name="original">original image</param>
        /// <returns>grayscale copy of image</returns>
        public static Bitmap MakeGrayscale3(Image original)
        {
            // create a blank bitmap the same size as original
            var newBitmap = new Bitmap(original.Width, original.Height);

            // get a graphics object from the new image
            using (var g = Graphics.FromImage(newBitmap))
            {

                // create the grayscale ColorMatrix
                var colorMatrix = new ColorMatrix(
                    new[]
                    {
                            new[] {.3f, .3f, .3f, 0, 0},
                            new[] {.59f, .59f, .59f, 0, 0},
                            new[] {.11f, .11f, .11f, 0, 0},
                            new float[] {0, 0, 0, 1, 0},
                            new float[] {0, 0, 0, 0, 1}
                        });

                // create some image attributes
                var attributes = new ImageAttributes();

                // set the color matrix attribute
                attributes.SetColorMatrix(colorMatrix);

                // draw the original image on the new image
                // using the grayscale color matrix
                g.DrawImage(original,
	                new Rectangle(0, 0, original.Width, original.Height), 
	                0, 
	                0, 
	                original.Width, 
	                original.Height, 
	                GraphicsUnit.Pixel, 
	                attributes);

                // dispose the Graphics object
                g.Dispose();
            }

            return newBitmap;
        }
    }
}
