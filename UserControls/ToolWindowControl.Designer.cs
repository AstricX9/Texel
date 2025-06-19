namespace Texel
{
    partial class ToolWindowControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnPen = new System.Windows.Forms.Button();
            this.btnFill = new System.Windows.Forms.Button();
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnRectangle = new System.Windows.Forms.Button();
            this.btnEllipse = new System.Windows.Forms.Button();
            this.btnEraser = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // btnPen
            // 
            this.btnPen.Location = new System.Drawing.Point(3, 3);
            this.btnPen.Name = "btnPen";
            this.btnPen.Size = new System.Drawing.Size(63, 23);
            this.btnPen.TabIndex = 0;
            this.btnPen.Tag = "Pen";
            this.btnPen.Text = "Pen";
            this.btnPen.UseVisualStyleBackColor = true;
            this.flowLayoutPanel1.Controls.Add(this.btnPen);
            // 
            // btnFill
            // 
            this.btnFill.Location = new System.Drawing.Point(3, 32);
            this.btnFill.Name = "btnFill";
            this.btnFill.Size = new System.Drawing.Size(63, 23);
            this.btnFill.TabIndex = 1;
            this.btnFill.Tag = "Fill";
            this.btnFill.Text = "Fill";
            this.btnFill.UseVisualStyleBackColor = true;
            this.flowLayoutPanel1.Controls.Add(this.btnFill);
            // 
            // btnSelect
            // 
            this.btnSelect.Location = new System.Drawing.Point(3, 61);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(63, 23);
            this.btnSelect.TabIndex = 2;
            this.btnSelect.Tag = "Select";
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.flowLayoutPanel1.Controls.Add(this.btnSelect);
            // 
            // btnRectangle
            // 
            this.btnRectangle.Location = new System.Drawing.Point(3, 90);
            this.btnRectangle.Name = "btnRectangle";
            this.btnRectangle.Size = new System.Drawing.Size(63, 23);
            this.btnRectangle.TabIndex = 3;
            this.btnRectangle.Tag = "Rectangle";
            this.btnRectangle.Text = "Rectangle";
            this.btnRectangle.UseVisualStyleBackColor = true;
            this.flowLayoutPanel1.Controls.Add(this.btnRectangle);
            // 
            // btnEllipse
            // 
            this.btnEllipse.Location = new System.Drawing.Point(3, 119);
            this.btnEllipse.Name = "btnEllipse";
            this.btnEllipse.Size = new System.Drawing.Size(63, 23);
            this.btnEllipse.TabIndex = 4;
            this.btnEllipse.Tag = "Ellipse";
            this.btnEllipse.Text = "Ellipse";
            this.btnEllipse.UseVisualStyleBackColor = true;
            this.flowLayoutPanel1.Controls.Add(this.btnEllipse);
            // 
            // btnEraser
            // 
            this.btnEraser.Location = new System.Drawing.Point(3, 148);
            this.btnEraser.Name = "btnEraser";
            this.btnEraser.Size = new System.Drawing.Size(60, 30);
            this.btnEraser.TabIndex = 5;
            this.btnEraser.Tag = ToolMode.Eraser;
            this.btnEraser.Text = "Eraser";
            this.btnEraser.UseVisualStyleBackColor = true;
            this.flowLayoutPanel1.Controls.Add(this.btnEraser);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(69, 368);
            this.flowLayoutPanel1.TabIndex = 6;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // ToolWindowControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "ToolWindowControl";
            this.Size = new System.Drawing.Size(69, 368);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnPen;
        private System.Windows.Forms.Button btnFill;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnRectangle;
        private System.Windows.Forms.Button btnEllipse;
        private System.Windows.Forms.Button btnEraser;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
