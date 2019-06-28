using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// Very basic slider control with selection range.
    /// </summary>
    [Description("Very basic slider control with selection range.")]
    public partial class SelectionRangeSlider : UserControl
    {
        /// <summary>
        /// Color of the text values.
        /// </summary>
        [Description("Color of the text values.")]
        public Brush TextColor
        {
            get { return textcolor; }
            set { textcolor = value; Invalidate(); }
        }
        Brush textcolor = Brushes.Black;

        /// <summary>
        /// Left-Right offset of control.
        /// </summary>
        [Description("Left-Right offset of control.")]
        public int Offset
        {
            get { return offset; }
            set { offset = value; Invalidate(); }
        }
        int offset = 10;

        /// <summary>
        /// Minimum value of the slider.
        /// </summary>
        [Description("Minimum value of the slider.")]
        public int Min
        {
            get { return min; }
            set { min = value; Invalidate(); }
        }
        int min = 0;
        /// <summary>
        /// Maximum value of the slider.
        /// </summary>
        [Description("Maximum value of the slider.")]
        public int Max
        {
            get { return max; }
            set { max = value; Invalidate(); }
        }
        int max = 100;
        /// <summary>
        /// Minimum value of the selection range.
        /// </summary>
        [Description("Minimum value of the selection range.")]
        public int SelectedMin
        {
            get { return selectedMin; }
            set
            {
                selectedMin = value;
                if (SelectionChanged != null)
                    SelectionChanged(this, null);
                Invalidate();
            }
        }
        int selectedMin = 0;
        /// <summary>
        /// Maximum value of the selection range.
        /// </summary>
        [Description("Maximum value of the selection range.")]
        public int SelectedMax
        {
            get { return selectedMax; }
            set
            {
                selectedMax = value;
                if (SelectionChanged != null)
                    SelectionChanged(this, null);
                Invalidate();
            }
        }
        int selectedMax = 100;
        /// <summary>
        /// Current value.
        /// </summary>
        [Description("Current value.")]
        public int Value
        {
            get { return value; }
            set
            {
                this.value = value;
                if (ValueChanged != null)
                    ValueChanged(this, null);
                Invalidate();
            }
        }
        int value = 50;
        /// <summary>
        /// Fired when SelectedMin or SelectedMax changes.
        /// </summary>
        [Description("Fired when SelectedMin or SelectedMax changes.")]
        public event EventHandler SelectionChanged;
        /// <summary>
        /// Fired when Value changes.
        /// </summary>
        [Description("Fired when Value changes.")]
        public event EventHandler ValueChanged;

        public SelectionRangeSlider()
        {
            InitializeComponent();
            //avoid flickering
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            Paint += new PaintEventHandler(SelectionRangeSlider_Paint);
            MouseDown += new MouseEventHandler(SelectionRangeSlider_MouseDown);
            MouseMove += new MouseEventHandler(SelectionRangeSlider_MouseMove);

        }

        void SelectionRangeSlider_Paint(object sender, PaintEventArgs e)
        {
            //paint background in white
            e.Graphics.FillRectangle(Brushes.Transparent, ClientRectangle);
            int x = ClientRectangle.X;
            int y = ClientRectangle.Y;
            int posMin = (selectedMin - Min) * Width / (Max - Min);
            posMin = (posMin > offset ? (posMin > (Width - offset) ? Width - offset : posMin) : offset);
            int posMax = (selectedMax - Min) * Width / (Max - Min);//posMin+
            posMax = (posMax > offset ? (posMax > Width - offset ? Width - offset : posMax) : offset);
            e.Graphics.FillPolygon(Brushes.Red, new Point[] { new Point(posMin, Height),
                                                              new Point(posMin - 10, Height),
                                                              new Point(posMin, 0) });
            e.Graphics.FillPolygon(Brushes.Green, new Point[] { new Point(posMax, Height),
                                                                new Point(posMax + 10, Height),
                                                                new Point(posMax, 0) });
            //paint selection range in blue
            Pen baselinePen = Pens.Red;
            if (selectedMax >= selectedMin)
            {
                baselinePen = Pens.Green;
            }
            /*if (selectedMax >= selectedMin)
            {
                Rectangle selectionRect = new Rectangle(
                    (selectedMin - Min) * Width / (Max - Min), 0,
                    (selectedMax - selectedMin) * Width / (Max - Min), Height);
                e.Graphics.FillRectangle(Brushes.Green, selectionRect);
            }
            else
            {
                Rectangle selectionRect = new Rectangle(
                    (selectedMax - Min) * Width / (Max - Min), 0,
                    (selectedMin - selectedMax) * Width / (Max - Min), Height);
                e.Graphics.FillRectangle(Brushes.Red, selectionRect);
            }*/
            //draw a black frame around our control
            //e.Graphics.DrawRectangle(Pens.Black, 0, 0, Width - 1, Height - 1);
            e.Graphics.DrawLine(baselinePen, offset, Height - 1, Width - 1-offset, Height - 1);
            if (textcolor != null)
            {
                e.Graphics.DrawString(SelectedMin.ToString(), this.Font, textcolor, offset, 0);//textcolor=Brushes.Red
                e.Graphics.DrawString(SelectedMax.ToString(), this.Font, textcolor, this.Width - e.Graphics.MeasureString(SelectedMax.ToString(),this.Font).Width-offset, 0);//textcolor=Brushes.Red
            }
        }

        void SelectionRangeSlider_MouseDown(object sender, MouseEventArgs e)
        {
            //check where the user clicked so we can decide which thumb to move
            int pointedValue = Min + e.X * (Max - Min) / Width;
            int diff = Min + 10 * (Max - Min) / Width;
            //int distValue = Math.Abs(pointedValue - Value);
            int distMin = Math.Abs(pointedValue - SelectedMin);
            int distMax = Math.Abs(pointedValue - SelectedMax);
            int minDist = Math.Min(distMin, distMax);// Math.Min(distValue, Math.Min(distMin, distMax));
            if (minDist == distMin)
                movingMode = MovingMode.MovingMin;
            else if (minDist == distMax)
                movingMode = MovingMode.MovingMax;
            //else
            //    movingMode = MovingMode.MovingRange;
            //call this to refreh the position of the selected thumb
            SelectionRangeSlider_MouseMove(sender, e);
        }

        void SelectionRangeSlider_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            //if the left button is pushed, move the selected thumb
            int pointedValue = Min + e.X * (Max - Min) / Width;
            //if Ctrl key is pressed already then move Min-Max range
            if (Control.ModifierKeys == Keys.Control){
                int diff1 = selectedMax - selectedMin;
                SelectedMin = (pointedValue - (int)(diff1 / 2.0))>0 ? pointedValue - (int)(diff1 / 2.0): 0 ;
                SelectedMin = SelectedMin < Min ? Min : SelectedMin;
                SelectedMax = (pointedValue + (int)(diff1 / 2.0))>0 ? pointedValue + (int)(diff1 / 2.0): 0 ;
                SelectedMax = SelectedMax > Max ? Max : SelectedMax;
            }else{
                if (movingMode == MovingMode.MovingMin){
                    SelectedMin = pointedValue > 0 ? pointedValue : 0;
                    SelectedMin = SelectedMin < Min ? Min : SelectedMin;
                }else if (movingMode == MovingMode.MovingMax){
                    SelectedMax = pointedValue > 0 ? pointedValue : 0;
                    SelectedMax = SelectedMax > Max ? Max : SelectedMax;
                }
            }
        }

        /// <summary>
        /// To know which thumb is moving
        /// </summary>
        enum MovingMode { MovingValue, MovingMin, MovingMax, MovingRange }
        MovingMode movingMode;
    }
}
