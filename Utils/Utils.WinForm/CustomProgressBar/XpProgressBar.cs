using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Utils.WinForm.CustomProgressBar
{

    public enum GradientMode
    {
        Vertical,
        VerticalCenter,
        Horizontal,
        HorizontalCenter,
        Diagonal
    };

    public class XpProgressBar : Control, IProgressBar
    {
        private const string CategoryName = "Xp ProgressBar";

        private Color mColor1 = Color.FromArgb(170, 240, 170);
        private Color mColor2 = Color.FromArgb(10, 150, 10);
        private Color mColorBackGround = Color.White;
        private Color mColorText = Color.Black;
        private Image mDobleBack = null;
        private GradientMode mGradientStyle = GradientMode.VerticalCenter;

        private int mMax = 100;
        private int mMin = 0;

        private int mPosition = 50;
        private byte mSteepDistance = 2;
        private byte mSteepWidth = 6;

        private Rectangle innerRect;
        private LinearGradientBrush mBrush1;
        private LinearGradientBrush mBrush2;
        private readonly Pen mPenIn = new Pen(Color.FromArgb(239, 239, 239));

        private readonly Pen mPenOut = new Pen(Color.FromArgb(104, 104, 104));
        private readonly Pen mPenOut2 = new Pen(Color.FromArgb(190, 190, 190));

        private Rectangle mSteepRect1;
        private Rectangle mSteepRect2;
        private Rectangle outnnerRect;
        private Rectangle outnnerRect2;

        public XpProgressBar()
        {

        }

        public Control ProgressBar => this;

        [Category(CategoryName)]
        [Description("The Back Color of the Progress Bar")]
        public Color ColorBackGround
        {
            get => mColorBackGround;
            set
            {
                mColorBackGround = value;
                this.InvalidateBuffer(true);
            }
        }

        [Category(CategoryName)]
        [Description("The Border Color of the gradient in the Progress Bar")]
        public Color ColorBarBorder
        {
            get => mColor1;
            set
            {
                mColor1 = value;
                this.InvalidateBuffer(true);
            }
        }

        [Category(CategoryName)]
        [Description("The Center Color of the gradient in the Progress Bar")]
        public Color ColorBarCenter
        {
            get => mColor2;
            set
            {
                mColor2 = value;
                this.InvalidateBuffer(true);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [RefreshProperties(RefreshProperties.Repaint)]
        [Description("Set to TRUE to reset all colors like the Windows XP Progress Bar ®")]
        [Category(CategoryName)]
        [DefaultValue(false)]
        public bool ColorsXP
        {
            get => false;
            set
            {
                ColorBarBorder = Color.FromArgb(170, 240, 170);
                ColorBarCenter = Color.FromArgb(10, 150, 10);
                ColorBackGround = Color.White;
            }
        }

        [Category(CategoryName)]
        [Description("The Color of the text displayed in the Progress Bar")]
        public Color ColorText
        {
            get => mColorText;
            set
            {
                mColorText = value;

                if (this.Text != String.Empty)
                {
                    this.Invalidate();
                }
            }
        }

        [RefreshProperties(RefreshProperties.Repaint)]
        [Category(CategoryName)]
        [Description("The Current Value of the Progress Bar")]
        public int Value { get => Position; set => Position = value; }


        [RefreshProperties(RefreshProperties.Repaint)]
        [Category(CategoryName)]
        [Description("The Current Position of the Progress Bar")]
        public int Position
        {
            get => mPosition;
            set
            {
                if (value > mMax)
                {
                    mPosition = mMax;
                }
                else if (value < mMin)
                {
                    mPosition = mMin;
                }
                else
                {
                    mPosition = value;
                }
                this.Invalidate();
            }
        }


        [RefreshProperties(RefreshProperties.Repaint)]
        [Category(CategoryName)]
        [Description("The Max Value of the Progress Bar")]
        public int Maximum { get => PositionMax; set => PositionMax = value; }


        [RefreshProperties(RefreshProperties.Repaint)]
        [Category(CategoryName)]
        [Description("The Max Position of the Progress Bar")]
        public int PositionMax
        {
            get => mMax;
            set
            {
                if (value <= mMin)
                    return;

                mMax = value;

                if (mPosition > mMax)
                {
                    Position = mMax;
                }

                this.InvalidateBuffer(true);
            }
        }

        [RefreshProperties(RefreshProperties.Repaint)]
        [Category(CategoryName)]
        [Description("The Min Position of the Progress Bar")]
        public int PositionMin
        {
            get => mMin;
            set
            {
                if (value >= mMax)
                    return;

                mMin = value;

                if (mPosition < mMin)
                {
                    Position = mMin;
                }
                this.InvalidateBuffer(true);
            }
        }

        [Category(CategoryName)]
        [Description("The number of Pixels between two Steeps in Progress Bar")]
        [DefaultValue((byte)2)]
        public byte SteepDistance
        {
            get => mSteepDistance;
            set
            {
                mSteepDistance = value;
                this.InvalidateBuffer(true);
            }
        }

        [Category(CategoryName)]
        [Description("The Style of the gradient bar in Progress Bar")]
        [DefaultValue(GradientMode.VerticalCenter)]
        public GradientMode GradientStyle
        {
            get => mGradientStyle;
            set
            {
                if (mGradientStyle == value)
                    return;
                mGradientStyle = value;
                CreatePaintElements();
                this.Invalidate();
            }
        }

        [Category(CategoryName)]
        [Description("The number of Pixels of the Steeps in Progress Bar")]
        [DefaultValue((byte)6)]
        public byte SteepWidth
        {
            get => mSteepWidth;
            set
            {
                if (value <= 0)
                    return;

                mSteepWidth = value;
                this.InvalidateBuffer(true);
            }
        }

        [RefreshProperties(RefreshProperties.Repaint)]
        [Category(CategoryName)]
        public override Image BackgroundImage
        {
            get => base.BackgroundImage;
            set
            {
                base.BackgroundImage = value;
                InvalidateBuffer();
            }
        }

        [Category(CategoryName)]
        [Description("The Text displayed in the Progress Bar")]
        [DefaultValue("")]
        public override string Text
        {
            get => base.Text;
            set
            {
                if (base.Text == value)
                    return;

                base.Text = value;
                this.Invalidate();
            }
        }

        private bool mTextShadow = true;

        [Category(CategoryName)]
        [Description("Set the Text shadow in the Progress Bar")]
        [DefaultValue(true)]
        public bool TextShadow
        {
            get => mTextShadow;
            set
            {
                mTextShadow = value;
                this.Invalidate();
            }
        }

        private byte mTextShadowAlpha = 150;

        [Category(CategoryName)]
        [Description("Set the Alpha Channel of the Text shadow in the Progress Bar")]
        [DefaultValue((byte)150)]
        public byte TextShadowAlpha
        {
            get => mTextShadowAlpha;
            set
            {
                if (mTextShadowAlpha == value)
                    return;

                mTextShadowAlpha = value;
                this.TextShadow = true;
            }
        }

        protected override Size DefaultSize => new Size(100, 29);

        protected override void OnPaint(PaintEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Paint " + this.Name + "  Pos: "+this.Position.ToString());
            if (this.IsDisposed)
                return;

            var mSteepTotal = mSteepWidth + mSteepDistance;
            float mUtilWidth = this.Width - 6 + mSteepDistance;

            if (mDobleBack == null)
            {
                mUtilWidth = this.Width - 6 + mSteepDistance;
                var mMaxSteeps = (int)(mUtilWidth / mSteepTotal);
                this.Width = 6 + mSteepTotal * mMaxSteeps;

                mDobleBack = new Bitmap(this.Width, this.Height);

                var g2 = Graphics.FromImage(mDobleBack);

                CreatePaintElements();

                g2.Clear(mColorBackGround);

                if (this.BackgroundImage != null)
                {
                    var textuBrush = new TextureBrush(this.BackgroundImage, WrapMode.Tile);
                    g2.FillRectangle(textuBrush, 0, 0, this.Width, this.Height);
                    textuBrush.Dispose();
                }
                //				g2.DrawImage()

                g2.DrawRectangle(mPenOut2, outnnerRect2);
                g2.DrawRectangle(mPenOut, outnnerRect);
                g2.DrawRectangle(mPenIn, innerRect);
                g2.Dispose();

            }

            var ima = new Bitmap(mDobleBack);

            var gtemp = Graphics.FromImage(ima);

            var mCantSteeps = (int)((((float)mPosition - mMin) / (mMax - mMin)) * mUtilWidth / mSteepTotal);

            for (var i = 0; i < mCantSteeps; i++)
            {
                DrawSteep(gtemp, i);
            }

            if (this.Text != string.Empty)
            {
                gtemp.TextRenderingHint = TextRenderingHint.AntiAlias;
                DrawCenterString(gtemp, this.ClientRectangle);
            }

            e.Graphics.DrawImage(ima, e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle, GraphicsUnit.Pixel);
            ima.Dispose();
            gtemp.Dispose();

        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (this.IsDisposed)
                return;

            if (this.Height < 12)
            {
                this.Height = 12;
            }

            base.OnSizeChanged(e);
            this.InvalidateBuffer(true);

        }

        private void DrawSteep(Graphics g, int number)
        {
            g.FillRectangle(mBrush1, 4 + number * (mSteepDistance + mSteepWidth), mSteepRect1.Y + 1, mSteepWidth, mSteepRect1.Height);
            g.FillRectangle(mBrush2, 4 + number * (mSteepDistance + mSteepWidth), mSteepRect2.Y + 1, mSteepWidth, mSteepRect2.Height - 1);
        }

        private void InvalidateBuffer(bool InvalidateControl = false)
        {
            if (mDobleBack != null)
            {
                mDobleBack.Dispose();
                mDobleBack = null;
            }

            if (InvalidateControl)
            {
                this.Invalidate();
            }
        }

        private void DisposeBrushes()
        {
            if (mBrush1 != null)
            {
                mBrush1.Dispose();
                mBrush1 = null;
            }

            if (mBrush2 != null)
            {
                mBrush2.Dispose();
                mBrush2 = null;
            }

        }

        private void DrawCenterString(Graphics gfx, Rectangle box)
        {
            var ss = gfx.MeasureString(this.Text, this.Font);

            var left = box.X + (box.Width - ss.Width) / 2;
            var top = box.Y + (box.Height - ss.Height) / 2;

            if (mTextShadow)
            {
                var mShadowBrush = new SolidBrush(Color.FromArgb(mTextShadowAlpha, Color.Black));
                gfx.DrawString(this.Text, this.Font, mShadowBrush, left + 1, top + 1);
                mShadowBrush.Dispose();
            }

            var mTextBrush = new SolidBrush(mColorText);
            gfx.DrawString(this.Text, this.Font, mTextBrush, left, top);
            mTextBrush.Dispose();

        }

        private void CreatePaintElements()
        {
            DisposeBrushes();

            switch (mGradientStyle)
            {
                case GradientMode.VerticalCenter:

                    mSteepRect1 = new Rectangle(
                        0,
                        2,
                        mSteepWidth,
                        this.Height / 2 + (int) (this.Height * 0.05));
                    mBrush1 = new LinearGradientBrush(mSteepRect1, mColor1, mColor2, LinearGradientMode.Vertical);

                    mSteepRect2 = new Rectangle(
                        0,
                        mSteepRect1.Bottom - 1,
                        mSteepWidth,
                        this.Height - mSteepRect1.Height - 4);
                    mBrush2 = new LinearGradientBrush(mSteepRect2, mColor2, mColor1, LinearGradientMode.Vertical);
                    break;

                case GradientMode.Vertical:
                    mSteepRect1 = new Rectangle(
                        0,
                        3,
                        mSteepWidth,
                        this.Height - 7);
                    mBrush1 = new LinearGradientBrush(mSteepRect1, mColor1, mColor2, LinearGradientMode.Vertical);
                    mSteepRect2 = new Rectangle(
                        -100,
                        -100,
                        1,
                        1);
                    mBrush2 = new LinearGradientBrush(mSteepRect2, mColor2, mColor1, LinearGradientMode.Horizontal);
                    break;


                case GradientMode.Horizontal:
                    mSteepRect1 = new Rectangle(
                        0,
                        3,
                        mSteepWidth,
                        this.Height - 7);

                    //					mBrush1 = new LinearGradientBrush(rTemp, mColor1, mColor2, LinearGradientMode.Horizontal);
                    mBrush1 = new LinearGradientBrush(this.ClientRectangle, mColor1, mColor2, LinearGradientMode.Horizontal);
                    mSteepRect2 = new Rectangle(
                        -100,
                        -100,
                        1,
                        1);
                    mBrush2 = new LinearGradientBrush(mSteepRect2, Color.Red, Color.Red, LinearGradientMode.Horizontal);
                    break;


                case GradientMode.HorizontalCenter:
                    mSteepRect1 = new Rectangle(
                        0,
                        3,
                        mSteepWidth,
                        this.Height - 7);
                    //					mBrush1 = new LinearGradientBrush(rTemp, mColor1, mColor2, LinearGradientMode.Horizontal);
                    mBrush1 = new LinearGradientBrush(this.ClientRectangle, mColor1, mColor2, LinearGradientMode.Horizontal);
                    mBrush1.SetBlendTriangularShape(0.5f);

                    mSteepRect2 = new Rectangle(
                        -100,
                        -100,
                        1,
                        1);
                    mBrush2 = new LinearGradientBrush(mSteepRect2, Color.Red, Color.Red, LinearGradientMode.Horizontal);
                    break;


                case GradientMode.Diagonal:
                    mSteepRect1 = new Rectangle(
                        0,
                        3,
                        mSteepWidth,
                        this.Height - 7);
                    //					mBrush1 = new LinearGradientBrush(rTemp, mColor1, mColor2, LinearGradientMode.ForwardDiagonal);
                    mBrush1 = new LinearGradientBrush(this.ClientRectangle, mColor1, mColor2, LinearGradientMode.ForwardDiagonal);
                    //					((LinearGradientBrush) mBrush1).SetBlendTriangularShape(0.5f);

                    mSteepRect2 = new Rectangle(
                        -100,
                        -100,
                        1,
                        1);
                    mBrush2 = new LinearGradientBrush(mSteepRect2, Color.Red, Color.Red, LinearGradientMode.Horizontal);
                    break;

                default:
                    mBrush1 = new LinearGradientBrush(mSteepRect1, mColor1, mColor2, LinearGradientMode.Vertical);
                    mBrush2 = new LinearGradientBrush(mSteepRect2, mColor2, mColor1, LinearGradientMode.Vertical);
                    break;

            }

            innerRect = new Rectangle(
                this.ClientRectangle.X + 2,
                this.ClientRectangle.Y + 2,
                this.ClientRectangle.Width - 4,
                this.ClientRectangle.Height - 4);
            outnnerRect = new Rectangle(
                this.ClientRectangle.X,
                this.ClientRectangle.Y,
                this.ClientRectangle.Width - 1,
                this.ClientRectangle.Height - 1);
            outnnerRect2 = new Rectangle(
                this.ClientRectangle.X + 1,
                this.ClientRectangle.Y + 1,
                this.ClientRectangle.Width,
                this.ClientRectangle.Height);

        }

        protected override void Dispose(bool disposing)
        {
            if (this.IsDisposed)
                return;

            mDobleBack?.Dispose();
            mBrush1?.Dispose();
            mBrush2?.Dispose();
            base.Dispose(disposing);
        }
    }
}
