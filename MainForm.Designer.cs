namespace Texel
{
    partial class MainForm
    {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolWindowControl1 = new Texel.ToolWindowControl();
            this.pixelCanvasControl1 = new Texel.PixelCanvasControl();
            this.SuspendLayout();
            // 
            // toolWindowControl1
            // 
            this.toolWindowControl1.Location = new System.Drawing.Point(12, 12);
            this.toolWindowControl1.Name = "toolWindowControl1";
            this.toolWindowControl1.Size = new System.Drawing.Size(69, 368);
            this.toolWindowControl1.TabIndex = 1;
            // 
            // pixelCanvasControl1
            // 
            this.pixelCanvasControl1.CurrentTool = Texel.ToolMode.Pen;
            this.pixelCanvasControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pixelCanvasControl1.Location = new System.Drawing.Point(0, 0);
            this.pixelCanvasControl1.Name = "pixelCanvasControl1";
            this.pixelCanvasControl1.SelectedColor = System.Drawing.Color.Black;
            this.pixelCanvasControl1.Size = new System.Drawing.Size(800, 450);
            this.pixelCanvasControl1.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.toolWindowControl1);
            this.Controls.Add(this.pixelCanvasControl1);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private PixelCanvasControl pixelCanvasControl1;
        private ToolWindowControl toolWindowControl1;
    }
}

