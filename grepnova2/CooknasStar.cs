using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;

//using grepnova2.Fits;

namespace CooknasStar
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// Summary description for cooknasStar.
    /// </summary>
    public partial class CooknasStar : System.Windows.Forms.UserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Class constructor
        /// </summary>
        public CooknasStar()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            myWidth = this.Width;
            myHeight = this.Height;
            originalOffsetX = myOffsetX;
            originalOffsetY = myOffsetY;
            this.Paint += new PaintEventHandler(CooknasStar_Paint);
            this.Resize += new EventHandler(CooknasStar_Resize);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
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
            // CooknasStar
            // 
            this.BackColor = System.Drawing.Color.Transparent;
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "CooknasStar";
            this.Size = new System.Drawing.Size(140, 90);
            this.ResumeLayout(false);

        }
        #endregion

        private void CooknasStar_Paint(object sender, PaintEventArgs e)
        {
            if (myIsDrawing)
            {
                Graphics g = e.Graphics;
                myWidth = this.Width;
                myHeight = this.Height;
                pointRadius = (float) Math.Min((int)(myWidth/100), (int)(myHeight/100));
                if (pointRadius < 1f) pointRadius = 1f;
                if (pointRadius > 6f) pointRadius = 6f;
                fontSize = 4 + 1 * pointRadius;
                if (fontSize > 10f) fontSize = 10f;
                int lastOffset = 6;
                myOffsetX = (int)originalOffsetX + 4 * (int)fontSize - 20;
                myOffsetY = (int)originalOffsetX + 4 * (int)fontSize - 20;
                myFont = new Font("Tahoma", fontSize);
                Pen myPen = new Pen(new SolidBrush(myColor), 1.0f);// myXUnit);
                Pen crossPen = new Pen(new SolidBrush(Color.Black), myXUnit);
                float diam = 2.0f * pointRadius;
                //Console.Out.WriteLine("width={0} height={1} radius={2} font={3}",myWidth,myHeight,pointRadius,fontSize);
                //We draw a rectangle surrounding the control.
                g.FillRectangle(new SolidBrush(Color.White), new Rectangle(myOffsetX, lastOffset, this.Width - myOffsetX - lastOffset, this.Height - myOffsetY - lastOffset));
                g.DrawRectangle(new System.Drawing.Pen(new SolidBrush(Color.Black), 2), myOffsetX, lastOffset, this.Width - myOffsetX - lastOffset, this.Height - myOffsetY - lastOffset);
                //The width of the pen is given by the XUnit for the control.
                for (int i = 0; i < myValues.Length; i++)
                {
                    //We draw each point (as circle with pointRadius radius) 
                    g.DrawEllipse(myPen, new Rectangle( myOffsetX + (int)(myDist[i] * myXUnit-pointRadius),//x coordinate of top left corner
                                                        this.Height - myOffsetY - (int)(myValues[i] * myYUnit+ pointRadius), //y coordinate of top left corner
                                                        (int)diam, (int)diam));//width and height of the surrounding rectangle
                }
                //We draw the graph axis grid (lines and numbers)
                for (int i = 0; i < 6; i++){
                    g.DrawLine(new Pen(Color.LightGray,0.5f), new Point(myOffsetX + (int)((i)* 2 * myXUnit), lastOffset), new Point(myOffsetX + (int)((i ) * 2 * myXUnit), this.Height - myOffsetY - lastOffset));
                    string txt = String.Format("{0:F0}",((i) * 2 ));
                    float w = g.MeasureString(txt, myFont).Width;
                    float h = g.MeasureString(txt, myFont).Height;
                    if (i == 5) w += 2*pointRadius;
                    g.DrawString(txt, myFont, new SolidBrush(Color.Black), myOffsetX + (int)((i) * 2 * myXUnit) - w / 2 , this.Height - myOffsetY, System.Drawing.StringFormat.GenericDefault);

                    g.DrawLine(new Pen(Color.LightGray, 0.5f), new Point(myOffsetX , this.Height - myOffsetY - (int)((i)*myYUnit*myMaxValue/5)), new Point(this.Width- lastOffset, this.Height - myOffsetY - (int)((i)*myYUnit*myMaxValue/5)));
                    txt = String.Format("{0:F0}", ((i)*myMaxValue/5));
                    w = g.MeasureString(txt, myFont).Width;
                    h = g.MeasureString(txt, myFont).Height;
                    if (i == 5) h = 4;
                    g.DrawString(txt, myFont, new SolidBrush(Color.Black), myOffsetX - w, this.Height - myOffsetY - (int)((i) * myYUnit * myMaxValue / 5) - h/2, System.Drawing.StringFormat.GenericDefault);
                }
            }
        }

        double myMaxValue;
        double myMaxDist;
        float pointRadius = 1f;
        private double[] myValues;
        private double[] myDist;
        private bool myIsDrawing;
        private int xPoint;
        private int yPoint;

        private float originalOffsetX;
        private float originalOffsetY;
        private float myYUnit; //this gives the vertical unit used to scale our values
        private float myXUnit; //this gives the horizontal unit used to scale our values
        private int myOffsetX = 20; //the X offset, in pixels, from the control margins.
        private int myOffsetY = 20; //the X offset, in pixels, from the control margins.
        private int myWidth; //the width, in pixels, of the control.
        private int myHeight; //the height, in pixels, of the control.

        private float fontSize = 6; 
        private Color myColor = Color.Red;
        private Font myFont = new Font("Tahoma", 6);

        /// <summary>
        /// Gets the dist[] array of distances from centroid
        /// </summary>
        public double[] GetDistances
        {
            get
            {
                return myDist;
            }
        }

        /// <summary>
        /// Gets the values[] array of intensities
        /// </summary>
        public double[] GetValues
        {
            get
            {
                return myValues;
            }
        }
        /// <summary>
        /// The horizontal distance from the margins for the graph
        /// </summary>
        [Category("CooknasStar Options")]
        [Description("The horizontal distance from the margins for the graph")]
        public int OffsetX
        {
            set
            {
                if (value > 0)
                    myOffsetX = value;
            }
            get
            {
                return myOffsetX;
            }
        }

        /// <summary>
        /// The vertical distance from the margins for the graph
        /// </summary>
        [Category("CooknasStar Options")]
        [Description("The vertical distance from the margins for the graph")]
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

        ///<summary>The color used to draw the graph points</summary>
        [Category("CooknasStar Options")]
        [Description("The color used to draw the graph points")]
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
        /// We draw the star sequence on the control
        /// </summary>
        /// <param name="dist">the distances from centroid</param>
        /// <param name="Values">The values beeing draw</param>
        /// <param name="myXPoint">the reference x coordinate</param>
        /// <param name="myYPoint">the reference y coordinate</param>
        public void DrawStar(double[] dist, double[] Values, int myXPoint, int myYPoint)
        {
            if (Values == null) return;
            myValues = new double[Values.Length];
            myDist = new double[dist.Length];
            xPoint = myXPoint;
            yPoint = myYPoint;
            myWidth = this.Width;
            myHeight = this.Height;
            originalOffsetX = myOffsetX;
            originalOffsetY = myOffsetY;
            Values.CopyTo(myValues, 0);
            dist.CopyTo(myDist, 0);

            myIsDrawing = true;
            myMaxValue = getMaxim(myValues,3);
            myMaxDist = getMaxim(myDist,0);
            //Console.Out.WriteLine("distMax={0} valMax={1}", myMaxDist, myMaxValue);

            ComputeXYUnitValues();

            this.Update();
            this.Refresh();
        }

