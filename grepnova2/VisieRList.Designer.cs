namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    partial class VisieRList
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.USNO = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RAJ2000 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DEJ2000 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.e_RAJ2000 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.e_DEJ2000 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Epoch = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pmRA = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pmDE = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Ndet = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.B1mag = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.R1mag = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.B2mag = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.R2mag = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Imag = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblFor = new System.Windows.Forms.Label();
            this.lblItemsFound = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Alignment = System.Windows.Forms.ListViewAlignment.SnapToGrid;
            this.listView1.BackColor = System.Drawing.Color.White;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.USNO,
            this.RAJ2000,
            this.DEJ2000,
            this.e_RAJ2000,
            this.e_DEJ2000,
            this.Epoch,
            this.pmRA,
            this.pmDE,
            this.Ndet,
            this.B1mag,
            this.R1mag,
            this.B2mag,
            this.R2mag,
            this.Imag});
            this.listView1.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.Location = new System.Drawing.Point(12, 45);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.ShowItemToolTips = true;
            this.listView1.Size = new System.Drawing.Size(1322, 528);
            this.listView1.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
            // 
            // USNO
            // 
            this.USNO.Text = "USNO-B1.0";
            this.USNO.Width = 150;
            // 
            // RAJ2000
            // 
            this.RAJ2000.Text = "RA J2000";
            this.RAJ2000.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.RAJ2000.Width = 140;
            // 
            // DEJ2000
            // 
            this.DEJ2000.Text = "DE J2000";
            this.DEJ2000.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.DEJ2000.Width = 140;
            // 
            // e_RAJ2000
            // 
            this.e_RAJ2000.Text = "e_RA";
            this.e_RAJ2000.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.e_RAJ2000.Width = 80;
            // 
            // e_DEJ2000
            // 
            this.e_DEJ2000.Text = "e_DE";
            this.e_DEJ2000.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.e_DEJ2000.Width = 80;
            // 
            // Epoch
            // 
            this.Epoch.Text = "Epoch";
            this.Epoch.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.Epoch.Width = 90;
            // 
            // pmRA
            // 
            this.pmRA.Text = "pmRA";
            this.pmRA.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.pmRA.Width = 80;
            // 
            // pmDE
            // 
            this.pmDE.Text = "pmDE";
            this.pmDE.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.pmDE.Width = 80;
            // 
            // Ndet
            // 
            this.Ndet.Text = "Ndet";
            this.Ndet.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.Ndet.Width = 50;
            // 
            // B1mag
            // 
            this.B1mag.Text = "B1mag";
            this.B1mag.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.B1mag.Width = 80;
            // 
            // R1mag
            // 
            this.R1mag.Text = "R1mag";
            this.R1mag.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.R1mag.Width = 80;
            // 
            // B2mag
            // 
            this.B2mag.Text = "B2mag";
            this.B2mag.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.B2mag.Width = 80;
            // 
            // R2mag
            // 
            this.R2mag.Text = "R2mag";
            this.R2mag.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.R2mag.Width = 80;
            // 
            // Imag
            // 
            this.Imag.Text = "Imag";
            this.Imag.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.Imag.Width = 80;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.button1.Location = new System.Drawing.Point(1220, 579);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(114, 42);
            this.button1.TabIndex = 1;
            this.button1.Text = "Close";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri Light", 7.8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.label1.Location = new System.Drawing.Point(12, 594);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(988, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "VizieR provides the most complete library of published astronomical catalogues --" +
    "tables and associated data-- with verified and enriched data, accessible via mul" +
    "tiple interfaces.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.label2.Location = new System.Drawing.Point(12, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 21);
            this.label2.TabIndex = 3;
            this.label2.Text = "VisieR results";
            // 
            // lblFor
            // 
            this.lblFor.AutoSize = true;
            this.lblFor.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.lblFor.Location = new System.Drawing.Point(112, 21);
            this.lblFor.Name = "lblFor";
            this.lblFor.Size = new System.Drawing.Size(30, 21);
            this.lblFor.TabIndex = 4;
            this.lblFor.Text = "for";
            // 
            // lblItemsFound
            // 
            this.lblItemsFound.AutoSize = true;
            this.lblItemsFound.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.lblItemsFound.Location = new System.Drawing.Point(181, 21);
            this.lblItemsFound.Name = "lblItemsFound";
            this.lblItemsFound.Size = new System.Drawing.Size(63, 21);
            this.lblItemsFound.TabIndex = 5;
            this.lblItemsFound.Text = "0 items";
            // 
            // VisieRList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1346, 629);
            this.Controls.Add(this.lblItemsFound);
            this.Controls.Add(this.lblFor);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VisieRList";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VisieR List";
            this.Load += new System.EventHandler(this.VisieRList_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader USNO;
        private System.Windows.Forms.ColumnHeader RAJ2000;
        private System.Windows.Forms.ColumnHeader DEJ2000;
        private System.Windows.Forms.ColumnHeader e_RAJ2000;
        private System.Windows.Forms.ColumnHeader e_DEJ2000;
        private System.Windows.Forms.ColumnHeader Epoch;
        private System.Windows.Forms.ColumnHeader pmRA;
        private System.Windows.Forms.ColumnHeader pmDE;
        private System.Windows.Forms.ColumnHeader Ndet;
        private System.Windows.Forms.ColumnHeader B1mag;
        private System.Windows.Forms.ColumnHeader R1mag;
        private System.Windows.Forms.ColumnHeader B2mag;
        private System.Windows.Forms.ColumnHeader R2mag;
        private System.Windows.Forms.ColumnHeader Imag;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.Label lblFor;
        public System.Windows.Forms.Label lblItemsFound;
    }
}