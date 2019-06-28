using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class MaxForm2 : Form
    {
        public int delayTime = 200;
        int image_no = 0;

        public MaxForm2()
        {
            InitializeComponent();
        }

        private void BlinkImages()
        {
            tabControl1.SelectedIndex = 3;
            int idx = 1;
            while (tabControl1.SelectedIndex == 3){
                if (idx == 1){
                    pictureBox4.Image = pictureBox2.Image;
                    lblImgBlink.Text = "Subject";
                    idx = 2;
                }else{
                    pictureBox4.Image = pictureBox3.Image;
                    lblImgBlink.Text = "Template";
                    idx = 1;
                }
                Application.DoEvents();
                System.Threading.Thread.Sleep(delayTime);
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 3) BlinkImages();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1.frm.cmdNext_Click(sender, e);
            Form1.UpdateMaxForm2();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form1.frm.cmdPrev_Click(sender, e);
            Form1.UpdateMaxForm2();
        }

        private void MaxForm2_FormClosing(object sender, FormClosingEventArgs e)
        {
            tabControl1.SelectedIndex = 0;
            if (Form1.iLogLevel > 2) Form1.frm.LogEntry("Returning to Normal-Screen mode");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 0;
            this.Close();
        }

        private void MaxForm2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape){
                tabControl1.SelectedIndex = 0;
                this.Close();
            }
        }

        // version 1.2.0 @ 20190317
        // shortcuts added for (T)emplate, (S)ubject, (A)ligned, (B)link, (>)next, (<)previous, 
        // Brightness Up/Down
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            //If there are textboxes (which need the focus) exclude them from key-catch
            //Control co = GetFocusedControl(frm);
            //if ((co.GetType() == typeof(TextBox) && !co.Equals(txtLog)) || co.GetType() == typeof(NumericUpDown))
            //    return base.ProcessCmdKey(ref msg, keyData);

            //Use of keys' combination: (Keys.Control | Keys.F) for Ctrl+F
            switch (keyData)
            {
                //T,S,A + Shift = value up  - T,S,A + Ctrl = value down 
                case (Keys.Shift | Keys.T):
                    Form1.frm.udTemp.Value += Form1.frm.udTemp.Value / 10;
                    Form1.UpdateMaxForm2();
                    break;
                case (Keys.Control | Keys.T):
                    Form1.frm.udTemp.Value -= Form1.frm.udTemp.Value / 10;
                    Form1.UpdateMaxForm2();
                    break;
                case (Keys.Shift | Keys.S):
                    Form1.frm.udSubj.Value += Form1.frm.udSubj.Value / 10;
                    Form1.UpdateMaxForm2();
                    break;
                case (Keys.Control | Keys.S):
                    Form1.frm.udSubj.Value -= Form1.frm.udSubj.Value / 10;
                    break;
                case (Keys.Shift | Keys.A):
                    Form1.frm.udAlig.Value += Form1.frm.udAlig.Value / 10;
                    Form1.UpdateMaxForm2();
                    break;
                case (Keys.Control | Keys.A):
                    Form1.frm.udAlig.Value -= Form1.frm.udAlig.Value / 10;
                    Form1.UpdateMaxForm2();
                    break;
                // Key T, S, A, B
                case Keys.T:
                    image_no = 0;
                    tabControl1.SelectedIndex = image_no;
                    break;
                case Keys.S:
                    image_no = 1;
                    tabControl1.SelectedIndex = image_no;
                    break;
                case Keys.A:
                    image_no = 2;
                    tabControl1.SelectedIndex = image_no;
                    break;
                case Keys.B:
                    image_no = 3;
                    tabControl1.SelectedIndex = image_no;
                    tabControl1_SelectedIndexChanged(tabControl1, null);
                    break;
                // Keys Next and Previous
                case Keys.OemPeriod:
                case Keys.Right:
                    button1_Click((object)button1, null);
                    break;
                case Keys.Oemcomma:
                case Keys.Left:
                    button2_Click((object)button2, null);
                    break;
                // Key Z for go-back to normal mode
                case Keys.Z:
                    button3_Click((object)button3, null);
                    break;
            }
            return true; // inform that we consumed the event
        }

        private Control GetFocusedControl(Control control) {
            var container = control as IContainerControl;
            while (container != null)
            {
                control = container.ActiveControl;
                container = control as IContainerControl;
            }
            return control;
        }
    }
}
