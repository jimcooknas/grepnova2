using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public partial class AppSettings : Form
    {
        public AppSettings()
        {
            InitializeComponent();

            if (load_app_config() != 0)
            {
                //set default values
                txtTemplatesPath.Text = "";
                txtSubjectsPath.Text = "";
                txtStartupTemplatesPath.Text = "";
                txtStartupSubjectsPath.Text = "";
                radioButton1.Checked = true;
                checkBox2.Checked = false;
                checkBox1.Checked = true;
                chkShowHistogram.Checked = true;
                chkResize.Checked = false;
                txtWinWidth.Text = "20";
                txtLogFile.Text = "logfile.txt";
                chkSort.Checked = false;
                chkBlankSubject.Checked = false;
                chkTestMenuVisible.Checked = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        int load_app_config()
        {
            if (File.Exists(Form1.app_configure_filename))
            {
                string[] lines = System.IO.File.ReadAllLines(Form1.app_configure_filename);
                foreach (string line in lines)
                {
                    string[] li = line.Split(',');
                    switch (li[0]){
                        case "localTemplatesPath":
                            txtTemplatesPath.Text = li[1].ToString();
                            if (!txtTemplatesPath.Text.EndsWith("\\")) txtTemplatesPath.Text = txtTemplatesPath.Text + "\\";
                            break;
                        case "localSubjectsPath":
                            txtSubjectsPath.Text = li[1].ToString();
                            if (!txtSubjectsPath.Text.EndsWith("\\")) txtSubjectsPath.Text = txtSubjectsPath.Text + "\\";
                            break;
                        case "StartupTemplatesPath":
                            txtStartupTemplatesPath.Text = li[1].ToString();
                            if (!txtStartupTemplatesPath.Text.EndsWith("\\")) txtStartupTemplatesPath.Text = txtStartupTemplatesPath.Text + "\\";
                            break;
                        case "StartupSubjectsPath":
                            txtStartupSubjectsPath.Text = li[1].ToString();
                            if (!txtStartupSubjectsPath.Text.EndsWith("\\")) txtStartupSubjectsPath.Text = txtStartupSubjectsPath.Text + "\\";
                            break;
                        case "Transformation type":
                            int t_type = int.Parse(li[1].ToString());
                            switch (t_type){
                                case 0:
                                    radioButton1.Checked = true;
                                    break;
                                case 1:
                                    radioButton2.Checked = true;
                                    break;
                                case 2:
                                    radioButton3.Checked = true;
                                    break;
                            }
                            break;
                        case "Use Always Negative Images":
                            checkBox2.Checked = bool.Parse(li[1].ToString());
                            break;
                        case "Stretch Image":
                            checkBox1.Checked = bool.Parse(li[1].ToString());
                            break;
                        case "Show histogram":
                            chkShowHistogram.Checked = bool.Parse(li[1].ToString());
                            break;
                        case "Resize Template if necessary":
                            chkResize.Checked = bool.Parse(li[1].ToString());
                            break;
                        case "Find brightest limit":
                            txtWinWidth.Text = int.Parse(li[1].ToString()).ToString();
                            break;
                        case "Log filename":
                            txtLogFile.Text = li[1].ToString();
                            break;
                        case "Log Level":
                            cbLogLevel.SelectedIndex = int.Parse(li[1].ToString());
                            break;
                        case "Sort images by date":
                            chkSort.Checked = bool.Parse(li[1].ToString());
                            break;
                        case "Always Blank Subject":
                            chkBlankSubject.Checked = bool.Parse(li[1].ToString());
                            break;
                        case "Plate Solver":
                            txtPlateSolverExe.Text = li[1].ToString();
                            break;
                        case "Testing Menus Visible":
                            chkTestMenuVisible.Checked = bool.Parse(li[1].ToString());
                            break;
                    }
                }
                return 0;
            }
            else
            {
                MessageBox.Show(String.Format("The configuration file '{0}' does not exist. Starting with default values", Form1.app_configure_filename), "Startup error");
                return 1;
            }
        }

        int save_app_config()
        {
            //save as text file
            string[] lines = new string[16];
            int t_type = 0;
            if (radioButton1.Checked){
                t_type = 0;
            }else if (radioButton2.Checked){
                t_type = 1;
            }else if (radioButton3.Checked){
                t_type = 2;
            }
            lines[0] = String.Format("localTemplatesPath,{0}", txtTemplatesPath.Text);
            lines[1] = String.Format("localSubjectsPath,{0}", txtSubjectsPath.Text);
            lines[2] = String.Format("StartupTemplatesPath,{0}", txtStartupTemplatesPath.Text);
            lines[3] = String.Format("StartupSubjectsPath,{0}", txtStartupSubjectsPath.Text);
            lines[4] = String.Format("Transformation type,{0}", t_type);
            lines[5] = String.Format("Use Always Negative Images,{0}", checkBox2.Checked);
            lines[6] = String.Format("Stretch Image,{0}", checkBox1.Checked);
            lines[7] = String.Format("Show histogram,{0}", chkShowHistogram.Checked);
            lines[8] = String.Format("Resize Template if necessary,{0}", chkResize.Checked);
            lines[9] = String.Format("Find brightest limit,{0}", txtWinWidth.Text);
            lines[10] = String.Format("Log filename,{0}", txtLogFile.Text);
            lines[11] = String.Format("Log Level,{0}", cbLogLevel.SelectedIndex);
            lines[12] = String.Format("Sort images by date,{0}", chkSort.Checked);
            lines[13] = String.Format("Always Blank Subject,{0}", chkBlankSubject.Checked);
            lines[14] = String.Format("Plate Solver,{0}", txtPlateSolverExe.Text);
            lines[15] = String.Format("Testing Menus Visible,{0}", chkTestMenuVisible.Checked);
            try
            {
                System.IO.File.WriteAllLines(Form1.app_configure_filename, lines);
            }catch(Exception ex){
                Form1.frm.LogError("save_app_config", "Error saving configuration file:" + ex.Message.ToString());
                if (Form1.iLogLevel > 0) Form1.frm.LogEntry("Error saving configuration file:" + ex.Message.ToString());
                Console.Out.WriteLine("Error saving configuration file:" + ex.Message.ToString());
            }
            return 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            save_app_config();
            this.Close();
        }

        private void btnTemplates_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = txtTemplatesPath.Text;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK){
                txtTemplatesPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnSubjects_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = txtSubjectsPath.Text;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtSubjectsPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnStartupTemplates_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = txtStartupTemplatesPath.Text;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtStartupTemplatesPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnStartupSubjects_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = txtStartupSubjectsPath.Text;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtStartupSubjectsPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnPlateSolverExe_Click(object sender, EventArgs e) {
            if (txtPlateSolverExe.Text.Length > 0){
                openFileDialog1.InitialDirectory =
                    txtPlateSolverExe.Text.Substring(0, txtPlateSolverExe.Text.LastIndexOf("\\"));
                openFileDialog1.FileName = "PlateSolver.exe";
            }
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtPlateSolverExe.Text = openFileDialog1.FileName;
            }
        }
    }
}