#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// We get the highest value from the array
        /// </summary>
        /// <param name="Vals">The array of values in which we look</param>
        /// <param name="trailingZeros">power of ten size of Vals</param>
        /// <returns>The maximum value</returns>
        private double getMaxim(double[] Vals, int trailingZeros=0)
        {
            if (myIsDrawing)
            {
                double max = 0.0;
                for (int i = 0; i < Vals.Length; i++)
                {
                    if (Vals[i] > max)
                        max = Vals[i];
                }
                switch (trailingZeros)
                {
                    case 0:
                        max = (float)((int)(max) + 1);
                        break;
                    case 1:
                        max=(float)(10*(int)(max / 10 + 1));
                        break;
                    case 2:
                        max = (float)(100*(int)(max / 100 + 1));
                        break;
                    case 3:
                        max = (float)(1000*(int)(max / 1000 + 1));
                        break;
                }
                return max;
            }
            return 1;
        }

        private void CooknasStar_Resize(object sender, EventArgs e)
        {
            if (myIsDrawing)
            {
                ComputeXYUnitValues();
            }
            this.Refresh();
        }

        private void ComputeXYUnitValues()
        {
            myYUnit = (float)(this.Height - myOffsetY - 2) /(float) myMaxValue;
            myXUnit = (float)(this.Width - myOffsetX - 2) / (float) myMaxDist;// (myValues.Length - 1);
        }
    }

    /// <summary>
    /// Class calculating the centroid of a star
    /// <para>
    /// <list type="table">
    /// <item><description>How Calculations are made:</description></item>
    /// <item><description>x sum is sum of all pixel intensities * their distance from X Val</description></item>
    /// <item><description>y sum is sum of all pixel intensities * their distance from Y Val</description></item>
    /// <item><description>sum is sum of all pixel intensities</description></item>
    /// <item><description>x centroid = x sum / sum</description></item>
    /// <item><description>y centroid = y sum / sum</description></item>
    /// </list></para>
    /// Note: Once highest intensity pixel was found, all sum data was restricted
    ///       to a SIZExSIZE box around it, as per the algorithm.
    /// </summary>
    public class Centroid
    {
        /// <summary>
        /// Structure that keeps the coordinates of a star (centroid) together with its intensity value and
        /// a text describing calculation data
        /// </summary>
        public struct STAR_COORD
        {
            /// <summary>x coordinate as double</summary>
            public double X;
            /// <summary>y coordinatem as double</summary>
            public double Y;
            /// <summary>intensity of specified coordinate as int</summary>
            public int val;
            /// <summary>formated text with calculation steps</summary>
            public string text;
        }
        /// <summary>size of grid (SIZExSIZE)</summary>
        private static int SIZE = 15;
        /// <summary>center of the grid</summary>
        private static int BACK = 7;
        private string myText = "";

        /// <summary>
        /// Gets the formatted text of centroid solve
        /// </summary>
        public string GetText
        {
            get
            {
                return myText;
            }
        }
        /// <summary>
        /// This function gets the mouse click position on an imageptr and calculates
        /// the centroid of the closest star.
        /// <para>Function returns a <see cref="STAR_COORD"/> structure with the coordinates of the
        /// centroid together with a printable string and (if cooknasStar control is given) it draws
        /// the CooknasStar graph.</para>
        /// </summary>
        /// <remarks>
        /// Idea taken from an alladin plugin, written in java, given in:
        /// https://aladin.u-strasbg.fr/java/Plugins/Centroid.java 
        /// </remarks> 
        /// <param name="img">imageptr of the clicked image</param>
        /// <param name="mouseX">the x coordinate of the clicked point</param>
        /// <param name="mouseY">the y coordinate of the clicked point</param>
        /// <param name="cooknasStar">the CooknasStar control where to draw the graph</param>
        /// <returns>a STAR_COORD structure with the coordinates of the centroid together with a printable string</returns>
        public STAR_COORD position(grepnova2.Fits.imageptr img, int mouseX, int mouseY, CooknasStar cooknasStar=null)
        {
            //mouseX and mouseY must be inside image at least 2*BACK pixels
            if (mouseX < (2*BACK+1) || mouseY < (2*BACK+1) || mouseX > (img.xsize-2*BACK-1) || mouseY > (img.ysize - 2*BACK-1)){
                if (grepnova2.Form1.iLogLevel > 1) grepnova2.Form1.frm.LogEntry("Centroid calculation @ "+mouseX+","+mouseY+": Mouse position outside calculation area");
                STAR_COORD sc = new STAR_COORD{
                    X = -1,
                    Y = -1,
                    val = -1,
                    text = null
                };
                return sc;
            }

            // Get information on current image. If image datapeak=0 then return
            if (img.datapeak == 0) {
                STAR_COORD sc = new STAR_COORD{
                    X = -1,
                    Y = -1,
                    val = -1,
                    text = null
                };
                return sc;
            }
            // get the (x, y) coords based on mouse click
            if (mouseX < 2*BACK){
                mouseX = 2*BACK + 1;
            }
            if (mouseY < 2*BACK){
                mouseY = 2*BACK + 1;
            }
            int X = mouseX;
            int Y = mouseY;
  
            //Get pixel values for image
            int[,] pix = grepnova2.Fits.ConvertImagePtrToIntArray(img.data, img.xsize, img.ysize);

            // *********************************************
            // * Find most intense pixel near mouse click  *
            // *********************************************
            bool search = true;
            int max;         //highest intensity pix val
            int oldMax;      //previous highest intensity pix val
            //newX and newY are used to keep track of next iteration (x, y) values
            int newX = 0;
            int newY = 0;

            max = pix[X,Y];  //intensity set to clicked pixel value
            //begin searching for most intense pixel in SIZExSIZE radius around mouse click
            while (search)
            {
                oldMax = max;   //save max value
                //compare the current (x, y) intensity to the surrounding pixels in a SIZExSIZE grid
                //if a surrounding pixel has a greater intensity, save the value and coords
                int loopX = X - BACK;   //by setting loopX and loopY, we get the pixel in the top left of the SIZE*SIZE grid
                int loopY = Y - BACK;

                //starting at the top left pixel in a SIZExSIZE grid, find the most intense pixel
                for (int i = 0; i < SIZE; i++){
                    for (int j = 0; j < SIZE; j++){
                        if (pix[loopX + i,loopY + j] > max){
                            max = pix[loopX + i,loopY + j];
                            newX = loopX + i;
                            newY = loopY + j;
                        }
                    }
                }
                //if a higher intensity was not found, the centroid algorithm is done
                if (max == oldMax) { search = false; }
                //else set (x, y) to the highest intensity pixel and loop
                else{
                    X = newX;
                    Y = newY;
                }
            }

            // *********************************************
            // * Create information arrays for grid pixels *
            // *********************************************
            double[] vals = new double[SIZE * SIZE];  // this array will contain all the intensities of the pixels in the SIZE*SIZE grid
            double[] dist = new double[SIZE * SIZE];  // contains all distances for each pixel to the central pixel
            double[] distX = new double[SIZE * SIZE]; // contains the X distance for each pixel to the central pixel
            double[] distY = new double[SIZE * SIZE]; // contains the Y distance for each pixel to the central pixel

            //X and Y are now central brightest pixel in SIZExSIZE grid
            int centerX = X - BACK;  //same purpose as loopX and loopY
            int centerY = Y - BACK;
            int count = 0;  //keeps track of pixel count
            for (int i = 0; i < SIZE; i++){
                for (int j = 0; j < SIZE; j++){
                    vals[count] = pix[centerX + i,centerY + j]; //save pixel intensity
                    int sideX = centerX + i - X; //x distance of pixel
                    int sideY = centerY + j - Y; //y distance of pixel
                    //distance of pixel from center, uses x and y to find hypotenuse
                    dist[count] = Math.Sqrt(Math.Pow(sideX, 2) + Math.Pow(sideY, 2)); 
                    distX[count] = sideX; //save x dist
                    distY[count] = sideY; //save y dist
                    count++;
                }
            }

            //if a CooknasStar control is given (not null) then draw the graph of the found star
            if(cooknasStar!=null)cooknasStar.DrawStar(dist, vals, 0, 0);

            // *********************************************
            // * Find value_Low and value_High in grid     *
            // *********************************************
            double level_Low = vals[0];
            double level_High = vals[0];
            for (int i = 0; i < vals.Length; i++){
                if (vals[i] < level_Low)
                    level_Low = vals[i];
                if (vals[i] > level_High)
                    level_High = vals[i];
            }

            // steps 2 - 6
            // *********************************************
            // * Create second_Moment array				   *
            // *********************************************
            List<double> secondMoment = new List<double>();
            double level_CurrHighest = level_High;
            double level_NextHighest = 0;
            double currentSum = 0;
            //find the second moment for level_High
            //for every pixel in the grid, check if intesity = current intensity level, 
            //find next highest intensity in grid
            for (int i = 0; i < vals.Length; i++){
                if (vals[i] == level_CurrHighest){
                    currentSum += Math.Sqrt(dist[i]); //sum the squares of the distances of the pixel from the center
                }
                if (vals[i] < level_CurrHighest && vals[i] > level_NextHighest){
                    level_NextHighest = vals[i]; // next highest intensity in grid, used for iterations
                }
            }
            level_CurrHighest = level_NextHighest; //set for next iteration
            secondMoment.Add(currentSum); //add currentSum to the secondMoment array

            //find second moment for all remaining levels
            count = 1; //keep track of level
            while (level_CurrHighest >= level_Low && count<1000){
                currentSum = 0;
                level_NextHighest = 0;
                //for every pixel in the grid, check if intesity = current intensity level, find next highest intensity in grid
                for (int i = 0; i < vals.Length; i++){
                    if (vals[i] == level_CurrHighest)
                        currentSum += Math.Sqrt(dist[i]); //sum the squares of the distances of the pixel from the center
                    if (vals[i] < level_CurrHighest && vals[i] > level_NextHighest)
                        level_NextHighest = vals[i]; //next highest intensity in grid, used for iterations
                }
                currentSum += secondMoment[count-1];
                secondMoment.Add(currentSum);
                count++;
                level_CurrHighest = level_NextHighest;
            }

            //step 7
            // *********************************************
            // * Create filtered_secondMoment array	 	   *
            // *********************************************
            double[] filtered_secondMoment = new double[secondMoment.Count];
            //for each value in second moment array, compute a normalized moving 5 point center weighted filter 
            //(weights of 1,2,3,2,1, Normalization factor = 9)
            //does not compute the first two or last two values of second moment
            for (int i = 2; i < secondMoment.Count - 2; i++){
                double filter_Val = (secondMoment[i - 2] + secondMoment[i - 1] * 2 + secondMoment[i] * 3 + secondMoment[i + 1] * 2 + secondMoment[i + 2]) / 9;
                filtered_secondMoment[i] = filter_Val;
            }
            //save the first two and last two values of second moment directly to filtered_secondMoment
            filtered_secondMoment[0] = secondMoment[0];
            filtered_secondMoment[1] = secondMoment[1];
            filtered_secondMoment[secondMoment.Count - 2] = secondMoment[secondMoment.Count - 2];
            filtered_secondMoment[secondMoment.Count - 1] = secondMoment[secondMoment.Count - 1];

            //step 9
            // *****************************
            // * Find the Threshold	 	*
            // *****************************
            int threshInt = 0; //holds the threshold pixel
            bool check = true;
            int level = filtered_secondMoment.Length - 3; //set search index to the level_Low and begin search there
            while (check){
                //if the difference between level and lower level is < 100, threshold has been found.
                //Of cource we have to be sure that threshInt<SIZE*SIZE (remember val[SIZE*SIZE] and if
                //thresInt > SIZE*SIZE this will give an error after the while loop in 
                //  double threshold = vals[threshInt];
                //So level should be less than SIZE*SIZE apart from checkVal < 100
                double checkVal = Math.Abs(filtered_secondMoment[level] - filtered_secondMoment[level - 1]);
                if (checkVal < 100 && level < SIZE * SIZE){
                    threshInt = level;
                    check = false;
                }
                level--;
                if (level <= 1){
                    MessageBox.Show("Threshold could not be found.\nPlease ask programer to be more creative!",
                                    "Centroid calculation",
                                    MessageBoxButtons.OK, 
                                    MessageBoxIcon.Warning);
                    //Console.Out.WriteLine("Threshold could not be found.\nPlease ask programer to be more creative!");
                    check = false;
                }
            }

            //step 10
            //set threshold value
            double threshold = vals[threshInt];

            //step 11 & sum of all intensities, xSum, ySum
            // *************************************
            // * Threshold & Calculate Centroid	   *
            // *************************************
            double sum = 0;  //sum of all of the intensities in the grid
            double xSum = 0; //sum of the product of the intensity of a pixel times it's x-distance from the center of the grid
            double ySum = 0; //sum of the product of the intensity of a pixel times it's y-distance from the center of the grid
            //threshold data and calculate the sums
            for (int i = 0; i < vals.Length; i++){
                if (vals[i] <= threshold){
                    vals[i] = 0; //if pix intensity <= thres, set to 0
                }else{
                    vals[i] = vals[i] - threshold; //if pix intensity > thres, calc subtraction
                }
                sum += vals[i];             // sum of intensities
                xSum += vals[i] * distX[i]; // sum of intensities * distance X
                ySum += vals[i] * distY[i]; // sum of intensities * distance Y
            }
            
            //set the numbers' format
            string shorten = "{0:F1}";

            //calculate centroid
            double x_Centroid = xSum / sum;
            double y_Centroid = ySum / sum;
            double endX = X + x_Centroid;
            double endY = Y + y_Centroid;
            STAR_COORD star_cen = new STAR_COORD
            {
                X = endX,
                Y = endY,
                val = pix[X, Y],
                //create information text
                text = ""
            };
            double psf = grepnova2.Align.seeing_star_psf(img, X, Y);
            string psf_s = "N/A";
            if (psf < 1000) psf_s = psf.ToString("#0.000");

            //set text in html format
            star_cen.text += "<font face='Calibri' size='2' color='blue'/><b>Centroid Data Input:</b><br>";
            star_cen.text += "<font color='grey'/>X Sum: <b>" + "<font color = 'black'/>" + String.Format(shorten, xSum) + "</b><br>";
            star_cen.text += "<font color='grey'/>Y Sum: <b>" + "<font color = 'black'/>" + String.Format(shorten, ySum) + "</b><br>";
            star_cen.text += "<font color='grey'/>  Sum: <b>" + "<font color = 'black'/>" + String.Format(shorten, sum) + "</b><br><br>";
            star_cen.text += "<font color='blue'/><b>Centroid Data Output:</b><br>";
            star_cen.text += "<font color='grey'/>X Centroid: <b>" + "<font color = 'black'/>" + String.Format(shorten, x_Centroid) + "</b><br>";
            star_cen.text += "<font color='grey'/>Y Centroid: <b>" + "<font color = 'black'/>" + String.Format(shorten, y_Centroid) + "</b><br><br>";
            star_cen.text += "<font color='blue'/><b>Highest Intensity Pixel:</b><br>";
            star_cen.text += "<font color='grey'/>X: <b>" + "<font color = 'black'/>" + X + "</b><br>";
            star_cen.text += "<font color='grey'/>Y: <b>" + "<font color = 'black'/>" + Y + "</b><br>";
            star_cen.text += "<font color='grey'/>Value: <b>" + "<font color = 'black'/>" + max + "</b><br><br>";
            star_cen.text += "<font color='blue'/><b>Point Spread Functon:</b><br>";
            star_cen.text += "<font color='grey'/>PSF Value: <b>" + "<font color = 'black'/>" + psf_s + "</b><br>";
            myText = star_cen.text;

            return star_cen;
        }
    }
}
