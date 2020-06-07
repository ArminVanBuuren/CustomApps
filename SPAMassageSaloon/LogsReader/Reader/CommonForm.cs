using System;
using System.Windows.Forms;
using SPAMassageSaloon.Common;

namespace LogsReader.Reader
{
    public sealed partial class CommonForm : UserControl, IUserForm
    {
	    public CommonForm()
        {
	        InitializeComponent();
        }

        public void ApplySettings()
        {
            
        }

        public void SaveData()
        {

        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {

        }

        private async void ButtonExport_Click(object sender, EventArgs e)
        {
        }

        private async void buttonFilter_Click(object sender, EventArgs e)
        {

        }

        private async void buttonReset_Click(object sender, EventArgs e)
        {
            
        }

        private void DgvFiles_SelectionChanged(object sender, EventArgs e)
        {
            
        }

        private void DgvFiles_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void OrderByText_Leave(object sender, EventArgs e)
        {

        }

        private void TxtPattern_TextChanged(object sender, EventArgs e)
        {

        }

        private void UseRegex_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void TraceNameFilter_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void traceNameFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void TraceMessageFilter_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void traceMessageFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            
        }
    }
}