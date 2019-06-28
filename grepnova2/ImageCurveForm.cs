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

    public partial class ImageCurveForm : Form
    {
        public ImageCurveForm()
        {
            InitializeComponent();
            if (Form1.bStretchImageIsOn) {
                Form1.stretchImage.Close();
                Form1.bStretchImageIsOn = false;
            }
            if(Form1.bBitmapFilteringIsOn){
                Form1.bmFiltering.Close();
                Form1.bBitmapFilteringIsOn = false;
            }
            if (Form1.bSharpeningIsOn){
                Form1.sharpenFITS.Close();
                Form1.bSharpeningIsOn = false;
            }
            this.Left = Form1.frm.Left - this.Width;
            this.Top = Form1.frm.Top;
            Form1.bImageCurveIsOn = true;
        }

        private void imageCurve1_LevelChangedEvent(object sender, LevelChangedEventArgs e) => Form1.StrechImageCurve(e);

        private void button1_Click(object sender, EventArgs e) => Form1.StrechImageCurve(null, true);

        private void ImageCurveForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form1.bImageCurveIsOn = false;
        }
    }
}
