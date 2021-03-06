using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NAudio.Gui
{
    /// <summary>
    /// Windows Forms control for painting audio waveforms
    /// </summary>
    public class WaveformPainter : Control
    {
        Pen foregroundPen;
        List<float> samples = new List<float>(1000);
        int maxSamples;
        int insertPos;

        /// <summary>
        /// Constructs a new instance of the WaveFormPainter class
        /// </summary>
        public WaveformPainter()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponent();
            OnForeColorChanged(EventArgs.Empty);
            OnResize(EventArgs.Empty);
        }

        /// <summary>
        /// On Resize
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            maxSamples = this.Width;
            base.OnResize(e);
        }

        /// <summary>
        /// On ForeColor Changed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnForeColorChanged(EventArgs e)
        {
            foregroundPen = new Pen(ForeColor);
            base.OnForeColorChanged(e);
        }

        /// <summary>
        /// Add Max Value
        /// </summary>
        /// <param name="maxSample"></param>
        public void AddMax(float maxSample)
        {
            if (maxSamples == 0)
            {
                // sometimes when you minimise, max samples can be set to 0
                return;
            }
            if (samples.Count <= maxSamples)
            {
                samples.Add(maxSample);
            }
            else if (insertPos < maxSamples)
            {
                samples[insertPos] = maxSample;
            }
            insertPos++;
            insertPos %= maxSamples;
            
            this.Invalidate();
        }

        /// <summary>
        /// On Paint
        /// </summary>
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            for (var x = 0; x < this.Width; x++)
            {
                var lineHeight = this.Height * GetSample(x - this.Width + insertPos);
                var y1 = (this.Height - lineHeight) / 2;
                pe.Graphics.DrawLine(foregroundPen, x, y1, x, y1 + lineHeight);
            }
        }

        float GetSample(int index)
        {
            if (index < 0)
                index += maxSamples;
            if (index >= 0 & index < samples.Count)
                return samples[index];
            return 0;
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
	        if (disposing && (components != null))
	        {
		        components.Dispose();
	        }
	        base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
	        components = new System.ComponentModel.Container();
        }

        #endregion
    }
}
