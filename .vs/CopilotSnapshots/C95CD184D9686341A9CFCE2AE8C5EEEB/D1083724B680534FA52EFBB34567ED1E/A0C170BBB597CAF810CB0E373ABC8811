using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Texel
{
    public partial class ToolWindowControl : UserControl
    {
        public event Action<ToolMode> ToolChanged;
        public ToolMode SelectedTool { get; private set; } = ToolMode.Pen;

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
            // ensure initial highlight
            var first = flowLayoutPanel1.Controls.OfType<Button>().FirstOrDefault();
            if (first != null)
            {
                HighlightSelected(first);
            }
        }

        private void ToolButton_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag != null && Enum.TryParse(btn.Tag.ToString(), out ToolMode tool))
            {
                SelectedTool = tool;
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
