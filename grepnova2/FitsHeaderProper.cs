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
    public partial class FitsHeaderProper : Form
    {
        public FitsHeaderProper() {
            InitializeComponent();
        }

        private void FitsHeaderProper_Load(object sender, EventArgs e) {
            FitsHeaderClass fHead = new FitsHeaderClass();
            int i = 1;
            foreach(FitsHeaderClass.FitsHeadStruct fhs in fHead.FitsHead){
                if (fhs.type == null)
                    dataGridView1.Rows.Add(new string[] { String.Format("{0:#00}", i++), fhs.keyword, "null", fhs.comment });
                else
                    dataGridView1.Rows.Add(new string[] { String.Format("{0:#00}", i++), fhs.keyword, fhs.type.Name, fhs.comment });
            }
        }
    }
}
