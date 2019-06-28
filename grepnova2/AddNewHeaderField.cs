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
    public partial class AddNewHeaderField : Form
    {
        FitsHeaderClass head = new FitsHeaderClass();
        FitsHeaders fhead = null;
        int fitsObj = 0;//0=Template 1=Subject

        public AddNewHeaderField(FitsHeaders fh, int fo)
        {
            fhead = fh;
            fitsObj = fo;
            InitializeComponent();
            foreach(FitsHeaderClass.FitsHeadStruct fhs in head.FitsHead){
                cbFieldName.Items.Add(fhs.keyword);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cbFieldName_SelectedIndexChanged(object sender, EventArgs e)
        {
              
            lblDesc.Text = head.GetFitsHeaderComment(cbFieldName.Text.ToString());
            Type type = head.GetFitsHeaderType(cbFieldName.Text.ToString());
            if (type == typeof(int)){
                textBox1.Text = "integer";
            } else if (type == typeof(float)) {
                textBox1.Text = "float";
            } else if (type == typeof(bool)){
                textBox1.Text = "boolean";
            } else if (type == typeof(string)){
                textBox1.Text = "string";
            } else {
                textBox1.Text = "string";
            }
            return;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string[] dataRow = new string[3];
            dataRow[0] = cbFieldName.Text.ToString();
            dataRow[1] = txtFieldValue.Text.ToString();
            dataRow[2] = lblDesc.Text.ToString();
            if (fitsObj == 0){
                int iPos1 = FieldExists(fhead.dataGridView1, dataRow[0]);
                if (iPos1>-1){
                    fhead.dataGridView1.Rows[iPos1].Cells[1].Value = dataRow[1];
                }else{
                    fhead.dataGridView1.Rows.Insert(fhead.dataGridView1.Rows.Count - 1, dataRow);
                }
                fhead.isSomethingToSaveTemplate = true;
                fhead.lblTemplatePath.ForeColor = Color.Red;
                fhead.btnSave.Enabled = true;
            }else{
                int iPos2 = FieldExists(fhead.dataGridView2, dataRow[0]);
                if (iPos2 > -1){
                    fhead.dataGridView2.Rows[iPos2].Cells[1].Value = dataRow[1];
                }else{
                    fhead.dataGridView2.Rows.Insert(fhead.dataGridView2.Rows.Count - 1, dataRow);
                }
                fhead.isSomethingToSaveSubject = true;
                fhead.lblSubjectPath.ForeColor = Color.Red;
                fhead.btnSave.Enabled = true;
            }
            this.Close();
        }

        private int FieldExists(DataGridView dataGrid, string data)
        {
            foreach(DataGridViewRow dgv in dataGrid.Rows){
                if (data.Equals((string)dgv.Cells[0].Value)) return dgv.Index;
            }
            return -1;
        }
    }
}
