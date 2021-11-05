using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Utils.WinForm.Expander
{
    /// <summary>
    /// The ExpandCollapsePanel control displays a header that has a collapsible window that displays content.
    /// </summary>
    [Designer(typeof(ExpandCollapsePanelDesigner))]
    public partial class ExpandCollapsePanel : Panel
    {
	    private readonly Dictionary<Control, bool> _controlsDict = new Dictionary<Control, bool>();

        /// <summary>
        /// Last stored size of panel's parent control
        /// <remarks>used for handling panel's Anchor property sets to Bottom when panel collapsed
        /// in OnSizeChanged method</remarks>
        /// </summary>
        private Size _previousParentSize = Size.Empty;
        private int _firstpanelHeaderWidth = 0;

        private int _collapsedHeight;
        /// <summary>
        /// Height of panel in expanded state
        /// </summary>
        private int _expandedHeight;

        /// <summary>
        /// Occurs when the panel has expanded or collapsed
        /// </summary>
        [Category("ExpandCollapsePanel")]
        [Description("Occurs when the panel has expanded or collapsed.")]
        [Browsable(true)]
        public event EventHandler<ExpandCollapseEventArgs> ExpandCollapse;

        /// <summary>
        /// Occurs when the button has expanded or collapsed
        /// </summary>
        [Category("ExpandCollapsePanelCheckedChanged")]
        [Description("Occurs when the button has CheckBox check changed.")]
        [Browsable(true)]
        public event EventHandler<ExpandCollapseEventArgs> CheckedChanged;

        /// <summary>
        /// Height of panel in expanded state
        /// </summary>
        [Category("ExpandCollapsePanel")]
        [Description("Height of panel in collapsed state.")]
        [Browsable(true)]
        public int CollapsedHeight
        {
	        get => _collapsedHeight + (BordersThickness * 2) + Padding.Top + Padding.Bottom;
	        private set => _collapsedHeight = value;
        }

        /// <summary>
        /// Height of panel in expanded state
        /// </summary>
        [Category("ExpandCollapsePanel")]
        [Description("Height of panel in expanded state.")]
        [Browsable(true)]
        public int ExpandedHeight
        {
            get => _expandedHeight;
            set
            {
                _expandedHeight = value;
                if (IsExpanded)
                {
                    Height = _expandedHeight;
                }
            }
        }

        /// <summary>
        /// Set flag for expand or collapse panel content
        /// (true - expanded, false - collapsed)
        /// </summary>
        [Category("ExpandCollapsePanel")]
        [Description("Expand or collapse panel content.\r\n" +
                     "Attention, for correct work with resizing child controls," +
                     " please set IsExpanded to \"false\" in code (for example in your Form class constructor after InitializeComponent method) and not in Forms Designer!")]
        [Browsable(true)]
        public bool IsExpanded
        {
            get => _btnExpandCollapse.IsExpanded;
            set
            {
	            if(_btnExpandCollapse.IsExpanded != value)
                    _btnExpandCollapse.IsExpanded = value;

	            Size = new Size(Size.Width, value ?  _expandedHeight : CollapsedHeight);
            }
        }

        /// <summary>
        /// Header of panel
        /// </summary>
        [Category("ExpandCollapsePanel")]
        [Description("Header of panel.")]
        [Browsable(true)]
        public override string Text
        {
            get => _btnExpandCollapse.Text;
            set => _btnExpandCollapse.Text = value;
        }

        /// <summary>
        /// CheckBox
        /// </summary>
        [Category("ExpandCollapsePanel")]
        [Description("CheckBox is shown in panel")]
        [Browsable(true)]
        public bool CheckBoxShown
        {
	        get => _btnExpandCollapse.CheckBoxShown;
	        set => _btnExpandCollapse.CheckBoxShown = value;
        }

        /// <summary>
        /// CheckBox
        /// </summary>
        [Category("ExpandCollapsePanel")]
        [Description("CheckBox is enabled in panel")]
        [Browsable(true)]
        public bool CheckBoxEnabled
        {
	        get => _btnExpandCollapse.CheckBoxEnabled;
	        set => _btnExpandCollapse.CheckBoxEnabled = value;
        }

        /// <summary>
        /// CheckBox
        /// </summary>
        [Category("ExpandCollapsePanel")]
        [Description("CheckBox is checked")]
        [Browsable(true)]
        public bool IsChecked
        {
	        get => _btnExpandCollapse.IsChecked;
	        set => _btnExpandCollapse.IsChecked = value;
        }

        /// <summary>
        /// Enable pretty simple animation of panel on expanding or collapsing
        /// </summary>
        [Category("ExpandCollapsePanel")]
        [Description("Enable pretty simple animation of panel on expanding or collapsing.")]
        [Browsable(true)]
        public bool UseAnimation { get; set; }

        /// <summary>
        /// Visual style of the expand-collapse button.
        /// </summary>
        [Category("ExpandCollapsePanel")]
        [Description("Visual style of the expand-collapse button.")]
        [Browsable(true)]
        public ExpandButtonStyle ButtonStyle
        {
            get => _btnExpandCollapse.ButtonStyle;
            set => _btnExpandCollapse.ButtonStyle = value;
        }

        /// <summary>
        /// Size preset of the expand-collapse button.
        /// </summary>
        [Category("ExpandCollapsePanel")]
        [Description("Size preset of the expand-collapse button.")]
        [Browsable(true)]
        public ExpandButtonSize ButtonSize
        {
            get => _btnExpandCollapse.ButtonSize;
            set => _btnExpandCollapse.ButtonSize = value;
        }

        /// <summary>
        /// AutoScroll property
        /// <remarks>Overridden only to hide from designer as mindless and useless</remarks>
        /// </summary>
        [Browsable(false)]
        public override bool AutoScroll
        {
            get => base.AutoScroll;
            set => base.AutoScroll = value;
        }

        /// <summary>
        /// Font used for displays header text
        /// </summary>
        public override Font Font
        {
            get => _btnExpandCollapse.Font;
            set => _btnExpandCollapse.Font = value;
        }

        /// <summary>
        /// Foreground color used for displays header text
        /// </summary>
        public override Color ForeColor
        {
            get => _btnExpandCollapse.ForeColor;
            set => _btnExpandCollapse.ForeColor = value;
        }

        /// <summary>
        /// HeaderColor
        /// </summary>
        [Category("ExpandCollapsePanel")]
        [Description("Background color of header")]
        [Browsable(true)]
        public Color HeaderBackColor
        {
	        get => _btnExpandCollapse.HeaderBackColor;
	        set
	        {
		        if (value == Color.Transparent)
		        {
			        _btnExpandCollapse.HeaderBackColor = value;
			        panelHeader.BackColor = value;
                    return;
                }
		        _btnExpandCollapse.HeaderBackColor = value;
            }
        }

        /// <summary>
        /// HeaderBorderBrush
        /// </summary>
        [Category("ExpandCollapsePanel")]
        [Description("Header borders color")]
        [Browsable(true)]
        public Color HeaderBorderBrush
        {
	        get => panelHeader.BackColor;
	        set
	        {
		        if (HeaderBackColor == Color.Transparent)
		        {
			        panelHeader.BackColor = HeaderBackColor;
                    return;
		        }
		        panelHeader.BackColor = value;
	        }
        }

        /// <summary>
        /// BorderThinkness
        /// </summary>
        [Category("ExpandCollapsePanel")]
        [Description("Header borders thinkness")]
        [Browsable(true)]
        public int BordersThickness
        {
	        get => panelHeader.Padding.All;
	        set
	        {
		        panelHeader.Padding = new Padding(value);
		        panelHeader.Size = new Size(_firstpanelHeaderWidth + (BordersThickness * 2), CollapsedHeight - base.Padding.Top - base.Padding.Bottom);
		        Size = new Size(Size.Width, IsExpanded ? _expandedHeight : CollapsedHeight);
            }
        }

        /// <summary>
        /// Padding
        /// </summary>
        public new Padding Padding
        {
	        get => base.Padding;
	        set
	        {
		        base.Padding = value;
		        panelHeader.Size = new Size(_firstpanelHeaderWidth + (BordersThickness * 2), CollapsedHeight - base.Padding.Top - base.Padding.Bottom);
		        Size = new Size(Size.Width, IsExpanded ? _expandedHeight : CollapsedHeight);
	        }
        }

        /// <summary>
        /// HeaderLineColor
        /// </summary>
        [Category("ExpandCollapsePanel")]
        [Description("Header line color")]
        [Browsable(true)]
        public Color HeaderLineColor
        {
	        get => _btnExpandCollapse.HeaderLineColor;
	        set => _btnExpandCollapse.HeaderLineColor = value;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ExpandCollapsePanel()
        {
            InitializeComponent();

            BordersThickness = 3;

            // in spite of we always manually scale button, setting Anchor and AutoSize properties provide correct redraw of control in forms designer window
            _btnExpandCollapse.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _btnExpandCollapse.AutoSize = true;
            CollapsedHeight = _btnExpandCollapse.Size.Height;
            _btnExpandCollapse.Anchor = AnchorStyles.None;
            _btnExpandCollapse.Dock = DockStyle.Fill;
            _btnExpandCollapse.AutoSize = false;
            _firstpanelHeaderWidth = panelHeader.Size.Width;
            panelHeader.Size = new Size(_firstpanelHeaderWidth + (BordersThickness * 2), CollapsedHeight);
            _btnExpandCollapse.CheckBoxShown = false; // checkBox shown
            
            _btnExpandCollapse.CheckedChanged += (sender, args) => CheckedChanged?.Invoke(this, args);
            // subscribe for button expand-collapse state changed event
            _btnExpandCollapse.ExpandCollapse += BtnExpandCollapseExpandCollapse;

            this.ControlAdded += (sender, args) => _controlsDict.Add(args.Control, true);
            this.ControlRemoved += (sender, args) => _controlsDict.Remove(args.Control);
        }

        /// <summary>
        /// Handle button expand-collapse state changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExpandCollapseExpandCollapse(object sender, ExpandCollapseEventArgs e)
        {
            if (e.IsExpanded) // if button is expanded now
            {
                Expand(); // expand the panel
            }
            else
            {
                Collapse(); // collapse the panel
            }

            // Retrieve expand-collapse state changed event for panel
            var handler = ExpandCollapse;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Expand panel content
        /// </summary>
        protected virtual void Expand()
        {
            // if animation enabled
            if (UseAnimation)
            {
                // set internal state for Expanding
                _internalPanelState = InternalPanelState.Expanding;
                // start animation now..
                StartAnimation();
            }
            else // no animation, just expand immediately
            {
                // set internal state to Normal
                _internalPanelState = InternalPanelState.Normal;
                // resize panel
                Size = new Size(Size.Width, _expandedHeight);
            }
        }

        /// <summary>
        /// Collapse panel content
        /// </summary>
        protected virtual void Collapse()
        {
            // if panel is completely expanded (animation on expanding is ended or no animation at all) 
            // *we don't want store half-expanded panel height
            if (_internalPanelState == InternalPanelState.Normal)
            {
                // store current panel height in expanded state
                _expandedHeight = Size.Height;
            }

            // if animation enabled
            if (UseAnimation)
            {
                // set internal state for Collapsing
                _internalPanelState = InternalPanelState.Collapsing;
                // start animation now..
                StartAnimation();
            }
            else // no animation, just collapse immediately
            {
                // set internal state to Normal
                _internalPanelState = InternalPanelState.Normal;
                // resize panel
                Size = new Size(Size.Width, CollapsedHeight);
            }
        }

        /// <summary>
        /// Handle panel resize event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSizeChanged(EventArgs e)
        {
	        base.OnSizeChanged(e);

	        // ignore height changing from animation timer
	        if (_internalPanelState != InternalPanelState.Normal)
		        return;

	        if (!IsExpanded // if panel collapsed
	            && ((Anchor & AnchorStyles.Bottom) != 0) //and panel's Anchor property sets to Bottom
	            && Size.Height != CollapsedHeight // and panel height is changed (it could happens only if parent control just has resized)
	            && Parent != null) // and panel has the parent control
	        {
		        // main, calculate the parent control resize diff and add it to expandedHeight value:
		        _expandedHeight += Parent.Height - _previousParentSize.Height;

		        // reset resized height (by base.OnSizeChanged anchor.Bottom handling) to collapsedHeight value:
		        Size = new Size(Size.Width, CollapsedHeight);
	        }

	        // store previous size of parent control (however we need only height)
	        if (Parent != null)
		        _previousParentSize = Parent.Size;
        }

        //#region Animation Code
        //	---------------------------------------------------------------------------------------
        //	The original source of this animation technique was written by
        //	Daren May for his Collapsible Panel implementation which can
        //	be found here:
        //		http://www.codeproject.com/cs/miscctrl/xpgroupbox.asp
        //   
        //	Although I found that piece of code in very good XPPanel implementation by Tom Guinther:
        //		http://www.codeproject.com/Articles/7332/Full-featured-XP-Style-Collapsible-Panel
        //  I have simplified things quite a bit, nothing is fundamentally different. 
        //  So I give many thanks to both for solving this problem.
        //	---------------------------------------------------------------------------------------

        // degree to adjust the height of the panel when animating
        private int _animationHeightAdjustment = 0;
        // current opacity level
        private int _animationOpacity = 0;

        /// <summary>
        /// Initialize animation values and start the timer
        /// </summary>
        private void StartAnimation()
        {
            _animationHeightAdjustment = 1;
            _animationOpacity = 5;
            animationTimer.Interval = 50;
            animationTimer.Enabled = true;
        }

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            //	---------------------------------------------------------------
            //	Gradually reduce the interval between timer events so that the
            //	animation begins slowly and eventually accelerates to completion
            //	---------------------------------------------------------------
            if (animationTimer.Interval > 10)
            {
                animationTimer.Interval -= 10;
            }
            else
            {
                _animationHeightAdjustment += 2;
            }

            // Increase transparency as we collapse
            if ((_animationOpacity + 5) < byte.MaxValue)
            {
                _animationOpacity += 5;
            }

            var currOpacity = _animationOpacity;

            switch (_internalPanelState)
            {
                case InternalPanelState.Expanding:
                    // still room to expand?
                    if ((Height + _animationHeightAdjustment) < _expandedHeight)
                    {
                        Height += _animationHeightAdjustment;
                    }
                    else
                    {
                        // we are done so we dont want any transparency
                        currOpacity = byte.MaxValue;
                        Height = _expandedHeight;
                        _internalPanelState = InternalPanelState.Normal;
                    }
                    break;

                case InternalPanelState.Collapsing:
                    // still something to collapse
                    if ((Height - _animationHeightAdjustment) > CollapsedHeight)
                    {
                        Height -= _animationHeightAdjustment;
                        // continue decreasing opacity
                        currOpacity = byte.MaxValue - _animationOpacity;
                    }
                    else
                    {
                        // we are done so we dont want any transparency
                        currOpacity = byte.MaxValue;
                        Height = CollapsedHeight;
                        _internalPanelState = InternalPanelState.Normal;
                    }
                    break;

                default:
                    return;
            }

            // set the opacity for all the controls on the XPPanel
            SetControlsOpacity(currOpacity);

            // are we done?
            if (_internalPanelState == InternalPanelState.Normal)
            {
                animationTimer.Enabled = false;
            }

            Invalidate();
        }

        /// <summary>
        /// Changes the transparency of controls based upon the height of the XPPanel
        /// </summary>
        /// <remarks>
        /// Only used during animation
        /// </remarks>
        private void SetControlsOpacity(int opacity)
        {
	        foreach (Control c in Controls)
	        {
		        if (!c.Visible)
			        continue;

		        bool? isCanOpacity = null;
		        if (_controlsDict.TryGetValue(c, out var canOpacity))
			        isCanOpacity = canOpacity;
		        try
		        {
			        if (isCanOpacity != null && isCanOpacity == true && c.BackColor != Color.Transparent)
			        {
				        c.BackColor = Color.FromArgb(opacity, c.BackColor);
			        }
		        }
		        catch
		        {
			        if (isCanOpacity != null)
				        _controlsDict[c] = false;
		        }

		        c.ForeColor = Color.FromArgb(opacity, c.ForeColor);
	        }
        }

        /// <summary>
        /// Internal state of panel used for checking that panel is animating now
        /// </summary>
        private InternalPanelState _internalPanelState;

        /// <summary>
        /// Internal state of panel
        /// </summary>
        private enum InternalPanelState
        {
            /// <summary>
            /// No animation, completely expanded or collapsed
            /// </summary>
            Normal,
            /// <summary>
            /// Expanding animation
            /// </summary>
            Expanding,
            /// <summary>
            /// Collapsing animation
            /// </summary>
            Collapsing
        }
    }
}
