using System;
using System.Drawing;
using System.Windows.Forms;

namespace Texel
{
    public partial class ToolWindowControl : UserControl
    {
        public event Action<ToolMode> ToolChanged;

        public ToolWindowControl()
        {
            InitializeComponent();
            foreach (Control ctrl in flowLayoutPanel1.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.Click += ToolButton_Click;
                }
            }
        }

        private void ToolButton_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && Enum.TryParse(btn.Tag.ToString(), out ToolMode tool))
            {
                HighlightSelected(btn);
                ToolChanged?.Invoke(tool);
            }
        }

        private void HighlightSelected(Button selected)
        {
            foreach (Control ctrl in flowLayoutPanel1.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.BackColor = btn == selected ? Color.LightBlue : SystemColors.Control;
                }
            }
        }
    }
}
