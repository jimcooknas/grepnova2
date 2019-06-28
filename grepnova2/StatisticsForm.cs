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
    public partial class StatisticsForm : Form
    {
        string[] obName = { "Template object", "Subject object", "Aligned object" };
        int[] data = null;
        int hSize = 0;

        public StatisticsForm(Fits.imageptr img, int TempOrSubj, string objName)
        {
            InitializeComponent();
            int[] fdata = Fits.calc_histogram_total(img);
            float[] data1 = Fits.calc_histogram(img, 5000, false);//new int[fdata.Length];
            hSize = data1.Length;
            data = new int[data1.Length];
            for (int i=0;i<data1.Length;i++)data[i]=(int)data1[i];
            AForge.Math.Histogram hist = new AForge.Math.Histogram(fdata);
            lblObjectName.Text = obName[TempOrSubj];
            txtObjectName.Text = objName;
            txtMean.Text = String.Format("{0:N0}\r\n", hist.Mean);
            txtStdDev.Text = String.Format("{0:F2}\r\n", hist.StdDev);
            txtMedian.Text = String.Format("{0:N0}\r\n", hist.Median);
            txtMin.Text = String.Format("{0:N0}\r\n", hist.Min);
            txtMax.Text = String.Format("{0:N0}\r\n", hist.Max);
            txtLowerDec.Text = String.Format("{0:N0}\r\n", img.data_lower_decile);
            txtUpperDec.Text = String.Format("{0:N0}\r\n", img.data_upper_decile);
            txtMeanLD.Text = String.Format("{0:F2}\r\n", img.mean_ld_excess);
            txtCounts.Text = String.Format("{0:N0}\r\n", hist.TotalCount);
            double entropy = Fits.CalcImageEntropy(img, 2.0);//AForge.Math.Statistics.Entropy(fdata);
            txtEntropy.Text = String.Format("{0:F2}\r\n", entropy);

            histogram1.Color= Color.Red;
            histogram1.AllowSelection = true;
            histogram1.IsLogarithmicView = chkLogarithmic.Checked;
            histogram1.Values = data;
            histogram1.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        int min = 0;
        int max = 0;

        private void histogram1_SelectionChanged(object sender, AForge.Controls.HistogramEventArgs e)
        {
            max = e.Max;
            if (max > hSize) max = hSize;
        }

        private void histogram1_MouseUp(object sender, MouseEventArgs e)
        {
            if (min > max)
            {
                int tmp = min;
                min = max;
                max = tmp;
            }
            int iMin = min;// (int)((float)(min * hSize) / (float)histogram1.Width);
            int iMax = max;// (int)((float)(max * hSize) / (float)histogram1.Width);
            int[] newdata = new int[hSize];// iMax - iMin + 1];
            for (int i = 0; i < hSize; i++)
            {
                if (i >= iMin && i <= iMax){
                    newdata[i] = data[i];
                }else{
                    newdata[i] = 0;
                }
            }
            try{
                histogram1.Values = newdata;
            }catch (ArgumentException){
                histogram1.Values = data;
            }
            histogram1.ClientSize = new Size(histogram1.Width, histogram1.Height);
            histogram1.Update();
            histogram1.Refresh();
            Console.Out.WriteLine("min="+min+" max="+max+ " iMin=" + iMin + " iMax=" + iMax + " hSize=" + hSize );
            //histogram1.Show();
        }

        private void histogram1_PositionChanged(object sender, AForge.Controls.HistogramEventArgs e)
        {
            min = e.Position;
            if (min < 0) min = 0;
        }

        private void chkLogarithmic_CheckedChanged(object sender, EventArgs e)
        {
            histogram1.IsLogarithmicView = chkLogarithmic.Checked;
            histogram1.Refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            histogram1.Values = data;
            histogram1.Refresh();
        }
    }
}
