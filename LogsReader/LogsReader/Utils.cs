using System;
using System.Windows.Forms;

namespace LogsReader
{
    public static class Util
    {
        public static void AssignValue<T>(this Control textBox, T value, EventHandler handler)
        {
            try
            {
                textBox.TextChanged -= handler;
                textBox.Text = value.ToString();
            }
            catch (Exception)
            {
                //ignored
            }
            finally
            {
                textBox.TextChanged += handler;
            }
        }

        //public static void AssignComboBox<T>(this ComboBox combobox, T value, EventHandler textChangeHandler, EventHandler selectChangedHandler)
        //{
        //    try
        //    {
        //        //combobox.TextChanged -= textChangeHandler;
        //        //combobox.SelectedIndexChanged -= selectChangedHandler;

        //        combobox.BeginUpdate();
        //        combobox.ValueMember = value.ToString();
        //        combobox.DisplayMember = value.ToString();
        //        combobox.Text = value.ToString();
        //        combobox.EndUpdate();
        //    }
        //    catch (Exception)
        //    {
        //        //ignored
        //    }
        //    finally
        //    {
        //        //combobox.TextChanged += textChangeHandler;
        //        //combobox.SelectedIndexChanged += selectChangedHandler;
        //    }
        //}

        public static void MessageShow(string msg, string caption, bool isError = true)
        {
            MessageBox.Show(msg, caption, MessageBoxButtons.OK, isError ? MessageBoxIcon.Error : MessageBoxIcon.Asterisk);
        }
    }
}
