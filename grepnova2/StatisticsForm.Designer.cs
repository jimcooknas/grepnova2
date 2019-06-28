namespace grepnova2
{
    partial class StatisticsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StatisticsForm));
            this.lblObjectName = new System.Windows.Forms.Label();
            this.txtObjectName = new System.Windows.Forms.TextBox();
            this.txtMin = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtMax = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMean = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtMedian = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtStdDev = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtCounts = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtEntropy = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.histogram1 = new AForge.Controls.Histogram();
            this.txtLowerDec = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtUpperDec = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtMeanLD = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.chkLogarithmic = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblObjectName
            // 
            this.lblObjectName.AutoSize = true;
            this.lblObjectName.Location = new System.Drawing.Point(12, 22);
            this.lblObjectName.Name = "lblObjectName";
            this.lblObjectName.Size = new System.Drawing.Size(52, 21);
            this.lblObjectName.TabIndex = 0;
            this.lblObjectName.Text = "label1";
            // 
            // txtObjectName
            // 
            this.txtObjectName.BackColor = System.Drawing.SystemColors.Control;
            this.txtObjectName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtObjectName.Enabled = false;
            this.txtObjectName.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.txtObjectName.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.txtObjectName.Location = new System.Drawing.Point(175, 18);
            this.txtObjectName.Name = "txtObjectName";
            this.txtObjectName.ReadOnly = true;
            this.txtObjectName.Size = new System.Drawing.Size(196, 25);
            this.txtObjectName.TabIndex = 1;
            this.txtObjectName.WordWrap = false;
            // 
            // txtMin
            // 
            this.txtMin.BackColor = System.Drawing.Color.White;
            this.txtMin.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.txtMin.Location = new System.Drawing.Point(175, 101);
            this.txtMin.Name = "txtMin";
            this.txtMin.ReadOnly = true;
            this.txtMin.Size = new System.Drawing.Size(84, 28);
            this.txtMin.TabIndex = 3;
            this.txtMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 104);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "Min Value";
            // 
            // txtMax
            // 
            this.txtMax.BackColor = System.Drawing.Color.White;
            this.txtMax.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.txtMax.Location = new System.Drawing.Point(175, 135);
            this.txtMax.Name = "txtMax";
            this.txtMax.ReadOnly = true;
            this.txtMax.Size = new System.Drawing.Size(84, 28);
            this.txtMax.TabIndex = 5;
            this.txtMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 138);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 21);
            this.label3.TabIndex = 4;
            this.label3.Text = "Max Value";
            // 
            // txtMean
            // 
            this.txtMean.BackColor = System.Drawing.Color.White;
            this.txtMean.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.txtMean.Location = new System.Drawing.Point(175, 169);
            this.txtMean.Name = "txtMean";
            this.txtMean.ReadOnly = true;
            this.txtMean.Size = new System.Drawing.Size(84, 28);
            this.txtMean.TabIndex = 7;
            this.txtMean.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 172);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 21);
            this.label4.TabIndex = 6;
            this.label4.Text = "Mean Value";
            // 
            // txtMedian
            // 
            this.txtMedian.BackColor = System.Drawing.Color.White;
            this.txtMedian.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.txtMedian.Location = new System.Drawing.Point(175, 203);
            this.txtMedian.Name = "txtMedian";
            this.txtMedian.ReadOnly = true;
            this.txtMedian.Size = new System.Drawing.Size(84, 28);
            this.txtMedian.TabIndex = 9;
            this.txtMedian.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 206);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 21);
            this.label5.TabIndex = 8;
            this.label5.Text = "Median Value";
            // 
            // txtStdDev
            // 
            this.txtStdDev.BackColor = System.Drawing.Color.White;
            this.txtStdDev.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.txtStdDev.Location = new System.Drawing.Point(175, 237);
            this.txtStdDev.Name = "txtStdDev";
            this.txtStdDev.ReadOnly = true;
            this.txtStdDev.Size = new System.Drawing.Size(84, 28);
            this.txtStdDev.TabIndex = 11;
            this.txtStdDev.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 240);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(142, 21);
            this.label6.TabIndex = 10;
            this.label6.Text = "Standard Deviation";
            // 
            // txtCounts
            // 
            this.txtCounts.BackColor = System.Drawing.Color.White;
            this.txtCounts.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.txtCounts.Location = new System.Drawing.Point(175, 67);
            this.txtCounts.Name = "txtCounts";
            this.txtCounts.ReadOnly = true;
            this.txtCounts.Size = new System.Drawing.Size(84, 28);
            this.txtCounts.TabIndex = 13;
            this.txtCounts.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 70);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(96, 21);
            this.label7.TabIndex = 12;
            this.label7.Text = "Total Counts";
            // 
            // txtEntropy
            // 
            this.txtEntropy.BackColor = System.Drawing.Color.White;
            this.txtEntropy.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.txtEntropy.Location = new System.Drawing.Point(175, 373);
            this.txtEntropy.Name = "txtEntropy";
            this.txtEntropy.ReadOnly = true;
            this.txtEntropy.Size = new System.Drawing.Size(84, 28);
            this.txtEntropy.TabIndex = 15;
            this.txtEntropy.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 376);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 21);
            this.label8.TabIndex = 14;
            this.label8.Text = "Entropy";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(604, 16);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(76, 32);
            this.button1.TabIndex = 16;
            this.button1.Text = "Close";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // histogram1
            // 
            this.histogram1.AllowSelection = true;
            this.histogram1.BackColor = System.Drawing.Color.White;
            this.histogram1.Color = System.Drawing.Color.Red;
            this.histogram1.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.histogram1.IsLogarithmicView = true;
            this.histogram1.Location = new System.Drawing.Point(265, 67);
            this.histogram1.Name = "histogram1";
            this.histogram1.Size = new System.Drawing.Size(415, 334);
            this.histogram1.TabIndex = 19;
            this.histogram1.Text = "histogram1";
            this.histogram1.Values = null;
            this.histogram1.PositionChanged += new AForge.Controls.HistogramEventHandler(this.histogram1_PositionChanged);
            this.histogram1.SelectionChanged += new AForge.Controls.HistogramEventHandler(this.histogram1_SelectionChanged);
            this.histogram1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.histogram1_MouseUp);
            // 
            // txtLowerDec
            // 
            this.txtLowerDec.BackColor = System.Drawing.Color.White;
            this.txtLowerDec.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.txtLowerDec.Location = new System.Drawing.Point(175, 271);
            this.txtLowerDec.Name = "txtLowerDec";
            this.txtLowerDec.ReadOnly = true;
            this.txtLowerDec.Size = new System.Drawing.Size(84, 28);
            this.txtLowerDec.TabIndex = 21;
            this.txtLowerDec.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 274);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 21);
            this.label1.TabIndex = 20;
            this.label1.Text = "Lower Decile";
            // 
            // txtUpperDec
            // 
            this.txtUpperDec.BackColor = System.Drawing.Color.White;
            this.txtUpperDec.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.txtUpperDec.Location = new System.Drawing.Point(175, 305);
            this.txtUpperDec.Name = "txtUpperDec";
            this.txtUpperDec.ReadOnly = true;
            this.txtUpperDec.Size = new System.Drawing.Size(84, 28);
            this.txtUpperDec.TabIndex = 23;
            this.txtUpperDec.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 308);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(98, 21);
            this.label9.TabIndex = 22;
            this.label9.Text = "Upper Decile";
            // 
            // txtMeanLD
            // 
            this.txtMeanLD.BackColor = System.Drawing.Color.White;
            this.txtMeanLD.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.txtMeanLD.Location = new System.Drawing.Point(175, 339);
            this.txtMeanLD.Name = "txtMeanLD";
            this.txtMeanLD.ReadOnly = true;
            this.txtMeanLD.Size = new System.Drawing.Size(84, 28);
            this.txtMeanLD.TabIndex = 25;
            this.txtMeanLD.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 342);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(115, 21);
            this.label10.TabIndex = 24;
            this.label10.Text = "Mean ld excess";
            // 
            // chkLogarithmic
            // 
            this.chkLogarithmic.AutoSize = true;
            this.chkLogarithmic.Checked = true;
            this.chkLogarithmic.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLogarithmic.Location = new System.Drawing.Point(484, 22);
            this.chkLogarithmic.Name = "chkLogarithmic";
            this.chkLogarithmic.Size = new System.Drawing.Size(114, 25);
            this.chkLogarithmic.TabIndex = 26;
            this.chkLogarithmic.Text = "Logarithmic";
            this.chkLogarithmic.UseVisualStyleBackColor = true;
            this.chkLogarithmic.CheckedChanged += new System.EventHandler(this.chkLogarithmic_CheckedChanged);
            // 
            // button2
            // 
            this.button2.FlatAppearance.BorderSize = 0;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Location = new System.Drawing.Point(602, 68);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(76, 31);
            this.button2.TabIndex = 27;
            this.button2.Text = "restore";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // StatisticsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(692, 415);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.chkLogarithmic);
            this.Controls.Add(this.txtMeanLD);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txtUpperDec);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtLowerDec);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.histogram1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtEntropy);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtCounts);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtStdDev);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtMedian);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtMean);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtMax);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtMin);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtObjectName);
            this.Controls.Add(this.lblObjectName);
            this.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "StatisticsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Statistics";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblObjectName;
        private System.Windows.Forms.TextBox txtObjectName;
        private System.Windows.Forms.TextBox txtMin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtMax;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMean;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtMedian;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtStdDev;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtCounts;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtEntropy;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button1;
        private AForge.Controls.Histogram histogram1;
        private System.Windows.Forms.TextBox txtLowerDec;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtUpperDec;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtMeanLD;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox chkLogarithmic;
        private System.Windows.Forms.Button button2;
    }
}