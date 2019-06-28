using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
//using System.Linq;
//using System.Text;
using System.Windows.Forms;

namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class ServerSite : Form
    {
        private string server_path = "";
        private string username="";
        private string pass = "";
        private int maxSlashes = 0;

        public ServerSite(string srvr_path, string uname, string spass)
        {
            InitializeComponent();
            textBox1.Text = srvr_path;
            this.server_path = srvr_path;
            this.username = uname;
            this.pass = spass;
            maxSlashes = server_path.Split('/').Length;
            label2.Text=String.Format("FITS files in selected folder: {0}", FillListBox());
            //btnLight.ImageIndex = 4;
        }

        private Int32 FillListBox()
        {
            List<HttpHelper.DirectoryItem> dirItems = HttpHelper.GetDirectoryInformation(textBox1.Text, this.username, this.pass);
            if (dirItems == null) {
                Form1.ftpFiles = null;
                btnLight.ImageIndex = 4;
                button1.Enabled = false;
                lblConnection.Visible = true;
                return -1;
            }
            btnLight.ImageIndex = 3;
            listView1.Items.Clear();
            listBox1.Items.Clear();
            listView1.Items.Add("..", 0);
            Int32 filesCount = 0;
            foreach (HttpHelper.DirectoryItem pi in dirItems)
            {
                if (pi.IsDirectory)
                {
                    listView1.Items.Add(pi.Name.ToString(),0);
                }
                else
                {
                    listBox1.Items.Add(pi.Name.ToString());
                    if(pi.Name.EndsWith(".fits") || pi.Name.EndsWith(".fts") || pi.Name.EndsWith(".fit"))filesCount++;
                }
            }
            return filesCount;
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                if (listView1.SelectedItems[0].ImageIndex == 0)
                {
                    string fi = listView1.SelectedItems[0].Text;
                    if (fi.Equals(".."))
                    {
                        string[] fiList = textBox1.Text.Split('/');
                        if (fiList.Length >= maxSlashes)
                        {
                            string newFi = fiList[0];
                            for (int i = 1; i < fiList.Length - 1; i++) newFi = newFi + "/" + fiList[i];
                            textBox1.Text = newFi;
                        }
                    }
                    else if (textBox1.Text.EndsWith("/"))
                    {
                        textBox1.Text = textBox1.Text + fi;
                    }
                    else
                    {
                        textBox1.Text = textBox1.Text + "/" + fi;
                    }
                    label2.Text = String.Format("FITS files in selected folder: {0}", FillListBox());
                }
            }catch (ArgumentOutOfRangeException ex){
                Form1.frm.LogError("listView1_DoubleClick", "ArgumentOutOfRangeException: "+ex.Message.ToString());
            }
            Cursor = Cursors.Default;
        }

        private void ListBox1_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            // Draw the background of the ListBox control for each item.
            e.DrawBackground();
            // Define the default color of the brush as black.
            Brush myBrush = Brushes.Black;
            //Font myFontBold = new Font(e.Font, FontStyle.Bold);
            Font myFont = e.Font;
            Icon ico;
            // Determine the color of the brush to draw each item based 
            // on the index of the item to draw.
            if (listBox1.Items[e.Index].ToString().EndsWith(".fts") || listBox1.Items[e.Index].ToString().EndsWith(".fits") || listBox1.Items[e.Index].ToString().EndsWith(".fit"))
            {
                myBrush = Brushes.Black;
                //myFont = new Font(e.Font, FontStyle.Bold);
                ico = Properties.Resources.Icon1;
            }
            else
            {
                myBrush = Brushes.DarkGray;
                ico = Properties.Resources.NOTE06;
            }
            // Draw the current icon and item text based on the current Font and the custom brush settings.
            e.Graphics.DrawIcon(ico, new Rectangle(e.Bounds.X, e.Bounds.Y, 16,16));
            e.Graphics.DrawString(listBox1.Items[e.Index].ToString(),
                myFont, myBrush, new Rectangle(e.Bounds.X+16,e.Bounds.Y,RestoreBounds.Width,e.Bounds.Height), StringFormat.GenericDefault);
            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
        }


        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Int32 maxFiles = listBox1.Items.Count;
            Form1.ftpFiles = new string[maxFiles];
            for (int i = 0; i < maxFiles; i++)
            {
                Form1.ftpFiles[i] = listBox1.Items[i].ToString();
            }
            Form1.ftpFolderPath = textBox1.Text;
            this.Close();
        }
    }
}
