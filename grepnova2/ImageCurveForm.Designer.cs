﻿namespace grepnova2
{
    partial class ImageCurveForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.imageCurve1 = new grepnova2.ImageCurve();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(85, 140);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(43, 22);
            this.button1.TabIndex = 1;
            this.button1.Text = "R";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // imageCurve1
            // 
            this.imageCurve1.Location = new System.Drawing.Point(-1, 0);
            this.imageCurve1.Name = "imageCurve1";
            this.imageCurve1.Size = new System.Drawing.Size(205, 150);
            this.imageCurve1.TabIndex = 0;
            this.imageCurve1.LevelChangedEvent += new grepnova2.LevelChangedEventHandler(this.imageCurve1_LevelChangedEvent);
            // 
            // ImageCurveForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(204, 162);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.imageCurve1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImageCurveForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Image Curve";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImageCurveForm_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private ImageCurve imageCurve1;
        private System.Windows.Forms.Button button1;
    }
}