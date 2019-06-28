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
    public partial class FitsHeaders : Form
    {
        bool scrolling = false; //to dissallow run SelectionChangeA routine when SelectionChangeB routine is running 
        Form1 frm;
        string fileTemplate = "";
        string fileSubject = "";
        public bool isSomethingToSaveTemplate = false;
        public bool isSomethingToSaveSubject = false;
        int fitsObj = 0; //0=Template 1=Subject

        public FitsHeaders(Control frmCaller, string[] data1, string[] data2, string filepath1, string filepath2)
        {
            InitializeComponent();
            fileTemplate = filepath1;
            fileSubject = filepath2;
            PopulateRows(dataGridView1, data1);
            PopulateRows(dataGridView2, data2);
            lblTemplatePath.Text = filepath1;
            lblSubjectPath.Text = filepath2;
            frm = (Form1)frmCaller;
        }

        private bool CheckDate(String date)
        {
            try{
                DateTime dt = DateTime.Parse(date);
                return true;
            }catch{
                return false;
            }
        }

        public void PopulateRows(DataGridView lst, string[] data)
        {
            foreach(string onedata in data){
                try{
                    Console.Out.WriteLine(onedata);
                    string[] key = onedata.Split('=');
                    string[] rowdata = new string[3];
                    if (key.Length == 2) {//all keys except HISTORY
                        rowdata[0] = key[0].Trim();
                        string[] seconddata = key[1].Split('/');
                        if (seconddata.Length == 2) {
                            seconddata[0] = seconddata[0].Trim();
                            seconddata[1] = seconddata[1].Trim();
                            rowdata[1] = seconddata[0].Replace("'", "");
                            rowdata[2] = seconddata[1].Replace("'", "");
                            rowdata[1] = rowdata[1].Trim();
                            rowdata[2] = rowdata[2].Trim();
                        } else if (seconddata.Length == 3) {
                            if (CheckDate(key[1].Replace("'", ""))) {//it's date
                                rowdata[1] = String.Format("{0}", key[1].Replace("'", ""));
                                rowdata[1] = rowdata[1].Trim();
                                rowdata[2] = "";
                            } else {
                                seconddata[0] = seconddata[0].Trim();
                                seconddata[1] = seconddata[1].Trim();
                                seconddata[2] = seconddata[2].Trim();
                                rowdata[1] = seconddata[0].Replace("'", "");
                                rowdata[2] = seconddata[1].Replace("'", "") + seconddata[2].Replace("'", "");
                                rowdata[1] = rowdata[1].Trim();
                                rowdata[2] = rowdata[2].Trim();
                            }
                        } else {
                            rowdata[1] = seconddata[0].Trim();
                            rowdata[1] = rowdata[1].Replace("'", "");
                            rowdata[1] = rowdata[1].Trim();
                            rowdata[2] = "";
                        }
                    }else if(key.Length > 2){//counts only the first =
                        rowdata[0] = key[0].Trim();
                        string newkey = "";
                        for (int i = 1; i < key.Length; i++) newkey += key[i];
                        string[] seconddata = newkey.Split('/');
                        if (seconddata.Length == 2){
                            seconddata[0] = seconddata[0].Trim();
                            seconddata[1] = seconddata[1].Trim();
                            rowdata[1] = seconddata[0].Replace("'", "");
                            rowdata[2] = seconddata[1].Replace("'", "");
                            rowdata[1] = rowdata[1].Trim();
                            rowdata[2] = rowdata[2].Trim();
                        }else if (seconddata.Length == 3){
                            if (CheckDate(newkey.Replace("'", ""))){//it's date
                                rowdata[1] = String.Format("{0}", newkey.Replace("'", ""));
                                rowdata[1] = rowdata[1].Trim();
                                rowdata[2] = "";
                            }else{
                                seconddata[0] = seconddata[0].Trim();
                                seconddata[1] = seconddata[1].Trim();
                                seconddata[2] = seconddata[2].Trim();
                                rowdata[1] = seconddata[0].Replace("'", "");
                                rowdata[2] = seconddata[1].Replace("'", "") + seconddata[2].Replace("'", "");
                                rowdata[1] = rowdata[1].Trim();
                                rowdata[2] = rowdata[2].Trim();
                            }
                        }
                        else
                        {
                            rowdata[1] = seconddata[0].Trim();
                            rowdata[1] = rowdata[1].Replace("'", "");
                            rowdata[1] = rowdata[1].Trim();
                            rowdata[2] = "";
                        }
                    }
                    else{//it must be HISTORY
                        rowdata[0] = key[0].Substring(0, 8);
                        rowdata[1] = key[0].Substring(8);
                        rowdata[0] = rowdata[0].Trim();
                        rowdata[1] = rowdata[1].Trim();
                    }
                    lst.Rows.Add(rowdata);
                }catch (Exception ex){
                    if (Form1.iLogLevel > 2) frm.LogEntry("Error in reading header: " + ex.Message.ToString());
                    Form1.frm.LogError("PopulateRows","Error in reading header: " + ex.Message.ToString());
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0 && scrolling == false)
            {
                scrolling = true;
                int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
                string key = Convert.ToString(selectedRow.Cells["Column1"].Value);
                foreach(DataGridViewRow row in dataGridView2.Rows)
                {
                    if (Convert.ToString(row.Cells["dataGridViewTextBoxColumn1"].Value).Equals(key))
                    {
                        row.Selected = true;
                        try{
                            dataGridView2.FirstDisplayedScrollingRowIndex = dataGridView1.FirstDisplayedScrollingRowIndex;
                        }catch { }
                        return;
                    }
                }
            }
            scrolling = false;
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedCells.Count > 0 && scrolling==false)
            {
                scrolling = true;
                int selectedrowindex = dataGridView2.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = dataGridView2.Rows[selectedrowindex];
                string key = Convert.ToString(selectedRow.Cells["dataGridViewTextBoxColumn1"].Value);
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (Convert.ToString(row.Cells["Column1"].Value).Equals(key))
                    {
                        row.Selected = true;
                        dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView2.FirstDisplayedScrollingRowIndex;
                        return;
                    }
                }
            }
            scrolling = false;
        }

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            isSomethingToSaveTemplate = true;
            lblTemplatePath.ForeColor = Color.Red;
            btnSave.Enabled = true;
        }

        private void dataGridView2_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            isSomethingToSaveSubject = true;
            lblSubjectPath.ForeColor = Color.Red;
            btnSave.Enabled = true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //save FITS header in both Template and Subject
            if (isSomethingToSaveTemplate){
                if(Fits.SaveFITSFromDataGridView(0, lblTemplatePath.Text.ToString(), dataGridView1)){
                    if (Form1.iLogLevel > 2) Form1.frm.LogEntry("Template's header modified successfully");
                }else{
                    if (Form1.iLogLevel > 0) Form1.frm.LogEntry("Template's header modification ERROR");
                }
            }
            if (isSomethingToSaveSubject){
                if(Fits.SaveFITSFromDataGridView(1, lblSubjectPath.Text.ToString(), dataGridView2)) {
                    if (Form1.iLogLevel > 2) Form1.frm.LogEntry("Subject's header modified successfully");
                }else{
                    if (Form1.iLogLevel > 0) Form1.frm.LogEntry("Subject's header modification ERROR");
                }
            }
            //set local variables
            lblSubjectPath.ForeColor = SystemColors.HotTrack;
            lblTemplatePath.ForeColor = SystemColors.HotTrack;
            isSomethingToSaveSubject = false;
            isSomethingToSaveTemplate = false;
            btnSave.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            fitsObj = 1;
            AddNewHeaderField head = new AddNewHeaderField(this, fitsObj){
                Location = new Point(dataGridView2.Location.X + this.Location.X, dataGridView2.Location.Y + this.Location.Y + dataGridView2.Size.Height / 2)
            };
            head.ShowDialog(this);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            fitsObj = 0;
            AddNewHeaderField head = new AddNewHeaderField(this, fitsObj){
                Location = new Point(dataGridView1.Location.X + this.Location.X, dataGridView1.Location.Y + this.Location.Y + dataGridView2.Size.Height / 2)
            };
            head.ShowDialog(this);
        }

        private void btnShowProperFields_Click(object sender, EventArgs e) {
            FitsHeaderProper fhp = new FitsHeaderProper();
            fhp.ShowDialog(this);
        }

        private void FitsHeaders_Load(object sender, EventArgs e) {

        }
    }
}
