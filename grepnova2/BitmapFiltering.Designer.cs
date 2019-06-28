namespace grepnova2
{
    partial class BitmapFiltering
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BitmapFiltering));
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.txtFilterParam2 = new System.Windows.Forms.TextBox();
            this.lblFilterParam2 = new System.Windows.Forms.Label();
            this.txtFilterParam = new System.Windows.Forms.TextBox();
            this.lblFilterParam = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.btnRestoreAll = new System.Windows.Forms.Button();
            this.cmdApplyFilter = new System.Windows.Forms.Button();
            this.btnRestoreCurrent = new System.Windows.Forms.Button();
            this.label38 = new System.Windows.Forms.Label();
            this.chkFilterParam = new System.Windows.Forms.CheckBox();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.txtFilterParam2);
            this.groupBox4.Controls.Add(this.lblFilterParam2);
            this.groupBox4.Controls.Add(this.txtFilterParam);
            this.groupBox4.Controls.Add(this.lblFilterParam);
            this.groupBox4.Controls.Add(this.comboBox1);
            this.groupBox4.Controls.Add(this.btnRestoreAll);
            this.groupBox4.Controls.Add(this.cmdApplyFilter);
            this.groupBox4.Controls.Add(this.btnRestoreCurrent);
            this.groupBox4.Controls.Add(this.label38);
            this.groupBox4.Controls.Add(this.chkFilterParam);
            this.groupBox4.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.groupBox4.Location = new System.Drawing.Point(12, 11);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(249, 291);
            this.groupBox4.TabIndex = 45;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Bitmap Filtering";
            // 
            // txtFilterParam2
            // 
            this.txtFilterParam2.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.txtFilterParam2.Location = new System.Drawing.Point(115, 120);
            this.txtFilterParam2.Name = "txtFilterParam2";
            this.txtFilterParam2.Size = new System.Drawing.Size(67, 26);
            this.txtFilterParam2.TabIndex = 48;
            this.txtFilterParam2.Visible = false;
            // 
            // lblFilterParam2
            // 
            this.lblFilterParam2.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.lblFilterParam2.Location = new System.Drawing.Point(54, 120);
            this.lblFilterParam2.Name = "lblFilterParam2";
            this.lblFilterParam2.Size = new System.Drawing.Size(55, 23);
            this.lblFilterParam2.TabIndex = 47;
            this.lblFilterParam2.Text = "label39";
            this.lblFilterParam2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblFilterParam2.Visible = false;
            // 
            // txtFilterParam
            // 
            this.txtFilterParam.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.txtFilterParam.Location = new System.Drawing.Point(115, 87);
            this.txtFilterParam.Name = "txtFilterParam";
            this.txtFilterParam.Size = new System.Drawing.Size(67, 26);
            this.txtFilterParam.TabIndex = 45;
            this.txtFilterParam.Visible = false;
            // 
            // lblFilterParam
            // 
            this.lblFilterParam.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.lblFilterParam.Location = new System.Drawing.Point(5, 88);
            this.lblFilterParam.Name = "lblFilterParam";
            this.lblFilterParam.Size = new System.Drawing.Size(104, 22);
            this.lblFilterParam.TabIndex = 44;
            this.lblFilterParam.Text = "lblFilterParam";
            this.lblFilterParam.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblFilterParam.Visible = false;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(11, 46);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(224, 26);
            this.comboBox1.TabIndex = 41;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // btnRestoreAll
            // 
            this.btnRestoreAll.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.btnRestoreAll.Location = new System.Drawing.Point(131, 197);
            this.btnRestoreAll.Name = "btnRestoreAll";
            this.btnRestoreAll.Size = new System.Drawing.Size(104, 27);
            this.btnRestoreAll.TabIndex = 52;
            this.btnRestoreAll.Text = "Restore all";
            this.btnRestoreAll.UseVisualStyleBackColor = true;
            this.btnRestoreAll.Click += new System.EventHandler(this.btnRestoreAll_Click);
            // 
            // cmdApplyFilter
            // 
            this.cmdApplyFilter.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold);
            this.cmdApplyFilter.Image = global::grepnova2.Properties.Resources.filter;
            this.cmdApplyFilter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmdApplyFilter.Location = new System.Drawing.Point(9, 230);
            this.cmdApplyFilter.Name = "cmdApplyFilter";
            this.cmdApplyFilter.Size = new System.Drawing.Size(226, 51);
            this.cmdApplyFilter.TabIndex = 43;
            this.cmdApplyFilter.Text = "Apply Filter";
            this.cmdApplyFilter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cmdApplyFilter.UseVisualStyleBackColor = true;
            this.cmdApplyFilter.Click += new System.EventHandler(this.cmdApplyFilter_Click);
            // 
            // btnRestoreCurrent
            // 
            this.btnRestoreCurrent.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.btnRestoreCurrent.Location = new System.Drawing.Point(9, 197);
            this.btnRestoreCurrent.Name = "btnRestoreCurrent";
            this.btnRestoreCurrent.Size = new System.Drawing.Size(118, 27);
            this.btnRestoreCurrent.TabIndex = 51;
            this.btnRestoreCurrent.Text = "Restore current";
            this.btnRestoreCurrent.UseVisualStyleBackColor = true;
            this.btnRestoreCurrent.Click += new System.EventHandler(this.btnRestoreCurrent_Click);
            // 
            // label38
            // 
            this.label38.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.label38.Location = new System.Drawing.Point(6, 27);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(106, 16);
            this.label38.TabIndex = 42;
            this.label38.Text = "Filter selection";
            this.label38.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkFilterParam
            // 
            this.chkFilterParam.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.chkFilterParam.Location = new System.Drawing.Point(11, 162);
            this.chkFilterParam.Name = "chkFilterParam";
            this.chkFilterParam.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkFilterParam.Size = new System.Drawing.Size(171, 22);
            this.chkFilterParam.TabIndex = 46;
            this.chkFilterParam.Text = "chkFilterParam";
            this.chkFilterParam.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkFilterParam.UseVisualStyleBackColor = true;
            this.chkFilterParam.Visible = false;
            // 
            // BitmapFiltering
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(272, 308);
            this.Controls.Add(this.groupBox4);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BitmapFiltering";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Bitmap Filtering";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BitmapFiltering_FormClosing);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label lblFilterParam2;
        private System.Windows.Forms.Label lblFilterParam;
        private System.Windows.Forms.Button btnRestoreAll;
        private System.Windows.Forms.Button cmdApplyFilter;
        private System.Windows.Forms.Button btnRestoreCurrent;
        private System.Windows.Forms.Label label38;
        public System.Windows.Forms.TextBox txtFilterParam2;
        public System.Windows.Forms.TextBox txtFilterParam;
        public System.Windows.Forms.ComboBox comboBox1;
        public System.Windows.Forms.CheckBox chkFilterParam;
    }
}