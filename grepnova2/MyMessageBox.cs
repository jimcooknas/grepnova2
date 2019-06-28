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
    public partial class MyMessageBox : Form
    {
        public MyMessageBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Custom MessageBox
        /// </summary>
        /// <param name="title">The title of the message-dialog</param>
        /// <param name="text_type">if text is plain text (0) or html formatted (1)</param>
        /// <param name="msg">The text of the message-dialog</param>
        /// <param name="Cbtn">The number of Buttons on the message-dialog. Default is 0</param>
        /// <param name="btnOK_text">The text to appear on btnOK button</param>
        /// <param name="btnNO_text">The text to appear on btnNO button</param>
        /// <param name="btnYES_text">The text to appear on btnYES button</param>
        /// <param name="imgtype">The type of picturebox (0=info, 1=warning, 2=critical). Default is 0</param>
        /// <returns></returns>
        public DialogResult ShowDialog(string title, int text_type , string msg, int Cbtn = 0, string btnOK_text="OK", string btnYES_text="", string btnNO_text="", int imgtype=0)
        {
            //if it is html then set the background color
            if (text_type==1)msg = "<body bgcolor='#F5F5F5'>" + msg + "</body>";
            switch (imgtype)
            {
                case 0:
                    pictureBox1.Image = null;
                    break;
                case 1:
                    pictureBox1.Image = Properties.Resources.if_messagebox_info;
                    break;
                case 2:
                    pictureBox1.Image = Properties.Resources.if_messagebox_warning;
                    break;
                case 3:
                    pictureBox1.Image = Properties.Resources.if_messagebox_critical;
                    break;
                default:
                    pictureBox1.Image = null;
                    break;
            }
            this.Text = title;
            switch (text_type)
            {
                case 0://text
                    textBox1.Visible = true;
                    webBrowser1.Visible = false;
                    textBox1.Text = msg;
                    break;
                case 1://html
                    textBox1.Visible = false;
                    webBrowser1.Visible = true;
                    webBrowser1.DocumentText = msg;
                    break;
                default:
                    textBox1.Visible = true;
                    webBrowser1.Visible = false;
                    textBox1.Text = msg;
                    break;
            }
            btnOK.Text = btnOK_text;
            btnNO.Text = btnNO_text;
            btnYES.Text = btnYES_text;

            switch (Cbtn){
                case 0:
                    btnNO.Visible = false;
                    btnYES.Visible = false;
                    btnOK.Visible = false;
                    break;
                case 1:
                    btnNO.Visible = false;
                    btnYES.Visible = false;
                    btnOK.Visible = true;
                    break;
                case 2:
                    btnNO.Visible = true;
                    btnYES.Visible = true;
                    btnOK.Visible = false;
                    break;
                case 3:
                    btnNO.Visible = true;
                    btnYES.Visible = true;
                    btnOK.Visible = true;
                    break;
            }
            return this.ShowDialog();
        }

    }
}
