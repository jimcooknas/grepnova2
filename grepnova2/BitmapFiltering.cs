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
    public partial class BitmapFiltering : Form
    {
        public BitmapFiltering()
        {
            InitializeComponent();
            for (int i = 0; i < Form1.fltParam.Length; i++) comboBox1.Items.Add(Form1.fltParam[i].funcName);
            comboBox1.SelectedIndex = Form1.bitmapFilterSelected;
            if (Form1.bSharpeningIsOn) { Form1.sharpenFITS.Close(); Form1.bSharpeningIsOn = false; }
            if (Form1.bImageCurveIsOn) { Form1.imageCurveForm.Close(); Form1.bImageCurveIsOn = false; }
            if (Form1.bStretchImageIsOn){
                this.Left = Form1.frm.Left - this.Width;
                this.Top = Form1.frm.Top + Form1.stretchImage.Height;
                Form1.iStretchImagePosition = 1;
            }else{
                this.Left = Form1.frm.Left - this.Width;
                this.Top = Form1.frm.Top;
                Form1.iStretchImagePosition = 2;
            }
        }

        private void cmdApplyFilter_Click(object sender, EventArgs e)
        {
            Form1.ApplyFilter(sender, e, comboBox1.SelectedIndex, txtFilterParam.Text, txtFilterParam2.Text, chkFilterParam.Checked);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblFilterParam.Visible = Form1.fltParam[comboBox1.SelectedIndex].textParamVisibility;
            txtFilterParam.Visible = Form1.fltParam[comboBox1.SelectedIndex].textParamVisibility;
            lblFilterParam.Text = Form1.fltParam[comboBox1.SelectedIndex].textLabel;
            txtFilterParam.Text = Form1.fltParam[comboBox1.SelectedIndex].textDefaultValue;
            lblFilterParam2.Visible = Form1.fltParam[comboBox1.SelectedIndex].textParamVisibility2;
            txtFilterParam2.Visible = Form1.fltParam[comboBox1.SelectedIndex].textParamVisibility2;
            lblFilterParam2.Text = Form1.fltParam[comboBox1.SelectedIndex].textLabel2;
            txtFilterParam2.Text = Form1.fltParam[comboBox1.SelectedIndex].textDefaultValue2;
            chkFilterParam.Visible = Form1.fltParam[comboBox1.SelectedIndex].checkParamVisibility;
            chkFilterParam.Text = Form1.fltParam[comboBox1.SelectedIndex].checkParamText;
            cmdApplyFilter.Text = "Apply " + Form1.fltParam[comboBox1.SelectedIndex].funcName;
        }

        private void btnRestoreCurrent_Click(object sender, EventArgs e)
        {
            Form1.RestoreBitmaps(sender, e, 0);
        }

        private void btnRestoreAll_Click(object sender, EventArgs e)
        {
            Form1.RestoreBitmaps(sender, e, 1);
        }

        private void BitmapFiltering_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form1.bBitmapFilteringIsOn = false;
        }
    }
}
