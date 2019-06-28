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
#pragma warning disable IDE1006 // Naming Styles
    public partial class ImageDiffWindow : Form
    {
        Bitmap bmp;

        public ImageDiffWindow(Bitmap bmp) {
            InitializeComponent();
            this.bmp = bmp;
        }

        private void button1_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void ImageDiffWindow_Load(object sender, EventArgs e) {
            pictureBox1.Image = bmp;
        }
    }
}
