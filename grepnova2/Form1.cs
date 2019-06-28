/*
 * ******* History *********************************************************
    version 1.1.8   @ 20181015: First release 
    version 1.2.0   @ 20190317: added shortcuts T S A B < > F ArrowUp ArrowDown Brightness Up/DownIronPython
                                added Class DSS to downoad frames from DSS
                                Internal Alignment (alignment_fit in Align class) without using grepnova-align.bin.exe
                                Plate Solve by Astrometry.net direct or All Sky Plate Solver if installed
    version 1.3.0   @ 20190407  Save selected Subject as new Template
                                Shortcuts are selectable now.
                                Form HotKeys.cs allows user to choose his shortcuts (from About --> HotKeys)
                    @ 20190419  Similarity class
                                Accord.net Cobyla algorithm implemented for optimization
*/

using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using AForge.Imaging.Filters;
using System.Threading.Tasks;
using System.Threading;
//using AllSkyPlateSolver;
//using IronPython.Hosting;
//using Microsoft.Scripting;
//using Microsoft.Scripting.Hosting;
//using Emgu.CV;
//using Emgu.CV.Structure;
//using Emgu.CV.CvEnum;
//using Emgu.CV.Util;
using Accord.Math.Optimization;

//FWHM_arcsec = FWHM_pixels x ( pixel_size_μm / focal_length_mm) x 206.3
//
//The formula for the simple sharpening algorithm is,
//  Lsharp(x) = (L(x) - ksharp/2 * (L(x-V) + L(x+V))) / (1-ksharp)
//  L(x) is the input pixel level and Lsharp(x) is the sharpened pixel level.
//  Ksharp is the sharpening constant (related to the slider setting scanning or editing program). 
//  V is the shift used for sharpening. 
//  V = R/dscan, where R is the sharpening radius(the number of pixels between original image 
//  and shifted replicas) in pixels. 1/dscan is the spacing between pixels.

namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0018 // Naming Styles
    public partial class Form1 : Form
    {
        bool bShowTestingItems = true;
        int alignMethod = 1; // 0 = grepnova-align.bin.exe method, 1 = Accord method
        Align.proposed_alignment currentAlign;
        StreamWriter logFile = null;
        bool printStarList = true;
        string logFileName = "grepnova_log.txt";
        public static Cursor cur = new Cursor(Properties.Resources.cross.Handle);
        public static int cursorType = 0; //values 0=cross and 1=circle
        public static IntPtr[] cursorHandle = { Properties.Resources.cross.Handle,
                                                Properties.Resources.circle.Handle
                                              };
        public static IntPtr[] cursorHandleBlack = { Properties.Resources.cross_black.Handle,
                                                     Properties.Resources.circle_black.Handle
                                                   };
        public static bool bDontRun = false;
        public static bool bNotChangeSelection = false;
        public static bool bResizeTemplate = false;//if we have to resize Template if its size differs from Subject
        public static string[] ftpFiles = null;
        public static string ftpFolderPath = null;
        public static string localTemplatesPath = null;//where the downloaded Template files stored
        public static string localSubjectsPath = null;//where the downloaded Subject files stored
        public static string startupTemplatesPath = null;//where the startup batch loading begins
        public static string startupSubjectsPath = null;//where the startup batch loading begins
        //public static string localTemplatesSub = "REF";
        //public static string localSubjectsSub = "NEW";
        public static string localImageDir = "";
        public static bool isNegative = false;
        public static bool bSortByDate = true;
        public static bool bBlankSubject = false;
        public static bool isAlignedBlack = false;//tells if the program has achieved to align images.
        public static Fits.imageptr[] image = new Fits.imageptr[4];
        Fits.imageptr zoom_image = new Fits.imageptr();
        CooknasStar.Centroid centroid = new CooknasStar.Centroid();
        string graphText = "";
        Bitmap[] copyImage = new Bitmap[3];
        int image_no = 0;
        string[] image_type = { "Template", "Subject", "Alligned Templ.", "Blinking" };
        string[] image_type_short = { "Tmpl", "Subj", "ATmp", "Blnk" };
        int defaultImageX = 765;
        int defaultImageY = 510;
        int halfSize = 30;
        int mouseX;
        int mouseY;
        double zoomScale = 2.0;
        int currentX;
        int currentY;
        int delayTime = 500;//milliseconds
        int alignType = 1;//0=fits 1=jpg //Don't change it, leave it 1
        public static string configure_filename = "grepnova.config";
        public static string app_configure_filename = "grepnova2.cfg";
        public static int iLogLevel = 0;
        public static string align_error_output = "";
        public static string align_output_output = "";
        public static bool autoSearchIsOn = false;
        public static int candidatesFound = 0;
        string ftpServer = "";
        string ftpStartPath = "";
        string ftpUserName = "";
        string ftpPassword = "";
        double[] gamma = { 1.0, 1.0, 1.0 };
        public static int[] iMin = { 0, 0, 0 };
        public static int[] iMax = { 65535, 65535, 65535 };
        public static int[] iMaxDefault = { 65535, 65535, 65535 };
        public static int[] iMinDefault = { 0, 0, 0 };
        public static int[] iCurrentMin = { 0, 0, 0 };
        public static int[] iCurrentMax = { 65535, 65535, 65535 };
        public static float[][] histogram_data = new float[3][];
        Fits.transform_type trans_type = Fits.transform_type.LIN;
        List<Fits.PointValue>[] Circles = new List<Fits.PointValue>[3];
        List<Fits.PointValue>[] Crosses = new List<Fits.PointValue>[3];
        Fits.PointValue FoundSN = new Fits.PointValue();
        List<Fits.TriplePoint>[] Lines = new List<Fits.TriplePoint>[3];

        public static BitmapFiltering bmFiltering;
        public static bool bBitmapFilteringIsOn = false;
        public static int bitmapFilterSelected = 0;
        public static bool bStretchImageIsOn = false;
        public static bool bSharpeningIsOn = false;
        public static bool bImageCurveIsOn = false;
        public static int iStretchImagePosition = 1;//1=StretchImage is up 2=StretchImage is down
        public static StretchImage stretchImage;
        public static SharpenFITS sharpenFITS;
        public static ImageCurveForm imageCurveForm;
        float sharpenK = 1.0f;
        int sharpenDscan = 0;
        public static Form1 frm;
        MaxForm2 maxForm2;
        int accordOptimizeMethod = 0;//version 1.3 @ 20190420: 0=Nelder-Mead algorithm 1=Cobyla algorithm
        bool bTestMenusVisible = false;

        public static string plateSolverPath = @"C:\Program Files (x86)\PlateSolver\PlateSolver.exe";

        public struct FilterParams
        {
            public bool textParamVisibility;
            public string textLabel;
            public string textDefaultValue;
            public bool textParamVisibility2;
            public string textLabel2;
            public string textDefaultValue2;
            public bool checkParamVisibility;
            public string checkParamText;
            public string funcName;

            public FilterParams(bool p1, string p2, string p3, bool p4, string p5, string p6, bool p7, string p8, string p9) {
                textParamVisibility = p1;
                textLabel = p2;
                textDefaultValue = p3;
                textParamVisibility2 = p4;
                textLabel2 = p5;
                textDefaultValue2 = p6;
                checkParamVisibility = p7;
                checkParamText = p8;
                funcName = p9;
            }
        }
        public static FilterParams[] fltParam = {new FilterParams(false, "", "", false, "", "", false, "", "None"),
                                   new FilterParams(true, "Matrix size", "3", false, "", "", false, "", "Median Filter"),
                                   new FilterParams(false, "", "", false, "", "", false, "", "Negative Filter"),
                                   new FilterParams(true, "Color level","255", false, "", "", true, "Reverse", "Color Balance"),
                                   new FilterParams(false, "", "", false, "", "", false, "", "Noise Removal"),
                                   new FilterParams(false, "", "", false, "", "", false, "", "Histogram Equalize"),
                                   new FilterParams(true, "Threshold (0-255)", "50", false, "", "", false, "", "Threshold Filter"),
                                   new FilterParams(false, "", "", false, "", "", false, "", "Equalize"),
                                   new FilterParams(true,"Min","0", true, "Max", "255", false,"","Normalization"),
                                   new FilterParams(true,"Range Median","0,9",false,"","",false,"","Linear Levels"),
                                   new FilterParams(false,"","",false,"","",false,"","Pixelate-Blur"),
                                   new FilterParams(true,"Freq. Low","18",true,"High","100",false,"","Frequency Filter")
        };

        public struct files_dates
        {
            public int index;
            public string path;
            public string filename;
            public string name;
            public string date;
            public bool bIsFTP;
        }
        List<files_dates> tempData = new List<files_dates>();
        List<files_dates> subjData = new List<files_dates>();
        int selectedTemp;
        int selectedSubj;

        // version 1.3.0   @ 20190407
        // ShortCut Keys
        Dictionary<string, int> dictShortcut = new Dictionary<string, int>();
        Keys shcutTemplate = Keys.T;
        Keys shcutSubject = Keys.S;
        Keys shcutAligned = Keys.A;
        Keys shcutBlink = Keys.B;
        Keys shcutNext = Keys.Right;
        Keys shcutPrev = Keys.Left;
        Keys shcutZoom = Keys.Z;
        Keys shcutTempUp = Keys.Shift | Keys.T;
        Keys shcutTempDown = Keys.Control | Keys.T;
        Keys shcutSubjUp = Keys.Shift | Keys.S;
        Keys shcutSubjDown = Keys.Control | Keys.S;
        Keys shcutAlignUp = Keys.Shift | Keys.A;
        Keys shcutAlignDown = Keys.Control | Keys.A;
        Keys shcutRefreshBatch = Keys.R;
        Keys shcutHeaders = Keys.H;
        Keys shcutRemoveAnnot = Keys.N;
        Keys shcutStretch = Keys.I;
        Keys shcutCurve = Keys.C;
        Keys shcutSaveAsTemp = Keys.W;
        Keys shcutDownload = Keys.D;
        Keys shcutBlank = Keys.L;
        string shortcutFileName = "AppShortcut.txt";

        //wavelength index and dictionary
        int wavelengthIndex = 0;
        Dictionary<string, int> wavelength = new Dictionary<string, int>()
        { {"None", 0},      // color: 000000
            {"Green", 94},    // color: 00FF00
            {"Teal", 131},    // color: 008080
            {"Red", 304},     // color: FF0000
            {"Blue", 335},    // color: 0000FF
            {"Gold", 171},    // color: FFD700
            {"Copper", 193},  // color: B87333
            {"Purple", 211},  // color: 800080
            {"Ocher", 1600},  // color: BBBB00
            {"Pink", 1700},   // color: FFC0CB
            {"Silver", 4500}  // color: C0C0C0
        };

        //Comparators for sorting files_data structures chronologically or alphabetically
        public class CustomDateComparer : IComparer<files_dates>
        {
            private readonly IComparer<files_dates> _baseComparer;

            public CustomDateComparer() {
            }

            public CustomDateComparer(IComparer<files_dates> baseComparer) {
                _baseComparer = baseComparer;
            }
            public int Compare(files_dates x, files_dates y) {
                return x.date.CompareTo(y.date);
            }
        }

        public class CustomNameComparer : IComparer<files_dates>
        {
            private readonly IComparer<files_dates> _baseComparer;

            public CustomNameComparer() {
            }

            public CustomNameComparer(IComparer<files_dates> baseComparer) {
                _baseComparer = baseComparer;
            }
            public int Compare(files_dates x, files_dates y) {
                return x.name.CompareTo(y.name);
            }
        }

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0017 // Simplify object initialization

        public Form1() {
            InitializeComponent();
            //Show-Hide testing menu items
            pyrhontestingToolStripMenuItem.Visible = bShowTestingItems;
            utilitiesToolStripMenuItem.Visible = bShowTestingItems;
            //create a reference to this form
            frm = this;
            //do initialization
            string ver = "version " + Assembly.GetExecutingAssembly().GetName().Version.Major + "." + Assembly.GetExecutingAssembly().GetName().Version.Minor + "  (build " + Assembly.GetExecutingAssembly().GetName().Version.Build + " rev. " + Assembly.GetExecutingAssembly().GetName().Version.Revision + ")";
            this.Text = "Grepnova2       " + ver;
            for (int i = 0; i < 4; i++)
                image[i] = Fits.CreateBlackImage(defaultImageX, defaultImageY);
            picZoom.Image = Fits.GetFITSImage(Fits.CreateBlackImage((int)(2 * zoomScale * halfSize), (int)(2 * zoomScale * halfSize)), 1.0, trans_type, iMin[0], iMax[0]);
            pictureBox1.Image = Fits.GetFITSImage(image[0], gamma[0], trans_type, -1, -1, wavelengthIndex);//, iMin[0], iMax[0]);
            pictureBox2.Image = Fits.GetFITSImage(image[1], gamma[1], trans_type, -1, -1, wavelengthIndex);//, iMin[1], iMax[1]);
            pictureBox3.Image = Fits.GetFITSImage(image[2], gamma[2], trans_type, -1, -1, wavelengthIndex);//, iMin[2], iMax[2]);
            //load_application settings
            load_app_config();
            //load grepnova
            if (load_config() == 0)
            {
                txtBlinkTime.Text = String.Format("{0}", delayTime);
                //Console.Out.WriteLine("selectListEntries(lstTemplates, txtTemplatesDir.Text)");
                selectListEntries(lstTemplates, txtTemplatesDir.Text);
                //Console.Out.WriteLine("selectListEntries(lstSubjects, txtTemplatesDir.Text)");
                selectListEntries(lstSubjects, txtSubjectsDir.Text);
                //Console.Out.WriteLine("lstSubjects.SelectedIndex = 0");
                if (lstTemplates.Items.Count > 0 && lstSubjects.Items.Count > 0) lstSubjects.SelectedIndex = 0;
            }
            //Console.Out.WriteLine("Out of Init Loop");
            for (int i = 0; i < fltParam.Length; i++) comboBox1.Items.Add(fltParam[i].funcName);
            comboBox1.SelectedIndex = 0;
            if (isNegative){
                cur = new Cursor(cursorType == 0 ? Properties.Resources.cross_black.Handle : Properties.Resources.circle_black.Handle);
            }else{
                cur = new Cursor(cursorType == 0 ? Properties.Resources.cross.Handle : Properties.Resources.circle.Handle);
            }
            pictureBox1.Cursor = cur;
            pictureBox2.Cursor = cur;
            pictureBox3.Cursor = cur;
            Cursor.Show();
            selectionRangeSlider1.TextColor = null;
            selectionRangeSlider1.Offset = 10;
            checkBox1.Checked = bStretchImageIsOn;//start with deactivated
            if (bStretchImageIsOn) { bStretchImageIsOn = false; stretchImageToolStripMenuItem_Click(this, null); }
            cookHist.OffsetX = 24;
            cookHist.Visible = chkShowHistogram.Checked;
            cooknasStar.Visible = !chkShowHistogram.Checked;
            logFile = new System.IO.StreamWriter(logFileName, true);
            // version 1.3.0   @ 20190407
            string[] lines = System.IO.File.ReadAllLines("Shortcuts.txt");
            foreach (string li in lines){
                string[] ss = li.Split('\t');
                dictShortcut.Add(ss[0], int.Parse(ss[1]));
            }
            GetKeysFromFile(shortcutFileName);
        }

        // version 1.3.0   @ 20190407
        private void GetKeysFromFile(string fn) {
            string[] lines = System.IO.File.ReadAllLines(fn);
            foreach (string ss in lines)
            {
                string[] sss = ss.Split('\t');
                string fld = sss[0];
                string special = sss[1];
                string key = sss[2];
                string value = sss[3];
                Keys specialKey = Keys.None;
                switch (special){
                    case "Ctrl":
                        specialKey = Keys.Control;
                        break;
                    case "Shift":
                        specialKey = Keys.Shift;
                        break;
                    case "Alt":
                        specialKey = Keys.Alt;
                        break;
                }
                switch (fld) {
                    case "Template":
                        shcutTemplate = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Subject":
                        shcutSubject = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Aligned Template":
                        shcutAligned = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Blink":
                        shcutBlink = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "ZoomForm":
                        shcutZoom = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Next":
                        shcutNext = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Previous":
                        shcutPrev = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Template Bright Up":
                        shcutTempUp = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Template Bright Down":
                        shcutTempDown = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Subject Bright Up":
                        shcutSubjUp = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Subject Bright Down":
                        shcutSubjDown = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Aligned Template Bright Up":
                        shcutAlignUp = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Aligned Template Bright Down":
                        shcutAlignDown = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Refresh Batch":
                        shcutRefreshBatch = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Show Images Headers":
                        shcutHeaders= (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Remove Annotations":
                        shcutRemoveAnnot= (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Stretch Image":
                        shcutStretch = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Image Curve":
                        shcutCurve = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Save As Template":
                        shcutSaveAsTemp = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Download from DSS":
                        shcutDownload = (specialKey | (Keys)dictShortcut[key]);
                        break;
                    case "Blank Subject":
                        shcutBlank = (specialKey | (Keys)dictShortcut[key]);
                        break;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
            // Nothing here
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            tabControl1.SelectedIndex = 0;
            logFile.Close();
        }

        //************ Static Methods to access non static Methods of Form1 ***********************

        //is needed in 'BitmapFiltering.cs'
        public static void ApplyFilter(object sender, EventArgs e, int cbIndex, string tFilterParam, string tFilterParam2, bool cFilterParam) {
            frm.comboBox1.SelectedIndex = bmFiltering.comboBox1.SelectedIndex;
            frm.txtFilterParam.Text = bmFiltering.txtFilterParam.Text;
            frm.txtFilterParam2.Text = bmFiltering.txtFilterParam2.Text;
            frm.chkFilterParam.Checked = bmFiltering.chkFilterParam.Checked;
            frm.cmdApplyFilter_Click(sender, e);
        }

        //is needed in 'BitmapFiltering.cs'
        public static void RestoreBitmaps(object sender, EventArgs e, int what) {
            switch (what)
            {
                case 0://current image
                    frm.btnRestoreCurrent_Click(sender, e);
                    break;
                case 1://all images
                    frm.btnRestoreAll_Click(sender, e);
                    break;
            }
        }

        //is needed in 'StretchImage.cs'
        public static void SliderSelectionChanged(object sender, EventArgs e, int selMin, int selMax, int selSelectedMin, int selSelectedMax) {
            bNotChangeSelection = true;
            frm.selectionRangeSlider1.Min = selMin;
            frm.selectionRangeSlider1.Max = selMax;
            frm.selectionRangeSlider1.SelectedMin = selSelectedMin;
            frm.selectionRangeSlider1.SelectedMax = selSelectedMax;
            bNotChangeSelection = false;
            frm.selectionRangeSlider1_SelectionChanged(sender, e);
        }

        //is needed in 'SharpenFITS.cs'
        public static void ApplySharpenFITS(object sender, EventArgs e, float k, int dscan) {
            frm.sharpenK = k;
            frm.sharpenDscan = dscan;
            frm.cmdApplySharpen_Click(sender, e);
        }


        public static void changeLineInTextFile(string filename, string strToChange, int lineNo) {
            if (File.Exists(filename))
            {
                string[] lines = System.IO.File.ReadAllLines(filename);
                //if asked line is different from strToChange then change line
                if (!lines[lineNo].Equals(strToChange))
                {
                    lines[lineNo] = strToChange;
                    //save file back
                    System.IO.File.WriteAllLines(filename, lines);
                }
            }
        }
        //is needed all over the application
        public static string friendly_timestring() {
            return DateTime.Now.ToString("MM/dd/yy HH:mm:ss.fff");
        }

        public static int getImageMin(Fits.imageptr img) {
            return (int)(img.data_lower_decile - (img.data_upper_decile - img.data_lower_decile) / 2.0);
        }

        public static int getImageMax(Fits.imageptr img) {
            return (int)(img.data_upper_decile + (img.data_upper_decile - img.data_lower_decile) / 2.0);
        }

        //********** End of static Methods *******************************

        public void LogEntry(string txt) {
            txtLog.AppendText(DateTime.Now.ToString("[MM/dd/yy HH:mm:ss.fff]") + " " + txt + "\r\n");
            //alternatively:
            //txtLog.Text = txtLog.Text + DateTime.Now.ToString("[MM/dd/yy HH:mm:ss.fff]") + " " + txt + "\r\n";
            //txtLog.SelectionStart = txtLog.TextLength;
            //txtLog.ScrollToCaret();
        }

        public void LogError(string where, string txt) {
            if (logFile != null)
            {
                logFile.WriteLine(DateTime.Now.ToString("[MM/dd/yy HH:mm:ss.fff]") + " Error in " + where + "-> " + txt);
            }
        }

        //load the confiburation file of grepnova
        int load_config() {
            try
            {
                if (File.Exists(configure_filename))
                {
                    string[] lines = System.IO.File.ReadAllLines(configure_filename);
                    if (lines[5].EndsWith("\\"))
                    {
                        localImageDir = lines[5];
                    }
                    else
                    {
                        if (lines[5].Length > 0)
                        {
                            localImageDir = lines[5] + "\\";
                        }
                        else
                        {
                            localImageDir = "";
                        }
                    }
                    //localImageDir = lines[5];
                    ftpServer = lines[7];
                    ftpStartPath = lines[9];
                    ftpUserName = lines[11];
                    ftpPassword = lines[13];
                    txtBlinkTime.Text = lines[45];
                    delayTime = Int32.Parse(lines[45]);
                    //remember: we do need to have 'grepnova.config' file without Fits path 
                    //if configuration file contains non-empty value for FITS PATH then set it to empty string ("")
                    if (!lines[3].Equals("")) changeLineInTextFile(configure_filename, "", 3);
                    return 0;
                }
                else
                {
                    if (iLogLevel > 0) LogEntry("The configuration file '" + configure_filename + "' does not exist. Starting with default values");
                    MessageBox.Show(String.Format("The configuration file '{0}' does not exist. Starting with default values", configure_filename), "Startup error");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                if (iLogLevel > 0) LogEntry("Malformed file '" + configure_filename + "'. Starting with default values\r\nException: " + ex.Message.ToString());
                MessageBox.Show("Malformed file '" + configure_filename + "'. Starting with default values", "Startup error");
                return 1;
            }
        }

        //load the confiburation file of Grepnova2
        int load_app_config() {
            try{
                if (File.Exists(app_configure_filename)){
                    string[] lines = System.IO.File.ReadAllLines(app_configure_filename);
                    foreach (string line in lines){
                        string[] li = line.Split(',');
                        switch (li[0]){
                            case "localTemplatesPath":
                                localTemplatesPath = li[1].ToString();
                                if (!localTemplatesPath.EndsWith("\\")) localTemplatesPath = localTemplatesPath + "\\";
                                if (!Directory.Exists(localTemplatesPath)){
                                    if (MessageBox.Show("Local Templates folder '" + localTemplatesPath + "' does not exist. Go you want to create it?", "Local Templates Folder", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                    {
                                        try{
                                            Directory.CreateDirectory(localTemplatesPath);
                                        }catch (Exception ex){
                                            LogError("load_app_config", "Could not create Local Templates folder. Error: " + ex.Message.ToString());
                                            LogEntry("Could not create Local Templates folder. Error: " + ex.Message.ToString());
                                        }
                                    }
                                }
                                break;
                            case "localSubjectsPath":
                                localSubjectsPath = li[1].ToString();
                                if (!localSubjectsPath.EndsWith("\\")) localSubjectsPath = localSubjectsPath + "\\";
                                if (!Directory.Exists(localSubjectsPath)){
                                    if (MessageBox.Show("Local Subjects folder '" + localSubjectsPath + "' does not exist. Go you want to create it?", "Local Subjects Folder", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                    {
                                        try{
                                            Directory.CreateDirectory(localSubjectsPath);
                                        }catch (Exception ex){
                                            LogError("load_app_config", "Could not create Local Subjects folder. Error: " + ex.Message.ToString());
                                            LogEntry("Could not create Local Subjects folder. Error: " + ex.Message.ToString());
                                        }
                                    }
                                }
                                break;
                            case "StartupTemplatesPath":
                                startupTemplatesPath = li[1].ToString();
                                if (!startupTemplatesPath.EndsWith("\\")) startupTemplatesPath = startupTemplatesPath + "\\";
                                if (!Directory.Exists(startupTemplatesPath)){
                                    MessageBox.Show("Startup Templates folder '" + startupTemplatesPath + "' does not exist. We continue with Local one", "Startup Templates Folder", MessageBoxButtons.OK);
                                    startupTemplatesPath = localTemplatesPath;
                                }
                                break;
                            case "StartupSubjectsPath":
                                startupSubjectsPath = li[1].ToString();
                                if (!startupSubjectsPath.EndsWith("\\")) startupSubjectsPath = startupSubjectsPath + "\\";
                                if (!Directory.Exists(startupSubjectsPath)){
                                    MessageBox.Show("Startup Subjects folder '" + startupSubjectsPath + "' does not exist. We continue with Local one", "Startup Subjects Folder", MessageBoxButtons.OK);
                                    startupSubjectsPath = localSubjectsPath;
                                }
                                break;
                            case "Transformation type":
                                int t_type = int.Parse(li[1].ToString());
                                switch (t_type){
                                    case 0:
                                        radioButton1.Checked = true;
                                        trans_type = Fits.transform_type.LIN;
                                        break;
                                    case 1:
                                        radioButton2.Checked = true;
                                        trans_type = Fits.transform_type.LOG;
                                        break;
                                    case 2:
                                        radioButton3.Checked = true;
                                        trans_type = Fits.transform_type.GAM;
                                        break;
                                }
                                break;
                            case "Use Always Negative Images":
                                checkBox2.Checked = bool.Parse(li[1].ToString());
                                isNegative = checkBox2.Checked;
                                break;
                            case "Stretch Image":
                                checkBox1.Checked = bool.Parse(li[1].ToString());
                                bStretchImageIsOn = checkBox1.Checked;
                                break;
                            case "Show histogram":
                                chkShowHistogram.Checked = bool.Parse(li[1].ToString());
                                break;
                            case "Resize Template if necessary":
                                bResizeTemplate = bool.Parse(li[1].ToString());
                                resizeTemplateWhenNeededToolStripMenuItem.Checked = bResizeTemplate;
                                break;
                            case "Find brightest limit":
                                txtWinWidth.Text = int.Parse(li[1].ToString()).ToString();
                                break;
                            case "Log filename":
                                logFileName = li[1].ToString();
                                break;
                            case "Log Level":
                                iLogLevel = int.Parse(li[1].ToString());
                                lblLogLevel.Text = String.Format("Log Level: {0}", iLogLevel);
                                break;
                            case "Sort images by date":
                                bSortByDate = bool.Parse(li[1].ToString().ToLower());
                                break;
                            case "Always Blank Subject":
                                bBlankSubject = bool.Parse(li[1].ToString());
                                blankSubjectToolStripMenuItem.Checked = bBlankSubject;
                                break;
                            case "Plate Solver":
                                string plateSolver = li[1].ToString();
                                if (plateSolver.Equals("") || !File.Exists(plateSolver)){
                                    MessageBox.Show("All Sky Plate Solver '" + plateSolver + "' does not exist.", "Plate Solver Executable", MessageBoxButtons.OK);
                                    plateSolver = plateSolverPath;
                                }
                                plateSolverPath = plateSolver;
                                break;
                            case "Testing Menus Visible":
                                bTestMenusVisible = bool.Parse(li[1].ToString());
                                break;
                        }
                    }
                    txtTemplatesDir.Text = startupTemplatesPath;
                    txtSubjectsDir.Text = startupSubjectsPath;
                    foreach(ToolStripMenuItem mi in menuStrip1.Items){
                        if (mi.Text.Contains("testing")) mi.Visible = bTestMenusVisible;
                    } 
                    return 0;
                }else{
                    if (iLogLevel > 0) LogEntry("The configuration file '" + app_configure_filename + "' does not exist. Starting with default values");
                    MessageBox.Show(String.Format("The configuration file '{0}' does not exist. Starting with default values", app_configure_filename), "Startup error");
                    return 1;
                }
            }catch (Exception ex){
                if (iLogLevel > 0) LogEntry("Malformed file '" + app_configure_filename + "'. Starting with default values\r\nException: " + ex.Message.ToString());
                MessageBox.Show("Malformed file '" + app_configure_filename + "'. Starting with default values", "Startup error");
                return 1;
            }
        }


        private void cmdTemplatesDir_Click(object sender, EventArgs e) {
            folderBrowserDialog1.Description = "Please select the Templates folder (the folder where older frames reside)";
            if (chkFTPTemplates.Checked)
            {
                ftpFiles = null;
                ServerSite serverSite = new ServerSite("ftp://" + ftpServer + ftpStartPath, ftpUserName, ftpPassword);
                serverSite.ShowDialog();
                if (ftpFiles != null)
                {
                    //lstTemplates.Items.Clear();//do not clear list
                    Int32 maxFiles = ftpFiles.Length;
                    if (!ftpFolderPath.EndsWith("/")) ftpFolderPath += "/";
                    for (int i = 0; i < maxFiles; i++)
                    {
                        if (iLogLevel > 2) Console.Out.WriteLine("ftpFile[" + i + "]=" + ftpFiles[i]);
                        if (ftpFiles[i].ToLower().EndsWith(".fts") || ftpFiles[i].ToLower().EndsWith(".fits") || ftpFiles[i].ToLower().EndsWith(".fit"))
                        {
                            files_dates fd = new files_dates();
                            fd.index = lstTemplates.Items.Count + i;
                            fd.path = ftpFolderPath + (ftpFolderPath.EndsWith("/") ? "" : "/");
                            //when in ftp mode it is not smart to read all files headers in order to access 
                            //OBJECT and DATE-OBS fields, so keep next two commentings
                            //string ttt = Fits.ReadCardFieldToString(ftpFiles[i], "OBJECT");
                            //fd.date = Fits.ReadCardFieldToDate(ftpFiles[i], "DATE-OBS");
                            fd.filename = ftpFiles[i];
                            fd.name = ftpFiles[i].Replace(".fts", "").Replace(".fits", "").Replace(".fit", "");
                            fd.bIsFTP = true;
                            tempData.Add(fd);
                            if (iLogLevel == 3) LogEntry("Item " + fd.index + ": " + fd.path + " > " + fd.filename + " > " + fd.name + " > " + fd.date);
                            if (iLogLevel == 3) Console.Out.WriteLine("Item " + fd.index + ": " + fd.path + " > " + fd.filename + " > " + fd.name + " > " + fd.date);
                        }
                    }
                    txtTemplatesDir.Text = ftpFolderPath;
                    foreach (files_dates o in tempData) lstTemplates.Items.Add(o.filename);
                    lblTemplateNo.Text = String.Format("{0}/{1}", 0, lstTemplates.Items.Count);
                }
                else
                {
                    chkFTPTemplates.Checked = false;
                }
            }
            else
            {
                folderBrowserDialog1.SelectedPath = ftpServer + ftpStartPath;// txtTemplatesDir.Text;
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    txtTemplatesDir.Text = folderBrowserDialog1.SelectedPath;
                    selectListEntries(lstTemplates, txtTemplatesDir.Text, false);
                }
            }
            if (lstTemplates.Items.Count > 0 && lstSubjects.Items.Count > 0) lstTemplates.SelectedIndex = 0;
        }

        private void cmdSubjectsDir_Click(object sender, EventArgs e) {
            folderBrowserDialog1.Description = "Please select the Subjects folder (the folder where the new frames reside)";
            if (chkFTPSubjects.Checked)
            {
                ftpFiles = null;
                ServerSite serverSite = new ServerSite("ftp://" + ftpServer + ftpStartPath, ftpUserName, ftpPassword);
                serverSite.ShowDialog();
                if (ftpFiles != null)
                {
                    //lstSubjects.Items.Clear();//do not clear list
                    Int32 maxFiles = ftpFiles.Length;
                    if (!ftpFolderPath.EndsWith("/")) ftpFolderPath += "/";
                    for (int i = 0; i < maxFiles; i++)
                    {
                        if (iLogLevel > 2) Console.Out.WriteLine("ftpFiles[" + i + "]=" + ftpFiles[i]);
                        if (ftpFiles[i].ToLower().EndsWith(".fts") || ftpFiles[i].ToLower().EndsWith(".fits") || ftpFiles[i].ToLower().EndsWith(".fit"))
                        {
                            files_dates fd = new files_dates();
                            fd.index = lstTemplates.Items.Count + i;
                            fd.path = ftpFolderPath + (ftpFolderPath.EndsWith("/") ? "" : "/"); ;
                            //when in ftp mode it is not smart to read all files headers in order to access 
                            //OBJECT and DATE-OBS fields, so keep next two commentings
                            //string ttt = Fits.ReadCardFieldToString(files[i], "OBJECT");
                            //fd.date = Fits.ReadCardFieldToDate(ftpFiles[i], "DATE-OBS");
                            fd.filename = ftpFiles[i];
                            fd.name = ftpFiles[i].Replace(".fts", "").Replace(".fits", "").Replace(".fit", "");
                            fd.bIsFTP = true;
                            subjData.Add(fd);
                            if (iLogLevel == 3)
                            {
                                LogEntry("Item " + fd.index + ": " + fd.path + " > " + fd.filename + " > " + fd.name + " > " + fd.date);
                                Console.Out.WriteLine("Item " + fd.index + ": " + fd.path + " > " + fd.filename + " > " + fd.name + " > " + fd.date);
                                Application.DoEvents();
                            }
                        }
                    }
                    txtSubjectsDir.Text = ftpFolderPath;
                    foreach (files_dates o in subjData) lstSubjects.Items.Add(o.filename);
                    lblSubjectNo.Text = String.Format("{0}/{1}", 0, lstSubjects.Items.Count);
                }
                else
                {
                    chkFTPSubjects.Checked = false;
                }
            }
            else
            {
                folderBrowserDialog1.SelectedPath = txtSubjectsDir.Text;
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    txtSubjectsDir.Text = folderBrowserDialog1.SelectedPath;
                    selectListEntries(lstSubjects, txtSubjectsDir.Text, false);
                }
            }
            if (lstTemplates.Items.Count > 0 && lstSubjects.Items.Count > 0) lstSubjects.SelectedIndex = 0;
        }

        private void refreshBatchToolStripMenuItem_Click(object sender, EventArgs e) {
            selectListEntries(lstTemplates, txtTemplatesDir.Text);
            selectListEntries(lstSubjects, txtSubjectsDir.Text);
            lstSubjects.SelectedIndex = 0;
        }

        private void lstTemplates_SelectedIndexChanged(object sender, EventArgs e) {
            if (lstTemplates.Items.Count > 0 && lstSubjects.Items.Count > 0)
                if (lstTemplates.SelectedItem != lstSubjects.SelectedItem)
                    lstSubjects.SelectedItem = lstTemplates.SelectedItem;
            //the rest of work is done in 'lstSubjects_SelectedIndexChanged'
        }

        private void lstSubjects_SelectedIndexChanged(object sender, EventArgs e) {
            if (lstTemplates.Items.Count == 0 || lstSubjects.Items.Count == 0) return;
            selectedSubj = getSubjectItemByFilename(lstSubjects.SelectedItem.ToString());
            if (selectedSubj < 0 || selectedSubj > lstSubjects.Items.Count)
            {
                if (iLogLevel > 0) LogEntry("Subject " + lstSubjects.SelectedItem.ToString() + " not found. Advancing to next Subject...");
                cmdNext_Click(sender, e);
                return;
            }
            selectedTemp = getTemplateItemByFilename(lstSubjects.SelectedItem.ToString());
            if (selectedTemp < 0 || selectedTemp > lstTemplates.Items.Count)
            {
                if (iLogLevel > 0) LogEntry("Template " + lstSubjects.SelectedItem.ToString() + " not found. Advancing to next Subject...");
                cmdNext_Click(sender, e);
                return;
            }
            //if (lstTemplates.SelectedIndex < 0) lstTemplates.SelectedIndex = 0;
            if (iLogLevel == 3) Console.Out.WriteLine("tempData.path=" + tempData[selectedTemp].path);
            if (iLogLevel == 3) Console.Out.WriteLine("subjData.path=" + subjData[selectedSubj].path);
            txtTemplatesDir.Text = tempData[selectedTemp].path;
            txtSubjectsDir.Text = subjData[selectedSubj].path;

            Circles[0] = null;
            Circles[1] = null;
            Circles[2] = null;
            Crosses[0] = null;
            Crosses[1] = null;
            Crosses[2] = null;
            Lines[0] = null;
            Lines[1] = null;
            Lines[2] = null;
            FoundSN = new Fits.PointValue(0, 0, 0);
            Application.UseWaitCursor = true;
            Application.DoEvents();
            //Download file from ftp Subjects
            string fn = "";
            if (subjData[selectedSubj].bIsFTP)
            {
                if (iLogLevel > 1) LogEntry("Downloading Subject '" + txtSubjectsDir.Text + "/" + lstSubjects.SelectedItem.ToString() + "'");
                loadingCircleSubject.Visible = true;
                loadingCircleSubject.Start();
                Application.DoEvents();
                //HttpHelper.ftpDownload(localSubjectsPath + lstSubjects.SelectedItem.ToString(), txtSubjectsDir.Text + "/" + lstSubjects.SelectedItem.ToString(), ftpUserName, ftpPassword);
                ThreadWithParams tws = new ThreadWithParams(localSubjectsPath + lstSubjects.SelectedItem.ToString(), txtSubjectsDir.Text + "/" + lstSubjects.SelectedItem.ToString(), ftpUserName, ftpPassword);
                Thread t = new Thread(new ThreadStart(tws.DownloadFileAsync));
                t.Start();
                while (t.IsAlive) { Application.DoEvents(); }
                loadingCircleSubject.Stop();
                loadingCircleSubject.Visible = false;
                Application.DoEvents();
                fn = localSubjectsPath + lstSubjects.SelectedItem.ToString();
            }
            else
            {
                fn = subjData[selectedSubj].path + lstSubjects.SelectedItem.ToString();
            }
            image_no = 1;
            if (File.Exists(fn))
            {
                image[1] = Fits.import_fits(fn);
                iMin[1] = image[1].datamin;// getImageMin(image[1]);
                iMax[1] = image[1].datapeak;// getImageMax(image[1]);
                iMinDefault[1] = iMin[1];
                iMaxDefault[1] = iMax[1];
                pictureBox2.Image = Fits.GetFITSImage(image[1], gamma[1], trans_type, -1, -1, wavelengthIndex);//, iMin[1], iMax[1]);
                SetLabels(1, image[1]);
            }
            else
            {
                pictureBox2.Image = null;
            }
            lstTemplates.SelectedItem = lstSubjects.SelectedItem;
            txtTemplatesDir.Text = tempData[selectedTemp].path;
            string fn1 = "";
            if (tempData[selectedTemp].bIsFTP)
            {
                if (iLogLevel > 1) LogEntry("Downloading Template '" + txtTemplatesDir.Text + "/" + lstTemplates.SelectedItem.ToString() + "'");
                loadingCircleTemplate.Visible = true;
                loadingCircleTemplate.Start();
                Application.DoEvents();
                //HttpHelper.ftpDownload(localTemplatesPath + lstTemplates.SelectedItem.ToString(), txtTemplatesDir.Text + "/" + lstTemplates.SelectedItem.ToString(), ftpUserName, ftpPassword);
                ThreadWithParams tws = new ThreadWithParams(localTemplatesPath + lstTemplates.SelectedItem.ToString(), txtTemplatesDir.Text + "/" + lstTemplates.SelectedItem.ToString(), ftpUserName, ftpPassword);
                Thread t = new Thread(new ThreadStart(tws.DownloadFileAsync));
                t.Start();
                while (t.IsAlive) { Application.DoEvents(); }// loadingCircleTemplate.Invalidate(); }//Thread.Sleep(10); }
                loadingCircleTemplate.Stop();
                loadingCircleTemplate.Visible = false;
                Application.DoEvents();
                fn1 = localTemplatesPath + lstTemplates.SelectedItem.ToString();
            }
            else
            {
                fn1 = tempData[selectedTemp].path + lstTemplates.SelectedItem.ToString();
            }
            if (File.Exists(fn1))
            {
                image[0] = Fits.import_fits(fn1);
                iMin[0] = image[0].datamin;// getImageMin(image[0]);
                iMax[0] = image[0].datapeak;// getImageMax(image[0]);
                iMinDefault[0] = iMin[0];
                iMaxDefault[0] = iMax[0];
                //resize template if its size differs from subject and bResizeTemplate is set to True
                if (image[0].xsize != image[1].xsize || image[0].ysize != image[1].ysize)
                {
                    if (bResizeTemplate) image[0] = Fits.ResizeImageptr(image[0], image[1], 0);////0<->1
                }
                pictureBox1.Image = Fits.GetFITSImage(image[0], gamma[0], trans_type, -1, -1, wavelengthIndex);
                SetLabels(0, image[0]);
                image[2] = Fits.CreateBlackImage(defaultImageX, defaultImageY);
                iMin[2] = iMin[0];// getImageMin(image[0]);
                iMax[2] = iMax[0];// getImageMax(image[0]);
                iMinDefault[2] = iMin[2];
                iMaxDefault[2] = iMax[2];
                pictureBox3.Image = Fits.GetFITSImage(image[2], gamma[2], trans_type, -1, -1, wavelengthIndex);
            }
            else
            {
                pictureBox1.Image = null;
            }
            lblImgTemplate.Text = image[0].object_name;
            lblImgSubject.Text = image[1].object_name;
            lblImgBlink.Text = image[0].object_name;

            lblCircleAlign.Visible = true;
            Application.DoEvents();
            switch (alignMethod) {
                case 0:
                    align_images(fn1, fn);
                    break;
                case 1:
                    //calculateAlignmentToolStripMenuItem_Click(sender, e);
                    grepnova2AlignToolStripMenuItem_Click(sender, e);
                    break;
            }
            // scale aligned_template ?????
            // if (image[2].xsize != image[1].xsize || image[2].ysize != image[1].ysize)
            //     image[2] = Fits.ResizeImageptr(image[2], image[1], 0);
            lblCircleAlign.Visible = false;
            Application.DoEvents();

            if (bBlankSubject && !isAlignedBlack)
            {//isAlignedBlack=false is in case that align could not been achieved
                image[1] = Fits.BlankSubject(image[1], image[2]);
                pictureBox2.Image = Fits.GetFITSImage(image[1], gamma[1], trans_type, -1, -1, wavelengthIndex);
            }

            for (int i = 0; i < 3; i++) histogram_data[i] = Fits.calc_histogram(image[i], 1000);// Console.Out.WriteLine("hist " + i + " = " + histogram_data[i].Length); }
            if (isNegative){
                pictureBox1.Image = pictureBox1.Image.CopyAsNegative();
                pictureBox2.Image = pictureBox2.Image.CopyAsNegative();
                pictureBox3.Image = pictureBox3.Image.CopyAsNegative();
            }
            if (tabControl1.SelectedIndex < 3){
                bNotChangeSelection = true;
                selectionRangeSlider1.Min = 0;
                selectionRangeSlider1.Max = image[tabControl1.SelectedIndex].datapeak;//image[tabControl1.SelectedIndex].datapeak;
                selectionRangeSlider1.SelectedMax = (iMax[tabControl1.SelectedIndex] - iMin[tabControl1.SelectedIndex]) / 2;
                selectionRangeSlider1.SelectedMin = iMin[tabControl1.SelectedIndex];
                bNotChangeSelection = false;
                if (bStretchImageIsOn) selectionRangeSlider1_SelectionChanged(sender, e);
                image_no = tabControl1.SelectedIndex;
                if (image[image_no].data != null) drawHistogram(histogram_data[image_no], cookHistImage);
                SetPixelScale(tabControl1.SelectedIndex);
            }else{
                if (image_no == 3) { lblAPP.Text = "Blinking..."; BlinkImages(); }
            }
            copyImage[0] = (Bitmap)pictureBox1.Image;
            copyImage[1] = (Bitmap)pictureBox2.Image;
            copyImage[2] = (Bitmap)pictureBox3.Image;
            for (int i = 0; i < 3; i++)
            {
                iCurrentMin[i] = iMin[i];
                iCurrentMax[i] = (iMax[i] - iMin[i]) / 2;
            }
            LogEntry(String.Format("Template Entropy = {0:0.000}", Fits.CalcImageEntropy(image[0], 2.0)));
            LogEntry(String.Format("Subject Entropy  = {0:0.000}", Fits.CalcImageEntropy(image[1], 2.0)));
            Application.UseWaitCursor = false;
        }


        private void SetLabels(int idx, Fits.imageptr im) {
            switch (idx)
            {
                case 0:
                    lblTempObject.Text = im.object_name;
                    lblTempObsDate.Text = im.date_numeric;
                    lblTempNaxis1.Text = String.Format("{0}", im.xsize);
                    lblTempNaxis2.Text = String.Format("{0}", im.ysize);
                    lblTempBitpix.Text = String.Format("{0}", im.bitpix);
                    lblTempMin.Text = String.Format("{0}", im.datamin);
                    lblTempMax.Text = String.Format("{0}", im.datapeak);
                    lblTempLoDec.Text = String.Format("{0}", im.data_lower_decile);
                    lblTempUpDec.Text = String.Format("{0}", im.data_upper_decile);
                    lblTempMean.Text = String.Format("{0:F2}", im.mean_ld_excess);
                    lblTemplateNo.Text = String.Format("{0}/{1}", (lstTemplates.SelectedIndex + 1), lstTemplates.Items.Count);
                    break;
                case 1:
                    lblSubObject.Text = im.object_name;
                    lblSubObsDate.Text = im.date_numeric;
                    lblSubNaxis1.Text = String.Format("{0}", im.xsize);
                    lblSubNaxis2.Text = String.Format("{0}", im.ysize);
                    lblSubBitpix.Text = String.Format("{0}", im.bitpix);
                    lblSubMin.Text = String.Format("{0}", im.datamin);
                    lblSubMax.Text = String.Format("{0}", im.datapeak);
                    lblSubLoDec.Text = String.Format("{0}", im.data_lower_decile);
                    lblSubUpDec.Text = String.Format("{0}", im.data_upper_decile);
                    lblSubMean.Text = String.Format("{0:F2}", im.mean_ld_excess);
                    lblSubjectNo.Text = String.Format("{0}/{1}", (lstSubjects.SelectedIndex + 1), lstSubjects.Items.Count);
                    break;
            }
        }


        public void cmdNext_Click(object sender, EventArgs e) {
            if (lstSubjects.Items.Count == 0 || lstTemplates.Items.Count == 0) return;
            Application.UseWaitCursor = true;
            if (frm.lstSubjects.SelectedIndex < frm.lstSubjects.Items.Count - 1)
                frm.lstSubjects.SelectedIndex++;
            else
                frm.lstSubjects.SelectedIndex = 0;
            Application.UseWaitCursor = false;
        }


        public void cmdPrev_Click(object sender, EventArgs e) {
            if (lstSubjects.Items.Count == 0 || lstTemplates.Items.Count == 0) return;
            Application.UseWaitCursor = true;
            if (frm.lstSubjects.SelectedIndex > 0)
                frm.lstSubjects.SelectedIndex--;
            else
                frm.lstSubjects.SelectedIndex = frm.lstSubjects.Items.Count - 1;
            Application.UseWaitCursor = false;
        }

        private int getTemplateItemByFilename(string fn) {
            for (int i = 0; i < tempData.Count; i++)
            {
                if (tempData[i].filename == fn)
                {
                    return i;
                }
            }
            return -1;
        }

        private int getSubjectItemByFilename(string fn) {
            //Console.Out.WriteLine("getSubjectItemByFilename: Count=" + subjData.Count+" fn="+fn);
            for (int i = 0; i < subjData.Count; i++)
            {
                if (subjData[i].filename == fn)
                {
                    return i;
                }
            }
            return -1;
        }

        private void selectListEntries(ListBox lst, string txt, bool isFTP = false) {
            using (new WaitCursor())
            {
                lst.Items.Clear();
                if (iLogLevel > 2) Console.Out.WriteLine("selectListEntries with txt= " + txt);
                if (txt.Equals("")) return;
                if (lst.Equals(lstTemplates)) lblTemplateNo.Text = "0/0";
                if (lst.Equals(lstSubjects)) lblSubjectNo.Text = "0/0";
                if (!txt.EndsWith("\\")) txt = txt + "\\";
                if (txt.Length > 0)
                {
                    if (System.IO.Directory.Exists(txt))
                    {
                        try
                        {
                            String[] files = System.IO.Directory.GetFiles(txt);
                            for (int i = 0; i < files.Length; i++)
                            {
                                string[] tmp = files[i].Split('\\');
                                if (iLogLevel > 2) Console.Out.WriteLine("tmp[tmp.Length-1]=" + tmp[tmp.Length - 1]);
                                if (tmp[tmp.Length - 1].ToLower().EndsWith(".fts") || tmp[tmp.Length - 1].ToLower().EndsWith(".fits") || tmp[tmp.Length - 1].ToLower().EndsWith(".fit"))
                                {
                                    files_dates fd = new files_dates();
                                    fd.index = lst.Items.Count + i;
                                    string p = files[i].Substring(0, files[i].Length - tmp[tmp.Length - 1].Length);
                                    fd.path = p + (p.EndsWith("\\") ? "" : "\\");
                                    string ttt = Fits.ReadCardFieldToString(files[i], "OBJECT");
                                    fd.filename = tmp[tmp.Length - 1];
                                    if (ttt != null)
                                    {
                                        if (ttt.Trim().Equals(""))
                                        {
                                            fd.name = tmp[tmp.Length - 1].Replace(".fts", "").Replace(".fits", "").Replace(".fit", "");
                                        }
                                        else
                                        {
                                            fd.name = ttt;
                                        }
                                    }
                                    else
                                    {
                                        fd.name = tmp[tmp.Length - 1].Replace(".fts", "").Replace(".fits", "").Replace(".fit", "");
                                    }
                                    fd.date = Fits.ReadCardFieldToDate(files[i], "DATE-OBS");
                                    fd.bIsFTP = isFTP;
                                    if (lst.Equals(lstSubjects))
                                    {
                                        if (!subjData.Contains(fd))
                                        {
                                            subjData.Add(fd);
                                            if (iLogLevel == 3) LogEntry("Subject Item " + fd.index + ": " + fd.path + " > " + fd.filename + " > " + fd.name + " > " + fd.date);
                                            if (iLogLevel == 3) Console.Out.WriteLine("Subject Item " + fd.index + ": " + fd.path + " > " + fd.filename + " > " + fd.name + " > " + fd.date);
                                        }
                                    }
                                    else
                                    {
                                        if (!tempData.Contains(fd))
                                        {
                                            tempData.Add(fd);
                                            if (iLogLevel == 3) LogEntry("Template Item " + fd.index + ": " + fd.path + " > " + fd.filename + " > " + fd.name + " > " + fd.date);
                                            if (iLogLevel == 3) Console.Out.WriteLine("Template Item " + fd.index + ": " + fd.path + " > " + fd.filename + " > " + fd.name + " > " + fd.date);
                                        }
                                    }
                                }
                            }
                            //sort List by date if it is the Subject list and bSortByDate is set to true
                            if (bSortByDate)
                            {
                                if (lst.Equals(lstSubjects)) subjData.Sort(new CustomDateComparer());
                            }
                            else
                            {
                                if (lst.Equals(lstSubjects)) subjData.Sort(new CustomNameComparer());
                            }
                            //...and fill the ListBox 
                            if (lst.Equals(lstSubjects))
                                foreach (files_dates o in subjData) lst.Items.Add(o.filename);
                            else
                                foreach (files_dates o in tempData) lst.Items.Add(o.filename);
                        }
                        catch (PathTooLongException)
                        {
                            if (iLogLevel > 0) LogEntry("Path " + txt + " too long.");
                            LogError("selectListEntries", "Path " + txt + " too long.");
                        }
                        catch (UnauthorizedAccessException)
                        {
                            if (iLogLevel > 0) LogEntry("Unauthorized access to path '" + txt + "'");
                            LogError("selectListEntries", "Unauthorized access to path '" + txt + "'");
                        }
                        catch (DirectoryNotFoundException)
                        {
                            if (iLogLevel > 0) LogEntry("Directory '" + txt + "' not found");
                            LogError("selectListEntries", "Directory '" + txt + "' not found");
                        }
                        catch (IOException)
                        {
                            if (iLogLevel > 0) LogEntry("General Input/Output error for path '" + txt + "'");
                            LogError("selectListEntries", "General Input/Output error for path '" + txt + "'");
                        }
                        catch (Exception ex)
                        {
                            if (iLogLevel > 0) LogEntry("Unknown type of error itterating path '" + txt + "'");
                            LogError("selectListEntries", "Unknown type of error itterating path '" + txt + "'");
                            if (iLogLevel > 0) Console.Out.WriteLine("Unknown type of error itterating path '" + txt + "'. Error " + ex.Message.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Call to toggle between the current cursor and the wait cursor
        /// </summary>
        /// <param name="toggleWaitCursorOn">True for wait cursor, false for default.</param>
        public void UseMyWaitCursor(bool toggleWaitCursorOn) {
            frm.UseWaitCursor = toggleWaitCursorOn;
            // Because of a weird quirk in .NET, just setting UseWaitCursor to true or false does not work
            // until the cursor's position changes. The following line of code fakes that and 
            // effectively forces the cursor to switch back from the wait cursor to default or vice versa.
            if (!toggleWaitCursorOn) { Cursor.Position = Cursor.Position; }
            Application.DoEvents();
        }

        private void drawElipses(int idx, List<starsCircles> circ, bool bDrawText = false, bool drawCrosses=false, int brushIndex = 0) {
            System.Windows.Forms.PictureBox pb = new System.Windows.Forms.PictureBox();
            int iC = 1;
            switch (idx){
                case 0:
                    pb = pictureBox1;
                    break;
                case 1:
                    pb = pictureBox2;
                    break;
                case 2:
                    pb = pictureBox3;
                    break;
            }
            Font font = new Font("Calibri", 8.0f);
            Brush brush;// = new SolidBrush(System.Drawing.Color.FromArgb(220, 50, 50, 50));
            Color col;
            switch (brushIndex){
                case 0:
                    brush = new SolidBrush(System.Drawing.Color.FromArgb(220, 255, 50, 50));
                    col = Color.Red;
                    break;
                case 1:
                    brush = new SolidBrush(System.Drawing.Color.FromArgb(220, 255, 255, 50));
                    col = Color.Yellow;
                    break;
                default:
                    brush = new SolidBrush(System.Drawing.Color.FromArgb(220, 50, 50, 50));
                    col = Color.Gray;
                    break;
            }
            int maxstar = int.Parse(txtWinWidth.Text);
            float yOrig = image[idx].ysize;
            foreach (starsCircles p in circ) {
                //if (iC > maxstar) break;
                //Console.Out.WriteLine("{0}: x={1} y={2} a={3} b={4} theta={5}", p.id, p.objX, p.objY, p.objA, p.objB, p.objTheta);
                Graphics gf = pb.CreateGraphics();
                //A circle with Red Color and 1 Pixel wide line
                float a = 6f * p.objA;
                float b = 6f * p.objB;
                //gf.DrawEllipse(new Pen(col, 1), new Rectangle((int)(p.objX - a/2f), (int)(p.objY - b/2f), (int)a, (int)b));
                //if (bDrawText) gf.DrawString(String.Format("{0}", p.id), font, brush, new PointF(0, 0));
                
                gf.TranslateTransform(p.objX, p.objY);
                gf.RotateTransform((p.objTheta * 180f / (float)Math.PI));//, MatrixOrder.Append);
                gf.DrawEllipse(new Pen(col, 1), new RectangleF( - a / 2f,  - b / 2f, a, b));

                //A cross 10x10
                if (drawCrosses){
                    gf.DrawLine(new Pen(col, 1), new PointF(0, -3), new PointF(0, 3));// new Point((int)p.objX, (int)p.objY - 5), new Point((int)p.objX, (int)p.objY + 5));
                    gf.DrawLine(new Pen(col, 1), new PointF(-3, 0), new PointF(3, 0));// new Point((int)p.objX - 5, (int)p.objY), new Point((int)p.objX + 5, (int)p.objY));
                }
                //the ranking of the brightness
                //string s = String.Format("{0:N0} {1:N0}", p.objX, p.objY);
                if (bDrawText) gf.DrawString(String.Format("{0}", p.id), font, brush, new PointF(0, 0));
                gf.ResetTransform();
                iC++;
            }
        }

        private void drawCircles(int idx, List<Fits.PointValue> circ, int radius = 10, bool bDrawText = true, int brushIndex = 0) {
            if (circ == null) return;
            if (circ.Count == 0) return;
            System.Windows.Forms.PictureBox pb = new System.Windows.Forms.PictureBox();
            double maxDoub = 0.0;
            double[] star_psf = new double[circ.Count];
            for (int j = 0; j < circ.Count; j++){
                star_psf[j] = Align.seeing_star_psf(image[tabControl1.SelectedIndex], circ[j].p.X, circ[j].p.Y);
                if (maxDoub < star_psf[j]) maxDoub = star_psf[j];
            }
            int r = radius;
            int iC = 1;
            switch (idx){
                case 0:
                    pb = pictureBox1;
                    break;
                case 1:
                    pb = pictureBox2;
                    break;
                case 2:
                    pb = pictureBox3;
                    break;
            }
            Font font = new Font("Calibri", 8.0f);
            Brush brush;// = new SolidBrush(System.Drawing.Color.FromArgb(220, 50, 50, 50));
            Color col;
            switch (brushIndex){
                case 0:
                    brush = new SolidBrush(System.Drawing.Color.FromArgb(220, 255, 50, 50));
                    col = Color.Red;
                    break;
                case 1:
                    brush = new SolidBrush(System.Drawing.Color.FromArgb(220, 255, 255, 50));
                    col = Color.Yellow;
                    break;
                default:
                    brush = new SolidBrush(System.Drawing.Color.FromArgb(220, 50, 50, 50));
                    col = Color.Gray;
                    break;
            }
            int i = 0;
            foreach (Fits.PointValue p in circ){
                Graphics gf = pb.CreateGraphics();
                //A circle with Red Color and 2 Pixel wide line
                if (starCirclesAccordingIntensitiesToolStripMenuItem.Checked) gf.DrawEllipse(new Pen(col, 2), new Rectangle(p.p.X - (int)(r * star_psf[i] / maxDoub), p.p.Y - (int)(r * star_psf[i] / maxDoub), 2 * (int)(r * star_psf[i] / maxDoub), 2 * (int)(r * star_psf[i] / maxDoub)));
                else gf.DrawEllipse(new Pen(col, 2), new Rectangle(p.p.X - r, p.p.Y - r, 2 * r, 2 * r));
                //A cross 10x10
                gf.DrawLine(new Pen(col, 1), new Point(p.p.X, p.p.Y - 5), new Point(p.p.X, p.p.Y + 5));
                gf.DrawLine(new Pen(col, 1), new Point(p.p.X - 5, p.p.Y), new Point(p.p.X + 5, p.p.Y));
                //the ranking of the brightness
                if (bDrawText) gf.DrawString(String.Format("{0}", iC), font, brush, new Point(p.p.X + r, p.p.Y - 4));
                i++;
                iC++;
            }
        }

        private void drawLines(int idx, List<Fits.TriplePoint> lin, int lineWidth = 2) {
            System.Windows.Forms.PictureBox pb = new System.Windows.Forms.PictureBox();
            if (lin == null) return;
            if (lin.Count == 0) return;
            int r = lineWidth;
            switch (idx){
                case 0:
                    pb = pictureBox1;
                    break;
                case 1:
                    pb = pictureBox2;
                    break;
                case 2:
                    pb = pictureBox3;
                    break;
            }
            Color[] cols = new Color[] {Color.Blue, Color.Green, Color.Yellow, Color.Cyan, Color.Gray,
                                        Color.Brown, Color.Coral, Color.DarkOliveGreen, Color.GreenYellow, Color.Indigo};
            Brush brush = new SolidBrush(System.Drawing.Color.FromArgb(220, 100, 100, 100));
            Color col = cols[0];
            int i = 0;
            foreach (Fits.TriplePoint p in lin){
                Graphics gf = pb.CreateGraphics();
                //3 Lines with Gray Color and 2 Pixel wide line
                gf.DrawLine(new Pen(cols[i], r), new PointF(p.pp[0].X, p.pp[0].Y), new PointF(p.pp[1].X, p.pp[1].Y));
                gf.DrawLine(new Pen(cols[i], r), new PointF(p.pp[1].X, p.pp[1].Y), new PointF(p.pp[2].X, p.pp[2].Y));
                gf.DrawLine(new Pen(cols[i], r), new PointF(p.pp[2].X, p.pp[2].Y), new PointF(p.pp[0].X, p.pp[0].Y));
                i++;
                if (i >= cols.Length) i = 0;
            }
        }

        private void drawCross(int idx, List<Fits.PointValue> circ, int radius = 10, bool bDrawText = true, int brushIndex = 0) {
            System.Windows.Forms.PictureBox pb = new System.Windows.Forms.PictureBox();
            int r = radius;
            int rsmall = 1;
            int iC = 1;
            switch (idx)
            {
                case 0:
                    pb = pictureBox1;
                    break;
                case 1:
                    pb = pictureBox2;
                    break;
                case 2:
                    pb = pictureBox3;
                    break;
            }
            Font font = new Font("Calibri", 8.0f);
            Brush brush;// = new SolidBrush(System.Drawing.Color.FromArgb(220, 50, 50, 50));
            Color col;
            switch (brushIndex)
            {
                case 0:
                    brush = new SolidBrush(System.Drawing.Color.FromArgb(220, 255, 50, 50));
                    col = Color.Red;
                    break;
                case 1:
                    brush = new SolidBrush(System.Drawing.Color.FromArgb(220, 255, 255, 50));
                    col = Color.Yellow;
                    break;
                default:
                    brush = new SolidBrush(System.Drawing.Color.FromArgb(220, 50, 50, 50));
                    col = Color.Gray;
                    break;
            }

            foreach (Fits.PointValue p in circ)
            {
                if (p.p.X > 0 && p.p.Y > 0)
                {
                    Graphics gf = pb.CreateGraphics();
                    //A circle with Red Color and 2 Pixel wide line
                    //gf.DrawEllipse(new Pen(col, 2), new Rectangle(p.p.X - r, p.p.Y - r, 2 * r, 2 * r));
                    //A cross 10x10
                    gf.DrawLine(new Pen(col, 1), new Point(p.p.X - 10, p.p.Y), new Point(p.p.X - 5, p.p.Y));
                    gf.DrawLine(new Pen(col, 1), new Point(p.p.X + 5, p.p.Y), new Point(p.p.X + 10, p.p.Y));
                    gf.DrawLine(new Pen(col, 1), new Point(p.p.X, p.p.Y - 10), new Point(p.p.X, p.p.Y - 5));
                    gf.DrawLine(new Pen(col, 1), new Point(p.p.X, p.p.Y + 5), new Point(p.p.X, p.p.Y + 10));

                    gf.DrawEllipse(new Pen(Color.Red, 2), new Rectangle(p.p.X - rsmall, p.p.Y - rsmall, 2 * rsmall, 2 * rsmall));
                    //the ranking of the brightness
                    if (bDrawText) gf.DrawString(String.Format("{0}", p.v), font, brush, new Point(p.p.X + 12, p.p.Y - 4));
                    iC++;
                }
            }
        }

        private void drawSN(Fits.PointValue p) {
            if (p.p.X > 0 && p.p.Y > 0){
                System.Windows.Forms.PictureBox pb = new System.Windows.Forms.PictureBox();
                pb = pictureBox2;//it happens only in pictureBox2
                Color col = Color.White;
                Graphics gf = pb.CreateGraphics();
                gf.DrawLine(new Pen(col, 3), new Point(p.p.X + 10, p.p.Y), new Point(p.p.X + 20, p.p.Y));
                gf.DrawLine(new Pen(col, 3), new Point(p.p.X, p.p.Y - 20), new Point(p.p.X, p.p.Y - 10));
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            //Find brightest button
            if (Circles[tabControl1.SelectedIndex] == null)
            {
                Circles[tabControl1.SelectedIndex] = new List<Fits.PointValue>();
                Align.starlist ast = Align.source_extract(image[tabControl1.SelectedIndex], Int16.Parse(txtWinWidth.Text));
                Fits.PointValue[] pp = new Fits.PointValue[ast.list_size];
                for (int ii = 0; ii < ast.list_size; ii++)
                {
                    pp[ii].p.X = ast.x_pos[ii];
                    pp[ii].p.Y = ast.y_pos[ii];
                    pp[ii].v = ast.intensity[ii];
                }
                string content = ""; 
                foreach (Fits.PointValue p in pp)
                {
                    if (p.p.X > 0 && p.p.Y > 0)
                    {
                        Circles[tabControl1.SelectedIndex].Add(p);
                        if (printStarList){
                            CooknasStar.Centroid.STAR_COORD cen = centroid.position(image[tabControl1.SelectedIndex], p.p.X, p.p.Y, null);
                            content += String.Format("{0};{1}\r\n", cen.X, cen.Y);
                        }
                    }
                }
                if (printStarList) File.WriteAllText("starlist.txt", content);
            }
            drawCircles(tabControl1.SelectedIndex, Circles[tabControl1.SelectedIndex]);
        }

        private void CheckForSNCandidatesInSubject(int num) {
            //Subject must be Blanked Subject
            List<Point> cand = Fits.CheckForSNCandidates(image[1], image[2], num);
            if (cand.Count > 0)
            {
                int r = 10;
                foreach (Point p in cand)
                {
                    //check if aligned image [2] at this point has some intensity (lower_decile?)
                    try
                    {
                        if (image[2].data[p.X + p.Y * image[2].xsize] < image[2].data_lower_decile)
                        {
                            Graphics gf = pictureBox2.CreateGraphics();
                            //A circle with Red Color and 2 Pixel wide line
                            gf.DrawEllipse(new Pen(Color.Red, 2), new Rectangle(p.X - r, p.Y - r, 2 * r, 2 * r));
                            //candidate found, so celebrate it ...
                            if (autoSearchIsOn == true)
                            {
                                LogEntry("*******************************************************************");
                                LogEntry(image[2].object_name + ": Candidate FOUND @ x=" + p.X + " y=" + p.Y + ". PLEASE CHECK IT");
                                LogEntry("*******************************************************************");
                            }
                            candidatesFound++;
                            if (iLogLevel > 0 && autoSearchIsOn == false) LogEntry("Candidate FOUND @ x=" + p.X + " y=" + p.Y + ". Please check it");
                        }
                        else
                        {
                            if (iLogLevel > 0 && autoSearchIsOn == false) LogEntry("Candidate checked @ x=" + p.X + " y=" + p.Y + " with intensity Is=" + image[1].data[p.X + p.Y * image[1].xsize] + " (It=" + image[2].data[p.X + p.Y * image[2].xsize] + "). Not a SuperNova.");
                        }
                    }
                    catch (Exception ex)
                    {//in case that points are out of boundaries (e.g. when scaling Template)
                        if (autoSearchIsOn == true)
                        {
                            LogEntry("Error checking at x=" + p.X + " y=" + p.Y + ". Error: " + ex.Message.ToString());
                        }
                        else
                        {
                            LogError("CheckForSNCandidatesInSubject", "Error checking at x=" + p.X + " y=" + p.Y + ". Error: " + ex.Message.ToString());
                        }
                    }
                }
            }

        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e) {
            image_no = 0;
            int x = e.X;
            int y = e.Y;
            mouseX = x;
            mouseY = y;
            int val = image[image_no].data[x + y * image[image_no].xsize];
            lblXPos.Text = x.ToString();
            lblYPos.Text = y.ToString();
            lblBright.Text = val.ToString();
            lblImageType.Text = image_type[image_no];

            if (x < halfSize) x = halfSize;
            if (y < halfSize) y = halfSize;
            if (x > image[image_no].xsize - halfSize) x = image[image_no].xsize - halfSize;
            if (y > image[image_no].ysize - halfSize) y = image[image_no].ysize - halfSize;
            using (Graphics grD = Graphics.FromImage(picZoom.Image))
            {
                grD.DrawImage(pictureBox1.Image, new Rectangle(0, 0, (int)(2 * zoomScale * halfSize), (int)(2 * zoomScale * halfSize)), new Rectangle(x - halfSize, y - halfSize, 2 * halfSize, 2 * halfSize), GraphicsUnit.Pixel);
                picZoom.Invalidate();
            }
            currentX = x;
            currentY = y;
        }


        private void pictureBox2_MouseMove(object sender, MouseEventArgs e) {
            image_no = 1;
            int x = e.X;
            int y = e.Y;
            mouseX = x;
            mouseY = y;
            int val = image[image_no].data[x + y * image[image_no].xsize];
            lblXPos.Text = x.ToString();
            lblYPos.Text = y.ToString();
            lblBright.Text = val.ToString();
            lblImageType.Text = image_type[image_no];
            if (x < halfSize) x = halfSize;
            if (y < halfSize) y = halfSize;
            if (x > image[image_no].xsize - halfSize) x = image[image_no].xsize - halfSize;
            if (y > image[image_no].ysize - halfSize) y = image[image_no].ysize - halfSize;
            using (Graphics grD = Graphics.FromImage(picZoom.Image))
            {
                grD.DrawImage(pictureBox2.Image, new Rectangle(0, 0, (int)(2 * zoomScale * halfSize), (int)(2 * zoomScale * halfSize)), new Rectangle(x - halfSize, y - halfSize, 2 * halfSize, 2 * halfSize), GraphicsUnit.Pixel);
                picZoom.Invalidate();
            }
            currentX = x;
            currentY = y;
        }


        private void pictureBox3_MouseMove(object sender, MouseEventArgs e) {
            image_no = 2;
            int x = e.X;
            int y = e.Y;
            mouseX = x;
            mouseY = y;
            int val = image[image_no].data[x + y * image[image_no].xsize];
            lblXPos.Text = x.ToString();
            lblYPos.Text = y.ToString();
            lblBright.Text = val.ToString();
            lblImageType.Text = image_type[image_no];
            if (x < halfSize) x = halfSize;
            if (y < halfSize) y = halfSize;
            if (x > image[image_no].xsize - halfSize) x = image[image_no].xsize - halfSize;
            if (y > image[image_no].ysize - halfSize) y = image[image_no].ysize - halfSize;
            using (Graphics grD = Graphics.FromImage(picZoom.Image))
            {
                grD.DrawImage(pictureBox3.Image, new Rectangle(0, 0, (int)(2 * zoomScale * halfSize), (int)(2 * zoomScale * halfSize)), new Rectangle(x - halfSize, y - halfSize, 2 * halfSize, 2 * halfSize), GraphicsUnit.Pixel);
                picZoom.Invalidate();
            }
            currentX = x;
            currentY = y;
        }


        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e) {
            image_no = tabControl1.SelectedIndex;
            lblImageType.Text = image_type[image_no];
            lblXPos.Text = "";
            lblYPos.Text = "";
            lblBright.Text = "";
            if (image_no < 3)
            {
                bNotChangeSelection = true;
                selectionRangeSlider1.Min = 0;
                selectionRangeSlider1.Max = image[image_no].datapeak;
                selectionRangeSlider1.SelectedMin = iCurrentMin[image_no];
                selectionRangeSlider1.SelectedMax = iCurrentMax[image_no];
                bNotChangeSelection = false;

                if (bStretchImageIsOn) selectionRangeSlider1_SelectionChanged(sender, e);
                if (image[image_no].data != null) drawHistogram(histogram_data[image_no], cookHistImage);
                //refresh tab before drawing circles and crosses
                tabControl1.Refresh();
                if (Crosses[image_no] != null) if (Crosses[image_no].Count > 0) drawCross(image_no, Crosses[image_no], 7, true, 0);
                if (Circles[image_no] != null) if (Circles[image_no].Count > 0) drawCircles(image_no, Circles[image_no]);
                if (Lines[image_no] != null) if (Lines[image_no].Count > 0) drawLines(image_no, Lines[image_no]);
                if (image_no == 1 && !Fits.IsPointValueNull(FoundSN)) drawSN(FoundSN);
                SetPixelScale(image_no);
            }
            if (bStretchImageIsOn) stretchImage.SetImage(tabControl1.SelectedIndex,
                                                          selectionRangeSlider1.Min,
                                                          selectionRangeSlider1.Max,
                                                          selectionRangeSlider1.SelectedMin,
                                                          selectionRangeSlider1.SelectedMax);
            if (image_no == 3){
                lblAPP.Text = "Blinking...";
                BlinkImages();
            }
        }

        private void SetPixelScale(int idx) {
            //Console.Out.WriteLine("focal:{0} psx={1} psy={2}", image[idx].focalLength, image[idx].pixelSizeX, image[idx].pixelSizeY);
            if (image[idx].focalLength > 0 && image[idx].pixelSizeX > 0 && image[idx].pixelSizeY > 0){
                float appx = image[idx].pixelSizeX * 206.3f / image[idx].focalLength;
                float appy = image[idx].pixelSizeY * 206.3f / image[idx].focalLength;
                lblAPP.Text = String.Format("Pixel scale={0:0.000} arcsec per pixel (Plate size: {1:0.00}x{2:0.00} arcmin)", appx, image[idx].xsize * appx / 60, image[idx].ysize * appy / 60);
            }else { lblAPP.Text = "No scale data available"; }
        }

        public void ShowZoom(int[] va, int xMax, int yMax) {
            image_no = tabControl1.SelectedIndex;
            zoom_image.data = va;
            zoom_image.xsize = 2 * xMax;
            zoom_image.ysize = 2 * yMax;
            zoom_image = Fits.image_minmax_update(zoom_image);
            picZoom.Image = Fits.GetFITSImage(zoom_image, gamma[image_no], trans_type, -1, -1, wavelengthIndex);//, iMin[image_no], iMax[image_no]);
        }
        public void ZoomHistogram() {
            Bitmap bmp = new Bitmap(picZoom.Image);
            int zoom = (int)zoomScale;
            int totalZoom = 2 * zoom * halfSize;
            int[] hist = new int[totalZoom];
            for (int x = 0; x < totalZoom; x++)
            {
                for (int y = 0; y < totalZoom; y++)
                {
                    hist[x] += (int)(255.0 * bmp.GetPixel(x, y).GetBrightness());
                }
            }
            cookHist.DrawHistogram(hist, currentX, currentY);
            lblHistName.Text = String.Format("{0}({1})@{2:F0},{3:F0}", image_type_short[tabControl1.SelectedIndex],
                                                    image[tabControl1.SelectedIndex].object_name, currentX, currentY);
        }

        public static System.Drawing.Image getHistogramWithSetSize(int width, int height, int[] pixelFreqs) {
            // http://al.cx/drawing-a-bitmap-histogram-of-pixel-intensities-in-c/
            // Builds you a histogram of canvas size width*height. Takes ~2ms on a 300*300, 15ms on a 2000*1000
            // Bigger ones look cleaner, because the UI does a nice job of scaling them down, and it means that
            // the graphics libraries have a nicer time finding a path through the points.

            // We want to be able to fit at least one 256 (w) by 100 (h) histogram into the space.
            // If the user gave us bad parameters, we won't complain to them, we will just make it
            // bigger than they want it. If their UI is set up to do so, it will scale it down for them.
            if (width < 280) { width = 280; }
            if (height < 120) { height = 130; }
            int widthPerFrequency = width / 256;
            //Width of actual hist we will center horizontally
            int histWidth = widthPerFrequency * 256;
            int spaceLeft = width - histWidth;
            int startX = spaceLeft / 2;
            // Define the brushes/pens we will use:
            var axisPen = new System.Drawing.Pen(System.Drawing.Brushes.DarkGray, 2);
            var gridPen = new System.Drawing.Pen(System.Drawing.Brushes.DarkGray, 0.5f);
            var linGrBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Point(startX, height - 10),
                new Point(startX + histWidth, height - 10),
                System.Drawing.Color.FromArgb(0, 0, 0),
                System.Drawing.Color.FromArgb(255, 255, 255)
                );
            var bluePen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(51, 153, 255), 2);
            var redPen = new System.Drawing.Pen(System.Drawing.Color.OrangeRed, 2);
            var polyBrush = new SolidBrush(System.Drawing.Color.FromArgb(120, 51, 153, 255));
            // Make canvas and get bitmap gfx handler g
            Bitmap hist = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(hist);
            // Paint the canvas white, add gradient bar
            g.FillRectangle(System.Drawing.Brushes.White, new Rectangle(0, 0, width, height));
            g.FillRectangle(linGrBrush, startX, height - 20, histWidth, 10);
            // Draw the axis
            g.DrawLine(axisPen, startX, 10, startX, height - 30); //y
            g.DrawLine(axisPen, startX, height - 30, startX + histWidth, height - 30); //x
            // Draw meaningless grid marks just beacause it looks nice
            // Draw them every 10th of an x length
            int seperatingPixel = histWidth / 10;
            for (int x = startX; x < startX + histWidth; x += seperatingPixel)
            {
                g.DrawLine(gridPen, x, 10, x, height - 30);
            }
            for (int y = height - 30; y > 10; y -= seperatingPixel)
            {
                g.DrawLine(gridPen, startX, y, startX + histWidth, y);
            }
            int biggestValue = pixelFreqs.Max();
            Point[] polygon = new Point[258];
            // Add a point for each and every pixel frequency
            for (int i = 0; i < 256; i++)
            {
                float percent = (float)pixelFreqs[i] / (float)biggestValue;
                percent = percent * (height - 40);
                int percInt = (int)percent;
                Point addedPoint = new Point((startX + widthPerFrequency * i), (height - 30 - percInt));
                polygon[i] = addedPoint;
            }
            polygon[256] = new Point(widthPerFrequency * 256, height - 30); //Add a line taking us back to the axis
            polygon[257] = new Point(startX, height - 30); //and back to the origin
            g.DrawLines(bluePen, polygon);
            g.FillPolygon(polyBrush, polygon);
            return hist;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e) {
            image_no = tabControl1.SelectedIndex;
            if (image_no > 2) return;
            picZoom.Image = Fits.GetFITSImage(Fits.CreateBlackImage((int)(2 * zoomScale * halfSize), (int)(2 * zoomScale * halfSize)), gamma[image_no], trans_type);//, iMin[image_no], iMax[image_no]);
            lblXPos.Text = "";
            lblYPos.Text = "";
            lblBright.Text = "";
        }


        private void pictureBox2_MouseLeave(object sender, EventArgs e) {
            image_no = tabControl1.SelectedIndex;
            if (image_no > 2) return;
            picZoom.Image = Fits.GetFITSImage(Fits.CreateBlackImage((int)(2 * zoomScale * halfSize), (int)(2 * zoomScale * halfSize)), gamma[image_no], trans_type);//, iMin[image_no], iMax[image_no]);
            lblXPos.Text = "";
            lblYPos.Text = "";
            lblBright.Text = "";
        }


        private void pictureBox3_MouseLeave(object sender, EventArgs e) {
            image_no = tabControl1.SelectedIndex;
            if (image_no > 2) return;
            picZoom.Image = Fits.GetFITSImage(Fits.CreateBlackImage((int)(2 * zoomScale * halfSize), (int)(2 * zoomScale * halfSize)), gamma[image_no], trans_type);//, iMin[image_no], iMax[image_no]);
            lblXPos.Text = "";
            lblYPos.Text = "";
            lblBright.Text = "";
        }


        private void chkShowHistogram_CheckedChanged(object sender, EventArgs e) {
            if (chkShowHistogram.Checked)
            {
                cookHist.Visible = true;
                cooknasStar.Visible = false;
                label39.Text = "Click on image to draw its histogram";
                chkShowHistogram.Text = "Show Histogram";
            }
            else
            {
                cookHist.Visible = false;
                cooknasStar.Visible = true;
                label39.Text = "Click on image to draw a star graph";
                chkShowHistogram.Text = "Show Star Graph";
            }
        }

        private void align_images(string inTemp, string inSubj) {
            if (iLogLevel > 2) Console.Out.WriteLine("grepnova Align tmp:{0} sbj:{1}", inTemp, inSubj);
            string outFts = (alignType == 0 ? "tmp.fts" : "tmp.jpg");
            pictureBox3.Image = null;
            if (File.Exists(outFts)) File.Delete(outFts);
            LaunchCommandLineApp(inTemp, inSubj, "null", alignType, bResizeTemplate);
            if (alignType == 0)
            {
                image[2] = Fits.import_fits(outFts);
                using (var bmpTemp = Fits.GetFITSImage(image[2], gamma[2], trans_type, iMin[2], iMax[2]))
                {
                    pictureBox3.Image = new Bitmap(bmpTemp);
                }
            }else{
                pictureBox3.Image = Fits.GetFITSImage(Fits.CreateBlackImage(image[0].xsize, image[0].ysize), gamma[2], trans_type);//, iMin[2], iMax[2]);
                if (File.Exists("likelihood.txt"))
                {
                    if (iLogLevel > 0) LogEntry(String.Format("Images {0} and {1} aligned", inTemp, inSubj));
                    string line = System.IO.File.ReadAllText("likelihood.txt");
                    string[] val = line.Split(' ');
                    float dx = float.Parse(val[0], CultureInfo.InvariantCulture);
                    float dy = float.Parse(val[1], CultureInfo.InvariantCulture);
                    double fangle = double.Parse(val[2], CultureInfo.InvariantCulture);
                    double dangle = (double)fangle * (180.0 / Math.PI);
                    float sc = 1.0f;
                    if (val.Length>3) sc = float.Parse(val[3], CultureInfo.InvariantCulture);
                    lblImgAlignedSize.Text = String.Format("dx={0:F0} dy={1:F0} r={2:F2}° s={3:F2}", dx, dy, dangle, sc);
                    currentAlign = new Align.proposed_alignment();
                    currentAlign.x = dx;
                    currentAlign.y = dy;
                    currentAlign.theta = dangle;
                    currentAlign.scale = sc;
                    // transform bitmap
                    transformImage(dx, dy, (float)dangle, sc);//angle in degrees
                    if (File.Exists("likelihood2.txt"))
                    {
                        image[2] = Fits.CloneImagePtr(image[0]);
                        image[2].data = Fits.ParseIntegerFile("likelihood2.txt");
                        if (image[2].xsize * image[2].ysize > image[2].data.Length){
                            int[] data = new int[image[2].xsize * image[2].ysize+1];
                            for (int i = 0; i < image[2].data.Length; i++)
                                data[i] = image[2].data[i];
                            for (int i = image[2].data.Length; i < image[2].xsize * image[2].ysize; i++)
                                data[i] = Fits.GREPNOVA_DATAMIN;
                            image[2].data = data;
                        }
                        image[2] = Fits.image_minmax_update(image[2]);
                        if (iLogLevel > 0) LogEntry(String.Format("Aligning transformation: dx={0} dy={1} a={2} s={3}\n", dx, dy, (float)dangle, sc));
                    }else{
                        image[2] = Fits.TransformImagePtr(image[0], fangle, (double)dx, (double)dy);//angle in radians
                        if (iLogLevel > 0) LogEntry(String.Format("Aligning transformation performed by internal methods: dx={0} dy={1} a={2}\n", dx, dy, (float)dangle + "\n"));
                    }
                    //it's time to delete 'likelihood.txt' and 'likelihood2.txt' files as we don't need them anymore.
                    if (File.Exists("likelihood.txt")) File.Delete("likelihood.txt");
                    if (File.Exists("likelihood2.txt")) File.Delete("likelihood2.txt");
                    isAlignedBlack = false;
                }else{
                    LogEntry(align_output_output);
                    LogEntry(align_error_output);
                    lblImgAlignedSize.Text = "No Alignment";
                    LogEntry(String.Format("Images {0} and {1} COULD NOT be aligned.", inTemp, inSubj));
                    LogEntry(String.Format("Run 'grepnova-align.bin.exe {0} {1} null 1' at DOS prompt to see the reason.", inTemp, inSubj));
                    //set AlignedTemplate to all-black
                    image[2] = Fits.CreateBlackImage(image[0].xsize, image[0].ysize);
                    isAlignedBlack = true;
                }
            }
        }

        static void LaunchCommandLineApp(string inTemp, string inSubj, string outFts, int alType, bool bResize) {
            string ex1 = inTemp;
            string ex2 = inSubj;
            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            if (alType == 0)
            {
                startInfo.FileName = "grepnova-align.fts.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.Arguments = ex1 + " " + ex2 + " " + outFts;
            }
            else
            {
                startInfo.FileName = "grepnova-align.bin.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                // 'grepnova-align.bin.exe' syntax (after modifications):
                //
                //      grepnova-align.bin.exe [tempFile] [subjFile] [null] [1 or 2]
                //
                // value 1 or 2 at the end of 'startInfo.Arguments' is for 'outfile' parameter in
                // likelihood_store_update(imageptr *T, imageptr *S, state_vector *state, int *status, int outfile)
                //
                // if outfile>=1 then 'likelihood_store_update' creates two files at the executable path:
                // 'likelihood.txt' containing displacement and rotation of two images
                // 'likelihood2.txt' containing the imageptr.data of the aligned image
                // Note that these two newly created files are deleted after used by 'align_images()' method
                // If outfile = 2 the grepnova-align.bin.exe tries to resize Template so to fit Subject size
                // If outfile = 1 the grepnova-align.bin.exe does not try to resize Template
                string sResize = "";
                if (bResize){
                    sResize = " 2";
                }else{
                    sResize = " 1";
                }

                startInfo.Arguments = ex1 + " " + ex2 + " " + outFts + sResize;// + " > align_dump.txt";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
            }
            align_error_output = "";
            try
            {
                // Start the process with the info we specified. 
                //Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    align_output_output = exeProcess.StandardOutput.ReadToEnd();
                    align_error_output = exeProcess.StandardError.ReadToEnd();
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Form1.frm.LogError("LaunchCommandLineApp", ex.Message.ToString());
                MessageBox.Show("Error {0}", ex.Message.ToString());
            }
            //frm.LogEntry(output);
        }


        private void transformImage(float dx, float dy, float angle, float scale=1.0f) {
            if (pictureBox1.Image == null) return;
            if (scale<0.01 || scale > 10.0)
            {
                MessageBox.Show(String.Format("Transformation cannot be handled.\r\n" +
                    "Translation:({0},{1}) Rotation:{2} Scale:{3}", dx, dy, angle, scale));
                return;
            }
            int ddx = 0;
            int ddy = 0;
            //bool isSameSize = true;
            //if (image[0].xsize != image[1].xsize) { ddx = (int)(dx*scale);ddy = (int)(dy*scale); isSameSize = false; }
            Matrix m = new Matrix();
            //Translate by dx,dy
            //if(isSameSize)
            m.Translate(dx, dy);
            //scale by scale
            m.Scale(scale, scale, MatrixOrder.Append);
            //rotate angle degrees clockwise
            m.Rotate(angle, MatrixOrder.Append);
            

            Graphics g = Graphics.FromImage(pictureBox3.Image);
            GraphicsPath gp = new GraphicsPath(FillMode.Winding);
            System.Drawing.Image imgpic = (System.Drawing.Image)pictureBox1.Image.Clone();
            //the coordinate of the polygon must be:
            //point 1 = left top corner, point 2 = right top corner, point 3 = right bottom corner (? should be Left Bottom)
            
            gp.AddPolygon(new Point[]{new Point(ddx,ddy),
                new Point(ddx+imgpic.Width,ddy),
                new Point(ddx,ddy+imgpic.Height)});
            //apply the transformation matrix on the graphical path
            gp.Transform(m);
            //get the resulting path points
            PointF[] pts = gp.PathPoints;
            //draw on the picturebox content of imgpic using the local transformation
            //using the resulting parralleogram described by pts
            g.DrawImage(imgpic, pts);
            pictureBox3.Refresh();
        }

        private void BlinkImages() {
            tabControl1.SelectedIndex = 3;
            int idx = 1;
            while (tabControl1.SelectedIndex == 3)
            {
                if (idx == 1)
                {
                    pictureBox4.Image = pictureBox2.Image;
                    lblImgBlink.Text = "Subject";
                    idx = 2;
                }
                else
                {
                    pictureBox4.Image = pictureBox3.Image;
                    lblImgBlink.Text = "Template";
                    idx = 1;
                }
                Application.DoEvents();
                System.Threading.Thread.Sleep(delayTime);
            }
        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            tabControl1.SelectedIndex = 0;
            Application.Exit();
        }

        private void applicationSettingsToolStripMenuItem_Click(object sender, EventArgs e) {
            AppSettings appsetting_form = new AppSettings();
            appsetting_form.ShowDialog();
            load_app_config();
        }

        private void grepnovaSettingsToolStripMenuItem_Click(object sender, EventArgs e) {
            Settings setting_form = new Settings();
            setting_form.ShowDialog();
        }

        /// <summary>
        /// Deletes all files contained in Templates Folder and Clears Tamplates list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearTemplatesFolderToolStripMenuItem_Click(object sender, EventArgs e) {
            if (MessageBox.Show(String.Format("Do you really want to clear all files in '{0}'?", "Templates"), "Delete Templates files", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                int iC = tempData.Count;
                tempData.Clear();
                txtTemplatesDir.Text = "";
                //Blank Template ...
                image[0] = Fits.CreateBlackImage(defaultImageX, defaultImageY);
                pictureBox1.Image = Fits.GetFITSImage(image[0], gamma[1], trans_type, -1, -1, wavelengthIndex);
                //... and Aligned Template
                image[2] = Fits.CreateBlackImage(defaultImageX, defaultImageY);
                pictureBox3.Image = Fits.GetFITSImage(image[2], gamma[1], trans_type, -1, -1, wavelengthIndex);
                if (iLogLevel > 0) Console.Out.WriteLine(String.Format("Cleared {0} Template files.", iC));
                if (iLogLevel > 0) LogEntry(String.Format("Cleared {0} Template files.", iC));
                selectListEntries(lstTemplates, txtTemplatesDir.Text);
                lblTemplateNo.Text = String.Format("{0}/{1}", 0, lstTemplates.Items.Count);
            }
        }

        /// <summary>
        /// Deletes all files contained in Subjects Folder and Clears Subjects list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearSubjectsFolderToolStripMenuItem_Click(object sender, EventArgs e) {
            if (MessageBox.Show(String.Format("Do you really want to clear all files in '{0}'?", "Subjects"), "Delete Subjects files", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                int iC = subjData.Count;
                subjData.Clear();
                txtSubjectsDir.Text = "";
                //Blank Subject
                image[1] = Fits.CreateBlackImage(defaultImageX, defaultImageY);
                pictureBox2.Image = Fits.GetFITSImage(image[1], gamma[1], trans_type, -1, -1, wavelengthIndex);
                if (iLogLevel > 0) Console.Out.WriteLine(String.Format("Cleared {0} Subject files.", iC));
                if (iLogLevel > 0) LogEntry(String.Format("Cleared {0} Subject files.", iC));
                selectListEntries(lstSubjects, txtSubjectsDir.Text);
                lblSubjectNo.Text = String.Format("{0}/{1}", 0, lstSubjects.Items.Count);
            }
        }


        private void txtBlinkTime_TextChanged(object sender, EventArgs e) {
            if (txtBlinkTime.Text.All(Char.IsDigit)){
                try{
                    delayTime = int.Parse(txtBlinkTime.Text);
                }catch (Exception) { }
            }
        }


        private void udTemp_ValueChanged(object sender, EventArgs e) {
            udTemp.Increment = udTemp.Value / 10;
            gamma[0] = (double)udTemp.Value;
            reDrawImage(0);
        }

        private void udSubj_ValueChanged(object sender, EventArgs e) {
            udSubj.Increment = udSubj.Value / 10;
            gamma[1] = (double)udSubj.Value;
            reDrawImage(1);
        }

        private void udAlig_ValueChanged(object sender, EventArgs e) {
            udAlig.Increment = udAlig.Value / 10;
            gamma[2] = (double)udAlig.Value;
            reDrawImage(2);
        }

        private void udAll_ValueChanged(object sender, EventArgs e) {
            udAll.Increment = udAll.Value / 10;
            udTemp.Value = udAll.Value;
            udTemp.Increment = udAll.Increment;
            udSubj.Value = udAll.Value;
            udSubj.Increment = udAll.Increment;
            udAlig.Value = udAll.Value;
            udAlig.Increment = udAll.Increment;

            reDrawImage(0);
            reDrawImage(1);
            reDrawImage(2);
        }

        private void reDrawImage(int idx) {
            switch (idx)
            {
                case 0:
                    pictureBox1.Image = Fits.GetFITSImage(image[idx], gamma[idx], trans_type, -1, -1, wavelengthIndex);//, iMin[idx], iMax[idx]);
                    if (isNegative) pictureBox1.Image = pictureBox1.Image.CopyAsNegative();
                    break;
                case 1:
                    pictureBox2.Image = Fits.GetFITSImage(image[idx], gamma[idx], trans_type, -1, -1, wavelengthIndex);//, iMin[idx], iMax[idx]);
                    if (isNegative) pictureBox2.Image = pictureBox2.Image.CopyAsNegative();
                    break;
                case 2:
                    pictureBox3.Image = Fits.GetFITSImage(image[idx], gamma[idx], trans_type, -1, -1, wavelengthIndex);//, iMin[idx], iMax[idx]);
                    if (isNegative) pictureBox3.Image = pictureBox3.Image.CopyAsNegative();
                    break;
            }
        }

        private void showImgesHeadersToolStripMenuItem_Click(object sender, EventArgs e) {
            string filepath1 = tempData[selectedTemp].path + lstTemplates.SelectedItem.ToString();
            string[] ret1 = GetHeadersData(filepath1);
            string filepath2 = subjData[selectedSubj].path + lstSubjects.SelectedItem.ToString();
            string[] ret2 = GetHeadersData(filepath2);
            FitsHeaders fHead = new FitsHeaders(this, ret1, ret2, filepath1, filepath2);
            fHead.ShowDialog();
        }

        private string[] GetHeadersData(string filepath) {
            string[] ret1 = null;
            List<string> result = Fits.readHDUFields(filepath);
            if (result == null)
            {
                if (iLogLevel > 0) Console.Out.WriteLine("Fits.readHDUField returned NULL");
            }
            else
            {
                ret1 = new string[result.Count];
                int i = 0;
                foreach (string ss in result)
                {
                    ret1[i] = ss;
                    i++;
                }
            }
            return ret1;
        }


        private void chkFTPTemplates_CheckedChanged(object sender, EventArgs e) {
            if (chkFTPTemplates.Checked)
            {
                cmdTemplatesDir.ImageIndex = 3;
            }
            else
            {
                cmdTemplatesDir.ImageIndex = 2;
            }
        }

        private void chkFTPSubjects_CheckedChanged(object sender, EventArgs e) {
            if (chkFTPSubjects.Checked)
            {
                cmdSubjectsDir.ImageIndex = 3;
            }
            else
            {
                cmdSubjectsDir.ImageIndex = 2;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e) {
            trans_type = Fits.transform_type.LIN;
            reDrawImage(0);
            reDrawImage(1);
            reDrawImage(2);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e) {
            trans_type = Fits.transform_type.LOG;
            reDrawImage(0);
            reDrawImage(1);
            reDrawImage(2);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e) {
            trans_type = Fits.transform_type.GAM;
            reDrawImage(0);
            reDrawImage(1);
            reDrawImage(2);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            lblFilterParam.Visible = fltParam[comboBox1.SelectedIndex].textParamVisibility;
            txtFilterParam.Visible = fltParam[comboBox1.SelectedIndex].textParamVisibility;
            lblFilterParam.Text = fltParam[comboBox1.SelectedIndex].textLabel;
            txtFilterParam.Text = fltParam[comboBox1.SelectedIndex].textDefaultValue;
            lblFilterParam2.Visible = fltParam[comboBox1.SelectedIndex].textParamVisibility2;
            txtFilterParam2.Visible = fltParam[comboBox1.SelectedIndex].textParamVisibility2;
            lblFilterParam2.Text = fltParam[comboBox1.SelectedIndex].textLabel2;
            txtFilterParam2.Text = fltParam[comboBox1.SelectedIndex].textDefaultValue2;
            chkFilterParam.Visible = fltParam[comboBox1.SelectedIndex].checkParamVisibility;
            chkFilterParam.Text = fltParam[comboBox1.SelectedIndex].checkParamText;
            cmdApplyFilter.Text = "Apply " + fltParam[comboBox1.SelectedIndex].funcName;
            bitmapFilterSelected = comboBox1.SelectedIndex;
        }

        private void cmdApplyFilter_Click(object sender, EventArgs e) {
            System.Windows.Forms.PictureBox picbox = null;
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    picbox = pictureBox1;
                    break;
                case 1:
                    picbox = pictureBox2;
                    break;
                case 2:
                    picbox = pictureBox3;
                    break;
            }
            if (picbox != null)
            {
                switch (comboBox1.SelectedIndex)
                {
                    case 0: //None
                        //
                        break;
                    case 1: //Median Filter
                        picbox.Image = picbox.Image.MedianFilter(int.Parse(txtFilterParam.Text));
                        break;
                    case 2: //Negative Filter
                        picbox.Image = picbox.Image.CopyAsNegative();//Filters.CopyAsNegative(picbox.Image);
                        break;
                    case 3://Color Balance                     
                        if (int.Parse(txtFilterParam.Text) > 255) txtFilterParam.Text = "255";
                        if (int.Parse(txtFilterParam.Text) < 0) txtFilterParam.Text = "0";
                        picbox.Image = picbox.Image.ColorBalance(byte.Parse(txtFilterParam.Text), byte.Parse(txtFilterParam.Text), byte.Parse(txtFilterParam.Text), chkFilterParam.Checked);
                        break;
                    case 4://Noise Removal
                        picbox.Image = picbox.Image.NoiseRemoval();
                        break;
                    case 5://Histogram Equalize
                        HistogramEqualization hefilter = new HistogramEqualization();
                        hefilter.ApplyInPlace((Bitmap)picbox.Image);
                        picbox.Refresh();
                        break;
                    case 6://Threshold Filter
                        Bitmap grayImage;
                        if (picbox.Image.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                        {
                            Grayscale filter1 = new Grayscale(0.7154, 0.7154, 0.7154);
                            grayImage = filter1.Apply((Bitmap)picbox.Image);
                        }
                        else
                        {
                            grayImage = (Bitmap)picbox.Image;
                        }
                        Threshold thfilter = new Threshold(int.Parse(txtFilterParam.Text));
                        thfilter.ApplyInPlace(grayImage);
                        picbox.Image = grayImage;
                        break;
                    case 7://Equalize
                        picbox.Image = picbox.Image.HistEq();
                        break;
                    case 8://Normalization
                        byte min = byte.Parse(txtFilterParam.Text);
                        byte max = byte.Parse(txtFilterParam2.Text);
                        picbox.Image = picbox.Image.Normalization(min, max);
                        break;
                    case 9://Linear Levels
                        Bitmap grayImage2;
                        if (picbox.Image.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                        {
                            Grayscale filter1 = new Grayscale(0.7154, 0.7154, 0.7154);//0.7154
                            grayImage2 = filter1.Apply((Bitmap)picbox.Image);
                        }
                        else
                        {
                            grayImage2 = (Bitmap)picbox.Image;
                        }
                        AForge.Imaging.ImageStatistics statistics = new AForge.Imaging.ImageStatistics(grayImage2);
                        // get the red histogram
                        AForge.Math.Histogram histogram = statistics.Gray;
                        // get the values
                        //double mean = histogram.Mean;     // mean red value
                        //double stddev = histogram.StdDev; // standard deviation of red values
                        //int median = histogram.Median;    // median red value
                        //int min = histogram.Min;          // min red value
                        //int max = histogram.Max;          // max value
                        // get 90% range around the median
                        AForge.IntRange range = histogram.GetRange(double.Parse(txtFilterParam.Text));
                        // create levels filter
                        AForge.Imaging.Filters.LevelsLinear llfilter = new AForge.Imaging.Filters.LevelsLinear();
                        llfilter.InGray = new AForge.IntRange(range.Min, range.Max);
                        // apply the filter
                        picbox.Image = llfilter.Apply((Bitmap)picbox.Image);
                        break;
                    case 10: // Pixelate-Blur
                        Bitmap grayImage3;
                        if (picbox.Image.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                        {
                            Grayscale filter1 = new Grayscale(1.0, 1.0, 1.0);//0.7154
                            grayImage3 = filter1.Apply((Bitmap)picbox.Image);
                        }
                        else
                        {
                            grayImage3 = (Bitmap)picbox.Image;
                        }
                        AForge.Imaging.Filters.FiltersSequence filter4 = new AForge.Imaging.Filters.FiltersSequence{
                            // add filters to the sequence
                            new AForge.Imaging.Filters.Pixellate(2)
                        };
                        // apply the sequence to an image
                        picbox.Image = filter4.Apply(grayImage3);
                        break;
                    case 11: //Low Pass Filter
                        Bitmap grayImage5;
                        if (picbox.Image.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                        {
                            Grayscale filter5 = new Grayscale(0.7154, 0.7154, 0.7154);//0.7154
                            grayImage5 = filter5.Apply((Bitmap)picbox.Image);
                        }
                        else
                        {
                            grayImage5 = (Bitmap)picbox.Image;
                        }
                        picbox.Image = grayImage5.LowHighPassFilter(Int16.Parse(txtFilterParam.Text), Int16.Parse(txtFilterParam2.Text));
                        break;
                }
            }
        }

        private void cmdApplySharpen_Click(object sender, EventArgs e) {
            System.Windows.Forms.PictureBox picbox = null;
            int idx = tabControl1.SelectedIndex;
            switch (tabControl1.SelectedIndex){
                case 0:
                    picbox = pictureBox1;
                    break;
                case 1:
                    picbox = pictureBox2;
                    break;
                case 2:
                    picbox = pictureBox3;
                    break;
            }
            picbox.Image = Fits.GetFITSImage(Fits.ImageSharpen(image[idx], sharpenK, sharpenDscan), gamma[idx], trans_type);
        }

            public Fits.imageptr updateImagePtrMinMax(Fits.imageptr img, int minVal, int maxVal) {
            Fits.imageptr im = Fits.CloneImagePtr(img);
            int dynamic = maxVal - minVal;
            for (int i = 0; i < im.data.Length; i++)
                im.data[i] = ((im.data[i] - minVal) / dynamic) * Fits.GREPNOVA_DATAMAX;
            return im;
        }

        public int[] calcHisto(Image pboximg) {
            Bitmap bmpimg = (Bitmap)pboximg.Clone();
            BitmapData data = bmpimg.LockBits(new System.Drawing.Rectangle(0, 0, bmpimg.Width, bmpimg.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int[] histogram = new int[256];
            unsafe
            {
                byte* ptr = (byte*)data.Scan0;

                int remain = data.Stride - data.Width * 3;
                for (int i = 0; i < histogram.Length; i++)
                    histogram[i] = 0;

                for (int i = 0; i < data.Height; i++)
                {
                    for (int j = 0; j < data.Width; j++)
                    {
                        int mean = ptr[0] + ptr[1] + ptr[2];
                        mean /= 3;

                        histogram[mean]++;
                        ptr += 3;
                    }
                    ptr += remain;
                }
            }
            bmpimg.UnlockBits(data);
            return histogram;
        }


        public static void drawHistogram(float[] histogram, PictureBox picbox) {
            if (histogram == null || picbox == null) return;
            float scale = 1.0f;
            Bitmap bmp = new Bitmap(histogram.Length + 10, picbox.Height + 10);
            picbox.Image = bmp;
            BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
                int remain = data.Stride - data.Width * 3;
                byte* ptr = (byte*)data.Scan0;
                for (int i = 0; i < data.Height; i++)
                {
                    for (int j = 0; j < data.Width; j++)
                    {
                        ptr[0] = ptr[1] = ptr[2] = 150;
                        ptr += 3;
                    }
                    ptr += remain;
                }

                float max = 0;
                for (int i = 0; i < histogram.Length; i++)
                {
                    if (max < histogram[i])
                        max = histogram[i];

                }

                for (int i = 0; i < histogram.Length; i++)
                {
                    ptr = (byte*)data.Scan0;
                    ptr += data.Stride * (picbox.Height + 5) + (i + 5) * 3;
                    int length = (int)(scale * histogram[i] * picbox.Height / max);
                    //Console.Out.WriteLine("i={0} hist={1} length={2}", i, histogram[i], length);
                    for (int j = 0; j < length; j++)
                    {
                        ptr[0] = 255;
                        ptr[1] = ptr[2] = 0;
                        ptr -= data.Stride;
                    }
                }
            }
            bmp.UnlockBits(data);
        }

        private void lblMin_TextChanged(object sender, EventArgs e) {
            if (bDontRun) return;
            try
            {
                bNotChangeSelection = true;
                selectionRangeSlider1.SelectedMin = int.Parse(lblMin.Text);
                bNotChangeSelection = false;
                selectionRangeSlider1_SelectionChanged(sender, e);
            }
            catch (Exception ee) { LogError("lblMin_TextChanged", ee.Message.ToString()); }
        }

        private void lblMax_TextChanged(object sender, EventArgs e) {
            if (bDontRun) return;
            try
            {
                bNotChangeSelection = true;
                selectionRangeSlider1.SelectedMax = int.Parse(lblMax.Text);
                bNotChangeSelection = false;
                selectionRangeSlider1_SelectionChanged(sender, e);
            }
            catch (Exception ee) { LogError("lblMax_TextChanged", ee.Message.ToString()); }
        }

        private void SetImageMinMax(int img_no, int min, int max) {
            if (img_no > 2) return;
            System.Windows.Forms.PictureBox picbox = null;
            switch (img_no)
            {
                case 0:
                    picbox = pictureBox1;
                    break;
                case 1:
                    picbox = pictureBox2;
                    break;
                case 2:
                    picbox = pictureBox3;
                    break;
            }
        }

        private void btnRestoreCurrent_Click(object sender, EventArgs e) {
            image_no = tabControl1.SelectedIndex;
            iMin[image_no] = getImageMin(image[image_no]);
            iMax[image_no] = getImageMax(image[image_no]);
            reDrawImage(image_no);
            selectionRangeSlider1_SelectionChanged(sender, e);
        }

        private void btnRestoreAll_Click(object sender, EventArgs e) {
            for (int i = 0; i < 3; i++)
            {
                iMin[i] = getImageMin(image[i]);
                iMax[i] = getImageMax(image[i]);
                reDrawImage(i);
            }
            selectionRangeSlider1_SelectionChanged(sender, e);
        }

        private void btnResetMinMax_Click(object sender, EventArgs e) {
            int i = tabControl1.SelectedIndex;
            iMin[i] = iMinDefault[i];
            iMax[i] = iMaxDefault[i];
            bNotChangeSelection = true;
            selectionRangeSlider1.SelectedMin = iMin[i];
            selectionRangeSlider1.SelectedMax = (iMax[i] - iMin[i]) / 2;
            bNotChangeSelection = false;
            selectionRangeSlider1_SelectionChanged(sender, e);
        }

        private void selectTemplatesFolderToolStripMenuItem_Click(object sender, EventArgs e) {
            cmdTemplatesDir_Click(sender, e);
        }

        private void selectSubjectsFolderToolStripMenuItem_Click(object sender, EventArgs e) {
            cmdSubjectsDir_Click(sender, e);
        }

        private void selectionRangeSlider1_SelectionChanged(object sender, EventArgs e) {
            if (bNotChangeSelection || !checkBox1.Checked) return;
            if (tabControl1.SelectedIndex == 3) return;
            image_no = tabControl1.SelectedIndex;
            bDontRun = true;
            lblMin.Text = "" + selectionRangeSlider1.SelectedMin;
            lblMax.Text = "" + selectionRangeSlider1.SelectedMax;
            image_no = tabControl1.SelectedIndex;
            bDontRun = false;
            iCurrentMin[image_no] = selectionRangeSlider1.SelectedMin;
            iCurrentMax[image_no] = selectionRangeSlider1.SelectedMax;
            int pcnt = 255;
            int contr = 0;
            if (selectionRangeSlider1.Max != selectionRangeSlider1.Min)
            {
                pcnt = (int)(255 * selectionRangeSlider1.SelectedMin / (selectionRangeSlider1.Max - selectionRangeSlider1.Min));
                contr = (int)(100 * (2 * selectionRangeSlider1.SelectedMax - (selectionRangeSlider1.Max - selectionRangeSlider1.Min)) / (selectionRangeSlider1.Max - selectionRangeSlider1.Min));
            }

            if (copyImage[image_no] != null)
            {
                switch (image_no)
                {
                    case 0:
                        pictureBox1.Image = Fits.AdjustContrast(Fits.AdjustBrightness(copyImage[image_no], pcnt), contr);
                        break;
                    case 1:
                        pictureBox2.Image = Fits.AdjustContrast(Fits.AdjustBrightness(copyImage[image_no], pcnt), contr);
                        break;
                    case 2:
                        pictureBox3.Image = Fits.AdjustContrast(Fits.AdjustBrightness(copyImage[image_no], pcnt), contr);
                        break;
                }
            }
        }

        //stretch image checkbox
        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            cookHistImage.Visible = checkBox1.Checked;
            selectionRangeSlider1.Visible = checkBox1.Checked;
            lblMin.Visible = checkBox1.Checked;
            lblMax.Visible = checkBox1.Checked;
            btnResetMinMax.Visible = checkBox1.Checked;
        }

        //Normal or Negative checkbox
        private void checkBox2_CheckedChanged(object sender, EventArgs e) {
            isNegative = checkBox2.Checked;
            if (isNegative)
            {
                pictureBox1.Image = pictureBox1.Image.CopyAsNegative();
                pictureBox2.Image = pictureBox2.Image.CopyAsNegative();
                pictureBox3.Image = pictureBox3.Image.CopyAsNegative();
                cur = new Cursor(cursorType == 0 ? Properties.Resources.cross_black.Handle : Properties.Resources.circle_black.Handle);
                pictureBox1.Cursor = cur;
                pictureBox2.Cursor = cur;
                pictureBox3.Cursor = cur;
            }
            else
            {
                pictureBox1.Image = pictureBox1.Image.CopyAsNegative();
                pictureBox2.Image = pictureBox2.Image.CopyAsNegative();
                pictureBox3.Image = pictureBox3.Image.CopyAsNegative();
                cur = new Cursor(cursorType == 0 ? Properties.Resources.cross.Handle : Properties.Resources.circle.Handle);
                pictureBox1.Cursor = cur;
                pictureBox2.Cursor = cur;
                pictureBox3.Cursor = cur;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e) {
            if (chkShowHistogram.Checked){
                ZoomHistogram();
            }else{
                CooknasStar.Centroid.STAR_COORD cen;
                try{
                    cen = centroid.position(image[0], mouseX, mouseY, cooknasStar);
                }catch (Exception) { cen.val = 0; cen.text = ""; cen.X = 0; cen.Y = 0; }
                if (cen.val < 0) return;
                if (Control.ModifierKeys == Keys.Control){

                }else{
                    if (Crosses[tabControl1.SelectedIndex] == null) Crosses[tabControl1.SelectedIndex] = new List<Fits.PointValue>();
                    Fits.PointValue p = new Fits.PointValue();
                    graphText = String.Format("Object in Template {0} @ X{1:F2} , Y{2:F2}", image[0].object_name, cen.X, cen.Y);
                    lblHistName.Text = String.Format("{0}({1})@{2:F0},{3:F0}", image_type_short[0], image[0].object_name, cen.X, cen.Y);
                    p.p = new Point((int)cen.X, (int)cen.Y);
                    p.v = cen.val;
                    Crosses[tabControl1.SelectedIndex].Add(p);
                    drawCross(0, Crosses[tabControl1.SelectedIndex], 7, true, 0);
                    if (cen.text == null) cen.text = "returned null";
                    cooknasStar_DoubleClick(sender, e);
                }
            }
        }

        int iAddStarPhase = 0;

        private void pictureBox2_Click(object sender, EventArgs e) {
            if (chkShowHistogram.Checked){
                ZoomHistogram();
            }else{
                CooknasStar.Centroid.STAR_COORD cen;
                try{
                    cen = centroid.position(image[1], mouseX, mouseY, cooknasStar);
                }catch (Exception) { cen.val = 0; cen.text = ""; cen.X = 0; cen.Y = 0; }
                if (cen.val < 0) return;
                if (Control.ModifierKeys == Keys.Control){
                    lblHistName.Text = String.Format("{0}({1})@{2:F0},{3:F0}", image_type_short[1], image[1].object_name, cen.X, cen.Y);
                    FoundSN.p = new Point((int)cen.X, (int)cen.Y);
                    FoundSN.v = cen.val;
                    pictureBox2.Invalidate();
                    Application.DoEvents();
                    if (Circles[1] != null) drawCircles(1, Circles[1]);
                    if (Crosses[1] != null) drawCross(1, Crosses[1]);
                    drawSN(FoundSN);
                }else if (Control.ModifierKeys == Keys.Shift){
                    iAddStarPhase++;
                    if (iAddStarPhase == 1){
                        mouseCopyImage = null;
                        CopyImageSubject((int)cen.X, (int)cen.Y);
                        LogEntry("Copied (" + ((int)cen.X - 10) + "," + ((int)cen.Y - 10) + "),(" + ((int)cen.X + 10) + "," + ((int)cen.Y + 10) + ")");
                    }else{
                        PasteImageSubject(mouseX, mouseY);
                        iAddStarPhase = 0;
                        mouseCopyImage = null;
                        LogEntry("Pasted (" + ((int)mouseX - 10) + "," + ((int)mouseY - 10) + "),(" + ((int)mouseX + 10) + "," + ((int)mouseY + 10) + ")");
                    }
                }else{
                    if (Crosses[tabControl1.SelectedIndex] == null) Crosses[tabControl1.SelectedIndex] = new List<Fits.PointValue>();
                    Fits.PointValue p = new Fits.PointValue();
                    graphText = String.Format("Object in Subject {0} @ {1:F2} , {2:F2}", image[1].object_name, cen.X, cen.Y);
                    lblHistName.Text = String.Format("{0}({1})@{2:F0},{3:F0}", image_type_short[1], image[1].object_name, cen.X, cen.Y);
                    p.p = new Point((int)cen.X, (int)cen.Y);
                    p.v = cen.val;
                    Crosses[tabControl1.SelectedIndex].Add(p);
                    drawCross(1, Crosses[tabControl1.SelectedIndex], 7, true, 0);
                    if (cen.text == null) cen.text = "returned null";
                    cooknasStar_DoubleClick(sender, e);
                }
            }
        }

        /////////////////////////////////////////////////////////////////
        static int[] mouseCopyImage = null;

        private void CopyImageSubject(int xPos, int yPos) {
            mouseCopyImage = new int[21 * 21];
            for (int y = (int)yPos - 10; y <= (int)yPos + 10; y++)
                for (int x = (int)xPos - 10; x < (int)xPos + 10; x++)
                    mouseCopyImage[(y - (int)yPos + 10) * 21 + x - (int)xPos + 10] = image[1].data[y * image[1].xsize + x];
        }

        private void PasteImageSubject(int xPos, int yPos) {
            if (mouseCopyImage == null) return;
            for (int y = (int)yPos - 10; y <= (int)yPos + 10; y++)
                for (int x = (int)xPos - 10; x < (int)xPos + 10; x++)
                    image[1].data[y * image[1].xsize + x] = mouseCopyImage[(y - (int)yPos + 10) * 21 + x - (int)xPos + 10];
            pictureBox2.Image = Fits.GetFITSImage(image[1], gamma[1], trans_type, -1, -1, wavelengthIndex);
            pictureBox2.Invalidate();
            Console.Out.WriteLine("PasteImageSubject OK.");
        }
        /////////////////////////////////////////////////////////////////


        private void pictureBox3_Click(object sender, EventArgs e) {
            if (chkShowHistogram.Checked)
            {
                ZoomHistogram();
            }
            else
            {
                CooknasStar.Centroid.STAR_COORD cen;
                try{
                    cen = centroid.position(image[2], mouseX, mouseY, cooknasStar);
                }catch(Exception) { cen.val = 0; cen.text = "";cen.X = 0;cen.Y = 0; }
                if (cen.val < 0) return;
                if (Control.ModifierKeys == Keys.Control){

                }else{
                    if (Crosses[tabControl1.SelectedIndex] == null) Crosses[tabControl1.SelectedIndex] = new List<Fits.PointValue>();
                    Fits.PointValue p = new Fits.PointValue();
                    graphText = String.Format("Object in Aligned Tmpl {0} @ {1:F2} , {2:F2}", image[2].object_name, cen.X, cen.Y);
                    lblHistName.Text = String.Format("{0}({1})@{2:F0},{3:F0}", image_type_short[2], image[2].object_name, cen.X, cen.Y);
                    p.p = new Point((int)cen.X, (int)cen.Y);
                    p.v = cen.val;
                    Crosses[tabControl1.SelectedIndex].Add(p);
                    drawCross(2, Crosses[tabControl1.SelectedIndex], 7, true, 0);
                    if (cen.text == null) cen.text = "returned null";
                    cooknasStar_DoubleClick(sender, e);
                }
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e) {
            if (File.Exists("grepnova2.pdf"))
            {
                System.Diagnostics.Process.Start("grepnova2.pdf");
            }
            else
            {
                MessageBox.Show(this, "Cannot find file 'grepnova2.pdf'", "Grepnova Help error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cooknasStar_DoubleClick(object sender, EventArgs e) {
            StarGraph starGraph = new StarGraph();
            starGraph.label1.Text = graphText;
            starGraph.cooknasStar1.DrawStar(cooknasStar.GetDistances, cooknasStar.GetValues, 0, 0);
            starGraph.webBrowser1.DocumentText = centroid.GetText;
            starGraph.Show();
        }

        private void removeAnnotationsbrightestsCentroidsToolStripMenuItem_Click(object sender, EventArgs e) {
            Circles[0] = null;
            Circles[1] = null;
            Circles[2] = null;
            Crosses[0] = null;
            Crosses[1] = null;
            Crosses[2] = null;
            Lines[0] = null;
            Lines[1] = null;
            tabControl1.Refresh();
            drawSN(FoundSN);
        }

        private void removeSNCandidateToolStripMenuItem_Click(object sender, EventArgs e) {
            FoundSN = new Fits.PointValue();
            if (tabControl1.SelectedIndex == 1)
            {
                tabControl1.Refresh();
                if (Circles[1] != null) drawCircles(1, Circles[1]);
                if (Crosses[1] != null) drawCross(1, Crosses[1]);
            }
        }

        private void aboutInfoToolStripMenuItem_Click(object sender, EventArgs e) {
            AboutBox1 about = new AboutBox1(1);
            about.ShowDialog();

        }

        private void aboutGrepNova2CooknasToolStripMenuItem_Click(object sender, EventArgs e) {
            AboutBox1 about = new AboutBox1(2);
            about.ShowDialog();
        }

        private void btnClear_Click(object sender, EventArgs e) {
            txtLog.Text = "";
        }

        private void btnSave_Click(object sender, EventArgs e) {
            saveFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.Title = "Save log as Text File";
            saveFileDialog1.DefaultExt = "txt";
            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.CheckFileExists = false;
            saveFileDialog1.CheckPathExists = true;
            string fn = "";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fn = saveFileDialog1.FileName;
                try
                {
                    System.IO.File.WriteAllText(fn, txtLog.Text.ToString());
                }
                catch (Exception ex)
                {
                    LogError("btnSave_Click", "Write file error: " + ex.Message.ToString());
                }
            }
        }

        private void imageStatisticsToolStripMenuItem_Click(object sender, EventArgs e) {
            image_no = tabControl1.SelectedIndex;
            StatisticsForm sf = new StatisticsForm(image[image_no], image_no, image[image_no].object_name);
            sf.ShowDialog();
        }

        private void bitmapFilteringToolStripMenuItem_Click(object sender, EventArgs e) {
            if (!bBitmapFilteringIsOn)
            {
                bmFiltering = new BitmapFiltering();
                bBitmapFilteringIsOn = true;
                bmFiltering.Show();
            }
        }

        private void stretchImageToolStripMenuItem_Click(object sender, EventArgs e) {
            if (!bStretchImageIsOn)
            {
                stretchImage = new StretchImage();
                stretchImage.SetImage(tabControl1.SelectedIndex,
                                      selectionRangeSlider1.Min,
                                      selectionRangeSlider1.Max,
                                      selectionRangeSlider1.SelectedMin,
                                      selectionRangeSlider1.SelectedMax);
                bStretchImageIsOn = true;
                checkBox1.Checked = true;
                stretchImage.Show();
            }
        }

        private void sharpenFITSToolStripMenuItem_Click(object sender, EventArgs e) {
            if (!bSharpeningIsOn){
                sharpenFITS = new SharpenFITS();
                bSharpeningIsOn = true;
                sharpenFITS.Show();
            }
        }

        private void Form1_Move(object sender, EventArgs e) {
            if (bStretchImageIsOn && bBitmapFilteringIsOn){
                switch (iStretchImagePosition){
                    case 1:
                        stretchImage.Left = Form1.frm.Left - stretchImage.Width;
                        stretchImage.Top = Form1.frm.Top;
                        bmFiltering.Left = Form1.frm.Left - bmFiltering.Width;
                        bmFiltering.Top = Form1.frm.Top + stretchImage.Height;
                        break;
                    case 2:
                        bmFiltering.Left = Form1.frm.Left - bmFiltering.Width;
                        bmFiltering.Top = Form1.frm.Top;
                        stretchImage.Left = Form1.frm.Left - stretchImage.Width;
                        stretchImage.Top = Form1.frm.Top + bmFiltering.Height;
                        break;
                }
            }else{
                if (bBitmapFilteringIsOn){
                    bmFiltering.Left = Form1.frm.Left - bmFiltering.Width;
                    bmFiltering.Top = Form1.frm.Top + stretchImage.Height;
                }
                if (bStretchImageIsOn){
                    stretchImage.Left = Form1.frm.Left - stretchImage.Width;
                    stretchImage.Top = Form1.frm.Top;
                }
            }
            if (bSharpeningIsOn){
                sharpenFITS.Left = Form1.frm.Left - sharpenFITS.Width;
                sharpenFITS.Top = Form1.frm.Top;
            }
            if (bImageCurveIsOn){
                imageCurveForm.Left = Form1.frm.Left - imageCurveForm.Width;
                imageCurveForm.Top = Form1.frm.Top;
            }
        }

        private void btnJPG_Click(object sender, EventArgs e) {
            System.Windows.Forms.PictureBox picbox = null;
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    picbox = pictureBox1;
                    break;
                case 1:
                    picbox = pictureBox2;
                    break;
                case 2:
                    picbox = pictureBox3;
                    break;
            }
            if (picbox != null)
            {
                saveFileDialog1.InitialDirectory = localImageDir;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.Title = "Save image as picture file";
                saveFileDialog1.DefaultExt = "jpg";
                string fileFilter = "BMP files (*.bmp)|*.bmp";
                fileFilter += "|EMF files (*.emf)|*.emf";
                fileFilter += "|GIF files (*.gif)|*.gif";
                fileFilter += "|JPEG files (*.jpg)|*.jpg";
                fileFilter += "|PNG files (*.png)|*.png";
                fileFilter += "|TIFF files (*.tiff)|*.tiff";
                fileFilter += "|WMF files (*.wmf)|*.wmf";
                saveFileDialog1.Filter = fileFilter;
                saveFileDialog1.FilterIndex = 4;
                saveFileDialog1.CheckFileExists = false;
                saveFileDialog1.CheckPathExists = true;
                string filename = "";
                ImageFormat imgFMT = null;
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    filename = saveFileDialog1.FileName;
                    if (filename != null && filename.Length > 0)
                    {
                        switch (saveFileDialog1.FilterIndex)
                        {
                            case 1://BMP
                                imgFMT = ImageFormat.Bmp;
                                break;
                            case 2://EMF
                                imgFMT = ImageFormat.Emf;
                                break;
                            case 3://GIF
                                imgFMT = ImageFormat.Gif;
                                break;
                            case 4://JPG
                                imgFMT = ImageFormat.Jpeg;
                                break;
                            case 5://PNG
                                imgFMT = ImageFormat.Png;
                                break;
                            case 6://TIFF
                                imgFMT = ImageFormat.Tiff;
                                break;
                            case 7://WMF
                                imgFMT = ImageFormat.Wmf;
                                break;
                        }
                        if (imgFMT != null)
                        {
                            var newImage = new Bitmap(picbox.Image.Width, picbox.Image.Height);
                            var gf = Graphics.FromImage(newImage);
                            gf.DrawImage(picbox.Image, new Rectangle(0, 0, picbox.Image.Width, picbox.Image.Height), new Rectangle(0, 0, picbox.Image.Width, picbox.Image.Height), GraphicsUnit.Pixel);
                            //ask if we have to save also annotations on the image
                            if (MessageBox.Show("Do you want to save the Annotations together with the image?", "Save Annotations?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                //draw circles on created Graphics gf
                                Color col = Color.Red;
                                int r = 10;
                                int iC = 1;
                                bool bDrawText = true;
                                Font font = new Font("Calibri", 8.0f);
                                Brush brush = new SolidBrush(System.Drawing.Color.FromArgb(220, 255, 50, 50));
                                if (Circles[image_no] != null)
                                {
                                    if (Circles[image_no].Count > 0)
                                    {
                                        foreach (Fits.PointValue p in Circles[tabControl1.SelectedIndex])
                                        {
                                            gf.DrawEllipse(new Pen(col, 2), new Rectangle(p.p.X - r, p.p.Y - r, 2 * r, 2 * r));
                                            gf.DrawLine(new Pen(col, 1), new Point(p.p.X, p.p.Y - 5), new Point(p.p.X, p.p.Y + 5));
                                            gf.DrawLine(new Pen(col, 1), new Point(p.p.X - 5, p.p.Y), new Point(p.p.X + 5, p.p.Y));
                                            if (bDrawText) gf.DrawString(String.Format("{0}", iC), font, brush, new Point(p.p.X + r, p.p.Y - 4));
                                            iC++;
                                        }
                                    }
                                }
                                //draw croses on created Graphics gf
                                int rsmall = 1;
                                if (Crosses[image_no] != null)
                                {
                                    if (Crosses[image_no].Count > 0)
                                    {
                                        foreach (Fits.PointValue p in Crosses[tabControl1.SelectedIndex])
                                        {
                                            if (p.p.X > 0 && p.p.Y > 0)
                                            {
                                                gf.DrawLine(new Pen(col, 1), new Point(p.p.X - 10, p.p.Y), new Point(p.p.X - 5, p.p.Y));
                                                gf.DrawLine(new Pen(col, 1), new Point(p.p.X + 5, p.p.Y), new Point(p.p.X + 10, p.p.Y));
                                                gf.DrawLine(new Pen(col, 1), new Point(p.p.X, p.p.Y - 10), new Point(p.p.X, p.p.Y - 5));
                                                gf.DrawLine(new Pen(col, 1), new Point(p.p.X, p.p.Y + 5), new Point(p.p.X, p.p.Y + 10));

                                                gf.DrawEllipse(new Pen(Color.Red, 2), new Rectangle(p.p.X - rsmall, p.p.Y - rsmall, 2 * rsmall, 2 * rsmall));
                                                //the ranking of the brightness
                                                if (bDrawText) gf.DrawString(String.Format("{0}", p.v), font, brush, new Point(p.p.X + 12, p.p.Y - 4));
                                                iC++;
                                            }
                                        }
                                    }
                                }
                                gf.Dispose();
                            }
                            //Now you can save the bitmap
                            newImage.Save(filename, imgFMT);
                        }
                    }
                }
            }
        }

        private void btnFITS_Click(object sender, EventArgs e) {
            Fits.imageptr picbox = Fits.CreateBlackImage(defaultImageX, defaultImageY);
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    picbox = image[0];
                    break;
                case 1:
                    picbox = image[1];
                    break;
                case 2:
                    picbox = image[2];
                    break;
            }
            if (tabControl1.SelectedIndex >= 0 & tabControl1.SelectedIndex <= 2)
            {
                saveFileDialog1.InitialDirectory = localImageDir;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.Title = "Save image as fits file";
                saveFileDialog1.DefaultExt = "fts";
                string fileFilter = "FITS files (*.fits)|*.fits|(*.fts)|*.fts|(*.fit)|*.fit";
                saveFileDialog1.Filter = fileFilter;
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.CheckFileExists = false;
                saveFileDialog1.CheckPathExists = true;
                string filename = "";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    filename = saveFileDialog1.FileName;
                    if (filename != null && filename.Length > 0)
                    {
                        Fits.save_fits(picbox, filename);
                        LogEntry("Image saved as '" + filename + "'");
                    }
                }
            }
        }

        private void crossToolStripMenuItem_Click(object sender, EventArgs e) {
            cursorType = 0;//cross
            crossToolStripMenuItem.Checked = true;
            circleToolStripMenuItem.Checked = false;
            if (isNegative)
            {
                cur = new Cursor(cursorType == 0 ? Properties.Resources.cross_black.Handle : Properties.Resources.circle_black.Handle);
            }
            else
            {
                cur = new Cursor(cursorType == 0 ? Properties.Resources.cross.Handle : Properties.Resources.circle.Handle);
            }
            pictureBox1.Cursor = cur;
            pictureBox2.Cursor = cur;
            pictureBox3.Cursor = cur;
        }

        private void circleToolStripMenuItem_Click(object sender, EventArgs e) {
            cursorType = 1;//circle
            circleToolStripMenuItem.Checked = true;
            crossToolStripMenuItem.Checked = false;
            if (isNegative)
            {
                cur = new Cursor(cursorType == 0 ? Properties.Resources.cross_black.Handle : Properties.Resources.circle_black.Handle);
            }
            else
            {
                cur = new Cursor(cursorType == 0 ? Properties.Resources.cross.Handle : Properties.Resources.circle.Handle);
            }
            pictureBox1.Cursor = cur;
            pictureBox2.Cursor = cur;
            pictureBox3.Cursor = cur;
        }

        private void resizeTemplateWhenNeededToolStripMenuItem_Click(object sender, EventArgs e) {
            resizeTemplateWhenNeededToolStripMenuItem.Checked = !resizeTemplateWhenNeededToolStripMenuItem.Checked;
            bResizeTemplate = resizeTemplateWhenNeededToolStripMenuItem.Checked;
        }

        //go to MaxForm2
        private void button2_Click(object sender, EventArgs e) {
            maxForm2 = new MaxForm2();
            maxForm2.delayTime = delayTime;
            UpdateMaxForm2();
            if (iLogLevel > 2) LogEntry("Switching to Full-Screen mode");
            maxForm2.ShowDialog();
        }

        public static void UpdateMaxForm2() {
            //set images
            frm.maxForm2.pictureBox1.Image = frm.pictureBox1.Image;
            frm.maxForm2.pictureBox2.Image = frm.pictureBox2.Image;
            frm.maxForm2.pictureBox3.Image = frm.pictureBox3.Image;
            frm.maxForm2.lblObjectName.Text = image[1].object_name;
            //set images' labels
            frm.maxForm2.lblImgTemplate.Text = frm.lblImgTemplate.Text;
            frm.maxForm2.lblImgSubject.Text = frm.lblImgSubject.Text;
            frm.maxForm2.lblImgAlignedSize.Text = frm.lblImgAlignedSize.Text;
            //set prev-next labels
            if (frm.lstSubjects.SelectedIndex < frm.lstSubjects.Items.Count - 1)
                frm.maxForm2.lblNext.Text = frm.lstSubjects.Items[frm.lstSubjects.SelectedIndex + 1].ToString();
            else
                frm.maxForm2.lblNext.Text = frm.lstSubjects.Items[0].ToString();
            if (frm.lstSubjects.SelectedIndex > 0)
                frm.maxForm2.lblPrev.Text = frm.lstSubjects.Items[frm.lstSubjects.SelectedIndex - 1].ToString();
            else
                frm.maxForm2.lblPrev.Text = frm.lstSubjects.Items[frm.lstSubjects.Items.Count - 1].ToString();
            //set blink delay
            frm.maxForm2.lblImgBlink.Text = frm.lblImgBlink.Text;
        }


        private void filterImagesToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            foreach (ToolStripMenuItem mi in filterImagesToolStripMenuItem.DropDownItems)
            {
                if (mi.Text == e.ClickedItem.Text)
                    mi.Checked = true;
                else
                    mi.Checked = false;
            }
            wavelengthIndex = wavelength[e.ClickedItem.Text];
            reDrawImage(2);
            reDrawImage(1);
            reDrawImage(0);
        }


        private void blankSubjectToolStripMenuItem_Click(object sender, EventArgs e) {
            blankSubjectToolStripMenuItem.Checked = !blankSubjectToolStripMenuItem.Checked;
            bBlankSubject = blankSubjectToolStripMenuItem.Checked;
            if (bBlankSubject)
            {
                image[1] = Fits.BlankSubject(image[1], image[2]);
                pictureBox2.Image = Fits.GetFITSImage(image[1], gamma[1], trans_type, -1, -1, wavelengthIndex);
            }
        }

        private void searchForCandidatesToolStripMenuItem_Click(object sender, EventArgs e) {
            int num = 20;
            try
            {
                num = int.Parse(txtWinWidth.Text);
            }
            catch (Exception) { }
            CheckForSNCandidatesInSubject(num);
        }

        private void autoSearchLoadedBatchToolStripMenuItem_Click(object sender, EventArgs e) {
            int num = 100;
            int iC = 0;
            autoSearchIsOn = !autoSearchIsOn;
            if (autoSearchIsOn)
            {
                autoSearchLoadedBatchToolStripMenuItem.Text = "Stop Auto-Searching";
            }
            else
            {
                autoSearchLoadedBatchToolStripMenuItem.Text = "Auto-Search the Loaded Batch";
                return;
            }
            //change some global variables just for auto-search
            bool prevBBlankSubject = bBlankSubject;
            bBlankSubject = true;
            int prevTabControl = tabControl1.SelectedIndex;
            tabControl1.SelectedIndex = 1;
            int prevILogLevel = iLogLevel;
            iLogLevel = 0;
            candidatesFound = 0;
            //loop through all images in batch starting with the already selected one
            lstSubjects_SelectedIndexChanged(sender, e);
            while (iC < lstSubjects.Items.Count && autoSearchIsOn)
            {
                LogEntry("" + (iC + 1) + "AutoSearching " + subjData[selectedSubj].path + subjData[selectedSubj].filename + "...");
                CheckForSNCandidatesInSubject(num);
                cmdNext_Click(sender, e);
                Application.DoEvents();
                iC++;
            }
            if (candidatesFound > 0){
                if (candidatesFound > 1) LogEntry("Entire Batch of " + iC + " images were searched. " + candidatesFound + " candidates were found.");
                else LogEntry("Entire Batch of " + iC + " images were searched. One candidate was found.");
            }else{
                LogEntry("Entire Batch of " + iC + " images were searched. No candidate was found.");
            }

            autoSearchIsOn = false;
            autoSearchLoadedBatchToolStripMenuItem.Text = "Auto-Search the Loaded Batch";
            //set back the changed global variables
            iLogLevel = prevILogLevel;
            bBlankSubject = prevBBlankSubject;
            tabControl1.SelectedIndex = prevTabControl;
        }

        private void fitsDataToConsoleToolStripMenuItem1_Click(object sender, EventArgs e) {
            saveFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.Title = "Save Image Data as Text File";
            saveFileDialog1.DefaultExt = "txt";
            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.CheckFileExists = false;
            saveFileDialog1.CheckPathExists = true;
            string fn = "";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fn = saveFileDialog1.FileName;
                if (tabControl1.SelectedIndex >= 0 && tabControl1.SelectedIndex < 3)
                {
                    Application.UseWaitCursor = true;
                    Application.DoEvents();
                    int idx = tabControl1.SelectedIndex;
                    List<int> list = image[idx].data.ToList<int>();
                    string outline = "Dumping image " + image[idx].object_name + " to console:\r\n";
                    int li = 0;
                    foreach (var batch in list.Batch(image[idx].xsize)){
                        //Console.WriteLine(String.Join(",", batch));
                        outline += "Line " + li + "\r\n" + String.Join(",", batch) + "\r\n";
                        li++;
                    }
                    try
                    {
                        System.IO.File.WriteAllText(fn, outline);
                    }
                    catch (Exception ex)
                    {
                        LogError("fitsDataToConsoleToolStripMenuItem1_Click", "Write file error: " + ex.Message.ToString());
                    }
                    //LogEntry(outline);// Fits.fitsdata2console(fn));
                    Application.UseWaitCursor = false;
                    Application.DoEvents();
                }
            }
        }

        private void convolveToolStripMenuItem_Click(object sender, EventArgs e) {
            ConvolveImage(2, 0);
        }

        private void ConvolveImage(int convType, int convMethod=0) {
            int[,] kernel = {{ -2, 0, 1},
                             { -1, 1, 1},
                             {  0, 1, 2}};
            int[,] Blur5x5Filter = {{ 0, 0, 1, 0, 0, },
                                            { 0, 1, 1, 1, 0, },
                                            { 1, 1, 1, 1, 1, },
                                            { 0, 1, 1, 1, 0, },
                                            { 0, 0, 1, 0, 0, }};
            int[,] Gaussian3x3BlurFilter = {{ 1, 2, 1, },
                                            { 2, 4, 2, },
                                            { 1, 2, 1, }};
            int[,] SharpenFilter = { { -1, -1, -1, },
                                     { -1,  9, -1, },
                                     { -1, -1, -1, }};
            int[,] Sharpen9x9Filter = { { -1, -1, -1, -1, -1, },
                                        { -1,  2,  2,  2, -1, },
                                        { -1,  2,  8,  2,  1, },
                                        { -1,  2,  2,  2, -1, },
                                        { -1, -1, -1, -1, -1, }};
            int[,] newMatrix = {{ 0, 0, 0, 2, 2, 2, 0, 0, 0 },
                                { 0, 0, 5, 3, 3, 3, 5, 0, 0 },
                                { 0, 0, 5, 3, 3, 3, 5, 0, 0 },
                                { 2, 1, 3, 4, 4, 4, 3, 1, 2 },
                                { 2, 1, 3, 4, 4, 4, 3, 1, 2 },
                                { 2, 1, 3, 4, 4, 4, 3, 1, 2 },
                                { 0, 0, 5, 3, 3, 3, 5, 0, 0 },
                                { 0, 0, 5, 3, 3, 3, 5, 0, 0 },
                                { 0, 0, 0, 2, 2, 2, 0, 0, 0 }};
            int[,] sextr = {{ 6319, 40599, 75183, 40599, 6319},
                            { 40599, 260856, 483068, 260856, 40599},
                            { 75183, 483068, 894573, 483068, 75183},
                            { 40599, 260856, 483068, 260856, 40599},
                            { 6319, 40599, 75183, 40599, 6319}};

            PictureBox pb;
            if (tabControl1.SelectedIndex == 0) pb = pictureBox1;
            else if (tabControl1.SelectedIndex == 1) pb = pictureBox2;
            else if (tabControl1.SelectedIndex == 2) pb = pictureBox3;
            else return;
            int devisor = 1;
            int[,] matrix = null;
            switch (convType)
            {
                case 0:
                    matrix = kernel;
                    break;
                case 1:
                    matrix = Blur5x5Filter;
                    break;
                case 2:
                    matrix = Gaussian3x3BlurFilter;
                    break;
                case 3:
                    matrix = SharpenFilter;
                    break;
                case 4:
                    matrix = Sharpen9x9Filter;
                    break;
                case 5:
                    matrix = newMatrix;
                    break;
                case 6:
                    matrix = sextr;
                    devisor = 1000000;
                    break;
            }
            if (convMethod == 0)
            {
                Bitmap bmp = (Bitmap)pb.Image;
                GrepnovaConvolution conv = new GrepnovaConvolution();
                pb.Image = conv.Convolve(bmp, matrix, devisor);
            }
            else if (convMethod == 1)
            {
                int idx = tabControl1.SelectedIndex;
                GrepnovaConvolution gc = new GrepnovaConvolution();
                image[idx] = gc.DirectConvolve(image[idx], matrix);
                pb.Image = Fits.GetFITSImage(image[idx], gamma[idx], trans_type, -1, -1, wavelengthIndex);
                if (idx == 0 || idx == 2) SetLabels(0, image[idx]);
                if (idx == 1) SetLabels(idx, image[idx]);
            }
        }

        private Bitmap ImageDifference(Bitmap bm1, Bitmap bm2) {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();
            // Make a difference image.
            int wid = Math.Min(bm1.Width, bm2.Width);
            int hgt = Math.Min(bm1.Height, bm2.Height);
            // Get the differences.
            int[,] diffs = new int[wid, hgt];
            int max_diff = 0;
            for (int x = 0; x < wid; x++)
            {
                for (int y = 0; y < hgt; y++)
                {
                    // Calculate the pixels' difference.
                    Color color1 = bm1.GetPixel(x, y);
                    Color color2 = bm2.GetPixel(x, y);
                    diffs[x, y] = (int)(
                        Math.Abs(color1.R - color2.R) +
                        Math.Abs(color1.G - color2.G) +
                        Math.Abs(color1.B - color2.B));
                    if (diffs[x, y] > max_diff)
                        max_diff = diffs[x, y];
                }
            }
            // Create the difference image.
            Bitmap bm3 = new Bitmap(wid, hgt);
            for (int x = 0; x < wid; x++){
                for (int y = 0; y < hgt; y++){
                    //if max_diff=0 then identical so return a black image
                    int clr = max_diff == 0 ? 0 : 255 - (int)(255.0 / max_diff * diffs[x, y]);
                    bm3.SetPixel(x, y, Color.FromArgb(clr, clr, clr));
                }
            }
            this.Cursor = Cursors.Default;
            // Display the result.
            return bm3;
        }

        //Feature scaling
        private Fits.imageptr Normalize(Fits.imageptr subj, int amin, int bmax) {
            int min = subj.data_lower_decile;//.datamin;
            int max = subj.data_upper_decile;//.datapeak;
            int a = amin;
            int b = bmax;
            Console.Out.WriteLine("Min=" + min + " Max=" + max);
            Fits.imageptr resImg = new Fits.imageptr();
            resImg.object_name = subj.object_name;
            resImg.xsize = subj.xsize;
            resImg.ysize = subj.ysize;
            resImg.bitpix = subj.bitpix;
            resImg.datamin = subj.datamin;
            resImg.datapeak = subj.datapeak;
            resImg.data_lower_decile = subj.data_lower_decile;
            resImg.data_upper_decile = subj.data_upper_decile;
            resImg.mean_ld_excess = subj.mean_ld_excess;
            resImg.date_numeric = subj.date_numeric;
            resImg.exposure = subj.exposure;
            resImg.data = new int[subj.xsize * subj.ysize];
            for (int y = 0; y < subj.ysize; y++)
            {
                for (int x = 0; x < subj.xsize; x++)
                {
                    resImg.data[y * subj.xsize + x] = a + (int)((float)(b - a) * ((float)(subj.data[y * subj.xsize + x] - min) / (float)(max - min)));
                }
            }
            resImg = Fits.image_minmax_update(resImg);
            return resImg;
        }

        private void NormalizeIntensities(Fits.imageptr subj, Fits.imageptr temp) {
            int minSubj = subj.datamin;
            int minTemp = temp.datamin;
            int maxSubj = subj.datapeak;
            int maxTemp = temp.datapeak;
            int minST = (minSubj + minTemp) / 2;
            int maxST = (maxSubj + maxTemp) / 2;
            int avgMinMax = (minST + maxST) / 2;
            LogEntry("avgMinMax=" + avgMinMax);
            for (int y = 0; y < subj.ysize; y++)
            {
                for (int x = 0; x < subj.xsize; x++)
                {
                    subj.data[y * subj.xsize + x] = subj.data[y * subj.xsize + x] / avgMinMax;
                    temp.data[y * subj.xsize + x] = temp.data[y * subj.xsize + x] / avgMinMax;
                }
            }
            subj = Fits.image_minmax_update(subj);
            temp = Fits.image_minmax_update(temp);
        }

        private void differenceToolStripMenuItem_Click(object sender, EventArgs e) {
            ImageDiffWindow idw = new ImageDiffWindow(ImageDifference((Bitmap)pictureBox2.Image, (Bitmap)pictureBox3.Image));
            idw.ShowDialog(this);
        }

        private void normalizeToolStripMenuItem_Click(object sender, EventArgs e) {
            this.Cursor = Cursors.WaitCursor;
            /*NormalizeIntensities(image[1], image[2]);
            SetLabels(1, image[1]);
            SetLabels(0, image[2]);
            pictureBox2.Image = Fits.GetFITSImage(image[1], gamma[1], trans_type);//, -1, -1, wavelengthIndex);
            pictureBox3.Image = Fits.GetFITSImage(image[2], gamma[2], trans_type);// -1, -1, wavelengthIndex);
            */
            int idx = tabControl1.SelectedIndex;
            if (idx == 1 || idx == 2)
            {
                switch (idx)
                {
                    case 1:
                        image[1] = Normalize(image[1], image[0].data_lower_decile, image[0].data_upper_decile);
                        pictureBox2.Image = Fits.GetFITSImage(image[1], gamma[1], trans_type, -1, -1, wavelengthIndex);
                        SetLabels(1, image[1]);
                        break;
                    case 2:
                        image[2] = Normalize(image[2], image[1].data_lower_decile, image[1].data_upper_decile);
                        pictureBox3.Image = Fits.GetFITSImage(image[2], gamma[2], trans_type, -1, -1, wavelengthIndex);
                        SetLabels(0, image[2]);
                        break;
                }
            }
            this.Cursor = Cursors.Default;
        }

        private void erodeToolStripMenuItem_Click(object sender, EventArgs e) {
            int idx = tabControl1.SelectedIndex;
            PictureBox pic = new PictureBox();
            switch (idx)
            {
                case 0:
                    pic = pictureBox1;
                    break;
                case 1:
                    pic = pictureBox2;
                    break;
                case 2:
                    pic = pictureBox3;
                    break;
                case 3:
                    return;
            }
            BinaryImage bi = new BinaryImage(image[idx].data, image[idx].xsize, image[idx].ysize);
            int v = image[idx].data_lower_decile;
            int[,] matrix = new int[,] {{ 0, v, 0 },
                                        { v, v, v },
                                        { 0, v, 0 }};
            image[idx].data = bi.Erode(matrix);
            pic.Image = Fits.GetFITSImage(image[idx], gamma[idx], trans_type, -1, -1, wavelengthIndex);
        }

        private Fits.imageptr ImageFunction(Fits.imageptr img, string func) {
            switch (func)
            {
                case "square root":
                    for (int y = 0; y < img.ysize; y++)
                    {
                        for (int x = 0; x < img.xsize; x++)
                            img.data[x + y * img.xsize] = (int)(img.data_lower_decile + Math.Sqrt(img.data[x + y * img.xsize]));
                    }
                    break;
                case "inverse log":
                    double E = 2.7182818284590452354;
                    for (int y = 0; y < img.ysize; y++)
                    {
                        for (int x = 0; x < img.xsize; x++)
                            img.data[x + y * img.xsize] = (int)Math.Pow(E, img.data[x + y * img.xsize] / img.data_upper_decile);
                    }
                    break;
                case "atan":
                    for (int y = 0; y < img.ysize; y++)
                    {
                        for (int x = 0; x < img.xsize; x++)
                            img.data[x + y * img.xsize] = (int)(Math.Atan(img.data[x + y * img.xsize]) * img.data_lower_decile);
                    }
                    break;
                case "square":
                    for (int y = 0; y < img.ysize; y++)
                    {
                        for (int x = 0; x < img.xsize; x++)
                            img.data[x + y * img.xsize] = (int)(Math.Pow(img.data[x + y * img.xsize], 2) > ushort.MaxValue ? ushort.MaxValue : Math.Pow(img.data[x + y * img.xsize], 2));
                    }
                    break;
            }

            return img;
        }

        private void ApplyFunction(string func) {
            int idx = tabControl1.SelectedIndex;
            PictureBox pic = new PictureBox();
            switch (idx)
            {
                case 0:
                    pic = pictureBox1;
                    break;
                case 1:
                    pic = pictureBox2;
                    break;
                case 2:
                    pic = pictureBox3;
                    break;
                case 3:
                    return;
            }
            Fits.imageptr ipt = ImageFunction(image[idx], func);
            image[idx] = Fits.image_minmax_update(ipt);
            pic.Image = Fits.GetFITSImage(image[idx], gamma[idx], trans_type, -1, -1, wavelengthIndex);
            SetLabels((idx == 2 ? 0 : idx), image[idx]);
        }

        private void squareRootToolStripMenuItem_Click(object sender, EventArgs e) {
            ApplyFunction("square root");
        }

        private void atanToolStripMenuItem_Click(object sender, EventArgs e) {
            ApplyFunction("atan");
        }

        private void inverseLogToolStripMenuItem_Click(object sender, EventArgs e) {
            ApplyFunction("inverse log");
        }

        private void squareToolStripMenuItem_Click(object sender, EventArgs e) {
            ApplyFunction("square");
        }

        private void solvePlateToolStripMenuItem_Click(object sender, EventArgs e) {
            //Focal Length 1217.0 mm
            LogEntry("Plate Solver form opened...");
            //PlateSolver1.runPlateSolve(fn);
            //PlateSolver1.astrometryPlateSolve(fn);

            //CallAstrometrySolvePlate(fn);
            int idx = tabControl1.SelectedIndex;
            string fn = "";
            if(idx==0) fn = tempData[selectedTemp].path + lstTemplates.SelectedItem.ToString();
            else if(idx==1) fn = subjData[selectedSubj].path + lstSubjects.SelectedItem.ToString();
            else {
                LogEntry("Plate Solving can be performed only for Template or Subject. Aborting");
                MessageBox.Show("Plate Solving can be performed only for Template or Subject.");
                return;
            }
            AstrometryForm af = new AstrometryForm(fn, image[idx]);
            af.Show(this);
        }

        public void CallAstrometrySolvePlate(string fn)
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var files = new List<Tuple<string, string, long, DateTime>>();
            string ret = "";
            Task<string> t=null;
            try
            {
                t = Task.Run<string>(() => {
                    ret = PlateSolver1.astrometryPlateSolve(fn);
                    if (token.IsCancellationRequested) token.ThrowIfCancellationRequested();
                    LogEntry(ret);
                    return ret;
                }, token);
                Console.WriteLine("Plate Solver returned successfully.");
            } catch(TaskCanceledException tce) {
                Console.WriteLine("Cancel message: " + tce.Message);
            } catch (AggregateException e){
                Console.WriteLine("Exception messages: "+e.Message);
                Console.WriteLine("\nTask status: {0}", t.Status);
            }finally{
                tokenSource.Dispose();
            }
            LogEntry(ret);
        }

        private void plain3x3ToolStripMenuItem_Click(object sender, EventArgs e) {
            ConvolveImage(0, convolutionMethod);
        }

        private void blur5x5FilterToolStripMenuItem_Click(object sender, EventArgs e) {
            ConvolveImage(1, convolutionMethod);
        }

        private void gaussian3x3BlurFilterToolStripMenuItem_Click(object sender, EventArgs e) {
            ConvolveImage(2, convolutionMethod);
        }

        private void sharpenFilterToolStripMenuItem_Click(object sender, EventArgs e) {
            ConvolveImage(3, convolutionMethod);
        }

        private void sharpen9x9FilterToolStripMenuItem_Click(object sender, EventArgs e) {
            ConvolveImage(4, convolutionMethod);
        }

        private void newMatrixToolStripMenuItem_Click(object sender, EventArgs e) {
            ConvolveImage(5, convolutionMethod);
        }

        private void sExtrToolStripMenuItem_Click(object sender, EventArgs e) {
            ConvolveImage(6, convolutionMethod);
        }

        int convolutionMethod = 0;

        private void aForgeToolStripMenuItem_Click(object sender, EventArgs e) {
            convolutionMethod = 0;//default 
        }

        private void directToolStripMenuItem_Click(object sender, EventArgs e) {
            convolutionMethod = 1;
        }

        public void CallPythonWithParameters(string parameter, List<List<float>> total_list) {
            /*var engine = Python.CreateEngine(); // Extract Python language engine from their grasp
            var scope = engine.CreateScope(); // Introduce Python namespace (scope)
            var d = new Dictionary<string, object>
            {
                { "total_list", total_list},
                { "parameter", parameter}
            }; // Add some sample parameters. Notice that there is no need in specifically setting the object type, interpreter will do that part for us in the script properly with high probability

            scope.SetVariable("params", d); // This will be the name of the dictionary in python script, initialized with previously created .NET Dictionary
            ScriptSource source = engine.CreateScriptSourceFromFile("test_sep.py"); // Load the script
            dynamic result = source.Execute(scope);
            List<List<float>> res = scope.GetVariable<List<List<float>>>("total_list"); // To get the finally set variable 'parameter' from the python script
            //string sres = scope.GetVariable<string>("parameter");
            foreach(List<float> r in res){
                string ss = "";
                foreach(float f in r){
                    ss += " " + f;
                }
                LogEntry(ss);
            }
            //return "result was " + res + " and " + sres;
            */
        }

        private struct starsCircles
        {
            public int id;
            public float objX;
            public float objY;
            public float objA;
            public float objB;
            public float objTheta;
        }

        private void callPythonToolStripMenuItem_Click(object sender, EventArgs e) {
            /*List<List<float>> total_list = new List<List<float>>();
            string st = txtTemplatesDir.Text+lstTemplates.Items[lstTemplates.SelectedIndex];
            LogEntry("Calling Python with " + st + " expecting to get recognized stars.");
            CallPythonWithParameters(st, total_list);
            LogEntry("Python returned: ");*/
            int idx = tabControl1.SelectedIndex;
            string st = "";
            switch (idx)
            {
                case 0:
                    st = txtTemplatesDir.Text + lstTemplates.Items[lstTemplates.SelectedIndex];
                    break;
                case 1:
                    st = txtSubjectsDir.Text + lstSubjects.Items[lstSubjects.SelectedIndex];
                    break;
            }
            string progToRun = "find_stars.py";//C:\\Users\\cooknas\\AndroidPython\\grepnova\\
            char[] splitter = { '\r' };
            
            Process proc = new Process();
            proc.StartInfo.FileName = "python.exe";
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            LogEntry("Running Python 'FindStars'");
            // call 'apply_sep.py' to concatenate passed parameters
            proc.StartInfo.Arguments = string.Concat(progToRun, " ", st);
            //LogEntry("Arguments: " + progToRun + " " + st);
            proc.Start();

            StreamReader sReader = proc.StandardOutput;
            StreamReader eReader = proc.StandardError;
            string[] output = sReader.ReadToEnd().Split(splitter);
            string[] error = eReader.ReadToEnd().Split(splitter);
            List<starsCircles> starCirc = new List<starsCircles>();
            float yOrig = (float)image[idx].ysize;
            float xOrig = (float)image[idx].xsize;
            foreach (string s in output){
                //LogEntry(">>>>" + s);
                string splain = s.Replace("\n", "");
                if (splain.StartsWith("data[[")){
                    string ss = splain.Replace("data[[", "").Replace("]]", "");
                    string[] sss = ss.Split(new string[] { "], [" }, StringSplitOptions.None);
                    foreach(string spart in sss){
                        string[] newst = spart.Split(',');
                        starsCircles sc = new starsCircles();
                        sc.id = int.Parse(newst[0].Trim());
                        sc.objX = float.Parse(newst[1].Trim().Replace('.',','));
                        sc.objY = float.Parse(newst[2].Trim().Replace('.', ','));
                        sc.objA = float.Parse(newst[3].Trim().Replace('.', ','));
                        sc.objB = float.Parse(newst[4].Trim().Replace('.', ','));
                        sc.objTheta = float.Parse(newst[5].Trim().Replace('.', ','));
                        starCirc.Add(sc);
                        //Console.Out.WriteLine("{0}: x-y={1}-{2} a-b={3}-{4} rot={5}", sc.id, sc.objX, sc.objY, sc.objA, sc.objB, sc.objTheta);
                    }
                }else{
                    LogEntry(s);
                }
            }
            LogEntry("Found " + starCirc.Count+ "stars.");
            foreach (string s in error)
                if(!s.Equals(""))LogEntry("Error:"+s);
            proc.WaitForExit();
            proc.Close();
            sReader.Close();
            sReader.Dispose();
            eReader.Close();
            eReader.Dispose();
            drawElipses(idx, starCirc, false, true);
        }


        private void alignWithPythonToolStripMenuItem_Click(object sender, EventArgs e) {
            string stTemp = txtTemplatesDir.Text + lstTemplates.Items[lstTemplates.SelectedIndex];
            string stSubj = txtSubjectsDir.Text + lstSubjects.Items[lstSubjects.SelectedIndex];
            string progToRun = "astroalign.py";//C:\\Users\\cooknas\\AndroidPython\\grepnova\\
            char[] splitter = { '\r' };
            Process proc = new Process();
            proc.StartInfo.FileName = "python.exe";
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            LogEntry("Running Python 'astroalign.py'...");
            // call 'apply_sep.py' to concatenate passed parameters
            proc.StartInfo.Arguments = string.Concat(progToRun, " ", stTemp, " ", stSubj);
            //LogEntry("Arguments: " + progToRun + " " + st);
            proc.Start();

            StreamReader sReader = proc.StandardOutput;
            StreamReader eReader = proc.StandardError;
            string[] output = sReader.ReadToEnd().Split(splitter);
            string[] error = eReader.ReadToEnd().Split(splitter);
            foreach (string s in output){
                //if (!s.Equals("")) LogEntry("Returned: " + s);
                //if (!s.Equals("")) Console.Out.WriteLine("Returned: " + s);
                if (!s.Equals("") && !s.Equals("\n")){
                    string[] param = s.Split(',');
                    float dangle = float.Parse(param[0].Replace("rotation=", "").Trim().Replace('.',','));
                    float scale = float.Parse(param[2].Replace("scale=", "").Trim().Replace('.', ','));
                    string[] trans = param[1].Replace("translation=", "").Trim().Replace("[", "").Replace("]", "").Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    float dx = float.Parse(trans[0].Trim().Replace('.', ','));
                    float dy = float.Parse(trans[1].Trim().Replace('.', ','));
                    //image[2] = Fits.CloneImagePtr(image[0]);
                    LogEntry(String.Format("astroalign.py: x={0:F3} y={1:F3} theta={2:F6} scale={3:F6}",dx,dy,dangle,scale));
                    pictureBox3.Image = Fits.GetFITSImage(Fits.CreateBlackImage(image[0].xsize, image[0].ysize), gamma[2], trans_type);
                    transformImage(dx, dy, dangle, 1/scale);
                }
            }
            foreach (string s in error){
                if (!s.Equals("")) LogEntry("Error:" + s);
            }
            proc.WaitForExit();
            proc.Close();
            sReader.Close();
            sReader.Dispose();
            eReader.Close();
            eReader.Dispose();
        }


        private void scaleTemplateToolStripMenuItem_Click(object sender, EventArgs e) {
            int wid = image[1].xsize;
            int hei = image[1].ysize;
            Bitmap result = new Bitmap(wid, hei);
            using (Graphics g = Graphics.FromImage(result)){
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(pictureBox1.Image, 0, 0, wid, hei);
            }
            pictureBox1.Image = result;
        }

        public class ThreadWithParams
        {
            // State information used in the task.
            public string str1;
            public string str2;
            public string str3;
            public string str4;

            // The constructor obtains the state information.
            public ThreadWithParams(string file, string address, string user, string pass) {
                str1 = file;
                str2 = address;
                str3 = user;
                str4 = pass;
            }

            // The thread procedure performs the task, such as downloading a document.
            public void DownloadFileAsync() {
                HttpHelper.ftpDownload(str1, str2, str3, str4);
            }
        }

        // version 1.2.0 @ 20190317
        // Draw stars' circles according PSF of each star
        private void starCirclesAccordingIntensitiesToolStripMenuItem_Click(object sender, EventArgs e) {
            starCirclesAccordingIntensitiesToolStripMenuItem.Checked = !starCirclesAccordingIntensitiesToolStripMenuItem.Checked;
        }

        // version 1.2.0 @ 20190317
        // Show triplets on Template and Subject
        private void showTripletsToolStripMenuItem_Click(object sender, EventArgs e) {
            int match_found = 0;
            Circles[0] = new List<Fits.PointValue>();
            Circles[1] = new List<Fits.PointValue>();
            Lines[0] = new List<Fits.TriplePoint>();
            Lines[1] = new List<Fits.TriplePoint>();
            Fits.PointValue ppT = new Fits.PointValue();
            Fits.PointValue ppS = new Fits.PointValue();
            unsafe
            {
                int status = 0;
                int max = 10;
                
                Align.starlist slT = Align.source_extract(image[0], 10);
                Align.starlist slS = Align.source_extract(image[1], 10);
                Align.tripletlist tlT = Align.source_triplets(image[0], slT, max, &status);
                Align.tripletlist tlS = Align.source_triplets(image[1], slS, max, &status);
                Align.tripletmatches trip_match = Align.match_triplets(tlT, tlS, slT, slS, &status);
                Console.Out.WriteLine("match_triplets return " + trip_match.list_size + " matches");
                for (int ti = 0; ti < Align.PATTERN_THETA_INTERVALS; ti++){
                    for (int tj = 0; tj < tlT.triplets[ti]; tj++){
                        for (int si = 0; si < Align.PATTERN_THETA_INTERVALS; si++){
                            for (int sj = 0; sj < tlT.triplets[si]; sj++){
                                if(Math.Pow(tlT.theta[ti][tj]-tlS.theta[si][sj], 2)/(Math.Pow(tlT.Stheta[ti][tj], 2) + Math.Pow(tlS.Stheta[si][sj], 2)) +
                                    Math.Pow(tlT.ratio[ti][tj] - tlS.ratio[si][sj], 2) / (Math.Pow(tlT.Sratio[ti][tj],2) + Math.Pow(tlS.Sratio[si][sj], 2))<1)
                                {
                                    match_found++;
                                    int[] tx = new int[3];
                                    int[] ty = new int[3];
                                    tx[0] = slT.x_pos[tlT.src_id_A[ti][tj]];
                                    ty[0] = slT.y_pos[tlT.src_id_A[ti][tj]];
                                    tx[1] = slT.x_pos[tlT.src_id_B[ti][tj]];
                                    ty[1] = slT.y_pos[tlT.src_id_B[ti][tj]];
                                    tx[2] = slT.x_pos[tlT.src_id_C[ti][tj]];
                                    ty[2] = slT.y_pos[tlT.src_id_C[ti][tj]];
                                    int[] sx = new int[3];
                                    int[] sy = new int[3];
                                    sx[0] = slS.x_pos[tlS.src_id_A[si][sj]];
                                    sy[0] = slS.y_pos[tlS.src_id_A[si][sj]];
                                    sx[1] = slS.x_pos[tlS.src_id_B[si][sj]];
                                    sy[1] = slS.y_pos[tlS.src_id_B[si][sj]];
                                    sx[2] = slS.x_pos[tlS.src_id_C[si][sj]];
                                    sy[2] = slS.y_pos[tlS.src_id_C[si][sj]];
                                    ppT.p = new Point(tx[0], ty[0]);
                                    ppT.v = slT.intensity[tlT.src_id_A[ti][tj]];
                                    Circles[0].Add(ppT);
                                    ppT.p = new Point(tx[1], ty[1]);
                                    ppT.v = slT.intensity[tlT.src_id_B[ti][tj]];
                                    Circles[0].Add(ppT);
                                    ppT.p = new Point(tx[2], ty[2]);
                                    ppT.v = slT.intensity[tlT.src_id_C[ti][tj]];
                                    Circles[0].Add(ppT);
                                    Lines[0].Add(new Fits.TriplePoint(tx[0], ty[0], tx[1], ty[1], tx[2], ty[2]));

                                    ppS.p = new Point(sx[0], sy[0]);
                                    ppS.v = slS.intensity[tlS.src_id_A[si][sj]];
                                    Circles[1].Add(ppS);
                                    ppS.p = new Point(sx[1], sy[1]);
                                    ppS.v = slS.intensity[tlS.src_id_B[si][sj]];
                                    Circles[1].Add(ppS);
                                    ppS.p = new Point(sx[2], sy[2]);
                                    ppS.v = slS.intensity[tlS.src_id_C[si][sj]];
                                    Circles[1].Add(ppS);
                                    Lines[1].Add(new Fits.TriplePoint(sx[0], sy[0], sx[1], sy[1], sx[2], sy[2]));

                                    Console.Out.WriteLine("Found match ({0}/{1})",match_found, trip_match.list_size);
                                    Console.Out.WriteLine("Temp: A({0}-{1}) B({2}-{3}) C({4}-{5})",tx[0],ty[0],tx[1],ty[1],tx[2],ty[2]);
                                    Console.Out.WriteLine("Subj: A({0}-{1}) B({2}-{3}) C({4}-{5})", sx[0], sy[0], sx[1], sy[1], sx[2], sy[2]);
                                    Console.Out.WriteLine("Theta: Temp({0}) Subj({1}) Ratio: Temp({2}) Subj({3})", tlT.theta[ti][tj], tlS.theta[si][sj], tlT.ratio[ti][tj], tlS.ratio[si][sj]);
                                }
                            }
                        }
                    }
                }
            }
            string msg = "No match found...";
            if (match_found > 0) msg = "Found " + match_found + " matches!!!";
            drawCircles(0, Circles[0]);
            drawCircles(1, Circles[1]);
            drawLines(0, Lines[0]);
            drawLines(1, Lines[1]);
            MessageBox.Show("Triplets finished\r\n"+msg);
        }

        // version 1.3.0   @ 20190407
        // Shortcuts selection by the user through menu About-->HotKeys
        // version 1.2.0 @ 20190317
        // shortcuts added for (T)emplate, (S)ubject, (A)ligned, (B)link, (>)next, (<)previous, (Z)oom 
        // Brightness Up/Down
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            //If there are textboxes (which need the focus) exclude them from key-catch
            Control co = GetFocusedControl(frm);
            if ((co.GetType() == typeof(TextBox) && !co.Equals(txtLog)) || co.GetType() == typeof(NumericUpDown))
                return base.ProcessCmdKey(ref msg, keyData);
            if (keyData == Keys.None) return false;
            //Use of keys' combination: (Keys.Control | Keys.F) for Ctrl+F
            //T,S,A + Shift = value up  - T,S,A + Ctrl = value down 
            if (keyData == shcutTempUp){//(Keys.Shift | Keys.T):
                udTemp.Value += udTemp.Value / 10;
            }else if (keyData == shcutTempDown){// (Keys.Control | Keys.T):
                udTemp.Value -= udTemp.Value / 10;
            }else if (keyData == shcutSubjUp){// (Keys.Shift | Keys.S):
                udSubj.Value += udSubj.Value / 10;
            }else if (keyData == shcutSubjDown){// (Keys.Control | Keys.S):
                udSubj.Value -= udSubj.Value / 10;
            }else if (keyData == shcutAlignUp){// (Keys.Shift | Keys.A):
                udAlig.Value += udAlig.Value / 10;
            }else if (keyData == shcutAlignDown){// (Keys.Control | Keys.A):
                udAlig.Value -= udAlig.Value / 10;
            }else if (keyData == shcutTemplate){ // Keys.T:
                image_no = 0;
                tabControl1.SelectedIndex = image_no;
            }else if (keyData == shcutSubject){ // Keys.S
                image_no = 1;
                tabControl1.SelectedIndex = image_no;
            }else if (keyData == shcutAligned){ // Keys.A:
                image_no = 2;
                tabControl1.SelectedIndex = image_no;
            }else if (keyData == shcutBlink){ //Keys.B:
                image_no = 3;
                tabControl1.SelectedIndex = image_no;
            }else if (keyData == shcutNext){ //Keys.Right:
                cmdNext_Click((object)cmdNext, null);
            }else if (keyData == shcutPrev){ //Keys.Left:
                cmdPrev_Click((object)cmdPrev, null);
            }else if (keyData == shcutZoom){ //Keys.Z:
                button2_Click((object)button2, null);
            }else if (keyData == shcutRefreshBatch){
                refreshBatchToolStripMenuItem_Click((object)refreshBatchToolStripMenuItem, null);
            }else if (keyData == shcutHeaders){
                showImgesHeadersToolStripMenuItem_Click((object)showImgesHeadersToolStripMenuItem, null);
            }else if (keyData == shcutRemoveAnnot){
                removeAnnotationsbrightestsCentroidsToolStripMenuItem_Click((object)removeAnnotationsbrightestsCentroidsToolStripMenuItem, null);
            }else if (keyData == shcutStretch){
                stretchImageToolStripMenuItem_Click((object)stretchImageToolStripMenuItem, null);
            }else if (keyData == shcutCurve){
                imageCurveToolStripMenuItem_Click((object)imageCurveToolStripMenuItem, null);
            }else if (keyData == shcutSaveAsTemp){
                saveAsTemplateToolStripMenuItem_Click((object)saveAsTemplateToolStripMenuItem, null);
            }else if (keyData == shcutDownload){
                downloadFromDSSToolStripMenuItem_Click((object)downloadFromDSSToolStripMenuItem, null);
            }else if (keyData == shcutBlank){
                blankSubjectToolStripMenuItem_Click((object)blankSubjectToolStripMenuItem, null);
            }
            return true; // inform that we consumed the event
        }

            private Control GetFocusedControl(Control control) {
            var container = control as IContainerControl;
            while (container != null){
                control = container.ActiveControl;
                container = control as IContainerControl;
            }
            return control;
        }

        //version 1.2.0   @ 20190317
        //downloads fits image given the OBJCTRA and OBJCTDEC header fields
        private void downloadFromDSSToolStripMenuItem_Click(object sender, EventArgs e) {
            string filepath1 = tempData[selectedTemp].path + lstTemplates.SelectedItem.ToString();
            string fitsWidth = "12.8787";//gives 765 pixels width
            string fitsHeight = "8.5672";//gives 510 pixels height
            string ra = Fits.ReadCardFieldToString(filepath1, "OBJCTRA");
            string dec = Fits.ReadCardFieldToString(filepath1, "OBJCTDEC");
            if(ra == "" || dec == ""){
                string filepath2 = subjData[selectedSubj].path + lstSubjects.SelectedItem.ToString();
                ra = Fits.ReadCardFieldToString(filepath2, "OBJCTRA");
                dec = Fits.ReadCardFieldToString(filepath2, "OBJCTDEC");
            }
            if(ra == "" || dec == "") {LogEntry("Cannot find Header Fields OBJCTRA and/or OBJCTDEC to search for.\r\nAborting..."); MessageBox.Show("Cannot find Header Fields OBJCTRA and/or OBJCTDEC to search for.\r\nAborting..."); return; }
            string[] ff = filepath1.Split('\\');
            string fn = ff[ff.Length - 1];
            DSS.PostDSSrequest(ra, dec, fn, fitsWidth, fitsHeight);
        }

        //version 1.2.0   @ 20190317
        //Testing: Find SNe using find_SNe function of old grepnova
        private void findSNeToolStripMenuItem_Click(object sender, EventArgs e) {
            //First of all blank the subject if it is not already done by menu choice
            if (!bBlankSubject){
                image[1] = Fits.BlankSubject(image[1], image[2]);
                pictureBox2.Image = Fits.GetFITSImage(image[1], gamma[1], trans_type, -1, -1, wavelengthIndex);
            }
            //extract n brighter stars form aligned template
            Align.starlist sl = Align.source_extract(image[2], 100);
            //calculate psf of subject
            double psf = Align.seeing_estimate(image[1], sl, 100);
            //now look for SNe
            Align.coordinate sne = Align.find_SNe(this, image[1], 50, sl, psf);
            if (sne.x == -1 || sne.value< -1/*image[1].data_lower_decile*/) MessageBox.Show("No SNe candidate found.");
            else{
                FoundSN = new Fits.PointValue(sne.x, sne.y, sne.value);
                drawSN(FoundSN);
                MessageBox.Show(String.Format("SNe candidate found at {0},{1} with indensity {2}",sne.x,sne.y, sne.value),"SNe candidate found");
            }
        }

        //version 1.2.0   @ 20190317
        //New Alignment method using patch_triplets and Nelder-Mead minimization algorithm
        private void calculateAlignmentToolStripMenuItem_Click(object sender, EventArgs e) {
            int status = 0;
            int max = 10;
            unsafe
            {
                Align.starlist slT = Align.source_extract(image[0], 20);
                Align.starlist slS = Align.source_extract(image[1], 20);
                Align.tripletlist tlT = Align.source_triplets(image[0], slT, max, &status);
                Align.tripletlist tlS = Align.source_triplets(image[1], slS, max, &status);
                Align.tripletmatches trip_match = Align.match_triplets(tlT, tlS, slT, slS, &status);
                Align.proposed_alignment prop_align = new Align.proposed_alignment();
                prop_align.x = 0.5;
                prop_align.y = 0.5;
                prop_align.theta = 0.002;
                prop_align.scale = 1;
                Align.proposed_alignment align = Align.alignment_fit(trip_match, tlT, tlS, slT, slS, prop_align);

                currentAlign.x = align.x;
                currentAlign.y = align.y;
                currentAlign.theta = align.theta;
                currentAlign.scale = align.scale;
                LogEntry(String.Format("Nelder-Mead internal: x={0:F3} y={1:F3} theta={2:F6} scale={3:F6}", align.x, align.y, align.theta , align.scale));//* 180/Math.PI
                transformImage((float)(align.x), (float)(align.y), (float)(align.theta), 1f/(float)align.scale);//*180/Math.PI
                //MessageBox.Show(String.Format("Alignment:\r\ndx={0} dy={1} theta={2} scale={3}", align.x , align.y , align.theta, align.scale));//* 180/Math.PI
            }
        }

        //version 1.2.0   @ 20190317
        //New Alignment method using patch_triplets and Accord Math library minimization algorithm
        private void grepnova2AlignToolStripMenuItem_Click(object sender, EventArgs e) {
            int status = 0;
            int max = 20;
            unsafe
            {
                Align.starlist slT = Align.source_extract(image[0], 20);
                Align.starlist slS = Align.source_extract(image[1], 20);
                Align.tripletlist tlT = Align.source_triplets(image[0], slT, max, &status);
                Align.tripletlist tlS = Align.source_triplets(image[1], slS, max, &status);
                Align.tripletmatches trip_match = Align.match_triplets(tlT, tlS, slT, slS, &status);
                Align.proposed_alignment prop_align = new Align.proposed_alignment();
                prop_align.x = 0.0;
                prop_align.y = 0.0;
                prop_align.theta = 0.01;
                prop_align.scale = 1;
                
                Align.TW = image[0].xsize;
                Align.TH = image[0].ysize;
                Align.TS = Math.Max(Align.TW, Align.TH);
                Align.SW = image[1].xsize;
                Align.SH = image[1].ysize;
                Align.SS = Math.Max(Align.SW, Align.SH);
                
                Align.proposed_alignment align = prop_align;
                string method = "";
                switch (accordOptimizeMethod) {
                    case 0:
                        //Nelder-Mead optimization algorithm
                        method = "Nelder-Mead";
                        align = Align.accord_fit_neldermead(trip_match, tlT, tlS, slT, slS, prop_align);
                        break;
                    case 1:
                        //Constrained Optimization BY Linear Approximation (COBYLA)
                        method = "Cobyla";
                        align = Align.accord_fit_cobyla(trip_match, tlT, tlS, slT, slS, prop_align);
                        break;
                }
                currentAlign.x = align.x;
                currentAlign.y = align.y;
                currentAlign.theta = align.theta;
                currentAlign.scale = align.scale;
                image[2] = Fits.TransformImagePtr(image[0], align.theta, align.x, align.y);//angle in radians
                transformImage((float)(align.x ), (float)(align.y ), (float)(align.theta * 180 / Math.PI), 1f/(float)align.scale);
                LogEntry(String.Format("Grepnova2 Alignment({4}): x={0:F3} y={1:F3} theta={2:F6} scale={3:F6}", align.x, align.y, align.theta * 180 / Math.PI, align.scale, method));
                lblImgAlignedSize.Text = String.Format("dx={0:F0} dy={1:F0} r={2:F2}° s={3:F2}", align.x, align.y, align.theta * 180 / Math.PI, align.scale);
                //MessageBox.Show(String.Format("Alignment:\r\ndx={0} dy={1} theta={2} scale={3}", align.x , align.y , align.theta * 180 / Math.PI, align.scale));
            }
            /*AmoebaProgram.Main(null);*/
        }

        //version 1.2.0   @ 20190317
        //All alignment methods one after another (for comparison)
        private void logAllAlignToolStripMenuItem_Click(object sender, EventArgs e) {
            using (System.IO.StreamWriter alignLog = new System.IO.StreamWriter("alignLog.txt", true))
            {
                alignLog.WriteLine(String.Format("Grepnova Alignment  (x y θ° s):\t{0:F3}\t{1:F3}\t{2:F6}\t{3:F6}", currentAlign.x, currentAlign.y, currentAlign.theta, currentAlign.scale));
                calculateAlignmentToolStripMenuItem_Click(sender, e);
                alignLog.WriteLine(String.Format("Nelder-Mead Align   (x y θ° s):\t{0:F3}\t{1:F3}\t{2:F6}\t{3:F6}", currentAlign.x, currentAlign.y, currentAlign.theta * 180 / Math.PI, currentAlign.scale));
                grepnova2AlignToolStripMenuItem_Click(sender, e);
                alignLog.WriteLine(String.Format("Grepnova2 Alignment (x y θ° s):\t{0:F3}\t{1:F3}\t{2:F6}\t{3:F6}", currentAlign.x, currentAlign.y, currentAlign.theta * 180 / Math.PI, currentAlign.scale));
            }
        }

        //version 1.2.0   @ 20190317
        //Static Function used by new form ImageCurveForm for curving image
        public static void StrechImageCurve(LevelChangedEventArgs e, bool bRestore=false)
        {
            PictureBox pic = new PictureBox();
            int idx = frm.tabControl1.SelectedIndex;
            switch (idx){
                case 0:
                    pic = frm.pictureBox1;
                    break;
                case 1:
                    pic = frm.pictureBox2;
                    break;
                case 3:
                    pic = frm.pictureBox3;
                    break;
                case 4:
                    return;
            }
            if (bRestore){
                pic.Image = frm.copyImage[idx];
            }else{
                pic.Image = ImageCurve.ChangeChannelLevel(frm.copyImage[idx], new Rectangle(0,0, frm.copyImage[idx].Width-1, frm.copyImage[idx].Height-1), Channel.All, e.LevelValue);
            }
            
        }

        //version 1.2.0   @ 20190317
        //Menu for loading the ImageCurveForm to curve active image bitmap
        private void imageCurveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!bImageCurveIsOn){
                imageCurveForm = new ImageCurveForm();
                imageCurveForm.Show(this);
            }
        }

        private void hotKeysToolStripMenuItem_Click(object sender, EventArgs e) {
            HotKeys hk = new HotKeys();
            hk.ShowDialog();
        }

        private void grepnovaToolStripMenuItem_Click(object sender, EventArgs e) {
            grepnovaToolStripMenuItem.Checked = true;
            grepnova2AccordOptimizationToolStripMenuItem.Checked = false;
            alignMethod = 0;
            MessageBox.Show("The old good grepnova alignment method selected as it was developed by Dominic Ford (needs 'grepnova-align.bin.exe' by D. Ford, modified by Cooknas).");
        }

        private void grepnova2AccordOptimizationToolStripMenuItem_Click(object sender, EventArgs e) {
            grepnova2AccordOptimizationToolStripMenuItem.Checked = true;
            grepnovaToolStripMenuItem.Checked = false;
            alignMethod = 1;
            MessageBox.Show("The new alignment method selected that uses the D. Ford algorithm and the Accord's Nelder-Mead optimization (no need for grepnova-align.bin.exe anymore).");
        }


        private void saveAsTemplateToolStripMenuItem_Click(object sender, EventArgs e) {
            if (tabControl1.SelectedIndex != 1) { MessageBox.Show("Only a Subject can be saved as new Template!!!"); return; }
            if (MessageBox.Show("Do you want to replace '" + lstSubjects.Items[lstSubjects.SelectedIndex] + 
                "' in Templates folder by the current Subject file?", 
                "Replace Template", 
                MessageBoxButtons.YesNo) == DialogResult.No) return;
            Fits.save_fits(image[tabControl1.SelectedIndex], txtTemplatesDir.Text + "\\" + lstSubjects.Items[lstSubjects.SelectedIndex]);
        }

        private void checkSimilarityToolStripMenuItem_Click(object sender, EventArgs e) {
            int match_found = 0;
            Circles[0] = new List<Fits.PointValue>();
            Circles[1] = new List<Fits.PointValue>();
            Lines[0] = new List<Fits.TriplePoint>();
            Lines[1] = new List<Fits.TriplePoint>();

            Align.starlist slT = Align.source_extract(image[0], 20);
            Align.starlist slS = Align.source_extract(image[1], 20);
            List<Similarity.similar_triangle> simTri = Similarity.TripletSimilarity(slT, slS);
            match_found = simTri.Count;
            
            if (simTri.Count > 0){
                /*
                for (int i = 0; i < simTri.Count; i++){
                    string s = String.Format("{0}. {13:F4} T=({1},{2})({3},{4})({5},{6}) S=({7},{8})({9},{10})({11},{12})",
                        i, simTri[i].triT.po[0].x, simTri[i].triT.po[0].y, simTri[i].triT.po[1].x, simTri[i].triT.po[1].y,
                        simTri[i].triT.po[2].x, simTri[i].triT.po[2].y,
                        simTri[i].triS.po[0].x, simTri[i].triS.po[0].y, simTri[i].triS.po[1].x, simTri[i].triS.po[1].y,
                        simTri[i].triS.po[2].x, simTri[i].triS.po[2].y, simTri[i].ratio);
                    LogEntry(s);
                    Console.Out.WriteLine(s);
                    Lines[0].Add(new Fits.TriplePoint((float)simTri[i].triT.po[0].x, (float)simTri[i].triT.po[0].y, 
                        (float)simTri[i].triT.po[1].x, (float)simTri[i].triT.po[1].y, 
                        (float)simTri[i].triT.po[2].x, (float)simTri[i].triT.po[2].y));
                    Lines[1].Add(new Fits.TriplePoint((float)simTri[i].triS.po[0].x, (float)simTri[i].triS.po[0].y,
                        (float)simTri[i].triS.po[1].x, (float)simTri[i].triS.po[1].y,
                        (float)simTri[i].triS.po[2].x, (float)simTri[i].triS.po[2].y));
                    for(int j = 0; j < 3; j++){
                        Circles[0].Add(new Fits.PointValue((int)simTri[i].triT.po[j].x, (int)simTri[i].triT.po[j].y));
                        Circles[1].Add(new Fits.PointValue((int)simTri[i].triS.po[j].x, (int)simTri[i].triS.po[j].y));
                    }
                    drawCircles(0, Circles[0]);
                    drawCircles(1, Circles[1]);
                    drawLines(0, Lines[0]);
                    drawLines(1, Lines[1]);
                    */
                    Align.proposed_alignment prop_align = new Align.proposed_alignment();
                    prop_align.x = 0.0;
                    prop_align.y = 0.0;
                    prop_align.theta = 0.01;
                    prop_align.scale = 1.0;
                    Align.proposed_alignment align = Similarity.accord_fit(simTri, prop_align);
                    currentAlign.x = align.x;
                    currentAlign.y = align.y;
                    currentAlign.theta = align.theta;
                    currentAlign.scale = align.scale;
                    transformImage((float)(align.x), (float)(align.y), (float)(align.theta * 180 / Math.PI), 1f / (float)align.scale);
                    LogEntry(String.Format("Similarity Alignment: x={0:F3} y={1:F3} theta={2:F6} scale={3:F6}", align.x, align.y, align.theta * 180 / Math.PI, align.scale));
                    //lblImgAlignedSize.Text = String.Format("dx={0:F0} dy={1:F0} r={2:F2}° s={3:F2}", align.x, align.y, align.theta * 180 / Math.PI, align.scale);
                //}
            }else{
                LogEntry("No similar triangles are found between Tempalte and Subject.");
                Console.Out.WriteLine("No similar triangles are found between Tempalte and Subject.");
            }
            
        }

        private void nelderMeadToolStripMenuItem_Click(object sender, EventArgs e) {
            accordOptimizeMethod = 0;
            nelderMeadToolStripMenuItem.Checked = true;
            cobylaToolStripMenuItem.Checked = false;
        }

        private void cobylaToolStripMenuItem_Click(object sender, EventArgs e) {
            accordOptimizeMethod = 1;
            cobylaToolStripMenuItem.Checked = true;
            nelderMeadToolStripMenuItem.Checked = false;
        }

        private void matrixMultiplicationToolStripMenuItem_Click(object sender, EventArgs e) {
            //CooknasMatrix.TestMatrixMultiplication();
            int seed = 10;
            Random rnd = new Random(seed);
            for(int i = 0; i < 10; i++){
                LogEntry(CooknasMatrix.TestCooknasMatrix(i, rnd.Next()));
            }
        }

        private void visieRToolStripMenuItem_Click(object sender, EventArgs e) {
            string filepath1 = tempData[selectedTemp].path + lstTemplates.SelectedItem.ToString();
            float fitsWidth = (float)(9 * 206.3 * 765 / (1217 * 60));// 8.3f;// 12.8787f;//gives 765 pixels width
            float fitsHeight = (float)(9 * 206.3 * 510 / (1217 * 60));//5.6f;// 8.5672f;//gives 510 pixels height
            string ra = Fits.ReadCardFieldToString(filepath1, "OBJCTRA");
            string dec = Fits.ReadCardFieldToString(filepath1, "OBJCTDEC");
            if (ra == "" || dec == "")
            {
                string filepath2 = subjData[selectedSubj].path + lstSubjects.SelectedItem.ToString();
                ra = Fits.ReadCardFieldToString(filepath2, "OBJCTRA");
                dec = Fits.ReadCardFieldToString(filepath2, "OBJCTDEC");
            }
            if (ra == "" || dec == "") { LogEntry("Cannot find Header Fields OBJCTRA and/or OBJCTDEC to search for.\r\nAborting..."); MessageBox.Show("Cannot find Header Fields OBJCTRA and/or OBJCTDEC to search for.\r\nAborting..."); return; }
            LogEntry(String.Format("Contacting VisieR with RA={0} DEC={1} Width={2} Height={3} arcmin", ra, dec, fitsWidth, fitsHeight));
            string res = HttpHelper.Unso(ra, dec, fitsWidth, fitsHeight, 50);
            string[] lines = res.Split(new string[] {"\n"}, StringSplitOptions.None);
            List<VisieRCatalog.Catalog> visier = new List<VisieRCatalog.Catalog>();
            //int i = 0;
            foreach (string s in lines){
                if (s.StartsWith("#")){
                    //ignore comment lines
                }else if(s.Length>1 && IsDigit(s[0])){
                    VisieRCatalog.Catalog vi = new VisieRCatalog.Catalog(s);
                    if(!vi.USNO.Equals("Error"))
                        visier.Add(vi);
                }
            }
            LogEntry(String.Format("Items extracted from VisieR Catalog = {0}",visier.Count));
            VisieRList vislist = new VisieRList(visier);
            vislist.lblFor.Text = String.Format("for RA={0} DEC={1} FOV={2}/{3} arcmin", ra, dec, fitsWidth, fitsHeight);
            vislist.lblItemsFound.Text = String.Format("{0} items found", visier.Count);
            vislist.lblItemsFound.Left = vislist.lblFor.Left + vislist.lblFor.Width + 4;
            vislist.Show();
        }

        private bool IsDigit(char c) {
            if (c.Equals('1') || c.Equals('2') || c.Equals('3') || c.Equals('4') 
                || c.Equals('5')|| c.Equals('6') || c.Equals('7') || c.Equals('8') 
                || c.Equals('9') || c.Equals('0')) return true;
            else return false;
        }

        private void momentsToolStripMenuItem_Click(object sender, EventArgs e) {
            ImageMoments im = new ImageMoments(image[0], image[1]);
            
            double I1 = im.calcImageDistance(im.A, im.B, ImageMoments.matchType.CONTOURS_MATCH_I1);
            double I2 = im.calcImageDistance(im.A, im.B, ImageMoments.matchType.CONTOURS_MATCH_I2);
            double I3 = im.calcImageDistance(im.A, im.B, ImageMoments.matchType.CONTOURS_MATCH_I3);
            double Ieucl = im.calcImageDistance(im.A, im.B, ImageMoments.matchType.CONTOURS_MATCH_EUCLIDEAN);
            LogEntry(String.Format("Distance Temp-Subj: I1={0:F3} I2={1:F3} I3={2:F3} Euclidean={3:F3}", I1, I2, I3, Ieucl));
        }
    }


    public static class MyExtensions
    {
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> items, int maxItems) {
            return items.Select((item, inx) => new { item, inx })
                        .GroupBy(x => x.inx / maxItems)
                        .Select(g => g.Select(x => x.item));
        }
    }
}

//TODO
/***********************************************************************************************************
0. TODO: Alignment με scaling εικόνας
1. TODO: Images from DSS (Digital Sky Survey) http://archive.stsci.edu/cgi-bin/dss_form
2. Sliders για το stretching: TODO:Να γίνεται και negative
3. TODO: Έλεγχος για την πιθανότητα ήδη γνωστού SN στο πεδίο της εικόνας: https://wis-tns.weizmann.ac.il/search 
4. Keyboard Shortcut Keys: OK @ 20190317
5. TODO: Αποθήκευση της καινούργιας εικόνας ώς εικόνα αναφοράς
6. Όταν υπάρχει ένας ύποπτος, πρέπει ο χρήστης να μπορεί να το δηλώσει: Η δήλωση γίνεται με Ctrl+Mouse_Click
   στο Subject και αποθηκεύεται ως JPG. TODO: Να προστεθεί και το χ,y (pixels) στη εικόνα.
   TODO: Να δούμε και την επίλυση της θέσης.
7. 
 **********************************************************************************************************/
