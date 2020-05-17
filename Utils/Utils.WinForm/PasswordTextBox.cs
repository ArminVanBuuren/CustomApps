using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Utils.WinForm
{
    /// <summary>
    /// 
    /// </summary>
    public class PasswordTextBox : TextBox
    {
        private readonly Timer timer;
        private readonly List<char> adminPassword;
	    //private readonly char DecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.ToCharArray()[0];
        private int m_iCaretPosition = 0;
        private bool canEdit = true;

        public int HideCharIntervalMsec => timer.Interval;

        /// <summary>
        /// 
        /// </summary>
        public PasswordTextBox()
        {
	        adminPassword = new List<char>(8);
            timer = new Timer { Interval = 1000 };
            timer.Tick += timer_Tick;
        }

        /// <summary>
        /// 
        /// </summary>
        public string AdminPassword => string.Join("", adminPassword).Trim('\0').Replace("\0", "");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTextChanged(EventArgs e)
        {
            if (canEdit)
            {
                base.OnTextChanged(e);
                txtInput_TextChanged(this, e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtInput_TextChanged(object sender, EventArgs e)
        {
            HidePasswordCharacters();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            m_iCaretPosition = this.GetCharIndexFromPosition(e.Location);
        }

        /// <summary>
        /// 
        /// </summary>
        private void HidePasswordCharacters()
        {
            var index = this.SelectionStart;

            if (index > 1)
            {
	            var s = new StringBuilder(this.Text)
	            {
		            [index - 2] = '*'
	            };
	            this.Text = s.ToString();
                this.SelectionStart = index;
                m_iCaretPosition = index;
            }
            timer.Enabled = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Delete)
            {
	            DeleteSelectedCharacters(this, e.KeyCode);
            }
        }

        /// <summary>
        /// Windows Timer elapsed eventhandler 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            timer.Enabled = false;
            var index = this.SelectionStart;

            if (index >= 1)
            {
	            var s = new StringBuilder(this.Text)
	            {
		            [index - 1] = '*'
	            };
	            this.SafeInvoke(new Action(() =>
                {
                    this.Text = s.ToString();
                    this.SelectionStart = index;
                    m_iCaretPosition = index;
                }));
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            var selectionStart = this.SelectionStart;
            var length = this.TextLength;
            var selectedChars = this.SelectionLength;
            var eModified = (Keys)e.KeyChar;
            canEdit = false;

            if (selectedChars == length)
            {
                /*
                 * Means complete text selected so clear it before using it
                 */
                if (eModified == Keys.Back)
                {
	                canEdit = true;
	                ClearCharBufferPlusTextBox();
                    return;
                }
                else
                {
	                ClearCharBufferPlusTextBox();
                }
            }

            //if (e.KeyChar == DecimalSeparator)
            //{
            //    e.Handled = true;
            //}
            if (IgnoreKey(eModified) || eModified == (Keys.RButton | Keys.Capital))
            {
	            e.Handled = true;
            }
            else if ((Keys.Delete != eModified) && (Keys.Back != eModified))
            {
	            if (Keys.Space == eModified && this.TextLength == 0)
	            {
		            e.Handled = true;
		            adminPassword.Clear();
                }
                else
                {
	                if (adminPassword.Count > selectionStart)
	                {
		                adminPassword.Insert(selectionStart, e.KeyChar);
	                }
	                else
	                {
		                adminPassword.Add(e.KeyChar);
	                }
                }
            }
            else if ((Keys.Back == eModified) || (Keys.Delete == eModified))
            {
                DeleteSelectedCharacters(this, eModified);
            }

            /*
             * Replace the characters with '*'
             */
            HidePasswordCharacters();

            canEdit = true;
        }

        /// <summary>
        /// Deletes the specific characters in the char array based on the key press action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="key"></param>
        private void DeleteSelectedCharacters(object sender, Keys key)
        {
	        try
	        {
                var selectionStart = this.SelectionStart;
                var length = this.TextLength;
                var selectedChars = this.SelectionLength;

                if (selectedChars == length)
                {
                    ClearCharBufferPlusTextBox();
                    return;
                }

                if (selectedChars > 0)
                {
                    var i = selectionStart;
                    this.Text.Remove(selectionStart, selectedChars);
                    adminPassword.RemoveRange(selectionStart, selectedChars);
                }
                else
                {
                    /*
                     * Basically this portion of code is to handle the condition 
                     * when the cursor is placed at the start or in the end 
                     */
                    if (selectionStart == 0)
                    {
                        /*
                        * Cursor in the beginning, before the first character 
                        * Delete the character only when Del is pressed, No action when Back key is pressed
                        */
                        if (key == Keys.Delete)
                        {
                            adminPassword.RemoveAt(0);
                        }
                    }
                    else if (selectionStart > 0 && selectionStart < length)
                    {
                        /*
                        * Cursor position anywhere in between 
                        * Backspace and Delete have the same effect
                        */
                        if (key == Keys.Delete)
                        {
                            adminPassword.RemoveAt(selectionStart);
                        }
                        else if (key == Keys.Back)
                        {
                            adminPassword.RemoveAt(selectionStart - 1);
                        }
                    }
                    else if (selectionStart == length)
                    {
                        /*
                        * Cursor at the end, after the last character 
                        * Delete the character only when Back key is pressed, No action when Delete key is pressed
                        */
                        if (key == Keys.Back)
                        {
                            adminPassword.RemoveAt(selectionStart - 1);
                        }
                    }
                }

                this.Select((selectionStart > this.Text.Length ? this.Text.Length : selectionStart), 0);
            }
	        catch (Exception)
	        {
		        // ignored
	        }
        }

        private void ClearCharBufferPlusTextBox()
        {
	        adminPassword.Clear();
            this.Clear();
        }

        static bool IgnoreKey(Keys key)
        {
	        switch (key)
	        {
                case Keys.KeyCode: return true;
                case Keys.Modifiers: return true;
                case Keys.None: return true;
                case Keys.LButton: return true;
                case Keys.RButton: return true;
                case Keys.Cancel: return true;
                case Keys.MButton: return true;
                case Keys.XButton1: return true;
                case Keys.XButton2: return true;
                case Keys.Tab: return true;
                case Keys.LineFeed: return true;
                case Keys.Clear: return true;
                case Keys.Enter: return true;
                case Keys.ShiftKey: return true;
                case Keys.ControlKey: return true;
                case Keys.Menu: return true;
                case Keys.Pause: return true;
                case Keys.CapsLock: return true;
                case Keys.KanaMode: return true;
                case Keys.JunjaMode: return true;
                case Keys.FinalMode: return true;
                case Keys.KanjiMode: return true;
                case Keys.Escape: return true;
                case Keys.IMEConvert: return true;
                case Keys.IMENonconvert: return true;
                case Keys.IMEAccept: return true;
                case Keys.IMEModeChange: return true;
                case Keys.NumLock: return true;
                case Keys.Scroll: return true;
                case Keys.LShiftKey: return true;
                case Keys.RShiftKey: return true;
                case Keys.LControlKey: return true;
                case Keys.RControlKey: return true;
                case Keys.LMenu: return true;
                case Keys.RMenu: return true;
                case Keys.BrowserBack: return true;
                case Keys.BrowserForward: return true;
                case Keys.BrowserRefresh: return true;
                case Keys.BrowserStop: return true;
                case Keys.BrowserSearch: return true;
                case Keys.BrowserFavorites: return true;
                case Keys.BrowserHome: return true;
                case Keys.VolumeMute: return true;
                case Keys.VolumeDown: return true;
                case Keys.VolumeUp: return true;
                case Keys.MediaNextTrack: return true;
                case Keys.MediaPreviousTrack: return true;
                case Keys.MediaStop: return true;
                case Keys.MediaPlayPause: return true;
                case Keys.LaunchMail: return true;
                case Keys.SelectMedia: return true;
                case Keys.LaunchApplication1: return true;
                case Keys.LaunchApplication2: return true;
                case Keys.OemSemicolon: return true;
                case Keys.Oemplus: return true;
                case Keys.Oemcomma: return true;
                case Keys.OemMinus: return true;
                case Keys.OemPeriod: return true;
                case Keys.OemQuestion: return true;
                case Keys.Oemtilde: return true;
                case Keys.OemOpenBrackets: return true;
                case Keys.OemPipe: return true;
                case Keys.OemCloseBrackets: return true;
                case Keys.OemQuotes: return true;
                case Keys.Oem8: return true;
                case Keys.OemBackslash: return true;
                case Keys.ProcessKey: return true;
                case Keys.Packet: return true;
                case Keys.Attn: return true;
                case Keys.Crsel: return true;
                case Keys.Exsel: return true;
                case Keys.EraseEof: return true;
                case Keys.Play: return true;
                case Keys.Zoom: return true;
                case Keys.NoName: return true;
                case Keys.Pa1: return true;
                case Keys.OemClear: return true;
                case Keys.Shift: return true;
                case Keys.Control: return true;
                case Keys.Alt: return true;
            }

	        return false;
        }
    }
}
