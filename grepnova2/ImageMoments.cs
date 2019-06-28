using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    /// <summary>
    /// A class that calculates the Image Moment, which is a weighted average of image pixel intensities.
    /// This class calculates Raw Moments, Central Moments, Normalized Central Moments and Hu Moments as well
    /// as the distance between two images. 
    /// Main idea from SATYA MALLICK in Learn OpenCV (see at https://www.learnopencv.com/shape-matching-using-hu-moments-c-python/)
    /// </summary>
    class ImageMoments
    {
        public double[,] A;
        public double[,] B;

        public enum matchType
        {
            CONTOURS_MATCH_I1,
            CONTOURS_MATCH_I2,
            CONTOURS_MATCH_I3,
            CONTOURS_MATCH_EUCLIDEAN
        }

        /// <summary>
        /// Constructor of ImageMoments class based on two 2D arrays of double
        /// </summary>
        /// <param name="a">First 2D array</param>
        /// <param name="b">Second 2D array</param>
        public ImageMoments(double[,] a, double[,] b) {
            this.A = a;
            this.B = b;
        }

        /// <summary>
        /// Constructor of ImageMoments class based on two Fits.imageptr structures
        /// </summary>
        /// <param name="a">First Fits.imageptr structure</param>
        /// <param name="b">Second Fits.imageptr structure</param>
        public ImageMoments(Fits.imageptr a, Fits.imageptr b) {
            this.A = convertFitsForMoment(a);
            this.B = convertFitsForMoment(b);
        }

        /// <summary>
        /// Gets an imageptr structure and calculates a 2D array of doubles 
        /// </summary>
        /// <param name="p">the Fits.imageptr</param>
        /// <returns></returns>
        private double[,] convertFitsForMoment(Fits.imageptr p) {
            int sizex = p.xsize;
            int sizey = p.ysize;
            double[,] img = new double[sizex, sizey];
            for (int y = 0; y < sizey; y++)
                for (int x= 0; x < sizex; x++)
                    img[x, y] = p.data[x + y * sizex];
            return img;
        }

        /// <summary>
        /// Calculates the wighted mean X and Y of an image based on Raw Moment of a 2D array
        /// </summary>
        /// <param name="m">the 2D array of doubles</param>
        /// <returns></returns>
        public double[] calcMomentMean(double[,] m) {
            double[] mean = new double[2];//0=x 1=y
            double m00 = calcRawMoment(m, 0, 0);
            double m10 = calcRawMoment(m, 1, 0);
            double m01 = calcRawMoment(m, 0, 1);
            mean[0] = m10 / m00;
            mean[1] = m01 / m00;
            return mean;
        }

        /// <summary>
        /// Calculates the Raw Moment of a 2D array
        /// </summary>
        /// <param name="m">the 2D array of doubles</param>
        /// <returns></returns>
        public double calcRawMoment(double[,] m, int i, int j) {
            double res = 0;
            int xmax=m.GetLength(0);
            int ymax = m.GetLength(1);
            for(int x = 0; x < xmax; x++)
                for (int y = 0; y < ymax; y++)
                    res += Math.Pow(x, i) * Math.Pow(y, j) * m[x, y];
            return res;
        }

        /// <summary>
        /// Calculates the Central Moment of a 2D array, given the orders of i and j
        /// </summary>
        /// <param name="m">the 2D array of doubles</param>
        /// <param name="i">the first order of Central Moment</param>
        /// <param name="j">the second order of Central Moment</param>
        /// <returns></returns>
        public double calcCentralMoment(double[,] m, int i, int j) {
            double res = 0;
            double[] mean = calcMomentMean(m);
            double xmean = mean[0];
            double ymean = mean[1];
            int xmax = m.GetLength(0);
            int ymax = m.GetLength(1);
            for (int x = 0; x < xmax; x++)
                for (int y = 0; y < ymax; y++)
                    res += Math.Pow(x-xmean, i) * Math.Pow(y-ymean, j) * m[x, y];
            return res;
        }

        /// <summary>
        /// Calculates theNormalized Central Moment of a 2D array, given the orders of i and j
        /// </summary>
        /// <param name="m">the 2D array of doubles</param>
        /// <param name="i">the first order of Central Moment</param>
        /// <param name="j">the second order of Central Moment</param>
        /// <returns></returns>
        public double calcNormCentralMoment(double[,] m, int i, int j) {
            double m00 = calcCentralMoment(m, 0, 0);
            double mij = calcCentralMoment(m, i, j);
            double ret = mij / Math.Pow(mij, (i + j) / 2 + 1);
            return ret;
        }

        /// <summary>
        /// Calculates theNormalized Central Moment of a 2D array, given the orders of i and j
        /// and the Raw Moment m00
        /// </summary>
        /// <param name="m">the 2D array of doubles</param>
        /// <param name="m00">the Raw Moment m00</param>
        /// <param name="i">the first order of Central Moment</param>
        /// <param name="j">the second order of Central Moment</param>
        /// <returns></returns>
        public double calcNormCentralMoment(double[,] m, double m00, int i, int j) {
            double mij = calcCentralMoment(m, i, j);
            double ret = mij / Math.Pow(m00, (i + j) / 2 + 1);
            return ret;
        }

        /// <summary>
        /// Calculates the Hu Moment of a 2D array
        /// </summary>
        /// <param name="m">the 2D array of doubles</param>
        /// <returns></returns>
        public double[] calcInvariantMoments(double[,] m) {
            double[] hu = new double[7];
            double m00 = calcCentralMoment(m, 0, 0);
            double n20 = calcNormCentralMoment(m, m00, 2, 0);
            double n02 = calcNormCentralMoment(m, m00, 0, 2);
            double n21 = calcNormCentralMoment(m, m00, 2, 1);
            double n11 = calcNormCentralMoment(m, m00, 1, 1);
            double n12 = calcNormCentralMoment(m, m00, 1, 2);
            double n30 = calcNormCentralMoment(m, m00, 3, 0);
            double n03 = calcNormCentralMoment(m, m00, 0, 3);

            hu[0] = n20 + n02;
            hu[1] = Math.Pow(n20 - n02, 2) + 4 * Math.Pow(n11, 2);
            hu[2] = Math.Pow(n30 - 3 * n12, 2) + Math.Pow(3 * n21 - n03, 2);
            hu[3] = Math.Pow(n30 + n12, 2) + Math.Pow(n21 + n03, 2);
            hu[4] = (n30 - 3 * n12) * (n30 + n12) * (Math.Pow(n20 + n12, 2) - 3 * Math.Pow(n21 - n03, 2)) 
                    + (3 * n21 - n03) * (3 * Math.Pow(n30 + n12, 2) - Math.Pow(n21 + n03, 2));
            hu[5] = (n20 - n02) * (Math.Pow(n30 + n12, 2) + Math.Pow(n21 + n03, 2)) 
                    + 4 * n11 * (n30 + n12) * (n21 + n03);/////////////
            hu[6] = (3 * n21 - n03) * (n30 + n21) * (Math.Pow(n30 + n12, 2) - 3 * Math.Pow(n21 + n03, 2))
                    - (n30 - 3 * n12) * (n21 * n03) * (3 * Math.Pow(n30 + n12, 2) - Math.Pow(n21 + n03, 2));
            return hu;
        }

        /// <summary>
        /// Calculates the Distance of two Images given as 2D arrays of double. There are four types of
        /// calculation: CONTOURS_MATCH_I1, CONTOURS_MATCH_I2, CONTOURS_MATCH_I3, CONTOURS_MATCH_EUCLIDEAN
        /// based on different formulas ("https://www.researchgate.net/publication/224146066_Analysis_of_Hu's_moment_invariants_on_image_scaling_and_rotation")
        /// </summary>
        /// <param name="m">the 2D array of doubles</param>
        /// <returns></returns>
        public double calcImageDistance(double[,] mA, double[,] mB, matchType cmt) {
            double distance = 0;
            double[] hA = calcInvariantMoments(mA);
            double[] hB = calcInvariantMoments(mB);
            double[] HA = new double[7];
            double[] HB = new double[7];
            for(int i = 0; i < 7; i++){
                HA[i] = -Math.Sign(hA[i]) * Math.Log(Math.Abs(hA[i]));
                HB[i] = -Math.Sign(hB[i]) * Math.Log(Math.Abs(hB[i]));
            }
            switch (cmt)
            {
                case matchType.CONTOURS_MATCH_I1:
                    for (int i = 0; i < 7; i++) distance += (1 / HB[i] - 1 / HA[i]);
                    break;
                case matchType.CONTOURS_MATCH_I2:
                    for (int i = 0; i < 7; i++) distance += (HB[i] - HA[i]);
                    break;
                case matchType.CONTOURS_MATCH_I3:
                    for (int i = 0; i < 7; i++) distance += (Math.Abs(HA[i]-HB[i])/Math.Abs(HA[i]));
                    break;
                case matchType.CONTOURS_MATCH_EUCLIDEAN:
                    for (int i = 0; i < 7; i++) distance += Math.Pow(HB[i] - HA[i], 2);
                    distance = Math.Sqrt(distance);
                    break;
            }
            return distance;
        }
    }
}
