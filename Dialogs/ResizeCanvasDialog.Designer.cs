namespace Texel.Dialogs
{
    partial class ResizeCanvasDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.numWidth = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numHeight = new System.Windows.Forms.NumericUpDown();
            this.chkSquare = new System.Windows.Forms.CheckBox();
            this.btnPowerOfTwo = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeight)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Width:";
            // 
            // numWidth
            // 
            this.numWidth.Location = new System.Drawing.Point(76, 13);
            this.numWidth.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numWidth.Name = "numWidth";
            this.numWidth.Size = new System.Drawing.Size(84, 20);
            this.numWidth.TabIndex = 1;
            this.numWidth.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.numWidth.ValueChanged += new System.EventHandler(this.numWidth_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Height:";
            // 
            // numHeight
            // 
            this.numHeight.Location = new System.Drawing.Point(76, 39);
            this.numHeight.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numHeight.Name = "numHeight";
            this.numHeight.Size = new System.Drawing.Size(84, 20);
            this.numHeight.TabIndex = 3;
            this.numHeight.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
            // 
            // chkSquare
            // 
            this.chkSquare.AutoSize = true;
            this.chkSquare.Location = new System.Drawing.Point(76, 65);
            this.chkSquare.Name = "chkSquare";
            this.chkSquare.Size = new System.Drawing.Size(86, 17);
            this.chkSquare.TabIndex = 4;
            this.chkSquare.Text = "Keep Square";
            this.chkSquare.UseVisualStyleBackColor = true;
            this.chkSquare.CheckedChanged += new System.EventHandler(this.chkSquare_CheckedChanged);
            // 
            // btnPowerOfTwo
            // 
            this.btnPowerOfTwo.Location = new System.Drawing.Point(175, 13);
            this.btnPowerOfTwo.Name = "btnPowerOfTwo";
            this.btnPowerOfTwo.Size = new System.Drawing.Size(88, 46);
            this.btnPowerOfTwo.TabIndex = 5;
            this.btnPowerOfTwo.Text = "Power of Two";
            this.btnPowerOfTwo.UseVisualStyleBackColor = true;
            this.btnPowerOfTwo.Click += new System.EventHandler(this.btnPowerOfTwo_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(107, 94);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(188, 94);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ResizeCanvasDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(275, 129);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnPowerOfTwo);
            this.Controls.Add(this.chkSquare);
            this.Controls.Add(this.numHeight);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numWidth);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ResizeCanvasDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Resize Canvas";
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numWidth;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numHeight;
        private System.Windows.Forms.CheckBox chkSquare;
        private System.Windows.Forms.Button btnPowerOfTwo;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}