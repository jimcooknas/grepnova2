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
#pragma warning disable IDE0018 // Naming Styles
#pragma warning disable IDE0028
    // version 1.3.0   @ 20190407
    // Form-Class created to allow user to select his shortcuts
    public partial class HotKeys : Form
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        string shortcutFileName = "AppShortcut.txt";
        /// <summary>
        /// Shows the application's hot-keys (shortcuts)
        /// </summary>
        public HotKeys() {
            InitializeComponent();
            label40.Text = "Select Modifier key and Keyboard key for each action.\r\n" +
                           "Modifiers can be <Shift>, <Ctrl>, <Alt> or <None>. Selecting " +
                           "a Modifier other than <None> means that the action will be invoked " +
                           "by simultaneously pressing both Modifier AND Keyboard keys";
            List<string> lst = new List<string>();
            string[] lines = System.IO.File.ReadAllLines("Shortcuts.txt");
            //foreach(var c in Enum.GetValues(typeof(Keys)))
            foreach (string li in lines)
            {
                string[] ss = li.Split('\t');
                lst.Add(ss[0]);
                dict.Add(ss[0], int.Parse(ss[1]));
            }
            //set combo-boxes
            cbTemplate.Items.AddRange(lst.ToArray<string>());
            cbSubject.Items.AddRange(lst.ToArray<string>());
            cbAligned.Items.AddRange(lst.ToArray<string>());
            cbBlink.Items.AddRange(lst.ToArray<string>());
            cbNext.Items.AddRange(lst.ToArray<string>());
            cbPrev.Items.AddRange(lst.ToArray<string>());
            cbZoom.Items.AddRange(lst.ToArray<string>());
            cbTempUp.Items.AddRange(lst.ToArray<string>());
            cbTempDown.Items.AddRange(lst.ToArray<string>());
            cbSubjUp.Items.AddRange(lst.ToArray<string>());
            cbSubjDown.Items.AddRange(lst.ToArray<string>());
            cbAlignUp.Items.AddRange(lst.ToArray<string>());
            cbAlignDown.Items.AddRange(lst.ToArray<string>());
            cbRefresh.Items.AddRange(lst.ToArray<string>());
            cbHeaders.Items.AddRange(lst.ToArray<string>());
            cbRemoveAnnot.Items.AddRange(lst.ToArray<string>());
            cbStretch.Items.AddRange(lst.ToArray<string>());
            cbCurve.Items.AddRange(lst.ToArray<string>());
            cbSaveAsTemp.Items.AddRange(lst.ToArray<string>());
            cbDownload.Items.AddRange(lst.ToArray<string>());
            cbBlank.Items.AddRange(lst.ToArray<string>());
            GetKeysFromFile(shortcutFileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fn"></param>
        private void GetKeysFromFile(string fn) {
            string[] lines = System.IO.File.ReadAllLines(fn);
            foreach(string ss in lines)
            {
                string[] sss = ss.Split('\t');
                string fld = sss[0];
                string special = sss[1];
                string key = sss[2];
                string value = sss[3];
                switch (fld)
                {
                    case "Template":
                        cbTemplate.SelectedItem = key;
                        specTemp.SelectedItem = special;
                        break;
                    case "Subject":
                        cbSubject.SelectedItem = key;
                        specSubj.SelectedItem = special;
                        break;
                    case "Aligned Template":
                        cbAligned.SelectedItem = key;
                        specAlign.SelectedItem = special;
                        break;
                    case "Blink":
                        cbBlink.SelectedItem = key;
                        specBlink.SelectedItem = special;
                        break;
                    case "ZoomForm":
                        cbZoom.SelectedItem = key;
                        specZoom.SelectedItem = special;
                        break;
                    case "Next":
                        cbNext.SelectedItem = key;
                        specNext.SelectedItem = special;
                        break;
                    case "Previous":
                        cbPrev.SelectedItem = key;
                        specPrev.SelectedItem = special;
                        break;
                    case "Template Bright Up":
                        cbTempUp.SelectedItem = key;
                        specTempUp.SelectedItem = special;
                        break;
                    case "Template Bright Down":
                        cbTempDown.SelectedItem = key;
                        specTempDown.SelectedItem = special;
                        break;
                    case "Subject Bright Up":
                        cbSubjUp.SelectedItem = key;
                        specSubjUp.SelectedItem = special;
                        break;
                    case "Subject Bright Down":
                        cbSubjDown.SelectedItem = key;
                        specSubjDown.SelectedItem = special;
                        break;
                    case "Aligned Template Bright Up":
                        cbAlignUp.SelectedItem = key;
                        specAlignUp.SelectedItem = special;
                        break;
                    case "Aligned Template Bright Down":
                        cbAlignDown.SelectedItem = key;
                        specAlignDown.SelectedItem = special;
                        break;
                    case "Refresh Batch":
                        cbRefresh.SelectedItem = key;
                        specRefresh.SelectedItem = special;
                        break;
                    case "Show Images Headers":
                        cbHeaders.SelectedItem = key;
                        specHeaders.SelectedItem = special;
                        break;
                    case "Remove Annotations":
                        cbRemoveAnnot.SelectedItem = key;
                        specRemoveAnnot.SelectedItem = special;
                        break;
                    case "Stretch Image":
                        cbStretch.SelectedItem = key;
                        specStretch.SelectedItem = special;
                        break;
                    case "Image Curve":
                        cbCurve.SelectedItem = key;
                        specCurve.SelectedItem = special;
                        break;
                    case "Save As Template":
                        cbSaveAsTemp.SelectedItem = key;
                        specSaveAsTemp.SelectedItem = special;
                        break;
                    case "Download from DSS":
                        cbDownload.SelectedItem = key;
                        specDownload.SelectedItem = special;
                        break;
                    case "Blank Subject":
                        cbBlank.SelectedItem = key;
                        specBlank.SelectedItem = special;
                        break;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void bntSave_Click(object sender, EventArgs e) {
            List<string> lst = new List<string>();
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Template", specTemp.Items[specTemp.SelectedIndex], cbTemplate.Items[cbTemplate.SelectedIndex], dict[cbTemplate.Items[cbTemplate.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Subject", specSubj.Items[specSubj.SelectedIndex], cbSubject.Items[cbSubject.SelectedIndex], dict[cbSubject.Items[cbSubject.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Aligned Template", specAlign.Items[specAlign.SelectedIndex], cbAligned.Items[cbAligned.SelectedIndex], dict[cbAligned.Items[cbAligned.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Blink", specBlink.Items[specBlink.SelectedIndex], cbBlink.Items[cbBlink.SelectedIndex], dict[cbBlink.Items[cbBlink.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "ZoomForm", specZoom.Items[specZoom.SelectedIndex], cbZoom.Items[cbZoom.SelectedIndex], dict[cbZoom.Items[cbZoom.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Next", specNext.Items[specNext.SelectedIndex], cbNext.Items[cbNext.SelectedIndex], dict[cbNext.Items[cbNext.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Previous", specPrev.Items[specPrev.SelectedIndex], cbPrev.Items[cbPrev.SelectedIndex], dict[cbPrev.Items[cbPrev.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Template Bright Up", specTempUp.Items[specTempUp.SelectedIndex], cbTempUp.Items[cbTempUp.SelectedIndex], dict[cbTempUp.Items[cbTempUp.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Template Bright Down", specTempDown.Items[specTempDown.SelectedIndex], cbTempDown.Items[cbTempDown.SelectedIndex], dict[cbTempDown.Items[cbTempDown.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Subject Bright Up", specSubjUp.Items[specSubjUp.SelectedIndex], cbSubjUp.Items[cbSubjUp.SelectedIndex], dict[cbSubjUp.Items[cbSubjUp.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Subject Bright Down", specSubjDown.Items[specSubjDown.SelectedIndex], cbSubjDown.Items[cbSubjDown.SelectedIndex], dict[cbSubjDown.Items[cbSubjDown.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Aligned Template Bright Up", specAlignUp.Items[specAlignUp.SelectedIndex], cbAlignUp.Items[cbAlignUp.SelectedIndex], dict[cbAlignUp.Items[cbAlignUp.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Aligned Template Bright Down", specAlignDown.Items[specAlignDown.SelectedIndex], cbAlignDown.Items[cbAlignDown.SelectedIndex], dict[cbAlignDown.Items[cbAlignDown.SelectedIndex].ToString()]));

            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Refresh Batch", specRefresh.Items[specRefresh.SelectedIndex], cbRefresh.Items[cbRefresh.SelectedIndex], dict[cbRefresh.Items[cbRefresh.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Show Images Headers", specHeaders.Items[specHeaders.SelectedIndex], cbHeaders.Items[cbHeaders.SelectedIndex], dict[cbHeaders.Items[cbHeaders.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Remove Annotations", specRemoveAnnot.Items[specRemoveAnnot.SelectedIndex], cbRemoveAnnot.Items[cbRemoveAnnot.SelectedIndex], dict[cbRemoveAnnot.Items[cbRemoveAnnot.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Stretch Image", specStretch.Items[specStretch.SelectedIndex], cbStretch.Items[cbStretch.SelectedIndex], dict[cbStretch.Items[cbStretch.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Image Curve", specCurve.Items[specCurve.SelectedIndex], cbCurve.Items[cbCurve.SelectedIndex], dict[cbCurve.Items[cbCurve.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Save As Template", specSaveAsTemp.Items[specSaveAsTemp.SelectedIndex], cbSaveAsTemp.Items[cbSaveAsTemp.SelectedIndex], dict[cbSaveAsTemp.Items[cbSaveAsTemp.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Download from DSS", specDownload.Items[specDownload.SelectedIndex], cbDownload.Items[cbDownload.SelectedIndex], dict[cbDownload.Items[cbDownload.SelectedIndex].ToString()]));
            lst.Add(String.Format("{0}\t{1}\t{2}\t{3}", "Blank Subject", specBlank.Items[specBlank.SelectedIndex], cbBlank.Items[cbBlank.SelectedIndex], dict[cbBlank.Items[cbBlank.SelectedIndex].ToString()]));
            string[] lines = lst.ToArray<string>();
            System.IO.File.WriteAllLines(shortcutFileName, lines);
        }
    }
}
