using System;
using System.Collections;
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
    public partial class VisieRList : Form
    {
        private ListViewColumnSorter lvwColumnSorter;

        List<VisieRCatalog.Catalog> vi;
        public VisieRList(List<VisieRCatalog.Catalog> vi) {
            InitializeComponent();
            // Create an instance of a ListView column sorter and assign it to the ListView control.
            lvwColumnSorter = new ListViewColumnSorter();
            this.listView1.ListViewItemSorter = lvwColumnSorter;
            this.vi = vi;
        }

        private void VisieRList_Load(object sender, EventArgs e) {
            FillGrid();
        }

        private void FillGrid() {
            foreach (VisieRCatalog.Catalog v in vi)
            {
                string[] it = new string[14];
                it[0] = v.USNO;
                it[1] = String.Format("{0:0.000000}", v.RAJ2000);
                it[2] = String.Format("{0:0.000000}", v.DEJ2000);
                it[3] = String.Format("{0}", v.e_RAJ2000);
                it[4] = String.Format("{0}", v.e_DEJ2000);
                it[5] = String.Format("{0:0.0}", v.Epoch);
                it[6] = "" + v.pmRA;
                it[7] = "" + v.pmDE;
                it[8] = "" + v.Ndet;
                if (v.B1mag == 0) it[9] = ""; else it[9] = String.Format("{0:0.00}", v.B1mag);
                if (v.R1mag == 0) it[10] = ""; else it[10] = String.Format("{0:0.00}", v.R1mag);
                if (v.B2mag == 0) it[11] = ""; else it[11] = String.Format("{0:0.00}", v.B2mag);
                if (v.R2mag == 0) it[12] = ""; else it[12] = String.Format("{0:0.00}", v.R2mag);
                if (v.Imag == 0) it[13] = ""; else it[13] = String.Format("{0:0.00}", v.Imag);
                ListViewItem item = new ListViewItem(it);
                listView1.Items.Add(item);
            }
            listView1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e) {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn) {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending){
                    lvwColumnSorter.Order = SortOrder.Descending;
                }else{
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }else{
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }
            // Perform the sort with these new sort options.
            this.listView1.Sort();
        }

        
    }


    /////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// This class is an implementation of the 'IComparer' interface.
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {
        /// <summary>
        /// Specifies the column to be sorted
        /// </summary>
        private int ColumnToSort;
        /// <summary>
        /// Specifies the order in which to sort (i.e. 'Ascending').
        /// </summary>
        private SortOrder OrderOfSort;
        /// <summary>
        /// Case insensitive comparer object
        /// </summary>
        private CaseInsensitiveComparer ObjectCompare;

        /// <summary>
        /// Class constructor.  Initializes various elements
        /// </summary>
        public ListViewColumnSorter() {
            // Initialize the column to '0'
            ColumnToSort = 0;

            // Initialize the sort order to 'none'
            OrderOfSort = SortOrder.None;

            // Initialize the CaseInsensitiveComparer object
            ObjectCompare = new CaseInsensitiveComparer();
        }

        /// <summary>
        /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y) {
            int compareResult;
            ListViewItem listviewX, listviewY;

            // Cast the objects to be compared to ListViewItem objects
            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;

            // Compare the two items
            compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);

            // Calculate correct return value based on object comparison
            if (OrderOfSort == SortOrder.Ascending){
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            }else if (OrderOfSort == SortOrder.Descending){
                // Descending sort is selected, return negative result of compare operation
                return (-compareResult);
            }else{
                // Return '0' to indicate they are equal
                return 0;
            }
        }

        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int SortColumn
        {
            set{
                ColumnToSort = value;
            }
            get{
                return ColumnToSort;
            }
        }

        /// <summary>
        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order
        {
            set{
                OrderOfSort = value;
            }
            get{
                return OrderOfSort;
            }
        }

    }
}
