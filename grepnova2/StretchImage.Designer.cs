namespace grepnova2
{
    partial class StretchImage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StretchImage));
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.btnResetMinMax = new System.Windows.Forms.Button();
            this.lblMax = new System.Windows.Forms.TextBox();
            this.lblMin = new System.Windows.Forms.TextBox();
            this.cookHistImage = new System.Windows.Forms.PictureBox();
            this.selectionRangeSlider1 = new grepnova2.SelectionRangeSlider();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cookHistImage)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.selectionRangeSlider1);
            this.groupBox7.Controls.Add(this.btnResetMinMax);
            this.groupBox7.Controls.Add(this.lblMax);
            this.groupBox7.Controls.Add(this.lblMin);
            this.groupBox7.Controls.Add(this.cookHistImage);
            this.groupBox7.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.groupBox7.Location = new System.Drawing.Point(9, 3);
            this.groupBox7.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox7.Size = new System.Drawing.Size(187, 160);
            this.groupBox7.TabIndex = 53;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Stretch Image";
            // 
            // btnResetMinMax
            // 
            this.btnResetMinMax.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
            this.btnResetMinMax.Font = new System.Drawing.Font("Calibri", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.btnResetMinMax.Location = new System.Drawing.Point(89, 130);
            this.btnResetMinMax.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnResetMinMax.Name = "btnResetMinMax";
            this.btnResetMinMax.Size = new System.Drawing.Size(18, 20);
            this.btnResetMinMax.TabIndex = 55;
            this.btnResetMinMax.Text = "R";
            this.btnResetMinMax.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btnResetMinMax.UseVisualStyleBackColor = true;
            this.btnResetMinMax.Click += new System.EventHandler(this.btnResetMinMax_Click);
            // 
            // lblMax
            // 
            this.lblMax.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblMax.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.lblMax.ForeColor = System.Drawing.Color.Green;
            this.lblMax.Location = new System.Drawing.Point(132, 132);
            this.lblMax.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.lblMax.Name = "lblMax";
            this.lblMax.Size = new System.Drawing.Size(34, 15);
            this.lblMax.TabIndex = 50;
            this.lblMax.Text = "0";
            this.lblMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.lblMax.WordWrap = false;
            this.lblMax.TextChanged += new System.EventHandler(this.lblMax_TextChanged);
            // 
            // lblMin
            // 
            this.lblMin.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblMin.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.lblMin.ForeColor = System.Drawing.Color.Red;
            this.lblMin.Location = new System.Drawing.Point(19, 132);
            this.lblMin.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.lblMin.Name = "lblMin";
            this.lblMin.Size = new System.Drawing.Size(34, 15);
            this.lblMin.TabIndex = 49;
            this.lblMin.Text = "0";
            this.lblMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.lblMin.WordWrap = false;
            this.lblMin.TextChanged += new System.EventHandler(this.lblMin_TextChanged);
            // 
            // cookHistImage
            // 
            this.cookHistImage.BackColor = System.Drawing.Color.White;
            this.cookHistImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cookHistImage.Location = new System.Drawing.Point(19, 20);
            this.cookHistImage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cookHistImage.Name = "cookHistImage";
            this.cookHistImage.Size = new System.Drawing.Size(148, 89);
            this.cookHistImage.TabIndex = 40;
            this.cookHistImage.TabStop = false;
            // 
            // selectionRangeSlider1
            // 
            this.selectionRangeSlider1.Font = new System.Drawing.Font("Calibri", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.selectionRangeSlider1.Location = new System.Drawing.Point(12, 109);
            this.selectionRangeSlider1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.selectionRangeSlider1.Max = 100;
            this.selectionRangeSlider1.Min = 0;
            this.selectionRangeSlider1.Name = "selectionRangeSlider1";
            this.selectionRangeSlider1.Offset = 10;
            this.selectionRangeSlider1.SelectedMax = 80;
            this.selectionRangeSlider1.SelectedMin = 20;
            this.selectionRangeSlider1.Size = new System.Drawing.Size(161, 10);
            this.selectionRangeSlider1.TabIndex = 56;
            this.selectionRangeSlider1.Value = 10;
            this.selectionRangeSlider1.SelectionChanged += new System.EventHandler(this.selectionRangeSlider1_SelectionChanged);
            // 
            // StretchImage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(204, 165);
            this.Controls.Add(this.groupBox7);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "StretchImage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Stretch Image";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StretchImage_FormClosing);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cookHistImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox7;
        private SelectionRangeSlider selectionRangeSlider1;
        private System.Windows.Forms.Button btnResetMinMax;
        private System.Windows.Forms.TextBox lblMax;
        private System.Windows.Forms.TextBox lblMin;
        private System.Windows.Forms.PictureBox cookHistImage;
    }
}