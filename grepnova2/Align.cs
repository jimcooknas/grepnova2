using Accord.Math.Optimization;
using System;
using System.Collections.Generic;

namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    /// <summary>
    /// Class is needed for 'source_extract' function, to be used by the 'find brightest' in Form1
    /// and in new Grepnova2 Alignment method
    /// </summary>
    public unsafe class Align {
        const double SNEFIND_MINIMUM_PSF = 0.80;
        const double SNEFIND_STAR_EXCLUSION_RADIUS = 3.0;
        public static bool glob_killdeviant = false;

        public static int NULLVAL = Fits.NULLVAL;
        public static double MIN_MULTIPLIER = 1.01;
        //static bool SOURCE_LIST = true;
        public static bool STAR_VERBOSE = false;
        static int DISTANCE_MATCH_TOLERANCE = 1;
        static double M_PI = Math.PI;
        static double MAGNIFICATION_TOLERANCE = 1.01;
        public const int PATTERN_THETA_INTERVALS = 32; /* Number of bins into which we place triplets */
        static int SEEING_MAXSTARS = 500;
        static int opt_iterations = 0;
        static int MAX_ITERATIONS = 1000;

        /* threading structure for images */
        public struct threadParams {
            public string file_in1;
            public string file_in2;
            public Fits.imageptr output;
            public int[] status_in;
        }

        /* Variable format used to store lists of sources */
        public struct starlist {
            public int list_size;
            public int[] x_pos;
            public int[] y_pos;
            public int[] intensity;
        }
#pragma warning disable IDE1006 // Naming Styles

        /* Variable format used to store triplets of sources */

        public struct tripletlist {
            public int triplets_total;                     /* Total number of triplets in catalogue */
            public int[] triplets;//= new int[PATTERN_THETA_INTERVALS]; /* We bin triplets according to theta to save comparison time */
            public double[][] ratio;//[PATTERN_THETA_INTERVALS]; /* Ratio of second longest side to third */
            public double[][] Sratio;//[PATTERN_THETA_INTERVALS]; /* Error in this ratio */
            public double[][] theta;//[PATTERN_THETA_INTERVALS]; /* Angle in triplet, opposite hypotenuse */
            public double[][] Stheta;//[PATTERN_THETA_INTERVALS]; /* Error in this angle */
            public int[][] src_id_A;//[PATTERN_THETA_INTERVALS]; /* ID of source opposite hypotenuse */
            public int[][] src_id_B;//[PATTERN_THETA_INTERVALS]; /* ID of second other source, brightest other source */
            public int[][] src_id_C;//[PATTERN_THETA_INTERVALS]; /* ID of third other source */
        }

        /* Variable format used to store matches between triplets of sources in two images */
        public struct tripletmatches {
            public int list_size;
            public int[] triplet_bin; /* Remember triplet catalogues binned by theta -- both partners in same bin */
            public int[] tripletT_id; /* This is a simple catalogue of the identities of the paired triplets */
            public int[] tripletS_id;
        }

        /* Variable format used to store state vector in likelihood space */
        public struct state_vector {
            public double x_offset;
            public double y_offset;
            public double theta;
            public double background;
            public double brightness;
            public double noise;
            public double convolution;
            public int SNe_ENABLE;
            public double SNe_x;
            public double SNe_y;
            public double SNe_brightness;
            public double SNe_size;
        }

        public struct coordinate {
            public int x;
            public int y;
            public int value;
        }

        /* Variable format used to return relative alignment of images as estimated by pattern recognition */
        public struct proposed_alignment {
            public double x;
            public double y;
            public double theta;
            public double scale;
        }

        public struct point_w_err {
            public double x;
            public double x_error;
        }

        //////////////////////////////////////////////////////////////////////////////////
        ///       starlist functions
        /////////////////////////////////////////////////////////////////////////////////

        public static starlist source_extract(grepnova2.Fits.imageptr image, int number) {
            starlist output;// = new starlist();// malloc(sizeof(Fits.starlist));
            int x, y;
            int significance;
            int i, ld;
            int extracted = 0;
            int* image_scan; // = new int[image.ysize][];//int[]
            fixed (int* tmp = image.data)
            {
                output.list_size = number;
                output.x_pos = new int[number];// (int*)malloc(number * sizeof(int));
                output.y_pos = new int[number];//(int*)malloc(number * sizeof(int));
                output.intensity = new int[number];//(int*)malloc(number * sizeof(int));

                for (i = 0; i < number; i++)   /* Zero new starlist */
                {
                    output.x_pos[i] = 0;
                    output.y_pos[i] = 0;
                    output.intensity[i] = 0;
                }

                ld = image.data_lower_decile;

                for (y = 3; y < ((image.ysize) - 3); y++)
                {
                    image_scan = (tmp + y * image.xsize + 3);
                    for (x = 3; x < ((image.xsize) - 3); x++)
                    {     /* Point source is brighter than all neighbours. Not next to any NULL pixel. */
                        if ((*image_scan > *(image_scan + 1 + 0 * image.xsize)) && (NULLVAL != *(image_scan + 1 + 0 * image.xsize)) &&
                            (*image_scan > *(image_scan + 1 + 1 * image.xsize)) && (NULLVAL != *(image_scan + 1 + 1 * image.xsize)) &&
                            (*image_scan > *(image_scan + 0 + 1 * image.xsize)) && (NULLVAL != *(image_scan + 0 + 1 * image.xsize)) &&
                            (*image_scan > *(image_scan - 1 + 1 * image.xsize)) && (NULLVAL != *(image_scan - 1 + 1 * image.xsize)) &&
                            (*image_scan > *(image_scan - 1 + 0 * image.xsize)) && (NULLVAL != *(image_scan - 1 + 0 * image.xsize)) &&
                            (*image_scan > *(image_scan - 1 - 1 * image.xsize)) && (NULLVAL != *(image_scan - 1 - 1 * image.xsize)) &&
                            (*image_scan > *(image_scan + 0 - 1 * image.xsize)) && (NULLVAL != *(image_scan + 0 - 1 * image.xsize)) &&
                            (*image_scan > *(image_scan + 1 - 1 * image.xsize)) && (NULLVAL != *(image_scan + 1 - 1 * image.xsize)))
                        {
                            significance = 0; /* And much brighter than anything within three squares */

                            if ((*image_scan - ld) > ((*(image_scan + 2 + 2 * image.xsize) - ld) * MIN_MULTIPLIER)) significance++;
                            if ((*image_scan - ld) > ((*(image_scan + 3 + 1 * image.xsize) - ld) * MIN_MULTIPLIER)) significance++;
                            if ((*image_scan - ld) > ((*(image_scan + 3 + 0 * image.xsize) - ld) * MIN_MULTIPLIER)) significance++;
                            if ((*image_scan - ld) > ((*(image_scan + 3 + 1 * image.xsize) - ld) * MIN_MULTIPLIER)) significance++;
                            if ((*image_scan - ld) > ((*(image_scan + 2 + 2 * image.xsize) - ld) * MIN_MULTIPLIER)) significance++;
                            if ((*image_scan - ld) > ((*(image_scan + 1 + 3 * image.xsize) - ld) * MIN_MULTIPLIER)) significance++;
                            if ((*image_scan - ld) > ((*(image_scan + 0 + 3 * image.xsize) - ld) * MIN_MULTIPLIER)) significance++;
                            if ((*image_scan - ld) > ((*(image_scan + 1 + 3 * image.xsize) - ld) * MIN_MULTIPLIER)) significance++;
                            if ((*image_scan - ld) > ((*(image_scan + 2 + 2 * image.xsize) - ld) * MIN_MULTIPLIER)) significance++;
                            if ((*image_scan - ld) > ((*(image_scan + 3 + 1 * image.xsize) - ld) * MIN_MULTIPLIER)) significance++;
                            if ((*image_scan - ld) > ((*(image_scan + 3 + 0 * image.xsize) - ld) * MIN_MULTIPLIER)) significance++;
                            if ((*image_scan - ld) > ((*(image_scan + 3 + 1 * image.xsize) - ld) * MIN_MULTIPLIER)) significance++;
                            if ((*image_scan - ld) > ((*(image_scan + 2 + 2 * image.xsize) - ld) * MIN_MULTIPLIER)) significance++;
                            if ((*image_scan - ld) > ((*(image_scan + 1 + 3 * image.xsize) - ld) * MIN_MULTIPLIER)) significance++;
                            if ((*image_scan - ld) > ((*(image_scan + 0 + 3 * image.xsize) - ld) * MIN_MULTIPLIER)) significance++;
                            if ((*image_scan - ld) > ((*(image_scan + 1 + 3 * image.xsize) - ld) * MIN_MULTIPLIER)) significance++;

                            if (significance > 8) /* More than half of squares within three pixels significantly fainter */
                            {
                                //double dX = image.xsize;
                                //double dY = image.ysize;
                                //for (int ii = 0; ii < output.list_size; ii++){
                                //    dX = Math.Min(dX, Math.Abs(output.x_pos[ii] - x));
                                //    dY = Math.Min(dY, Math.Abs(output.y_pos[ii] - y));
                                //}
                                //if (Math.Sqrt(dX * dX + dY * dY) > 10) { 
                                    starlist_addstar(output, x, y, *image_scan);
                                    extracted++;
                                //}
                            }
                        }
                        image_scan++;
                    }
                }
            }
            if (extracted < number) output.list_size = extracted;
            return (output);
        }

        /* STARLIST_ADDSTAR(): Add star to a starlist */
        public static void starlist_addstar(starlist list, int x, int y, int intensity) {
            int i = 0;
            int j = (list.list_size - 1); /* Last entry in list */

            while ((i < list.list_size) && (intensity < list.intensity[i])) i++;

            if (i == list.list_size) return; /* Star is too faint to be one of brightest */

            while (j > i) /* Shunt lower part of list down a place */
            {
                list.intensity[j] = list.intensity[j - 1];
                list.x_pos[j] = list.x_pos[j - 1];
                list.y_pos[j] = list.y_pos[j - 1];
                j--;
            }

            list.intensity[i] = intensity;
            list.x_pos[i] = x;
            list.y_pos[i] = y;

            return;
        }

        /* SOURCE_FIND_NEAREST(): Find nearest source to point (x,y) on list of stars */
        static double source_find_nearest(starlist list, int x, int y) {
            int i;
            double dist;
            double min_dist = 1e9;

            for (i = 0; i < list.list_size; i++)
            {
                dist = Math.Sqrt(Math.Pow(x - list.x_pos[i], 2) + Math.Pow(y - list.y_pos[i], 2));
                if (dist < min_dist) min_dist = dist;
            }

            return (min_dist);
        }

        /* SOURCE_TRIPLETS(): From a list of sources, construct a list of triplets of sources for pattern recognition */
        /*                    Use first (max) sources on list - Zero means use whole list, but as this scales as N^3, be very careful with this! */

        public static tripletlist source_triplets(Fits.imageptr image, starlist list, int max, int* status) {
            tripletlist output;// = malloc(sizeof(tripletlist));
            int max_triplets;
            int i, j, k, cpy, bin_no = 0;
            double[] dist = new double[3];
            double theta = 0, Stheta = 0, ratio = 0, Sratio = 0;

            if (max <= 0) max = list.list_size; // max=0 means use all stars 
            if (list.list_size < max) max = list.list_size; // If max is more than whole list, downsize 

            if ((list.list_size < 3) || (max < 3))
            {
                //Form1.LogEntry("Error: Less than three stars could be identified in the image, so pattern recognition is not possible. Could not match images together.\n");
                *status = 1;
                output = starlist_triplets_null();
                return (output);
            }

            max_triplets = max * (max - 1) * (max - 2) + 1;

            output = starlist_triplets_new(PATTERN_THETA_INTERVALS);
            output.triplets_total = 0;

            for (i = 0; i < PATTERN_THETA_INTERVALS; i++)
            {
                output.triplets[i] = 0;
                output.ratio[i] = new double[max_triplets];// (double*)malloc(max_triplets * sizeof(double));
                output.Sratio[i] = new double[max_triplets];//(double*)malloc(max_triplets * sizeof(double));
                output.theta[i] = new double[max_triplets];//(double*)malloc(max_triplets * sizeof(double));
                output.Stheta[i] = new double[max_triplets];//(double*)malloc(max_triplets * sizeof(double));
                output.src_id_A[i] = new int[max_triplets];//(int*)malloc(max_triplets * sizeof(int)); // This is star opposite hypotenuse 
                output.src_id_B[i] = new int[max_triplets];//(int*)malloc(max_triplets * sizeof(int));
                output.src_id_C[i] = new int[max_triplets];//(int*)malloc(max_triplets * sizeof(int));
            }

            for (i = 0; i < max; i++) for (j = i + 1; j < max; j++) for (k = j + 1; k < max; k++)
                    {
                        dist[0] = Math.Sqrt(Math.Pow((double)(list.x_pos[i] - list.x_pos[j]), 2.0) + Math.Pow((double)(list.y_pos[i] - list.y_pos[j]), 2.0));
                        dist[1] = Math.Sqrt(Math.Pow((double)(list.x_pos[j] - list.x_pos[k]), 2.0) + Math.Pow((double)(list.y_pos[j] - list.y_pos[k]), 2.0));
                        dist[2] = Math.Sqrt(Math.Pow((double)(list.x_pos[i] - list.x_pos[k]), 2.0) + Math.Pow((double)(list.y_pos[i] - list.y_pos[k]), 2.0));

                        // Three possibilities, as we want to take the angle theta opposite the hypotenuse 

                        if ((dist[0] >= dist[1]) && (dist[0] >= dist[2]))
                        {
                            theta = Math.Atan2(list.x_pos[i] - list.x_pos[k], list.y_pos[i] - list.y_pos[k]) - Math.Atan2(list.x_pos[j] - list.x_pos[k], list.y_pos[j] - list.y_pos[k]);
                            Stheta = Math.Sqrt(Math.Pow(Math.Atan2(DISTANCE_MATCH_TOLERANCE, dist[1]), 2) + Math.Pow(Math.Atan2(DISTANCE_MATCH_TOLERANCE, dist[2]), 2));
                            ratio = dist[1] / dist[2]; if (ratio < 1) ratio = (1 / ratio);
                            Sratio = Math.Sqrt(Math.Pow(ratio, 2) * (Math.Pow(1 / dist[1], 2) + Math.Pow(1 / dist[2], 2)));

                            while (theta < 0) theta += 2 * M_PI;
                            while (theta >= (2 * M_PI)) theta -= 2 * M_PI;

                            bin_no = (int)Math.Floor(theta / (2 * M_PI) * PATTERN_THETA_INTERVALS);

                            (output.src_id_A[bin_no][output.triplets[bin_no]]) = k;
                            (output.src_id_B[bin_no][output.triplets[bin_no]]) = i;
                            (output.src_id_C[bin_no][output.triplets[bin_no]]) = j;
                        }

                        if ((dist[1] >= dist[0]) && (dist[1] >= dist[2]))
                        {
                            theta = Math.Atan2(list.x_pos[j] - list.x_pos[i], list.y_pos[j] - list.y_pos[i]) - Math.Atan2(list.x_pos[k] - list.x_pos[i], list.y_pos[k] - list.y_pos[i]);
                            Stheta = Math.Sqrt(Math.Pow(Math.Atan2(DISTANCE_MATCH_TOLERANCE, dist[0]), 2) + Math.Pow(Math.Atan2(DISTANCE_MATCH_TOLERANCE, dist[2]), 2));
                            ratio = dist[0] / dist[2]; if (ratio < 1) ratio = (1 / ratio);
                            Sratio = Math.Sqrt(Math.Pow(ratio, 2) * (Math.Pow(1 / dist[0], 2) + Math.Pow(1 / dist[2], 2)));

                            while (theta < 0) theta += 2 * M_PI;
                            while (theta >= (2 * M_PI)) theta -= 2 * M_PI;

                            bin_no = (int)Math.Floor(theta / (2 * M_PI) * PATTERN_THETA_INTERVALS);

                            (output.src_id_A[bin_no][output.triplets[bin_no]]) = i;
                            (output.src_id_B[bin_no][output.triplets[bin_no]]) = j;
                            (output.src_id_C[bin_no][output.triplets[bin_no]]) = k;
                        }

                        if ((dist[2] >= dist[0]) && (dist[2] >= dist[1]))
                        {
                            theta = Math.Atan2(list.x_pos[i] - list.x_pos[j], list.y_pos[i] - list.y_pos[j]) - Math.Atan2(list.x_pos[k] - list.x_pos[j], list.y_pos[k] - list.y_pos[j]);
                            Stheta = Math.Sqrt(Math.Pow(Math.Atan2(DISTANCE_MATCH_TOLERANCE, dist[0]), 2) + Math.Pow(Math.Atan2(DISTANCE_MATCH_TOLERANCE, dist[1]), 2));
                            ratio = dist[0] / dist[1]; if (ratio < 1) ratio = (1 / ratio);
                            Sratio = Math.Sqrt(Math.Pow(ratio, 2) * (Math.Pow(1 / dist[0], 2) + Math.Pow(1 / dist[1], 2)));

                            while (theta < 0) theta += 2 * M_PI;
                            while (theta >= (2 * M_PI)) theta -= 2 * M_PI;

                            bin_no = (int)Math.Floor(theta / (2 * M_PI) * PATTERN_THETA_INTERVALS);

                            (output.src_id_A[bin_no][output.triplets[bin_no]]) = j;
                            (output.src_id_B[bin_no][output.triplets[bin_no]]) = i;
                            (output.src_id_C[bin_no][output.triplets[bin_no]]) = k;
                        }

                (output.ratio[bin_no][output.triplets[bin_no]]) = ratio;
                        (output.Sratio[bin_no][output.triplets[bin_no]]) = Sratio;
                        (output.theta[bin_no][output.triplets[bin_no]]) = theta;
                        (output.Stheta[bin_no][output.triplets[bin_no]]) = Stheta;

                        // Check which star is brighter, B or C? Swap as necessary so that B is brighter than C to give consistent ordering 

                        if (list.intensity[(output.src_id_C[bin_no][output.triplets[bin_no]])] >
                            list.intensity[(output.src_id_B[bin_no][output.triplets[bin_no]])])
                        {
                            cpy = (output.src_id_C[bin_no][output.triplets[bin_no]]);
                            (output.src_id_C[bin_no][output.triplets[bin_no]]) = (output.src_id_B[bin_no][output.triplets[bin_no]]);
                            (output.src_id_B[bin_no][output.triplets[bin_no]]) = cpy;
                        }

                        // printf("Triplet: {%2d,%2d,%2d}, Ratio = %5f, Error = %5e, Theta = %5f, Error = %5e.\n", *(output->src_id_A[bin_no] + output->triplets[bin_no]), *(output->src_id_B[bin_no] + output->triplets[bin_no]), *(output->src_id_C[bin_no] + output->triplets[bin_no]), *(output->ratio   [bin_no] + output->triplets[bin_no]), *(output->Sratio  [bin_no] + output->triplets[bin_no]), *(output->theta   [bin_no] + output->triplets[bin_no]), *(output->Stheta  [bin_no] + output->triplets[bin_no]) ); 
                        output.triplets[bin_no]++;
                        output.triplets_total++;
                    }
            return (output);
        }

        /* MATCH_TRIPLETS(): Compare two lists of triplets sources, proposing rotations/translations to make similar triplets coincide on frame */

        public static tripletmatches match_triplets(tripletlist T, tripletlist S, starlist Ts, starlist Ss, int* status) {
            tripletmatches output;// = malloc(sizeof(tripletmatches));
            int max_matches = S.triplets_total;
            int i, j, k;
            int matches, firstmatch = 0;
            int triplets_matched;
            double S_ratio, S_Sratio, S_theta, S_Stheta;
            double magnification;
            double sqdist;

            output.triplet_bin = new int[max_matches];// (int*)malloc(max_matches * sizeof(int));
            output.tripletT_id = new int[max_matches];//(int*)malloc(max_matches * sizeof(int));
            output.tripletS_id = new int[max_matches];//(int*)malloc(max_matches * sizeof(int));

            triplets_matched = 0;

            for (i = 0; i < PATTERN_THETA_INTERVALS; i++)
                for (j = 0; j < S.triplets[i]; j++)
                {
                    S_ratio = (S.ratio[i][j]);
                    S_Sratio = (S.Sratio[i][j]);
                    S_theta = (S.theta[i][j]);
                    S_Stheta = (S.Stheta[i][j]);

                    matches = 0;

                    for (k = 0; k < T.triplets[i]; k++)
                    {
                        sqdist = Math.Pow((T.ratio[i][k]) - S_ratio, 2) / (Math.Pow(S_Sratio, 2) + Math.Pow((T.Sratio[i][k]), 2)) +
                                 Math.Pow((T.theta[i][k]) - S_theta, 2) / (Math.Pow(S_Stheta, 2) + Math.Pow((T.Stheta[i][k]), 2)); // Distance in terms of tolerances 

                        if (sqdist < 1)
                        {
                            matches++;
                            firstmatch = k;
                        }
                    }

                    if (matches == 1)
                    {
                        // We have found a single match for a similar triplet in T to one we have in S 
                        // NB: Below magnification formula assumes B is the same star in both triplets... 
                        magnification = Math.Sqrt(Math.Pow((Ss.x_pos[(S.src_id_A[i][j])] - Ss.x_pos[(S.src_id_B[i][j])]), 2) +
                                             Math.Pow((Ss.y_pos[(S.src_id_A[i][j])] - Ss.y_pos[(S.src_id_B[i][j])]), 2)) /
                                        Math.Sqrt(Math.Pow((Ts.x_pos[(T.src_id_A[i][firstmatch])] - Ts.x_pos[(T.src_id_B[i][firstmatch])]), 2) +
                                             Math.Pow((Ts.y_pos[(T.src_id_A[i][firstmatch])] - Ts.y_pos[(T.src_id_B[i][firstmatch])]), 2));

                        if ((magnification > (1 / MAGNIFICATION_TOLERANCE)) && (magnification < MAGNIFICATION_TOLERANCE))
                        {
                            output.triplet_bin[triplets_matched] = i;          // Remember, triplets binning according to theta 
                            output.tripletT_id[triplets_matched] = firstmatch;
                            output.tripletS_id[triplets_matched++] = j;

                            if (STAR_VERBOSE == true) { string logline = String.Format("Pattern recognition: Triplet match found - subject triplet [{0:D3},{1:D3},{2:D3}] (r={3:F5} +- {4:F5}; t={5:F5} +- {6:F5}) - template triplet [{7:D3},{8:D3},{9:D3}] (r={10:F5} +- {11:F5}; t={12:F5} +- {13:F5}) Magnification = {14:F3}\n", (S.src_id_A[i][j]), (S.src_id_B[i][j]), (S.src_id_C[i][j]), (S.ratio[i][j]), (S.Sratio[i][j]), (S.theta[i][j]), (S.Stheta[i][j]), (T.src_id_A[i][firstmatch]), (T.src_id_B[i][firstmatch]), (T.src_id_C[i][firstmatch]), (T.ratio[i][firstmatch]), (T.Sratio[i][firstmatch]), (T.theta[i][firstmatch]), (T.Stheta[i][firstmatch]), magnification); Console.Out.WriteLine(logline); }
                        }
                    }
                }

            output.list_size = triplets_matched;
            if (STAR_VERBOSE == true) { string logline = String.Format("Matching of star triplets complete: {0} star matches found.\n", triplets_matched); Console.Out.WriteLine(logline); }

            if (triplets_matched == 0)
            {
                //Form1.LogEntry("Error: Attempted pattern recognition to match two images together, but no match was found. Maybe images aren't of the same object or of the same size? \n");
                output = matches_null();
                *status = 1;
                return (output);
            }

            return (output);
        }

        /* STARLIST_FREE(): Free up starlist structure */

        starlist starlist_null() {
            starlist output;
            output.x_pos = null;
            output.y_pos = null;
            output.intensity = null;
            output.list_size = 0;
            return (output);
        }

        /* STARLIST_TRIPLETS_FREE(): Free up tripletlist structure */
        /* return a nullified triplelist */

        public static tripletlist starlist_triplets_null() {
#pragma warning disable IDE0017 // Simplify object initialization
            tripletlist output = new tripletlist();
            output.triplets_total = 0;
            output.ratio = null;
            output.triplets = null;
            output.ratio = null;
            output.Sratio = null;
            output.theta = null;
            output.Stheta = null;
            output.src_id_A = null;
            output.src_id_B = null;
            output.src_id_C = null;

            return (output);
        }

        /* creates a new tripletlist with given maximum number of size (usually PATTERN_THETA_INTERVALS) */

        public static tripletlist starlist_triplets_new(int m) {
            tripletlist output;
            output.triplets_total = 0;
            output.triplets = new int[m];
            output.ratio = new double[m][];
            output.Sratio = new double[m][];
            output.theta = new double[m][];
            output.Stheta = new double[m][];
            output.src_id_A = new int[m][];  // This is star opposite hypotenuse 
            output.src_id_B = new int[m][];
            output.src_id_C = new int[m][];
            return (output);
        }

        /* MATCHES_FREE(): Free up distmatches structure */
        static tripletmatches matches_null() {
            tripletmatches output;
            output.triplet_bin = null;
            output.tripletT_id = null;
            output.tripletS_id = null;
            output.list_size = 0;
            return (output);

        }

        /////////////////////////////////////////////////////////////////////
        /////     seeing function
        /////////////////////////////////////////////////////////////////////

        /* SEEING_ESTIMATE(): Estimate PSF of image from the Gaussian FWHM of stars */
        /*                    Use first (max) stars on list. max=0 means use whole list. */

        static int GET_PIXEL(Fits.imageptr image, starlist list, int A, int B, int i) {
            fixed (int* data = image.data)
            {
                return (*(data + (list.x_pos[i] + (A)) + (list.y_pos[i] + (B)) * image.xsize) - image.data_lower_decile);
            }
        }

        public static double seeing_estimate(Fits.imageptr image, starlist list, int max) {
            int i, j;
            int x0, x1, x2;
            int minimum_j = 0;
            double minimum_val;
            double[] seeing_est = new double[10 * SEEING_MAXSTARS];
            int count = 0;

            if ((max > list.list_size) || (max <= 0)) max = list.list_size;

            for (i = 0; i < max; i++)
            {
                x0 = GET_PIXEL(image, list, -1, 0, i); x1 = GET_PIXEL(image, list, 0, 0, i); x2 = GET_PIXEL(image, list, 1, 0, i); /* Estimate PSF of star along x-axis */
                seeing_est[count++] = seeing_evaluate(x0, x1, x2, 1.0);

                x0 = GET_PIXEL(image, list, 0, -1, i); x2 = GET_PIXEL(image, list, 0, 1, i); seeing_est[count++] = seeing_evaluate(x0, x1, x2, 1.0); /* Estimate PSF of star along y-axis */
                x0 = GET_PIXEL(image, list, -1, -1, i); x2 = GET_PIXEL(image, list, 1, 1, i); seeing_est[count++] = seeing_evaluate(x0, x1, x2, Math.Sqrt(2)); /* Estimate PSF of star along diagonal */
                x0 = GET_PIXEL(image, list, 1, -1, i); x2 = GET_PIXEL(image, list, -1, 1, i); seeing_est[count++] = seeing_evaluate(x0, x1, x2, Math.Sqrt(2)); /* Estimate PSF of star along diagonal */
                x0 = GET_PIXEL(image, list, -2, 0, i); x2 = GET_PIXEL(image, list, 2, 0, i); seeing_est[count++] = seeing_evaluate(x0, x1, x2, 2.0); /* Estimate PSF of star along x-axis */
                x0 = GET_PIXEL(image, list, 0, -2, i); x2 = GET_PIXEL(image, list, 0, 2, i); seeing_est[count++] = seeing_evaluate(x0, x1, x2, 2.0); /* Estimate PSF of star along y-axis */
                x0 = GET_PIXEL(image, list, 2, -1, i); x2 = GET_PIXEL(image, list, -2, 1, i); seeing_est[count++] = seeing_evaluate(x0, x1, x2, Math.Sqrt(5)); /* Estimate PSF of star along diagonal */
                x0 = GET_PIXEL(image, list, 1, -2, i); x2 = GET_PIXEL(image, list, -1, 2, i); seeing_est[count++] = seeing_evaluate(x0, x1, x2, Math.Sqrt(5)); /* Estimate PSF of star along diagonal */
                x0 = GET_PIXEL(image, list, 2, 1, i); x2 = GET_PIXEL(image, list, -2, -1, i); seeing_est[count++] = seeing_evaluate(x0, x1, x2, Math.Sqrt(5)); /* Estimate PSF of star along diagonal */
                x0 = GET_PIXEL(image, list, 1, 2, i); x2 = GET_PIXEL(image, list, -1, -2, i); seeing_est[count++] = seeing_evaluate(x0, x1, x2, Math.Sqrt(5)); /* Estimate PSF of star along diagonal */
            }

            for (i = 0; i < (count / 2); i++)  /* Find median seeing estimate */
            {
                if (i != 0) seeing_est[minimum_j] = 9999; /* Delete minimum value, but miss initialisation loop */
                minimum_j = 0;
                minimum_val = 9999;
                for (j = 0; j < count; j++) if (seeing_est[j] < minimum_val) { minimum_val = seeing_est[j]; minimum_j = j; }
            }

            return (seeing_est[minimum_j]);
        }

        static int GET_PIXEL(Fits.imageptr image, int x, int y, int A, int B) {
            fixed (int* data = image.data)
            {
                return (*(data + (x + (A)) + (y + (B)) * image.xsize) - image.data_lower_decile);
            }
        }

        /* SEEING_STAR_PSF(): Estimate PSF surrounding a single star */
        public static double seeing_star_psf(Fits.imageptr image, int x, int y) {
            int i, j;
            int x0, x1, x2;
            int minimum_j = 0;
            double minimum_val;
            double[] seeing_est = new double[10];
            int count = 0;

            x0 = GET_PIXEL(image, x, y, -1, 0); x1 = GET_PIXEL(image, x, y, 0, 0); x2 = GET_PIXEL(image, x, y, 1, 0); /* Estimate PSF of star along x-axis */
            seeing_est[count++] = seeing_evaluate(x0, x1, x2, 1.0);

            x0 = GET_PIXEL(image, x, y, 0, -1); x2 = GET_PIXEL(image, x, y, 0, 1); seeing_est[count++] = seeing_evaluate(x0, x1, x2, 1.0); /* Estimate PSF of star along y-axis */
            x0 = GET_PIXEL(image, x, y, -1, -1); x2 = GET_PIXEL(image, x, y, 1, 1); seeing_est[count++] = seeing_evaluate(x0, x1, x2, Math.Sqrt(2)); /* Estimate PSF of star along diagonal */
            x0 = GET_PIXEL(image, x, y, 1, -1); x2 = GET_PIXEL(image, x, y, -1, 1); seeing_est[count++] = seeing_evaluate(x0, x1, x2, Math.Sqrt(2)); /* Estimate PSF of star along diagonal */
            x0 = GET_PIXEL(image, x, y, -2, 0); x2 = GET_PIXEL(image, x, y, 2, 0); seeing_est[count++] = seeing_evaluate(x0, x1, x2, 2.0); /* Estimate PSF of star along x-axis */
            x0 = GET_PIXEL(image, x, y, 0, -2); x2 = GET_PIXEL(image, x, y, 0, 2); seeing_est[count++] = seeing_evaluate(x0, x1, x2, 2.0); /* Estimate PSF of star along y-axis */
            x0 = GET_PIXEL(image, x, y, 2, -1); x2 = GET_PIXEL(image, x, y, -2, 1); seeing_est[count++] = seeing_evaluate(x0, x1, x2, Math.Sqrt(5)); /* Estimate PSF of star along diagonal */
            x0 = GET_PIXEL(image, x, y, 1, -2); x2 = GET_PIXEL(image, x, y, -1, 2); seeing_est[count++] = seeing_evaluate(x0, x1, x2, Math.Sqrt(5)); /* Estimate PSF of star along diagonal */
            x0 = GET_PIXEL(image, x, y, 2, 1); x2 = GET_PIXEL(image, x, y, -2, -1); seeing_est[count++] = seeing_evaluate(x0, x1, x2, Math.Sqrt(5)); /* Estimate PSF of star along diagonal */
            x0 = GET_PIXEL(image, x, y, 1, 2); x2 = GET_PIXEL(image, x, y, -1, -2); seeing_est[count++] = seeing_evaluate(x0, x1, x2, Math.Sqrt(5)); /* Estimate PSF of star along diagonal */

            /* printf(" { "); for (i=0; i<count; i++) printf("%4f ",seeing_est[i]); printf("} "); */

            for (i = 0; i < (count / 2); i++)  /* Find median seeing estimate */
            {
                if (i != 0) seeing_est[minimum_j] = 9999; /* Delete minimum value, but don't need to first time around loop */
                minimum_j = 0;
                minimum_val = 9999;
                for (j = 0; j < count; j++) if (seeing_est[j] < minimum_val) { minimum_val = seeing_est[j]; minimum_j = j; }
            }

            return (seeing_est[minimum_j]);
        }

        /* SEEING_EVALUATE(): Using three data values for neighbouring pixels, estimate width of PSF */
        /*                    This is known as Laplace's Method */

        public static double seeing_evaluate(double x0, double x1, double x2, double sep) {
            double log_x0 = Math.Log(x0);
            double log_x1 = Math.Log(x1);
            double log_x2 = Math.Log(x2);

            double d2_log = (log_x0 - 2 * log_x1 + log_x2) / Math.Pow(sep, 2);

            if ((x0 == NULLVAL) || (x1 == NULLVAL) || (x2 == NULLVAL)) return (1e-99); /* One of the points is a NULLVALL */

            if ((x0 <= 0) || (x1 <= 0) || (x2 <= 0)) return (1001);
            //if (d2_log != d2_log) return (1001); /* One of the points is <=0, so log not real. Assume big PSF */

            if (d2_log >= 0) return (1000); /* Unexpected behaviour. Star has positive second differential. Assume big PSF */

            return (1 / Math.Sqrt(-d2_log));
        }

        ////////////////////////////////////////////////////////////////////
        ///   alignment functions
        ////////////////////////////////////////////////////////////////////

        static double STEPSIZE_X = 20;//1.0;//20.0;
        static double STEPSIZE_Y = 20;//1.0;//20.0;
        static double STEPSIZE_THETA = 0.01;//1.0;//0.01;
        static double STEPSIZE_SCALE = 0.1;//1.0;//0.1;

        //scaling
        public static double TW = 765;//width of template
        public static double TH = 510;//heigh of template
        public static double SW = 765;//width of subject
        public static double SH = 510;//height of subject
        public static double TS = 765;//Max of TW, TH
        public static double SS = 510;//Max of SW, SH

        /* Catalogue of coordinate matches - stored for quick lookup */
        static int catalogue_size;    /* Number of matches in catalogue */
        static double[] catalogue_Tx;    /* Template image - X pos of star */
        static double[] catalogue_Ty;
        static double[] catalogue_Ti;
        static double[] catalogue_Sx;    /* Subject image  - X pos of star */
        static double[] catalogue_Sy;
        static double[] catalogue_Si;
        static int[] catalogue_flag;  /* We flag stars matches we now think dubious */

        public struct NelderMead_struct{
            public double reflectionParam;//reflexion = 1
            public double expansionParam;//expansion = 2
            public double contractionParam;//contraction = 0.5
            public double shrinkParam;//shrink = 0.5
            public double tolerance;//1e-12
        }

        public static NelderMead_struct initializeNelderMead(double refl=1, double expa=2, double contr=0.5, double shri=0.5, double tole = 1e-12) {
            NelderMead_struct nm = new NelderMead_struct();
            nm.reflectionParam = refl;
            nm.expansionParam = expa;
            nm.contractionParam = contr;
            nm.shrinkParam = shri;
            nm.tolerance = tole;
            return nm;
        }


        /// <summary>
        /// Go through catalogue with proposed x,y,theta combination, seeing which star is worst fit
        /// This star is chucked out of catalogue. If all stars are within three times the mean misfit, 
        /// then we're done. (from 'grepnova' of D. Ford) 
        /// </summary>
        /// <param name="x">The proposed X displacement</param>
        /// <param name="y">The proposed Y displacement</param>
        /// <param name="theta">The proposed theta rotation angle (in radians)</param>
        /// <param name="sca">The proposed scale</param>
        /// <returns></returns>
        static int alignment_killmostdeviantstar(double x, double y, double theta, double sca=1)
        {
            double largest_distance = -1;
            double sum_distance = 0;
            int n = 0;
            int largest_id = -1;
            int i;
            double distance;
            double cost = Math.Cos(theta);
            double sint = Math.Sin(theta);

            for (i = 0; i < catalogue_size; i++)
                if ((catalogue_flag[i]) == 0){
                    /*
                    int newTX = (int)((2 * catalogue_Tx[i] - TW) / TS);
                    int newTY = (int)((2 * catalogue_Ty[i] - TH) / TS);
                    int newSX = (int)((2 * catalogue_Sx[i] - SW) / SS);
                    int newSY = (int)((2 * catalogue_Sy[i] - SH) / SS);
                    double newx = (2 * x - TW) / TS;
                    double newy = (2 * y - TH) / TS;
                    distance = Math.Sqrt(Math.Pow(newTX - (sca * newSX * cost + sca * newSY * sint - newx), 2) +
                                         Math.Pow(newTY - (sca * newSX * -sint + sca * newSY * cost - newy), 2));
                    */
                    distance = Math.Sqrt(Math.Pow(catalogue_Tx[i] - (sca * catalogue_Sx[i] * cost + sca*catalogue_Sy[i] * sint - x), 2) +
                                         Math.Pow(catalogue_Ty[i] - (sca * catalogue_Sx[i] * -sint + sca*catalogue_Sy[i] * cost - y), 2));

                    sum_distance += distance;
                    if (distance > largest_distance) { largest_distance = distance; largest_id = i; };
                    n += 1;
                }

            if (largest_distance <= (3 * sum_distance / n)){
                largest_id = -1;
            }else{
                if (largest_id > -1) catalogue_flag[largest_id] = 1;
            }
            return (largest_id);
        }

        /// <summary> 
        /// Sums over all stars in catalogue, applying rotation/translation to template position, and comparing to subject position
        /// (from 'grepnova' of D. Ford)
        /// </summary>
        /// <param name="input">doubles x, y and theta (translation and rotation)</param>
        /// <returns>The value calculated by the function</returns>
        public static double alignment_measurefit(double[] input) {
            double sumsquares;
            double x, y, theta, sca;
            double cost, sint;
            int i, j = 0;

            sumsquares = 0;
            x = input[0] * Align.STEPSIZE_X;// Normalisation factors to make GSL stepsizes sensible 
            y = input[1] * Align.STEPSIZE_Y;
            theta = input[2] * Align.STEPSIZE_THETA;
            sca = input[3] * Align.STEPSIZE_SCALE;
            cost = Math.Cos(theta);
            sint = Math.Sin(theta);
            for (i = 0; i < catalogue_size; i++) if ((catalogue_flag[i]) == 0) {
                    /*
                    int newTX = (int)((2 * catalogue_Tx[i] - TW) / TS);
                    int newTY = (int)((2 * catalogue_Ty[i] - TH) / TS);
                    int newSX = (int)((2 * catalogue_Sx[i] - SW) / SS);
                    int newSY = (int)((2 * catalogue_Sy[i] - SH) / SS);
                    double newx = (2 * x - TW) / TS;
                    double newy = (2 * y - TH) / TS;
                    sumsquares += (Math.Pow(newTX - (sca * newSX * cost + sca * newSY * sint - newx), 2) +
                              (Math.Pow(newTY - (sca * newSX * -sint + sca * newSY * cost - newy), 2)));
                    */
                    sumsquares += (Math.Pow(catalogue_Tx[i] - (sca * catalogue_Sx[i] * cost + sca * catalogue_Sy[i] * sint - x), 2) +
                              (Math.Pow(catalogue_Ty[i] - (sca * catalogue_Sx[i] * -sint + sca * catalogue_Sy[i] * cost - y), 2)));

                    j++;
            }
            if (STAR_VERBOSE == true) { string logline = String.Format("{0:D3} {1:D5} {2:D5} {3:F7} {4:F7}\n", j, (int)x, (int)y, theta, sca); Console.Out.WriteLine(logline); }
            opt_iterations++;
            if (glob_killdeviant) alignment_killmostdeviantstar(x, y, theta, sca);
            return (sumsquares);
        }

        // version 1.3.1 @ 20190422: for testing accord_fit_neldermead
        public static double alignment_measurefit2(double[] input) {
            double sumsquares;
            double x, y, theta, sca;
            double cost, sint;
            int i, j = 0;

            sumsquares = 0;
            x = input[0] * Align.STEPSIZE_X;// Normalisation factors to make GSL stepsizes sensible 
            y = input[1] * Align.STEPSIZE_Y;
            theta = input[2] * Align.STEPSIZE_THETA;
            sca = input[3] * Align.STEPSIZE_SCALE;
            cost = Math.Cos(theta);
            sint = Math.Sin(theta);
            for (i = 0; i < catalogue_size; i++) if ((catalogue_flag[i]) == 0)
                {
                    
                    int newTX = (int)((TS * catalogue_Tx[i] + TW) / 2);
                    int newTY = (int)((TS * catalogue_Ty[i] + TH) / 2);
                    int newSX = (int)((SS * catalogue_Sx[i] + SW) / 2);
                    int newSY = (int)((SS * catalogue_Sy[i] + SH) / 2);
                    double newx = (TS * x + TW) / 2;
                    double newy = (TS * y + TH) / 2;
                    sumsquares += (Math.Pow(newTX - (sca * newSX * cost + sca * newSY * sint - newx), 2) +
                              (Math.Pow(newTY - (sca * newSX * -sint + sca * newSY * cost - newy), 2)));
                    
                    //sumsquares += (Math.Pow(catalogue_Tx[i] - (sca * catalogue_Sx[i] * cost + sca * catalogue_Sy[i] * sint - x), 2) +
                    //          (Math.Pow(catalogue_Ty[i] - (sca * catalogue_Sx[i] * -sint + sca * catalogue_Sy[i] * cost - y), 2)));

                    j++;
                }
            if (STAR_VERBOSE == true) { string logline = String.Format("{0:D3} {1:D5} {2:D5} {3:F7} {4:F7}\n", j, (int)x, (int)y, theta, sca); Console.Out.WriteLine(logline); }
            opt_iterations++;
            if (glob_killdeviant) alignment_killmostdeviantstar(x, y, theta, sca);
            return (sumsquares);
        }

        //version 1.2.0   @ 20190317
        /// <summary>
        /// Takes a list of matched triplets between two images, and tries to find a rotation/translation 
        /// which brings these triplets to coincide, using an internal Nelder-Mead argorithm.
        /// Nelder-Mead minimiser is applied to the summing over all matched stars the square distance 
        /// between the position of that star in the two images, given proposed re-alignment. 
        /// As some matches may well be erroneous, and dominate the sum of squares, this is performed 
        /// repeated, each time discarding the worst fitting match. 
        /// The process ends when all stars are within three times the average deviation
        /// </summary>
        /// <param name="matches">Tripletsmatches structure with matched triplets</param>
        /// <param name="T">tripletlist structure for the triplets of Template</param>
        /// <param name="S">tripletlist structure for the triplets of Subject</param>
        /// <param name="Ts">starlist structure for the stars of Template</param>
        /// <param name="Ss">starlist structure for the stars of Subject</param>
        /// <param name="prop_align">the proposed_alignment structure to start minimization</param>
        /// <returns>A proposed_alignment structure containing the found translation/rotation of alignment</returns>
        public static proposed_alignment alignment_fit(tripletmatches matches, tripletlist T, tripletlist S, starlist Ts, starlist Ss, proposed_alignment prop_align) {
            //initialize structures
            proposed_alignment output = new proposed_alignment();
            int status = 0;
            alignment_init(matches, &status);
            //initial guess is that two images are identical
            output.x = prop_align.x;
            output.y = prop_align.y;
            output.theta = prop_align.theta;
            output.scale = prop_align.scale;
            int j = 0;
            for (int i = 0; i < matches.list_size; i++)
            {  // Each triplet contains three matched stars 
                (catalogue_Tx[j]) = Ts.x_pos[(T.src_id_A[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ty[j]) = Ts.y_pos[(T.src_id_A[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ti[j]) = Ts.intensity[(T.src_id_A[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Sx[j]) = Ss.x_pos[(S.src_id_A[matches.triplet_bin[i]][matches.tripletS_id[i]])];
                (catalogue_Sy[j]) = Ss.y_pos[(S.src_id_A[matches.triplet_bin[i]][matches.tripletS_id[i]])];
                (catalogue_Si[j]) = Ss.intensity[(S.src_id_A[matches.triplet_bin[i]][matches.tripletS_id[i]])]; j++;

                (catalogue_Tx[j]) = Ts.x_pos[(T.src_id_B[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ty[j]) = Ts.y_pos[(T.src_id_B[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ti[j]) = Ts.intensity[(T.src_id_B[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Sx[j]) = Ss.x_pos[(S.src_id_B[matches.triplet_bin[i]][matches.tripletS_id[i]])];
                (catalogue_Sy[j]) = Ss.y_pos[(S.src_id_B[matches.triplet_bin[i]][ matches.tripletS_id[i]])];
                (catalogue_Si[j]) = Ss.intensity[(S.src_id_B[matches.triplet_bin[i]][matches.tripletS_id[i]])]; j++;

                (catalogue_Tx[j]) = Ts.x_pos[(T.src_id_C[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ty[j]) = Ts.y_pos[(T.src_id_C[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ti[j]) = Ts.intensity[(T.src_id_C[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Sx[j]) = Ss.x_pos[(S.src_id_C[matches.triplet_bin[i]][matches.tripletS_id[i]])];
                (catalogue_Sy[j]) = Ss.y_pos[(S.src_id_C[matches.triplet_bin[i]][matches.tripletS_id[i]])];
                (catalogue_Si[j]) = Ss.intensity[(S.src_id_C[matches.triplet_bin[i]][matches.tripletS_id[i]])]; j++;
            }
            opt_iterations = 0;
            //optimize in do-while loop
            double[] opt = new double[] { output.x / STEPSIZE_X, output.y / STEPSIZE_Y, output.theta / STEPSIZE_THETA, output.scale / STEPSIZE_SCALE };
            do {
                opt = minimizerNelderMead(alignment_measurefit, 4, opt, initializeNelderMead());
            } while ((alignment_killmostdeviantstar(opt[0]*STEPSIZE_X, opt[1]*STEPSIZE_Y, opt[2]*STEPSIZE_THETA, opt[3]*STEPSIZE_SCALE) != -1)
                     && (opt_iterations < MAX_ITERATIONS)); // Find worst fitting star 
            output.x = opt[0]*STEPSIZE_X;
            output.y = opt[1]*STEPSIZE_Y;
            output.theta = opt[2]*STEPSIZE_THETA;
            output.scale = opt[3] * STEPSIZE_SCALE;
            string st = String.Format("({0} {1} {2} {3}", output.x, output.y, output.theta*180/Math.PI, output.scale);
            Console.Out.WriteLine("Converged at {0} after {1} itterations", st, opt_iterations);
            return output;
        }

        // version 1.3.1 @ 20190422 change in normalizing coordinates [x'=(2*x-Width)/Max(W,H) x'=(2*y-Height)/Max(W,H) ]
        public static proposed_alignment accord_fit_neldermead2(tripletmatches matches, tripletlist T, tripletlist S, starlist Ts, starlist Ss, proposed_alignment prop_align) {
            //initialize structures
            proposed_alignment output = new proposed_alignment();
            int status = 0;
            alignment_init(matches, &status);
            //initial guess is that two images are identical
            output.x = prop_align.x;
            output.y = prop_align.y;
            output.theta = prop_align.theta;
            output.scale = prop_align.scale;
            int j = 0;
            for (int i = 0; i < matches.list_size; i++)
            {  // Each triplet contains three matched stars 
                (catalogue_Tx[j]) = (2 * Ts.x_pos[(T.src_id_A[matches.triplet_bin[i]][matches.tripletT_id[i]])] - TW) / TS;
                (catalogue_Ty[j]) = (2 * Ts.y_pos[(T.src_id_A[matches.triplet_bin[i]][matches.tripletT_id[i]])] - TH) / TS;
                (catalogue_Ti[j]) = Ts.intensity[(T.src_id_A[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Sx[j]) = (2 * Ss.x_pos[(S.src_id_A[matches.triplet_bin[i]][matches.tripletS_id[i]])] - SW) / SS;
                (catalogue_Sy[j]) = (2 * Ss.y_pos[(S.src_id_A[matches.triplet_bin[i]][matches.tripletS_id[i]])] - SH) / SS;
                (catalogue_Si[j]) = Ss.intensity[(S.src_id_A[matches.triplet_bin[i]][matches.tripletS_id[i]])]; j++;

                (catalogue_Tx[j]) = (2 * Ts.x_pos[(T.src_id_B[matches.triplet_bin[i]][matches.tripletT_id[i]])] - TW) / TS;
                (catalogue_Ty[j]) = (2 * Ts.y_pos[(T.src_id_B[matches.triplet_bin[i]][matches.tripletT_id[i]])] - TH) / TS;
                (catalogue_Ti[j]) = Ts.intensity[(T.src_id_B[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Sx[j]) = (2 * Ss.x_pos[(S.src_id_B[matches.triplet_bin[i]][matches.tripletS_id[i]])] - SW) / SS;
                (catalogue_Sy[j]) = (2 * Ss.y_pos[(S.src_id_B[matches.triplet_bin[i]][matches.tripletS_id[i]])] - SH) / SS;
                (catalogue_Si[j]) = Ss.intensity[(S.src_id_B[matches.triplet_bin[i]][matches.tripletS_id[i]])]; j++;

                (catalogue_Tx[j]) = (2 * Ts.x_pos[(T.src_id_C[matches.triplet_bin[i]][matches.tripletT_id[i]])] - TW) / TS;
                (catalogue_Ty[j]) = (2 * Ts.y_pos[(T.src_id_C[matches.triplet_bin[i]][matches.tripletT_id[i]])] - TH) / TS;
                (catalogue_Ti[j]) = Ts.intensity[(T.src_id_C[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Sx[j]) = (2 * Ss.x_pos[(S.src_id_C[matches.triplet_bin[i]][matches.tripletS_id[i]])] - SW) / SS;
                (catalogue_Sy[j]) = (2 * Ss.y_pos[(S.src_id_C[matches.triplet_bin[i]][matches.tripletS_id[i]])] - SH) / SS;
                (catalogue_Si[j]) = Ss.intensity[(S.src_id_C[matches.triplet_bin[i]][matches.tripletS_id[i]])]; j++;
            }
            opt_iterations = 0;
            glob_killdeviant = true;
            double[] opt = new double[] { output.x / STEPSIZE_X, output.y / STEPSIZE_Y, output.theta / STEPSIZE_THETA, output.scale / STEPSIZE_SCALE };

            var solver = new NelderMead(numberOfVariables: 4)
            {
                Function = alignment_measurefit2
            };
            solver.Convergence.AbsoluteParameterTolerance = new double[] { 1e-9, 1e-9, 1e-12, 1e-12 };
            solver.Convergence.MaximumEvaluations = 3000;

            // Now, we can minimize it with:
            bool success = solver.Minimize(opt);

            // And get the solution vector using
            double[] solution = solver.Solution;

            // The minimum at this location would be:
            double minimum = solver.Value;
            glob_killdeviant = false;
            Console.WriteLine("\nBest solution found: \n");
            output.x = (TS * solution[0] * STEPSIZE_X + TW) / 2;
            output.y = (TS * solution[1] * STEPSIZE_Y + TH) / 2;
            output.theta = solution[2] * STEPSIZE_THETA;
            output.scale = solution[3] * STEPSIZE_SCALE;
            string st = String.Format("({0} {1} {2} {3}", output.x, output.y, output.theta * 180 / Math.PI, output.scale);
            Console.Out.WriteLine("Converged (Nelder-Mead) at {0} after {1} evaluations ({2} itterations)", st, solver.Convergence.Evaluations, opt_iterations);
            return output;
        }

        // version 1.2.0   @ 20190317
        /// <summary>
        /// Takes a list of matched triplets between two images, and tries to find a rotation/translation 
        /// which brings these triplets to coincide, using the Accord.net Nelder-Mead argorithm.
        /// Nelder-Mead minimiser is applied to the summing over all matched stars the square distance 
        /// between the position of that star in the two images, given proposed re-alignment. 
        /// As some matches may well be erroneous, and dominate the sum of squares, this is performed 
        /// repeated, each time discarding the worst fitting match. 
        /// The process ends when all stars are within three times the average deviation
        /// </summary>
        /// <param name="matches">Tripletsmatches structure with matched triplets</param>
        /// <param name="T">tripletlist structure for the triplets of Template</param>
        /// <param name="S">tripletlist structure for the triplets of Subject</param>
        /// <param name="Ts">starlist structure for the stars of Template</param>
        /// <param name="Ss">starlist structure for the stars of Subject</param>
        /// <param name="prop_align">the proposed_alignment structure to start minimization</param>
        /// <returns>A proposed_alignment structure containing the found translation/rotation of alignment</returns>
        public static proposed_alignment accord_fit_neldermead(tripletmatches matches, tripletlist T, tripletlist S, starlist Ts, starlist Ss, proposed_alignment prop_align) {
            //initialize structures
            proposed_alignment output = new proposed_alignment();
            int status = 0;
            alignment_init(matches, &status);
            //initial guess is that two images are identical
            output.x = prop_align.x;
            output.y = prop_align.y;
            output.theta = prop_align.theta;
            output.scale = prop_align.scale;
            int j = 0;
            for (int i = 0; i < matches.list_size; i++)
            {  // Each triplet contains three matched stars 
                (catalogue_Tx[j]) = Ts.x_pos[(T.src_id_A[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ty[j]) = Ts.y_pos[(T.src_id_A[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ti[j]) = Ts.intensity[(T.src_id_A[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Sx[j]) = Ss.x_pos[(S.src_id_A[matches.triplet_bin[i]][matches.tripletS_id[i]])];
                (catalogue_Sy[j]) = Ss.y_pos[(S.src_id_A[matches.triplet_bin[i]][matches.tripletS_id[i]])];
                (catalogue_Si[j]) = Ss.intensity[(S.src_id_A[matches.triplet_bin[i]][matches.tripletS_id[i]])]; j++;

                (catalogue_Tx[j]) = Ts.x_pos[(T.src_id_B[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ty[j]) = Ts.y_pos[(T.src_id_B[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ti[j]) = Ts.intensity[(T.src_id_B[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Sx[j]) = Ss.x_pos[(S.src_id_B[matches.triplet_bin[i]][matches.tripletS_id[i]])];
                (catalogue_Sy[j]) = Ss.y_pos[(S.src_id_B[matches.triplet_bin[i]][matches.tripletS_id[i]])];
                (catalogue_Si[j]) = Ss.intensity[(S.src_id_B[matches.triplet_bin[i]][matches.tripletS_id[i]])]; j++;

                (catalogue_Tx[j]) = Ts.x_pos[(T.src_id_C[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ty[j]) = Ts.y_pos[(T.src_id_C[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ti[j]) = Ts.intensity[(T.src_id_C[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Sx[j]) = Ss.x_pos[(S.src_id_C[matches.triplet_bin[i]][matches.tripletS_id[i]])];
                (catalogue_Sy[j]) = Ss.y_pos[(S.src_id_C[matches.triplet_bin[i]][matches.tripletS_id[i]])];
                (catalogue_Si[j]) = Ss.intensity[(S.src_id_C[matches.triplet_bin[i]][matches.tripletS_id[i]])]; j++;
            }
            opt_iterations = 0;
            glob_killdeviant = true;
            double[] opt = new double[] { output.x / STEPSIZE_X, output.y / STEPSIZE_Y, output.theta / STEPSIZE_THETA, output.scale / STEPSIZE_SCALE };
            
            var solver = new NelderMead(numberOfVariables: 4)
            {
                Function = alignment_measurefit
            };
            solver.Convergence.AbsoluteParameterTolerance = new double[] { 1e-9, 1e-9, 1e-12, 1e-12 };
            solver.Convergence.MaximumEvaluations = 3000;
            
            // Now, we can minimize it with:
            bool success = solver.Minimize(opt);

            // And get the solution vector using
            double[] solution = solver.Solution; 

            // The minimum at this location would be:
            double minimum = solver.Value;
            glob_killdeviant = false;
            Console.WriteLine("\nBest solution found: \n");
            output.x = solution[0] * STEPSIZE_X;
            output.y = solution[1] * STEPSIZE_Y;
            output.theta = solution[2] * STEPSIZE_THETA;
            output.scale = solution[3] * STEPSIZE_SCALE;
            string st = String.Format("({0} {1} {2} {3}", output.x, output.y, output.theta * 180 / Math.PI, output.scale);
            Console.Out.WriteLine("Converged (Nelder-Mead) at {0} after {1} evaluations ({2} itterations)", st, solver.Convergence.Evaluations, opt_iterations);
            return output;
        }

        //version 1.3.0   @ 20190420
        /// <summary>
        /// Takes a list of matched triplets between two images, and tries to find a rotation/translation 
        /// which brings these triplets to coincide, using the Accord.net COBYLA argorithm.
        /// Cobyla minimiser is applied to the summing over all matched stars the square distance 
        /// between the position of that star in the two images, given proposed re-alignment. 
        /// As some matches may well be erroneous, and dominate the sum of squares, this is performed 
        /// repeated, each time discarding the worst fitting match.
        /// Cobyla uses constraint about the scale result (always > 0)
        /// The process ends when all stars are within three times the average deviation
        /// </summary>
        /// <param name="matches">Tripletsmatches structure with matched triplets</param>
        /// <param name="T">tripletlist structure for the triplets of Template</param>
        /// <param name="S">tripletlist structure for the triplets of Subject</param>
        /// <param name="Ts">starlist structure for the stars of Template</param>
        /// <param name="Ss">starlist structure for the stars of Subject</param>
        /// <param name="prop_align">the proposed_alignment structure to start minimization</param>
        /// <returns>A proposed_alignment structure containing the found translation/rotation of alignment</returns>
        public static proposed_alignment accord_fit_cobyla(tripletmatches matches, tripletlist T, tripletlist S, starlist Ts, starlist Ss, proposed_alignment prop_align) {
            //initialize structures
            proposed_alignment output = new proposed_alignment();
            int status = 0;
            alignment_init(matches, &status);
            //initial guess is that two images are identical
            output.x = prop_align.x;
            output.y = prop_align.y;
            output.theta = prop_align.theta;
            output.scale = prop_align.scale;
            int j = 0;
            for (int i = 0; i < matches.list_size; i++)
            {  // Each triplet contains three matched stars 
                (catalogue_Tx[j]) = Ts.x_pos[(T.src_id_A[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ty[j]) = Ts.y_pos[(T.src_id_A[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ti[j]) = Ts.intensity[(T.src_id_A[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Sx[j]) = Ss.x_pos[(S.src_id_A[matches.triplet_bin[i]][matches.tripletS_id[i]])];
                (catalogue_Sy[j]) = Ss.y_pos[(S.src_id_A[matches.triplet_bin[i]][matches.tripletS_id[i]])];
                (catalogue_Si[j]) = Ss.intensity[(S.src_id_A[matches.triplet_bin[i]][matches.tripletS_id[i]])]; j++;

                (catalogue_Tx[j]) = Ts.x_pos[(T.src_id_B[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ty[j]) = Ts.y_pos[(T.src_id_B[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ti[j]) = Ts.intensity[(T.src_id_B[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Sx[j]) = Ss.x_pos[(S.src_id_B[matches.triplet_bin[i]][matches.tripletS_id[i]])];
                (catalogue_Sy[j]) = Ss.y_pos[(S.src_id_B[matches.triplet_bin[i]][matches.tripletS_id[i]])];
                (catalogue_Si[j]) = Ss.intensity[(S.src_id_B[matches.triplet_bin[i]][matches.tripletS_id[i]])]; j++;

                (catalogue_Tx[j]) = Ts.x_pos[(T.src_id_C[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ty[j]) = Ts.y_pos[(T.src_id_C[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Ti[j]) = Ts.intensity[(T.src_id_C[matches.triplet_bin[i]][matches.tripletT_id[i]])];
                (catalogue_Sx[j]) = Ss.x_pos[(S.src_id_C[matches.triplet_bin[i]][matches.tripletS_id[i]])];
                (catalogue_Sy[j]) = Ss.y_pos[(S.src_id_C[matches.triplet_bin[i]][matches.tripletS_id[i]])];
                (catalogue_Si[j]) = Ss.intensity[(S.src_id_C[matches.triplet_bin[i]][matches.tripletS_id[i]])]; j++;
            }
            opt_iterations = 0;
            glob_killdeviant = true;
            double[] opt = new double[] { output.x / STEPSIZE_X, output.y / STEPSIZE_Y, output.theta / STEPSIZE_THETA, output.scale / STEPSIZE_SCALE };
            var constraints = new[]
            {
                new NonlinearConstraint(4, x => x[3] >= 0),
            };
            NonlinearObjectiveFunction func = new NonlinearObjectiveFunction(4, alignment_measurefit);
            var solver = new Cobyla(func, constraints);
            solver.MaxIterations = 3000;
            
            // Now, we can minimize it with:
            bool success = solver.Minimize(opt);

            // And get the solution vector using
            double[] solution = solver.Solution;

            // The minimum at this location would be:
            double minimum = solver.Value;
            glob_killdeviant = false;
            Console.WriteLine("\nBest solution found: \n");
            output.x = solution[0] * STEPSIZE_X;
            output.y = solution[1] * STEPSIZE_Y;
            output.theta = solution[2] * STEPSIZE_THETA;
            output.scale = solution[3] * STEPSIZE_SCALE;
            string st = String.Format("({0} {1} {2} {3}", output.x, output.y, output.theta * 180 / Math.PI, output.scale);
            Console.Out.WriteLine("Converged (Cobyla) at {0} after {1} evaluations ({2} itterations)", st, solver.Iterations, opt_iterations);//solver.Convergence.Evaluations
            return output;
        }

        //version 1.2.0   @ 20190317
        /// <summary>
        /// Internal Nelder-Mead minimization algorithm based on Sachin Joglekar's blog and
        /// JAD (https://codereview.stackexchange.com/questions/176282/nelder-mead-algorithm-in-c)
        /// </summary>
        /// <param name="function">The function which return value we need to minimize</param>
        /// <param name="N">Number of parameters the function uses as input valuws</param>
        /// <param name="init_simplex">Initial values that must be a double[N] array</param>
        /// <param name="nm">NelderMead_struct containing optimization parameters</param>
        /// <returns>The optimized values of the initial array when function reaches its minimum</returns>
        public static double[] minimizerNelderMead(Func<double[], double> function, int N, double[] init_simplex, NelderMead_struct nm) {
            Random rnd = new Random();
            double[][] simplex = new double[N + 1][];

            // Generate N + 1 initial arrays.
            for (int array = 0; array <= N; array++){
                simplex[array] = new double[N];
                for (int index = 0; index < N; index++){
                    simplex[array][index] = rnd.NextDouble() * 200 - 100;
                }
            }
            //set initial values
            simplex[0][0] = init_simplex[0];
            simplex[0][1] = init_simplex[1];
            simplex[0][2] = init_simplex[2];
            simplex[0][3] = init_simplex[3];

            double alpha = 1;// nm.reflectionParam;//reflexion = 1
            double gamma = 2;// nm.expansionParam;//expansion = 2
            double rho = 0.5;// nm.contractionParam;//contraction = 0.5
            double sigma = 0.5; //nm.shrinkParam;//shrink = 0.5
            double tolerance = 1e-12;// nm.tolerance;//tolerance = 1e-12

            // Infinite loop until convergence
            while (true)
            {
                // Evaluation
                double[] functionValues = new double[N + 1];
                int[] indices = new int[N + 1];
                for (int vertex_of_simplex = 0; vertex_of_simplex <= N; vertex_of_simplex++)
                {
                    functionValues[vertex_of_simplex] = function(simplex[vertex_of_simplex]);
                    indices[vertex_of_simplex] = vertex_of_simplex;
                }
                // Order
                Array.Sort(functionValues, indices);
                //Console.Out.WriteLine("Eval Iter {0}: funcval={1} {2} {3} {4}", opt_iterations, functionValues[0], functionValues[1], functionValues[2], functionValues[3]);
                // Check for convergence (old calling point). Moved just before Reflection
                //if (functionValues[N] - functionValues[0] < 1e-9) break;

                // Find centroid of the simplex excluding the vertex with highest functionvalue.
                double[] centroid = new double[N];

                for (int index = 0; index < N; index++)
                {
                    centroid[index] = 0;
                    for (int vertex_of_simplex = 0; vertex_of_simplex <= N; vertex_of_simplex++)
                    {
                        if (vertex_of_simplex != indices[N])
                        {
                            centroid[index] += simplex[vertex_of_simplex][index] / N;
                        }
                    }
                }

                // Check for convergence (new calling point)
                if (Math.Abs(function(centroid) - functionValues[0]) < tolerance) {//1e-12
                    break;
                }

                //Reflection
                double[] reflection_point = new double[N];
                for (int index = 0; index < N; index++)
                {
                    reflection_point[index] = centroid[index] + alpha * (centroid[index] - simplex[indices[N]][index]);
                }

                double reflection_value = function(reflection_point);

                if (reflection_value >= functionValues[0] & reflection_value < functionValues[N - 1])
                {
                    simplex[indices[N]] = reflection_point;
                    continue;
                }
                Console.Out.WriteLine("Refl Iter {0}: funcval={1} {2} {3}", opt_iterations, functionValues[0], functionValues[1], functionValues[2]);//, functionValues[3]);

                // Expansion
                if (reflection_value < functionValues[0])
                {
                    double[] expansion_point = new double[N];
                    for (int index = 0; index < N; index++)
                    {
                        expansion_point[index] = centroid[index] + gamma * (reflection_point[index] - centroid[index]);
                    }
                    double expansion_value = function(expansion_point);

                    if (expansion_value < reflection_value)
                    {
                        simplex[indices[N]] = expansion_point;
                    }
                    else
                    {
                        simplex[indices[N]] = reflection_point;
                    }
                    continue;
                }
                Console.Out.WriteLine("Expa Iter {0}: funcval={1} {2} {3}", opt_iterations, functionValues[0], functionValues[1], functionValues[2]);//, functionValues[3]);

                // Contraction
                double[] contraction_point = new double[N];
                for (int index = 0; index < N; index++)
                {
                    contraction_point[index] = centroid[index] + rho * (simplex[indices[N]][index] - centroid[index]);
                }

                double contraction_value = function(contraction_point);

                if (contraction_value < functionValues[N])
                {
                    simplex[indices[N]] = contraction_point;
                    continue;
                }
                Console.Out.WriteLine("Contr Iter {0}: funcval={1} {2} {3}", opt_iterations, functionValues[0], functionValues[1], functionValues[2]);//, functionValues[3]);

                //Shrink
                double[] best_point = simplex[indices[0]];
                for (int vertex_of_simplex = 0; vertex_of_simplex <= N; vertex_of_simplex++)
                {
                    for (int index = 0; index < N; index++)
                    {

                        simplex[vertex_of_simplex][index] = best_point[index] + sigma * (simplex[vertex_of_simplex][index] - best_point[index]);

                    }
                }
                Console.Out.WriteLine("Shrink Iter {0}: funcval={1} {2} {3}", opt_iterations, functionValues[0], functionValues[1], functionValues[2]);//, functionValues[3]);
            }
            return simplex[0];
        }

        //version 1.2.0   @ 20190317
        /// <summary>
        /// Constructs arrays (and initialize catalogue_flag array) for quick lookup table of matched 
        /// star positions
        /// </summary>
        /// <param name="matches">A tripletmatches structure containing thee matching triplets</param>
        /// <param name="status">An int pointer to keep the status of the function (0=SUCCESS else FAILURE)</param>
        static void alignment_init(tripletmatches matches, int* status)
        {
            int i;
            catalogue_size = 3 * matches.list_size;  // We make a copy of the list of matched star positions for quick lookup 

            if (catalogue_size < 3){
                Console.Out.WriteLine("alignment_fit: No match between two star fields could be found. No triplet matches.\n");
                *status = 1;
                return;
            }

            catalogue_Tx = new double[catalogue_size];// (int*)malloc(catalogue_size * sizeof(int));
            catalogue_Ty = new double[catalogue_size];//(int*)malloc(catalogue_size * sizeof(int));
            catalogue_Ti = new double[catalogue_size];//(int*)malloc(catalogue_size * sizeof(int));
            catalogue_Sx = new double[catalogue_size];//(int*)malloc(catalogue_size * sizeof(int));
            catalogue_Sy = new double[catalogue_size];//(int*)malloc(catalogue_size * sizeof(int));
            catalogue_Si = new double[catalogue_size];//(int*)malloc(catalogue_size * sizeof(int));
            catalogue_flag = new int[catalogue_size];//(int*)malloc(catalogue_size * sizeof(int));

            for (i = 0; i < catalogue_size; i++) catalogue_flag[i] = 0;
        }

        /* ALIGNMENT_POINTSRCRATIO(): Using the matched stars in the image, estimate the ratio of central */
        /* brightnesses of point sources. Exposure estimate */
        /* Warning: Call this ONLY after alignment_fit(), and not after alignment_free(). */

        point_w_err alignment_pointsrcratio()
        {
            int i;
            double ratio_sum = 0;
            double ratio_sum_sq = 0;
            double ratio_points = 0;

            point_w_err output;

            for (i = 0; i < catalogue_size; i++)
                if ((catalogue_flag[i]) == 0)
                {
                    ratio_sum += (double)catalogue_Si[i] / catalogue_Ti[i];
                    ratio_sum_sq += Math.Pow((double)catalogue_Si[i] / catalogue_Ti[i], 2);
                    ratio_points += 1;
                }

            output.x = ratio_sum / ratio_points;
            output.x_error = Math.Sqrt((ratio_sum_sq / ratio_points) - Math.Pow(output.x, 2));

            return (output);
        }


        ////////////////////////////////////////////////////////////////////////////////////////
        //     SNe_Find functions
        /////////////////////////////////////////////////////////////////////////////////////////
        static bool SNEFIND_VERBOSE = true;

        /* FIND_SNE(): Look for peak in image, this is most promising SNe candidate */
        /*             Rejects sources significantly smaller than seeing PSF as probably noise blips */
        /*             Rejects sources close to those on exclude list (normally those in the template image) as pre-existing stars */
        public static coordinate find_SNe(Form1 frm, Fits.imageptr image, int max_candidates, starlist exclude, double seeing) {
            coordinate output;
            starlist candidates;
            int x, y, i, val;

            candidates = source_extract(image, max_candidates); /* Make list of bright sources in difference image */

            output.x = -1;
            output.y = -1;
            output.value = -1;

            i = -1;
            if (SNEFIND_VERBOSE == true) frm.LogEntry("\r\n");
            do {  /* Now work down the list until we find brightest source which isn't a pre-existing star or noise blip */
                i++;
                if (i < candidates.list_size) {
                    x = candidates.x_pos[i];
                    y = candidates.y_pos[i];
                    val = candidates.intensity[i];
                    if (seeing_star_psf(image, x, y) < (SNEFIND_MINIMUM_PSF * seeing)) { /* Noise blip? */
                        if (SNEFIND_VERBOSE == true) { frm.LogEntry(String.Format("Candidate at ({0:D4},{1:D4}) rejected: PSF too small, so probably noise.\n", x, y));}
                        continue;
                    }
                    if (source_find_nearest(exclude, x, y) < SNEFIND_STAR_EXCLUSION_RADIUS) {    /* Close to star in template image? */
                        if (SNEFIND_VERBOSE == true) { frm.LogEntry(String.Format("Candidate at ({0:D4},{1:D4}) rejected: Appears to be present in template image.\n", x, y)); }
                        continue;
                    }
                    if (SNEFIND_VERBOSE == true) { frm.LogEntry(String.Format("Candidate at ({0:D4},{1:D4}) is selected as most likely SNe.\n", x, y)); }
                    output.x = x;
                    output.y = y;
                    output.value = val;
                }
            } while ((i < candidates.list_size) && (output.x == -1));
            //candidates; /* Free up list of candidate SNes */
            return (output);
        }

        /* FIND_SNE_BRIGHTNESS(): Find central excess brightness of chosen SNe candidate */
        double find_SNe_brightness(Fits.imageptr T, Fits.imageptr S, state_vector state, coordinate pos) {
            int S_pixel = S.data[pos.x + pos.y * S.xsize];
            int T_pixel = T.data[pos.x + pos.y * T.xsize];
            double excess = ((double)(S_pixel) - (((double)T_pixel) * state.brightness + state.background));
            return (excess / state.brightness);
        }

    }
    //////////////////////////////////////////////////////////////////////////
    //// Similarity Class
    //////////////////////////////////////////////////////////////////////////

    //version 1.3.0   @ 20190419
    /// <summary>
    /// A new approach with this Similarity class.
    /// With the 20 brightest stars in two images, tries to find out similar triangles.
    /// When similar triangles are found, performs Nelder-Mead optimization to these triangles
    /// to calculate the Translation, Rotation and Scaling of the two images.
    /// Structures: point, triangle, similar_triangle
    /// Functions : CheckSimilarityBySides, TripletSimilarity, MostSimilarTriangles,
    ///             alignment_measurefit, accord_fit
    /// </summary>
    public class Similarity{
        const double DIFF_TOLERANCE = 0.001;

        static double STEPSIZE_X = 20.0;//1;//20.0;
        static double STEPSIZE_Y = 20.0;//1;//20.0;
        static double STEPSIZE_THETA = 0.01;//1;// 0.01;
        static double STEPSIZE_SCALE = 1.0;//1;//0.1;

        static int catalogue_size;
        //public static bool glob_killdeviant = false;
        static int[] catalogue_flag;
        static int opt_iterations = 0;
        static List<similar_triangle> most_similarTri = new List<similar_triangle>();

        public struct point{
            public double x;
            public double y;
            
            public point(double xx, double yy) {
                x = xx;
                y = yy;
            }
        }

        public struct triangle
        {
            public double[] side;
            public point[] po;

            public triangle(point[] p) {
                po = p;
                side = new double[3];
                side[0] = Math.Sqrt(Math.Pow(po[0].x - po[1].x, 2) + Math.Pow(po[0].y - po[1].y, 2));
                side[1] = Math.Sqrt(Math.Pow(po[1].x - po[2].x, 2) + Math.Pow(po[1].y - po[2].y, 2));
                side[2] = Math.Sqrt(Math.Pow(po[2].x - po[0].x, 2) + Math.Pow(po[2].y - po[0].y, 2));
            }
        }

        public struct similar_triangle{
            public triangle triS;
            public triangle triT;
            public double ratio;
        }

        public static double CheckSimilarityBySides(double[] s1, double[] s2) {
            Array.Sort(s1);
            Array.Sort(s2);
            double sim0 = s1[0] / s2[0]; 
            double sim1 = s1[1] / s2[1];
            double sim2 = s1[2] / s2[2];
            if (Math.Abs(sim0-sim1) < DIFF_TOLERANCE
                && Math.Abs(sim1-sim2) < DIFF_TOLERANCE
                && Math.Abs(sim2-sim0) < DIFF_TOLERANCE)
                return (sim0+sim1+sim2)/3;
            else
                return 0;
        }

        public static List<similar_triangle> TripletSimilarity(Align.starlist slT, Align.starlist slS) {
            // constract Template triangles list
            List<triangle> triT = new List<triangle>();
            for (int i=0; i < slT.list_size-2; i++){
                for(int j = i; j < slT.list_size-1; j++){
                    for(int k = j; k < slT.list_size; k++){
                        triangle tri = new triangle(new point[] {new point(slT.x_pos[i], slT.y_pos[i]),
                            new point(slT.x_pos[j], slT.y_pos[j]), new point(slT.x_pos[k], slT.y_pos[k]) });
                        triT.Add(tri);
                    }
                }
            }
            // constract Subject triangles list
            List<triangle> triS = new List<triangle>();
            for (int i = 0; i < slS.list_size - 2; i++){
                for (int j = i; j < slS.list_size - 1; j++){
                    for (int k = j; k < slS.list_size; k++){
                        triangle tri = new triangle(new point[] {new point(slS.x_pos[i], slS.y_pos[i]),
                            new point(slS.x_pos[j], slS.y_pos[j]), new point(slS.x_pos[k], slS.y_pos[k]) });
                        triS.Add(tri);
                    }
                }
            }
            // constract similar triangles list ...
            List<similar_triangle> simTri = new List<similar_triangle>();
            for(int t=0; t < triT.Count; t++){
                for(int s = 0; s < triS.Count; s++){
                    double[] s1 = new double[] { triT[t].side[0], triT[t].side[1], triT[t].side[2] };
                    double[] s2 = new double[] { triS[s].side[0], triS[s].side[1], triS[s].side[2] };
                    double sim = CheckSimilarityBySides(s1, s2);
                    if (sim > 0) {
                        similar_triangle simt = new similar_triangle();
                        simt.triT = triT[t];
                        simt.triS = triS[s];
                        simt.ratio = sim;
                        simTri.Add(simt);
                    }
                }
            }
            // ... find the most similar triangles list ...
            List<similar_triangle> most_simTri = MostSimilarTriangles(simTri);
            catalogue_size = most_simTri.Count;
            catalogue_flag= new int[catalogue_size];
            for (int i = 0; i < catalogue_size; i++) catalogue_flag[i] = 0;
            // ... and return it.
            return most_simTri;
        }


        public static List<similar_triangle> MostSimilarTriangles(List<similar_triangle> sTri) {
            List<similar_triangle> most_sTri = new List<similar_triangle>();
            int[] ratio = new int[sTri.Count];
            for(int i = 0; i < sTri.Count; i++){
                for(int j = 0; j < sTri.Count; j++){
                    if (Math.Abs(sTri[i].ratio - sTri[j].ratio) < DIFF_TOLERANCE) ratio[i]++;
                }
            }
            int max_ratio_bin = 0;
            int max_ratio_id = -1;
            for (int i = 0; i < sTri.Count; i++) {
                if (ratio[i] > max_ratio_bin){
                    max_ratio_bin = ratio[i];
                    max_ratio_id = i;
                }
            }
            if (max_ratio_id > -1){
                for (int i = 0; i < sTri.Count; i++){
                    if (Math.Abs(sTri[i].ratio - sTri[max_ratio_id].ratio) < DIFF_TOLERANCE) most_sTri.Add(sTri[i]);
                }
            }
            return most_sTri;
        }

        public static double alignment_measurefit(double[] input) {
            double sumsquares;
            double x, y, theta, sca;
            double cost, sint;
            int i, j = 0;

            sumsquares = 0;
            x = input[0] * STEPSIZE_X;// Normalisation factors to make GSL stepsizes sensible 
            y = input[1] * STEPSIZE_Y;
            theta = input[2] * STEPSIZE_THETA;
            sca = input[3] * STEPSIZE_SCALE;
            cost = Math.Cos(theta);
            sint = Math.Sin(theta);
            for (i = 0; i < catalogue_size; i++)
                if ((catalogue_flag[i]) == 0){
                    double sumsum = 0;
                    for (int k = 0; k < 3; k++){
                        sumsum += Math.Sqrt((Math.Pow(most_similarTri[i].triT.po[k].x - (sca * most_similarTri[i].triS.po[k].x * cost + sca * most_similarTri[i].triS.po[k].y * sint - x), 2) +
                                  (Math.Pow(most_similarTri[i].triT.po[k].y - (sca * most_similarTri[i].triS.po[k].x * -sint + sca * most_similarTri[i].triS.po[k].y * cost - y), 2))));
                    }
                    sumsquares += sumsum / 3;
                    j++;
                }
            if (Align.STAR_VERBOSE == true) { string logline = String.Format("{0:D3} {1:D5} {2:D5} {3:F7} {4:F7}\n", j, (int)x, (int)y, theta, sca); Console.Out.WriteLine(logline); }
            opt_iterations++;
            //if (glob_killdeviant) alignment_killmostdeviantstar(x, y, theta, sca, simTri);
            return (sumsquares);
        }

        public static Align.proposed_alignment accord_fit(List<similar_triangle> simTri, Align.proposed_alignment prop_align) {
            //initialize structures
            Align.proposed_alignment output = new Align.proposed_alignment();
            //initial guess is that two images are identical
            output.x = prop_align.x;
            output.y = prop_align.y;
            output.theta = prop_align.theta;
            output.scale = prop_align.scale;
            //int j = 0;
            most_similarTri = simTri;
            opt_iterations = 0;
            double[] opt = new double[] { output.x / STEPSIZE_X, output.y / STEPSIZE_Y, output.theta / STEPSIZE_THETA, output.scale / STEPSIZE_SCALE };
            var constraints = new[]
            {
                new NonlinearConstraint(4, x => x[3] - 0.1 >= 0),
            };
            NonlinearObjectiveFunction func = new NonlinearObjectiveFunction(4, alignment_measurefit);
            var solver = new Cobyla(func, constraints);
            solver.MaxIterations = 9000;
            //var solver = new NelderMead(numberOfVariables: 4){
            //    Function = alignment_measurefit
            //};
            //solver.Convergence.AbsoluteParameterTolerance = new double[] { 1e-9, 1e-9, 1e-9, 1e-9 };
            //solver.Convergence.MaximumEvaluations = 3000;
            // Now, we can minimize it with:
            bool success = solver.Minimize(opt);

            // And get the solution vector using
            double[] solution = solver.Solution;

            // The minimum at this location would be:
            double minimum = solver.Value;
            Console.WriteLine("\nSimilarity->Best solution found: \n");
            output.x = solution[0] * STEPSIZE_X;
            output.y = solution[1] * STEPSIZE_Y;
            output.theta = solution[2] * STEPSIZE_THETA;
            output.scale = solution[3] * STEPSIZE_SCALE;
            string st = String.Format("Min={4} with {0} {1} {2} {3}", output.x, output.y, output.theta * 180 / Math.PI, output.scale, minimum);
            Console.Out.WriteLine("Similarity converged at {0} after {1} evaluations ({2} itterations)", st, solver.Iterations, opt_iterations);//solver.Convergence.Evaluations
            return output;
        }

    }

    public class VisieRCatalog
    {
        public static string decSep = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
        public static string thoSep = System.Globalization.NumberFormatInfo.CurrentInfo.NumberGroupSeparator;

        public struct Catalog{
            /// <summary>
            /// Designation of the object
            /// </summary>
            public string USNO;    //(a12)     (1) [ucd=meta.id;meta.main]
            /// <summary>
            /// Right Ascension at Eq=J2000, Ep=J2000 
            /// </summary>
            public double RAJ2000; //(F10.6)	(2) [ucd=pos.eq.ra;meta.main]
            /// <summary>
            /// Declination at Eq=J2000, Ep=J2000 
            /// </summary>
            public double DEJ2000; //(F10.6)	(2) [ucd=pos.eq.dec;meta.main]
            /// <summary>
            /// Mean error on RAdeg*cos(DEdeg) at Epoch 
            /// </summary>
            public int e_RAJ2000;  //(I3)	    [ucd=stat.error;pos.eq.ra]
            /// <summary>
            /// Mean error on DEdeg at Epoch 
            /// </summary>
            public int e_DEJ2000;  //(I3)	    [ucd=stat.error;pos.eq.dec]
            /// <summary>
            /// Mean epoch of observation 
            /// </summary>
            public double Epoch;   //(F6.1)	(2) [ucd=time.epoch;obs]
            /// <summary>
            /// Proper motion in RA (relative to YS4.0) 
            /// </summary>
            public int pmRA;       //(I6)	    [ucd=pos.pm;pos.eq.ra]
            /// <summary>
            /// Proper motion in DE (relative to YS4.0) 
            /// </summary>
            public int pmDE;       //(I6)	    [ucd=pos.pm;pos.eq.dec]
            /// <summary>
            /// Number of detections (7) 
            /// </summary>
            public int Ndet;       //(I1)	    [0,5] [ucd=meta.number]
            /// <summary>
            /// First blue magnitude 
            /// </summary>
            public double B1mag;   //(F5.2)	? [ucd=phot.mag;em.opt.B]
            /// <summary>
            /// First red magnitude 
            /// </summary>
            public double R1mag;   //(F5.2)	? [ucd=phot.mag;em.opt.R]
            /// <summary>
            /// Second blue magnitude 
            /// </summary>
            public double B2mag;   //(F5.2)	? [ucd=phot.mag;em.opt.B]
            /// <summary>
            /// Second red magnitude 
            /// </summary>
            public double R2mag;   //(F5.2)	? [ucd=phot.mag;em.opt.R]
            /// <summary>
            /// Infrared (N) magnitude 
            /// </summary>
            public double Imag;    //(F5.2)	? [ucd=phot.mag;em.opt.I]

            public Catalog(string s) {
                string[] li = s.Split('\t');
                this.USNO = "Error";
                this.RAJ2000 = 0.0;
                this.DEJ2000 = 0.0;
                this.e_RAJ2000 = 0;
                this.e_DEJ2000 = 0;
                this.Epoch = 0.0;
                this.pmRA = 0;
                this.pmDE = 0;
                this.Ndet = 0;
                this.B1mag = 0.0;
                this.R1mag = 0.0;
                this.B2mag = 0.0;
                this.R2mag = 0.0;
                this.Imag = 0.0;
                //Console.Out.WriteLine("Length = " + li.Length);
                if (li.Length >= 14){
                    this.USNO = li[0];
                    if (Fits.IsDouble(li[1])) this.RAJ2000 = double.Parse(li[1].Replace(thoSep,decSep), System.Globalization.NumberStyles.Number);
                    if (Fits.IsDouble(li[2])) this.DEJ2000 = double.Parse(li[2].Replace(thoSep, decSep), System.Globalization.NumberStyles.Number);
                    if (Fits.IsNumber(li[3])) this.e_RAJ2000 = int.Parse(li[3]);
                    if (Fits.IsNumber(li[4])) this.e_DEJ2000 = int.Parse(li[4]);
                    if (Fits.IsDouble(li[5])) this.Epoch = double.Parse(li[5].Replace(thoSep, decSep), System.Globalization.NumberStyles.Number);
                    if (Fits.IsNumber(li[6])) this.pmRA = int.Parse(li[6]);
                    if (Fits.IsNumber(li[7])) this.pmDE = int.Parse(li[7]);
                    if (Fits.IsNumber(li[8])) this.Ndet = int.Parse(li[8]);
                    if (Fits.IsDouble(li[9])) this.B1mag = double.Parse(li[9].Replace(thoSep, decSep), System.Globalization.NumberStyles.Number);
                    if (Fits.IsDouble(li[10])) this.R1mag = double.Parse(li[10].Replace(thoSep, decSep), System.Globalization.NumberStyles.Number);
                    if (Fits.IsDouble(li[11])) this.B2mag = double.Parse(li[11].Replace(thoSep, decSep), System.Globalization.NumberStyles.Number);
                    if (Fits.IsDouble(li[12])) this.R2mag = double.Parse(li[12].Replace(thoSep, decSep), System.Globalization.NumberStyles.Number);
                    if (Fits.IsDouble(li[13])) this.Imag = double.Parse(li[13].Replace(thoSep, decSep), System.Globalization.NumberStyles.Number);
                }
            }
        }

        
    }

}
