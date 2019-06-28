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
    public partial class StretchImage : Form
    {
        int image_no = 0;
        public StretchImage()
        {
            InitializeComponent();
            if (Form1.bSharpeningIsOn) { Form1.sharpenFITS.Close(); Form1.bSharpeningIsOn = false; }
            if (Form1.bImageCurveIsOn) { Form1.imageCurveForm.Close(); Form1.bImageCurveIsOn = false; }
            if (Form1.bBitmapFilteringIsOn){
                this.Left = Form1.frm.Left - this.Width;
                this.Top = Form1.frm.Top + Form1.bmFiltering.Height;
                Form1.iStretchImagePosition = 2;
            }else{
                this.Left = Form1.frm.Left - this.Width;
                this.Top = Form1.frm.Top;
                Form1.iStretchImagePosition = 1;
            }
            Form1.frm.checkBox1.Checked = true;
        }

        public void SetImage(int image_no, int selMin, int selMax, int selSelectedMin, int selSelectedMax)
        {
            if (image_no == 3) return;
            this.image_no = image_no;
            selectionRangeSlider1.TextColor = null;
            selectionRangeSlider1.Offset = 10;
            selectionRangeSlider1.Min = selMin;
            selectionRangeSlider1.Max = selMax;
            selectionRangeSlider1.SelectedMin = selSelectedMin;
            selectionRangeSlider1.SelectedMax = selSelectedMax;
            Form1.drawHistogram(Form1.histogram_data[image_no], cookHistImage);
        }

        private void selectionRangeSlider1_SelectionChanged(object sender, EventArgs e)
        {
            if (Form1.bNotChangeSelection) return;
            Form1.bDontRun = true;
            lblMin.Text = "" + selectionRangeSlider1.SelectedMin;
            lblMax.Text = "" + selectionRangeSlider1.SelectedMax;
            Form1.bDontRun = false;
            Form1.SliderSelectionChanged(sender, e, selectionRangeSlider1.Min, selectionRangeSlider1.Max, selectionRangeSlider1.SelectedMin, selectionRangeSlider1.SelectedMax);
        }

        private void lblMin_TextChanged(object sender, EventArgs e)
        {
            if (Form1.bDontRun) return;
            try{
                Form1.bNotChangeSelection = true;
                selectionRangeSlider1.SelectedMin = int.Parse(lblMin.Text);
                Form1.bNotChangeSelection = false;
                selectionRangeSlider1_SelectionChanged(sender, e);
            }catch (Exception ex){
                Form1.frm.LogError("lblMin_TextChanged", "Parsing integer Exception: " + ex.Message.ToString());
            }
        }

        private void lblMax_TextChanged(object sender, EventArgs e)
        {
            if (Form1.bDontRun) return;
            try{
                Form1.bNotChangeSelection = true;
                selectionRangeSlider1.SelectedMax = int.Parse(lblMax.Text);
                Form1.bNotChangeSelection = false;
                selectionRangeSlider1_SelectionChanged(sender, e);
            }catch (Exception ex) {
                Form1.frm.LogError("lblMax_TextChanged", "Parsing integer Exception: " + ex.Message.ToString());
            }
        }

        private void StretchImage_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form1.bStretchImageIsOn = false;
            Form1.frm.checkBox1.Checked = false;
        }

        private void btnResetMinMax_Click(object sender, EventArgs e)
        {
            int i = Form1.frm.tabControl1.SelectedIndex;

            if (i < 3)
            {
                Form1.iMin[i] = Form1.iMinDefault[i];
                Form1.iMax[i] = Form1.iMaxDefault[i];
                Form1.bNotChangeSelection = true;
                selectionRangeSlider1.SelectedMin = Form1.iMin[i];
                selectionRangeSlider1.SelectedMax = (Form1.iMax[i] - Form1.iMin[i]) / 2;
                Form1.bNotChangeSelection = false;
                selectionRangeSlider1_SelectionChanged(sender, e);
            }
        }

    }
}
