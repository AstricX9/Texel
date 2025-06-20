using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Texel.Dialogs
{
    public partial class TextInputDialog : Form
    {
        public string InputText { get; private set; }
        
        public TextInputDialog(string title, string prompt)
        {
            InitializeComponent();
            
            this.Text = title;
            lblPrompt.Text = prompt;
            InputText = string.Empty;
        }
        
        private void btnOK_Click(object sender, EventArgs e)
        {
            InputText = txtInput.Text;
            DialogResult = DialogResult.OK;
            Close();
        }
        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
