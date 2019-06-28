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
    public partial class SharpenFITS : Form
    {
        float k = 0.5f;
        int dScan = 1;

        public SharpenFITS() {
            InitializeComponent();
            if (Form1.bStretchImageIsOn){
                Form1.stretchImage.Close();
                Form1.bStretchImageIsOn = false;
            }
            if (Form1.bBitmapFilteringIsOn){
                Form1.bmFiltering.Close();
                Form1.bBitmapFilteringIsOn = false;
            }
            if (Form1.bImageCurveIsOn){
                Form1.imageCurveForm.Close();
                Form1.bImageCurveIsOn = false;
            }
            this.Left = Form1.frm.Left - this.Width;
            this.Top = Form1.frm.Top;
            Form1.bSharpeningIsOn = true;
            k = (float)trackBar1.Value / 100f;
            dScan = trackBar3.Value;
        }

        private void SharpenFITS_FormClosing(object sender, FormClosingEventArgs e) {
            Form1.bSharpeningIsOn = false;
        }

        private void cmdApplyFilter_Click(object sender, EventArgs e) {
            Form1.ApplySharpenFITS(sender, e, k, dScan);
        }

        private void trackBar1_Scroll(object sender, EventArgs e) {
            k = (float)(trackBar1.Value / 100f);
            txtK.Text = "" + k;
        }

        private void trackBar3_Scroll(object sender, EventArgs e) {
            dScan = trackBar3.Value;
            txtDScan.Text = "" + dScan;
        }

        private void btnRestoreCurrent_Click(object sender, EventArgs e) {
            Form1.RestoreBitmaps(sender, e, 0);
        }

        private void btnRestoreAll_Click(object sender, EventArgs e) {
            Form1.RestoreBitmaps(sender, e, 1);
        }
    }
}
