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
        private readonly char DecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.ToCharArray()[0];
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
                if (Keys.Back == eModified)
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

            if (e.KeyChar == DecimalSeparator)
            {
                e.Handled = true;
            }
            if ((Keys.Delete != eModified) && (Keys.Back != eModified))
            {
                if (Keys.Space != eModified)
                {
                    if (e.KeyChar != '-')
                    {
                        if (!char.IsLetterOrDigit(e.KeyChar))
                        {
                            e.Handled = true;
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
                }
                else
                {
                    if (this.TextLength == 0)
                    {
                        e.Handled = true;
                        adminPassword.Clear();
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
    }
}
