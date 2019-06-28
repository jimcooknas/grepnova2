namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    partial class AstrometryForm
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.txt = new System.Windows.Forms.TextBox();
            this.lblObject = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnSolve = new System.Windows.Forms.Button();
            this.cbSolveMethod = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnSavePlate = new System.Windows.Forms.Button();
            this.btnIndexWizz = new System.Windows.Forms.Button();
            this.btnConfigureSolver = new System.Windows.Forms.Button();
            this.btnRunSolver = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.chkVisier = new System.Windows.Forms.CheckBox();
            this.progressIndicator1 = new grepnova2.ProgressIndicator();
            this.progressIndicator2 = new grepnova2.ProgressIndicator();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackColor = System.Drawing.Color.Black;
            this.pictureBox1.Location = new System.Drawing.Point(12, 38);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(765, 510);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // txt
            // 
            this.txt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.txt.Location = new System.Drawing.Point(794, 38);
            this.txt.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txt.Multiline = true;
            this.txt.Name = "txt";
            this.txt.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt.Size = new System.Drawing.Size(530, 510);
            this.txt.TabIndex = 1;
            // 
            // lblObject
            // 
            this.lblObject.AutoSize = true;
            this.lblObject.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.lblObject.Location = new System.Drawing.Point(12, 15);
            this.lblObject.Name = "lblObject";
            this.lblObject.Size = new System.Drawing.Size(52, 21);
            this.lblObject.TabIndex = 2;
            this.lblObject.Text = "label1";
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.Font = new System.Drawing.Font("Calibri", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.btnExit.Location = new System.Drawing.Point(1209, 592);
            this.btnExit.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(115, 73);
            this.btnExit.TabIndex = 3;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.lblStatus.ForeColor = System.Drawing.Color.RoyalBlue;
            this.lblStatus.Location = new System.Drawing.Point(790, 15);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(298, 21);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Press \'Solve Plate\' button to start solving";
            // 
            // btnSolve
            // 
            this.btnSolve.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSolve.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.btnSolve.Location = new System.Drawing.Point(265, 27);
            this.btnSolve.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnSolve.Name = "btnSolve";
            this.btnSolve.Size = new System.Drawing.Size(127, 46);
            this.btnSolve.TabIndex = 5;
            this.btnSolve.Text = "Solve Plate";
            this.btnSolve.UseVisualStyleBackColor = true;
            this.btnSolve.Click += new System.EventHandler(this.btnSolve_Click);
            // 
            // cbSolveMethod
            // 
            this.cbSolveMethod.FormattingEnabled = true;
            this.cbSolveMethod.Items.AddRange(new object[] {
            "Astrometry.net Direct",
            "All Sky Plate Solver"});
            this.cbSolveMethod.Location = new System.Drawing.Point(9, 44);
            this.cbSolveMethod.Margin = new System.Windows.Forms.Padding(4);
            this.cbSolveMethod.Name = "cbSolveMethod";
            this.cbSolveMethod.Size = new System.Drawing.Size(247, 26);
            this.cbSolveMethod.TabIndex = 7;
            this.cbSolveMethod.SelectedIndexChanged += new System.EventHandler(this.cbSolveMethod_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.label1.Location = new System.Drawing.Point(5, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 17);
            this.label1.TabIndex = 8;
            this.label1.Text = "Plate Solve method";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.btnSavePlate);
            this.groupBox1.Controls.Add(this.btnIndexWizz);
            this.groupBox1.Controls.Add(this.btnConfigureSolver);
            this.groupBox1.Controls.Add(this.btnRunSolver);
            this.groupBox1.Controls.Add(this.btnSolve);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cbSolveMethod);
            this.groupBox1.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.groupBox1.Location = new System.Drawing.Point(12, 584);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(1159, 79);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Solve Plate (Astrometry.net or All Sky Plate Solver)";
            // 
            // btnSavePlate
            // 
            this.btnSavePlate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSavePlate.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.btnSavePlate.Location = new System.Drawing.Point(599, 28);
            this.btnSavePlate.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnSavePlate.Name = "btnSavePlate";
            this.btnSavePlate.Size = new System.Drawing.Size(111, 46);
            this.btnSavePlate.TabIndex = 10;
            this.btnSavePlate.Text = "Save Plate";
            this.btnSavePlate.UseVisualStyleBackColor = true;
            this.btnSavePlate.Click += new System.EventHandler(this.btnSavePlate_Click);
            // 
            // btnIndexWizz
            // 
            this.btnIndexWizz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnIndexWizz.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.btnIndexWizz.Location = new System.Drawing.Point(728, 27);
            this.btnIndexWizz.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnIndexWizz.Name = "btnIndexWizz";
            this.btnIndexWizz.Size = new System.Drawing.Size(201, 46);
            this.btnIndexWizz.TabIndex = 11;
            this.btnIndexWizz.Text = "ASPS Index Wizzard";
            this.btnIndexWizz.UseVisualStyleBackColor = true;
            this.btnIndexWizz.Click += new System.EventHandler(this.btnIndexWizz_Click);
            // 
            // btnConfigureSolver
            // 
            this.btnConfigureSolver.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnConfigureSolver.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.btnConfigureSolver.Location = new System.Drawing.Point(935, 28);
            this.btnConfigureSolver.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnConfigureSolver.Name = "btnConfigureSolver";
            this.btnConfigureSolver.Size = new System.Drawing.Size(211, 46);
            this.btnConfigureSolver.TabIndex = 10;
            this.btnConfigureSolver.Text = "Configure PlateSolver";
            this.btnConfigureSolver.UseVisualStyleBackColor = true;
            this.btnConfigureSolver.Click += new System.EventHandler(this.btnConfigureSolver_Click);
            // 
            // btnRunSolver
            // 
            this.btnRunSolver.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRunSolver.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.btnRunSolver.Location = new System.Drawing.Point(397, 27);
            this.btnRunSolver.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnRunSolver.Name = "btnRunSolver";
            this.btnRunSolver.Size = new System.Drawing.Size(196, 46);
            this.btnRunSolver.TabIndex = 9;
            this.btnRunSolver.Text = "Run AllSkyPlateSolver";
            this.btnRunSolver.UseVisualStyleBackColor = true;
            this.btnRunSolver.Click += new System.EventHandler(this.btnRunSolver_Click);
            // 
            // chkVisier
            // 
            this.chkVisier.AutoSize = true;
            this.chkVisier.Location = new System.Drawing.Point(15, 556);
            this.chkVisier.Name = "chkVisier";
            this.chkVisier.Size = new System.Drawing.Size(208, 21);
            this.chkVisier.TabIndex = 10;
            this.chkVisier.Text = "Request objects from VisieR";
            this.chkVisier.UseVisualStyleBackColor = true;
            this.chkVisier.CheckedChanged += new System.EventHandler(this.chkVisier_CheckedChanged);
            // 
            // progressIndicator1
            // 
            this.progressIndicator1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.progressIndicator1.Location = new System.Drawing.Point(1292, 5);
            this.progressIndicator1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.progressIndicator1.Name = "progressIndicator1";
            this.progressIndicator1.Percentage = 0F;
            this.progressIndicator1.Size = new System.Drawing.Size(31, 31);
            this.progressIndicator1.TabIndex = 6;
            this.progressIndicator1.Text = "progressIndicator1";
            this.progressIndicator1.Visible = false;
            // 
            // progressIndicator2
            // 
            this.progressIndicator2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.progressIndicator2.Location = new System.Drawing.Point(229, 552);
            this.progressIndicator2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.progressIndicator2.Name = "progressIndicator2";
            this.progressIndicator2.Percentage = 0F;
            this.progressIndicator2.Size = new System.Drawing.Size(25, 25);
            this.progressIndicator2.TabIndex = 11;
            this.progressIndicator2.Text = "progressIndicator2";
            this.progressIndicator2.Visible = false;
            // 
            // AstrometryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(1340, 669);
            this.Controls.Add(this.progressIndicator2);
            this.Controls.Add(this.chkVisier);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.progressIndicator1);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.lblObject);
            this.Controls.Add(this.txt);
            this.Controls.Add(this.pictureBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AstrometryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AstrometryForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AstrometryForm_FormClosing);
            this.Shown += new System.EventHandler(this.AstrometryForm_Shown);
            this.Resize += new System.EventHandler(this.AstrometryForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox txt;
        private System.Windows.Forms.Label lblObject;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnSolve;
        private ProgressIndicator progressIndicator1;
        private System.Windows.Forms.ComboBox cbSolveMethod;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnConfigureSolver;
        private System.Windows.Forms.Button btnRunSolver;
        private System.Windows.Forms.Button btnIndexWizz;
        private System.Windows.Forms.Button btnSavePlate;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.CheckBox chkVisier;
        private ProgressIndicator progressIndicator2;
    }
}