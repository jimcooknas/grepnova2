using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
//using Emgu.CV;
//using Emgu.CV.CvEnum;
//using Emgu.CV.Features2D;
//using Emgu.CV.Flann;
//using Emgu.CV.Structure;
//using Emgu.CV.Util;

namespace grepnova2
{
#pragma warning disable IDE1006 // Naming Styles
    class DrawMatches
    {
        /*
        public static void FindMatch(Mat modelImage, Mat observedImage, out long matchTime, 
            out VectorOfKeyPoint modelKeyPoints, out VectorOfKeyPoint observedKeyPoints, 
            VectorOfVectorOfDMatch matches, out Mat mask, out Mat homography, out long score)
        {
            int k = 2;
            double uniquenessThreshold = 0.80;
            Stopwatch watch;
            homography = null;
            modelKeyPoints = new VectorOfKeyPoint();
            observedKeyPoints = new VectorOfKeyPoint();
            using (UMat uModelImage = modelImage.GetUMat(AccessType.Read))
            using (UMat uObservedImage = observedImage.GetUMat(AccessType.Read))
            {
                KAZE featureDetector = new KAZE();
                Mat modelDescriptors = new Mat();
                featureDetector.DetectAndCompute(uModelImage, null, modelKeyPoints, modelDescriptors, false);
                watch = Stopwatch.StartNew();
                Mat observedDescriptors = new Mat();
                featureDetector.DetectAndCompute(uObservedImage, null, observedKeyPoints, observedDescriptors, false);
                // KdTree for faster results / less accuracy
                using (var ip = new Emgu.CV.Flann.LinearIndexParams())// KdTreeIndexParams())
                using (var sp = new SearchParams())
                using (DescriptorMatcher matcher = new FlannBasedMatcher(ip, sp))
                {
                    matcher.Add(modelDescriptors);
                    matcher.KnnMatch(observedDescriptors, matches, k, null);
                    mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                    mask.SetTo(new MCvScalar(255));
                    Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);
                    // Calculate score based on matches size
                    // ---------------------------------------------->
                    score = 0;
                    for (int i = 0; i < matches.Size; i++)
                    {
                        if (mask.GetData(i)[0] == 0) continue;
                        foreach (var e in matches[i].ToArray())
                            ++score;
                    }
                    // <----------------------------------------------
                    int nonZeroCount = CvInvoke.CountNonZero(mask);
                    if (nonZeroCount >= 4)
                    {
                        nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints, matches, mask, 1.5, 20);
                        if (nonZeroCount >= 4)
                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints, observedKeyPoints, matches, mask, 2);
                    }
                }
                watch.Stop();
            }
            matchTime = watch.ElapsedMilliseconds;
        }


        /// <summary>
        /// Draw the model image and observed image, the matched features and homography projection.
        /// </summary>
        /// <param name="modelImage">The model image</param>
        /// <param name="observedImage">The observed image</param>
        /// <param name="matchTime">The output total time for computing the homography matrix.</param>
        /// <param name="score">The output score for computing the homography matrix.</param>
        /// <returns>The model image and observed image, the matched features and homography projection.</returns>
        public static Mat Draw(Mat modelImage, Mat observedImage, out long matchTime, out long score)
        {
            Mat homography;
            
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
            {
                Mat mask;
                FindMatch(modelImage, observedImage, out matchTime, out modelKeyPoints, out observedKeyPoints, matches,
                out mask, out homography, out score);
                Console.Out.WriteLine("Homography: size={0} Matrix dimensions= {1} x {2}", homography.ElementSize, homography.Rows, homography.Cols);
                for(int r=0;r < homography.Rows;r++)
                        Console.Out.WriteLine("Row {0}: {1} {2} {3}", r, BitConverter.ToDouble(homography.GetData(r, 0),0), BitConverter.ToDouble(homography.GetData(r, 1), 0), BitConverter.ToDouble(homography.GetData(r, 2), 0));
                //Draw the matched keypoints
                Mat result = new Mat();
                Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints,
                matches, result, new MCvScalar(255, 255, 255), new MCvScalar(255, 255, 255), mask);
                #region draw the projected region on the image\

                if (homography != null)
                {
                    //draw a rectangle along the projected model
                    Rectangle rect = new Rectangle(Point.Empty, modelImage.Size);
                    PointF[] pts = new PointF[]
                    {
                         new PointF(rect.Left, rect.Bottom),
                         new PointF(rect.Right, rect.Bottom),
                         new PointF(rect.Right, rect.Top),
                         new PointF(rect.Left, rect.Top)
                    };
                    pts = CvInvoke.PerspectiveTransform(pts, homography);
    #if NETFX_CORE
                    Point[] points = Extensions.ConvertAll<PointF, Point>(pts, Point.Round);
    #else
                    Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
    #endif
                    using (VectorOfPoint vp = new VectorOfPoint(points))
                    {
                        CvInvoke.Polylines(result, vp, true, new MCvScalar(255, 0, 0, 255), 5);
                    }
                }
                #endregion
                return result;
            }
        }*/
    }
}
