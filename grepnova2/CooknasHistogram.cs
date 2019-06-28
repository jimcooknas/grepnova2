using System;
//using System.Collections;
using System.ComponentModel;
using System.Drawing;
//using System.Data;
using System.Windows.Forms;

namespace CooknasHistogram
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// Summary description for HistogramaDesenat.
    /// </summary>
    public class CooknasHistogram : Control
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public CooknasHistogram()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

			this.Paint += new PaintEventHandler(CooknasHistogram_Paint);
			this.Resize+=new EventHandler(CooknasHistogram_Resize);
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.SuspendLayout();
            // 
            // CooknasHistogram
            // 
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "CooknasHistogram";
            this.Size = new System.Drawing.Size(208, 176);
            this.ResumeLayout(false);

		}
		#endregion

		private void CooknasHistogram_Paint(object sender, PaintEventArgs e)
		{
			if (myIsDrawing)
			{
				Graphics g = e.Graphics;
				Pen myPen = new Pen(new SolidBrush(myColor),myXUnit);
                Pen crossPen = new Pen(new SolidBrush(Color.Black), myXUnit);
				//The width of the pen is given by the XUnit for the control.
				for (int i=0;i<myValues.Length;i++)
				{
					//We draw each line 
					g.DrawLine(myPen,
						new PointF(myOffsetX + (i*myXUnit), this.Height - myOffsetY), 
						new PointF(myOffsetX + (i*myXUnit), this.Height - myOffsetY - myValues[i] * myYUnit));
					//We plot the coresponding index for the maximum value.
					/*if (myValues[i]==myMaxValue)
					{
						SizeF mySize = g.MeasureString(i.ToString(),myFont);

						g.DrawString(i.ToString(),myFont,new SolidBrush(myColor),
							new PointF(myOffset + (i*myXUnit) - (mySize.Width/2), this.Height - myFont.Height ),
							System.Drawing.StringFormat.GenericDefault);
					}*/
				}
                //We draw the crooss at the center of histogram
                //vertical
                g.DrawLine(crossPen, (this.Width-myOffsetX-2)/2 +myOffsetX, 0, (this.Width - myOffsetX - 2) / 2 + myOffsetX, this.Height-myOffsetY-2);
                //horizontal
                g.DrawLine(crossPen, myOffsetX, (this.Height-myOffsetY-2)/2+2, this.Width-2, (this.Height-myOffsetY-2)/2+2);
                //We draw the indexes for xPoint and yPoint beeing plotted
                g.DrawString(xPoint.ToString(),myFont, new SolidBrush(Color.Black),new PointF((this.Width - myOffsetX - 2) / 2 + myOffsetX - g.MeasureString(xPoint.ToString(), myFont).Width/2, this.Height - myFont.Height),System.Drawing.StringFormat.GenericDefault);
				g.DrawString(yPoint.ToString(),myFont, 
					new SolidBrush(Color.Black),
					new PointF(0,//myOffset + (myValues.Length * myXUnit) - g.MeasureString((myValues.Length-1).ToString(),myFont).Width,
                    (this.Height - myOffsetY - 2) / 2 + 2 - myFont.Height/2),
					System.Drawing.StringFormat.GenericDefault);

				//We draw a rectangle surrounding the control.
				g.DrawRectangle(new System.Drawing.Pen(new SolidBrush(Color.Black),1), myOffsetX, 2, this.Width - myOffsetX - 2, this.Height - myOffsetY - 2);
			}
		}

		long myMaxValue;
		private int[] myValues;
		private bool myIsDrawing;
        private int xPoint;
        private int yPoint;

		private float myYUnit; //this gives the vertical unit used to scale our values
		private float myXUnit; //this gives the horizontal unit used to scale our values
		private int myOffsetX = 20; //the X offset, in pixels, from the control margins.
        private int myOffsetY = 20; //the X offset, in pixels, from the control margins.

        private Color myColor = Color.Red;
		private Font myFont = new Font("Tahoma",7);

		[Category("Histogram Options")]
		[Description ("The horizontal distance from the margins for the histogram")]
		public int OffsetX
		{
			set
			{
				if (value>0)
					myOffsetX= value;
			}
			get
			{
				return myOffsetX;
			}
		}

        [Category("Histogram Options")]
        [Description("The horizontal distance from the margins for the histogram")]
        public int OffsetY
        {
            set
            {
                if (value > 0)
                    myOffsetY = value;
            }
            get
            {
                return myOffsetY;
            }
        }

        [Category("Histogram Options")]
		[Description ("The color used within the control")]
		public Color DisplayColor
		{
			set
			{
				myColor = value;
			}
			get
			{
				return myColor;
			}
		}

        /// <summary>
        /// We draw the histogram on the control
        /// </summary>
        /// <param name="Values">The values beeing draw</param>
        /// <param name="myXPoint">X coordinate</param>
        /// <param name="myYPoint">Y coordinate</param>
        public void DrawHistogram(int[] Values, int myXPoint, int myYPoint)
		{
			myValues = new int[Values.Length];
            xPoint = myXPoint;
            yPoint = myYPoint;
			Values.CopyTo(myValues,0);

			myIsDrawing = true;
			myMaxValue = getMaxim(myValues);

			ComputeXYUnitValues();

			this.Refresh();
		}

#pragma warning disable IDE1006 // Naming Styles
		/// <summary>
		/// We get the highest value from the array
		/// </summary>
		/// <param name="Vals">The array of values in which we look</param>
		/// <returns>The maximum value</returns>
		private long getMaxim(int[] Vals)
		{
			if (myIsDrawing)
			{
				int max = 0;
				for (int i=0;i<Vals.Length;i++)
				{
					if (Vals[i] > max)
						max = Vals[i];
				}
				return max;
			}
			return 1;
		}

		private void CooknasHistogram_Resize(object sender, EventArgs e)
		{
			if (myIsDrawing)
			{
				ComputeXYUnitValues();
			}
			this.Refresh();
		}

		private void ComputeXYUnitValues()
		{
			myYUnit = (float) (this.Height - myOffsetY - 2) / myMaxValue;
			myXUnit = (float) (this.Width - myOffsetX - 2) / (myValues.Length-1);
		}
	}
}
