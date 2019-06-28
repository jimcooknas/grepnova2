using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.IO;
//using System.Linq;
//using System.Text;
using System.Windows.Forms;

namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class Settings : Form
    {
        //configure_filename = "grepnova.config" comes from Form1 public variable;
        private bool bShowPass = false;

        public Settings()
        {
            InitializeComponent();
            if (bShowPass){
                btnLockPass.Image = System.Drawing.Image.FromHbitmap(Properties.Resources.eye_pass_no.ToBitmap().GetHbitmap());
                txtFtpPass.PasswordChar = '\0';
            }
            else{
                btnLockPass.Image = System.Drawing.Image.FromHbitmap(Properties.Resources.eye_pass1.GetHbitmap());
                txtFtpPass.PasswordChar = '*';
            }
            load_config();
        }


        int load_config()
        {
            string tmpStr = "";
            string[] lines = System.IO.File.ReadAllLines(Form1.configure_filename);
            tmpStr = lines[0];// "## GREPNOVA CONFIGURATION FILE. COMPATIBLE WITH THE FOLLOWING VERSION NUMBER\n");
            txtVERSION.Text = lines[1];// "%s\n", VERSION);
            tmpStr = lines[2];// "## FILE PATH FOR FITS INPUT FILES (DON'T FORGET \\ AT THE END)\n");
            txtFitsPath.Text = lines[3];// "%s\n", gtk_entry_get_text(GTK_ENTRY(fits_path_enter)));
            tmpStr = lines[4];// "## FILE PATH FOR JPEG OUTPUT ILLUSTRATIONS (DON'T FORGET \\ AT THE END)\n");
            txtJpgPath.Text = lines[5] ;// "%s\n", gtk_entry_get_text(GTK_ENTRY(jpg_path_enter)));
            tmpStr = lines[6];//, "## FTP SERVER ADDRESS (e.g. databank.gr)\n");
            txtFtpServer.Text = lines[7];
            tmpStr = lines[8];//"## FTP SERVER START DIRECTORY (START-END WITH SLASHES)\n");
            txtFtpPath.Text = lines[9];
            tmpStr = lines[10];// "## FTP USER-NAME\n");
            txtFtpUser.Text = lines[11];
            tmpStr = lines[12];// "## FTP PASSWORD\n");
            txtFtpPass.Text = lines[13];
            tmpStr = lines[14];// "## JPEG ILLUSTRATION OPTIONS\n");
            string cb = lines[15];
            //Console.Out.WriteLine("{0} {1} {2} {3}", cb, cb.Substring(0, 1), cb.Substring(1, 1), cb.Substring(2, 1));
            cbIllustration1.SelectedIndex = "0TSDLF".IndexOf(cb.Substring(0, 1).ToString());
            cbIllustration2.SelectedIndex = "0TSDLF".IndexOf(cb.Substring(1, 1).ToString());
            cbIllustration3.SelectedIndex = "0TSDLF".IndexOf(cb.Substring(2, 1).ToString());
            tmpStr = lines[16];//"## SUBJECT ALWAYS NEWER THAN TEMPLATE (var SUBJECT_ALWAYS_NEWER)\n");
            if (lines[17] == "TRUE") { 
                checkBox1.Checked = true;
            }else{
                checkBox1.Checked = false;
            }
            tmpStr = lines[18];//"## COMB FOR HOT PIXELS (var FITS_COMB_HOTPIXELS)\n");
            if (lines[19] == "TRUE"){
                checkBox2.Checked = true;
            }else{
                checkBox2.Checked = false;
            }
            tmpStr = lines[20];//"## DISPLAY NEGATIVE RESULTS (var DISPLAY_NEGATIVES)\n");
            if (lines[21] == "TRUE"){
                checkBox3.Checked = true;
            }else{
                checkBox3.Checked = false;
            }
            tmpStr = lines[22];// "## DISPLAY WARNINGS (var DISPLAY_WARNINGS)\n");
            if (lines[23] == "TRUE"){
                checkBox4.Checked = true;
            }else{
                checkBox4.Checked = false;
            }
            tmpStr = lines[24];// "## VERBOSE ANALYSIS (var MAIN_VERBOSE)\n");
            if (lines[25] == "TRUE"){
                checkBox5.Checked = true;
            }else{
                checkBox5.Checked = false;
            }
            tmpStr = lines[26];// "## VERBOSE FITS IMPORTATION (var GREPNOVA_FITS_VERBOSE)\n");
            if (lines[27] == "TRUE"){
                checkBox6.Checked = true;
            }else{
                checkBox6.Checked = false;
            }
            tmpStr = lines[28];// "## VERBOSE OPTIMISATION (var LIKELIHOOD_VERBOSE)\n");
            if (lines[29] == "TRUE"){
                checkBox7.Checked = true;
            }else{
                checkBox7.Checked = false;
            }
            tmpStr = lines[30];//"## DISPLAY OPTIMISATION STEPS (var LIKELIHOOD_STEPS)\n");
            if (lines[31] == "TRUE"){
                checkBox8.Checked = true;
            }else{
                checkBox8.Checked = false;
            }
            tmpStr = lines[32];// "## VERBOSE SOURCE EXTRACTION (var STAR_VERBOSE)\n");
            if (lines[33] == "TRUE"){
                checkBox9.Checked = true;
            }else{
                checkBox9.Checked = false;
            }
            tmpStr = lines[34];// "## VERBOSE LIST OF SOURCES (var SOURCE_LIST)\n");
            if (lines[35] == "TRUE"){
                checkBox10.Checked = true;
            }else{
                checkBox10.Checked = false;
            }
            tmpStr = lines[36];// "## VERBOSE_SNE_IDENTIFICATION (var SNEFIND_VERBOSE)\n");
            if (lines[37] == "TRUE"){
                checkBox11.Checked = true;
            }else{
                checkBox11.Checked = false;
            }
            tmpStr = lines[38];// "## MAXIMUM NUMBER OF STARS EXTRACTED FROM IMAGES\n");
            txtMaxNoStar.Text = lines[39];
            tmpStr = lines[40];// "## MAXIMUM NUMBER OF STARS USED FOR PATTERN RECOGNITION\n");
            txtMaxNoStarRec.Text = lines[41];
            tmpStr = lines[42];// "## MAXIMUM NUMBER OF STARS USED TO DETERMINE SEEING\n");
            txtMaxNoStarSee.Text = lines[43];
            tmpStr = lines[44];// "## BLINKING TIME\n");
            txtBlinkTime.Text = lines[45];
            tmpStr = lines[46];// "## PRE-SNE-SEARCH OPTIMISATION STEPS\n");
            string[] tt = lines[47].Split(' ');
            txtPre11.Text = tt[0];
            txtPre12.Text = tt[1];
            string[] tt1 = lines[48].Split(' ');
            txtPre21.Text = tt1[0];
            txtPre22.Text = tt1[1];
            string[] tt2 = lines[49].Split(' ');
            txtPre31.Text = tt2[0];
            txtPre32.Text = tt2[1];
            string[] tt3 = lines[50].Split(' ');
            txtPre41.Text = tt3[0];
            txtPre42.Text = tt3[1];
            tmpStr = lines[51];// "## POST-SNE-SEARCH OPTIMISATION STEPS\n");
            string[] tt4 = lines[52].Split(' ');
            txtPost11.Text = tt4[0];
            txtPost12.Text = tt4[1];
            string[] tt5 = lines[53].Split(' ');
            txtPost21.Text = tt5[0];
            txtPost22.Text = tt5[1];
            string[] tt6 = lines[54].Split(' ');
            txtPost31.Text = tt6[0];
            txtPost32.Text = tt6[1];
            tmpStr = lines[55];// "## END\n");
            return 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            if (folderBrowserDialog1.SelectedPath.Length > 0)
            {
                txtFitsPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            if (folderBrowserDialog1.SelectedPath.Length > 0)
            {
                txtJpgPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        int save_config()
        {
            string[] lines = new string[56];
            lines[0] = "## GREPNOVA CONFIGURATION FILE. COMPATIBLE WITH THE FOLLOWING VERSION NUMBER";
            lines[1] = txtVERSION.Text;
            lines[2] = "## FILE PATH FOR FITS INPUT FILES (DON'T FORGET \\ AT THE END)";
            lines[3] = "";// txtFitsPath.Text;
            lines[4] = "## FILE PATH FOR JPEG OUTPUT ILLUSTRATIONS (DON'T FORGET \\ AT THE END)";
            lines[5] = txtJpgPath.Text;
            lines[6] = "## FTP SERVER ADDRESS (e.g. databank.gr)";
            lines[7] = txtFtpServer.Text;
            lines[8] = "## FTP SERVER START DIRECTORY (START-END WITH SLASHES)";
            lines[9] = txtFtpPath.Text;
            lines[10] = "## FTP USER-NAME";
            lines[11] = txtFtpUser.Text;
            lines[12] = "## FTP PASSWORD";
            lines[13] = txtFtpPass.Text;
            lines[14] = "## JPEG ILLUSTRATION OPTIONS";
            lines[15] = cbIllustration1.SelectedItem.ToString() + cbIllustration2.SelectedItem.ToString() + cbIllustration3.SelectedItem.ToString();
            lines[16] = "## SUBJECT ALWAYS NEWER THAN TEMPLATE (var SUBJECT_ALWAYS_NEWER)";
            if (checkBox1.Checked == true)
            {
                lines[17] = "TRUE";
            }
            else
            {
                lines[17] = "FALSE";
            }
            lines[18] = "## COMB FOR HOT PIXELS (var FITS_COMB_HOTPIXELS)";
            if (checkBox2.Checked == true)
            {
                lines[19] = "TRUE";
            }
            else
            {
                lines[19] = "FALSE";
            }
            lines[20] = "## DISPLAY NEGATIVE RESULTS (var DISPLAY_NEGATIVES)";
            if (checkBox3.Checked == true)
            {
                lines[21] = "TRUE";
            }
            else
            {
                lines[21] = "FALSE";
            }
            lines[22] = "## DISPLAY WARNINGS (var DISPLAY_WARNINGS)";
            if (checkBox4.Checked == true)
            {
                lines[23] = "TRUE";
            }
            else
            {
                lines[23] = "FALSE";
            }
            lines[24] = "## VERBOSE ANALYSIS (var MAIN_VERBOSE)";
            if (checkBox5.Checked == true)
            {
                lines[25] = "TRUE";
            }
            else
            {
                lines[25] = "FALSE";
            }
            lines[26] = "## VERBOSE FITS IMPORTATION (var GREPNOVA_FITS_VERBOSE)";
            if (checkBox6.Checked == true)
            {
                lines[27] = "TRUE";
            }
            else
            {
                lines[27] = "FALSE";
            }
            lines[28] = "## VERBOSE OPTIMISATION (var LIKELIHOOD_VERBOSE)";
            if (checkBox7.Checked == true)
            {
                lines[29] = "TRUE";
            }
            else
            {
                lines[29] = "FALSE";
            }
            lines[30] = "## DISPLAY OPTIMISATION STEPS (var LIKELIHOOD_STEPS)";
            if (checkBox8.Checked == true)
            {
                lines[31] = "TRUE";
            }
            else
            {
                lines[31] = "FALSE";
            }
            lines[32] =  "## VERBOSE SOURCE EXTRACTION (var STAR_VERBOSE)";
            if (checkBox9.Checked == true)
            {
                lines[33] = "TRUE";
            }
            else
            {
                lines[33] = "FALSE";
            }
            lines[34] = "## VERBOSE LIST OF SOURCES (var SOURCE_LIST)";
            if (checkBox10.Checked == true)
            {
                lines[35] = "TRUE";
            }
            else
            {
                lines[35] = "FALSE";
            }
            lines[36] = "## VERBOSE_SNE_IDENTIFICATION (var SNEFIND_VERBOSE)";
            if (checkBox11.Checked == true)
            {
                lines[37] = "TRUE";
            }
            else
            {
                lines[37] = "FALSE";
            }
            lines[38] = "## MAXIMUM NUMBER OF STARS EXTRACTED FROM IMAGES";
            lines[39] = txtMaxNoStar.Text;
            lines[40] = "## MAXIMUM NUMBER OF STARS USED FOR PATTERN RECOGNITION";
            lines[41] = txtMaxNoStarRec.Text;
            lines[42] = "## MAXIMUM NUMBER OF STARS USED TO DETERMINE SEEING";
            lines[43] = txtMaxNoStarSee.Text;
            lines[44] = "## BLINKING TIME";
            lines[45] = txtBlinkTime.Text;
            lines[46] = "## PRE-SNE-SEARCH OPTIMISATION STEPS";
            lines[47] = String.Format("{0} {1}  # Optimise orientation first", txtPre11.Text, txtPre12.Text);
            lines[48] = String.Format("{0} {1}  # Find convolution width", txtPre21.Text, txtPre22.Text);
            lines[49] = String.Format("{0} {1}  # Optimise brightness", txtPre31.Text, txtPre32.Text);
            lines[50] = String.Format("{0} {1}  # Find noise level (do this last)", txtPre41.Text, txtPre42.Text);
            lines[51] = "## POST-SNE-SEARCH OPTIMISATION STEPS";
            lines[52] = String.Format("{0} {1}  # Optimise SNe position", txtPost11.Text, txtPost12.Text);
            lines[53] = String.Format("{0} {1}  # Optimise brightness / size", txtPost21.Text, txtPost22.Text);
            lines[54] = String.Format("{0} {1}  # Find noise level (do this last)", txtPost31.Text, txtPost32.Text);
            lines[55] = "## END";
            System.IO.File.WriteAllLines(Form1.configure_filename, lines);
            return 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            save_config();
            this.Close();
        }

        private void btnLockPass_Click(object sender, EventArgs e)
        {
            bShowPass = !bShowPass;
            if (bShowPass)
            {
                btnLockPass.Image = System.Drawing.Image.FromHbitmap(Properties.Resources.eye_pass_no.ToBitmap().GetHbitmap());
                txtFtpPass.PasswordChar = '\0';
            }
            else
            {
                btnLockPass.Image = System.Drawing.Image.FromHbitmap(Properties.Resources.eye_pass1.GetHbitmap());
                txtFtpPass.PasswordChar = '*';
            }
        }
    }
}
