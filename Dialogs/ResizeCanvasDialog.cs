using System;
using System.Windows.Forms;

namespace Texel.Dialogs
{
    public partial class ResizeCanvasDialog : Form
    {
        public int NewWidth { get; private set; }
        public int NewHeight { get; private set; }
        
        public ResizeCanvasDialog(int currentWidth, int currentHeight)
        {
            InitializeComponent();
            
            // Set current dimensions
            numWidth.Value = currentWidth;
            numHeight.Value = currentHeight;
            
            NewWidth = currentWidth;
            NewHeight = currentHeight;
        }
        
        private void btnOK_Click(object sender, EventArgs e)
        {
            NewWidth = (int)numWidth.Value;
            NewHeight = (int)numHeight.Value;
            
            DialogResult = DialogResult.OK;
            Close();
        }
        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        
        private void chkSquare_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSquare.Checked)
            {
                // Make height match width
                numHeight.Value = numWidth.Value;
                numHeight.Enabled = false;
            }
            else
            {
                numHeight.Enabled = true;
            }
        }
        
        private void numWidth_ValueChanged(object sender, EventArgs e)
        {
            if (chkSquare.Checked)
            {
                numHeight.Value = numWidth.Value;
            }
        }
        
        private void btnPowerOfTwo_Click(object sender, EventArgs e)
        {
            // Find the next power of two for width and height
            int powerWidth = FindNextPowerOfTwo((int)numWidth.Value);
            int powerHeight = chkSquare.Checked ? powerWidth : FindNextPowerOfTwo((int)numHeight.Value);
            
            numWidth.Value = powerWidth;
            numHeight.Value = powerHeight;
        }
        
        private int FindNextPowerOfTwo(int value)
        {
            int power = 1;
            while (power < value)
            {
                power *= 2;
            }
            return power;
        }
    }
}
