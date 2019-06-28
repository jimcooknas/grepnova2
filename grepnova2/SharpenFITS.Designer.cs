namespace grepnova2
{
    partial class SharpenFITS
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
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
        private void InitializeComponent() {
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.txtK = new System.Windows.Forms.TextBox();
            this.txtDScan = new System.Windows.Forms.TextBox();
            this.lblDScan = new System.Windows.Forms.Label();
            this.trackBar3 = new System.Windows.Forms.TrackBar();
            this.cmdApplyFilter = new System.Windows.Forms.Button();
            this.btnRestoreAll = new System.Windows.Forms.Button();
            this.btnRestoreCurrent = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar3)).BeginInit();
            this.SuspendLayout();
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(-2, 37);
            this.trackBar1.Maximum = 100;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(208, 56);
            this.trackBar1.TabIndex = 0;
            this.trackBar1.TickFrequency = 10;
            this.trackBar1.Value = 50;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(186, 18);
            this.label1.TabIndex = 1;
            this.label1.Text = "Sharpening factor k (0.0…1.0)";
            // 
            // txtK
            // 
            this.txtK.BackColor = System.Drawing.Color.White;
            this.txtK.Location = new System.Drawing.Point(206, 37);
            this.txtK.Name = "txtK";
            this.txtK.ReadOnly = true;
            this.txtK.Size = new System.Drawing.Size(54, 26);
            this.txtK.TabIndex = 2;
            this.txtK.Text = "0,5";
            this.txtK.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtDScan
            // 
            this.txtDScan.BackColor = System.Drawing.Color.White;
            this.txtDScan.Location = new System.Drawing.Point(206, 107);
            this.txtDScan.Name = "txtDScan";
            this.txtDScan.ReadOnly = true;
            this.txtDScan.Size = new System.Drawing.Size(54, 26);
            this.txtDScan.TabIndex = 8;
            this.txtDScan.Text = "1";
            this.txtDScan.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblDScan
            // 
            this.lblDScan.AutoSize = true;
            this.lblDScan.Location = new System.Drawing.Point(12, 82);
            this.lblDScan.Name = "lblDScan";
            this.lblDScan.Size = new System.Drawing.Size(151, 18);
            this.lblDScan.TabIndex = 7;
            this.lblDScan.Text = "Sharpening Shift (-2…2)";
            // 
            // trackBar3
            // 
            this.trackBar3.Location = new System.Drawing.Point(-2, 107);
            this.trackBar3.Maximum = 2;
            this.trackBar3.Minimum = -2;
            this.trackBar3.Name = "trackBar3";
            this.trackBar3.Size = new System.Drawing.Size(208, 56);
            this.trackBar3.TabIndex = 6;
            this.trackBar3.Value = 1;
            this.trackBar3.Scroll += new System.EventHandler(this.trackBar3_Scroll);
            // 
            // cmdApplyFilter
            // 
            this.cmdApplyFilter.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold);
            this.cmdApplyFilter.Image = global::grepnova2.Properties.Resources.filter;
            this.cmdApplyFilter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmdApplyFilter.Location = new System.Drawing.Point(12, 208);
            this.cmdApplyFilter.Name = "cmdApplyFilter";
            this.cmdApplyFilter.Size = new System.Drawing.Size(248, 51);
            this.cmdApplyFilter.TabIndex = 44;
            this.cmdApplyFilter.Text = "Apply Sharpening";
            this.cmdApplyFilter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cmdApplyFilter.UseVisualStyleBackColor = true;
            this.cmdApplyFilter.Click += new System.EventHandler(this.cmdApplyFilter_Click);
            // 
            // btnRestoreAll
            // 
            this.btnRestoreAll.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.btnRestoreAll.Location = new System.Drawing.Point(156, 175);
            this.btnRestoreAll.Name = "btnRestoreAll";
            this.btnRestoreAll.Size = new System.Drawing.Size(104, 27);
            this.btnRestoreAll.TabIndex = 54;
            this.btnRestoreAll.Text = "Restore all";
            this.btnRestoreAll.UseVisualStyleBackColor = true;
            this.btnRestoreAll.Click += new System.EventHandler(this.btnRestoreAll_Click);
            // 
            // btnRestoreCurrent
            // 
            this.btnRestoreCurrent.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.btnRestoreCurrent.Location = new System.Drawing.Point(12, 175);
            this.btnRestoreCurrent.Name = "btnRestoreCurrent";
            this.btnRestoreCurrent.Size = new System.Drawing.Size(118, 27);
            this.btnRestoreCurrent.TabIndex = 53;
            this.btnRestoreCurrent.Text = "Restore current";
            this.btnRestoreCurrent.UseVisualStyleBackColor = true;
            this.btnRestoreCurrent.Click += new System.EventHandler(this.btnRestoreCurrent_Click);
            // 
            // SharpenFITS
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(272, 272);
            this.Controls.Add(this.btnRestoreAll);
            this.Controls.Add(this.btnRestoreCurrent);
            this.Controls.Add(this.cmdApplyFilter);
            this.Controls.Add(this.txtDScan);
            this.Controls.Add(this.lblDScan);
            this.Controls.Add(this.trackBar3);
            this.Controls.Add(this.txtK);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.trackBar1);
            this.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.Name = "SharpenFITS";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Sharpen FITS";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SharpenFITS_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtK;
        private System.Windows.Forms.TextBox txtDScan;
        private System.Windows.Forms.Label lblDScan;
        private System.Windows.Forms.TrackBar trackBar3;
        private System.Windows.Forms.Button cmdApplyFilter;
        private System.Windows.Forms.Button btnRestoreAll;
        private System.Windows.Forms.Button btnRestoreCurrent;
    }
}