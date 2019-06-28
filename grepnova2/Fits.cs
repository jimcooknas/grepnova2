using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using nom.tam.fits;
using nom.tam.image;
using nom.tam.util;
using System.Windows.Forms;



namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public class Fits
    {
        public const int TRANSFORM_LIN = 2001; /* Need these #defines to allow us to use switch with transform type */
        public const int TRANSFORM_LOG = 2002;
        public const int TRANSFORM_GAM = 2003;
        public const int NULLVAL = -999;
        public const int GREPNOVA_DATAMIN = 0;
        public const int GREPNOVA_DATAMAX = 65535;
        public static int iPointContainedLimit = 2; 

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0017 // Simplify object initialization
        public struct imageptr
        {
            public int xsize;
            public int ysize;
            public int datapeak;          /* Maximum brightness within image */
            public int datamin;           /* Minimum brightness within image */
            public int data_lower_decile; /* Brightness of faintest pixel, excluding 10% of most faint pixels */
            public int data_upper_decile; /* Brightness of brightest pixel, excluding 10% of most bright pixels */
            public double mean_ld_excess; /* Average excess brightness of pixels over lower decile */
            public int[] data;            /* Pointer to array of image pixels */
            public string object_name;    /* Name of object from FITS header [85] */
            public string date_numeric;    /* Date from FITS file, in numeric format */
            //public Int64 time_numeric;    /* Time from FITS file, in numeric format */
            public int bitpix;            /* BITPIX field (need for resize) #cooknas-9/2/2018*/
            public float exposure;        /* exposure of frame #cooknas-6/5/2018*/
            public float pixelSizeX;
            public float pixelSizeY;
            public float focalLength;
        }

        public struct COORD {
            public int X;
            public int Y;
        }

        public enum transform_type
        {
            LIN = TRANSFORM_LIN,
            LOG = TRANSFORM_LOG,
            GAM = TRANSFORM_GAM
        }

        public struct transform_config{
            public double normalisation;
            public double min;
            public double gamma;
            public transform_type transform;
        }

        public struct PointValue
        {
            public Point p;
            public int v;
            public PointValue(Point p, int v)
            {
                    this.p = p;
                    this.v = v;
            }
            public PointValue(int x=0, int y=0, int v=0)
            {
                this.p = new Point(x,y);
                this.v = v;
            }
        }

        public struct TriplePoint
        {
            public PointF[] pp;
            public TriplePoint(PointF a, PointF b, PointF c) {
                pp = new PointF[3];
                this.pp[0] = a;
                this.pp[1] = b;
                this.pp[2] = c;
            }

            public TriplePoint(float ax, float ay, float bx, float by, float cx, float cy) {
                pp = new PointF[3];
                this.pp[0] = new PointF(ax, ay);
                this.pp[1] = new PointF(bx, by);
                this.pp[2] = new PointF(cx, cy);
            }
        }

        public static bool IsPointValueNull(PointValue pv)
        {
            if (pv.p.X == 0 && pv.p.Y == 0 && pv.v == 0)
                return true;
            else
                return false;
        }

        static string friendly_timestring()
        {
            return DateTime.Now.ToString("HH:mm:ss.fff");
        }

        public static imageptr CloneImagePtr(imageptr img)
        {
            imageptr output = new imageptr();
            output.object_name = img.object_name;
            output.xsize = img.xsize;
            output.ysize = img.ysize;
            output.bitpix = img.bitpix;
            output.datamin = img.datamin;
            output.datapeak = img.datapeak;
            output.data_lower_decile = img.data_lower_decile;
            output.data_upper_decile = img.data_upper_decile;
            output.mean_ld_excess = img.mean_ld_excess;
            output.data = new int[output.xsize * output.ysize];
            for (int i = 0; i < output.xsize * output.ysize; i++) output.data[i] = img.data[i];
            //for (int i = 0; i < output.ysize; i++)
            //    for (int j = 0; j < output.xsize; j++)
            //        output.data[i * output.xsize + j] = img.data[;
            return output;
        }

        public static imageptr CreateBlackImage(int xmax, int ymax)
        {
            imageptr output = new imageptr();
            output.object_name = "Empty";
            output.xsize = xmax;
            output.ysize = ymax;
            output.bitpix = 16;
            output.datamin = 0;
            output.datapeak = 0;
            output.data_lower_decile = 0;
            output.data_upper_decile = 0;
            output.mean_ld_excess = 0;
            output.data = new int[xmax * ymax];
            for (int i = 0; i < ymax; i++)
                for (int j = 0; j < xmax; j++)
                    output.data[i * xmax + j] = 0;
            return output;
        }

        public static imageptr BlankSubject(imageptr subj, imageptr aligned)
        {
            //no need to blank anything, so return the subject
            if (subj.data == null || aligned.data == null) { Form1.frm.LogEntry("Subject or Alignd Template image was null."); return subj; }
            //blanking subject cannot be done if the two images have not the same dimensions
            if (subj.xsize != aligned.xsize || subj.ysize != aligned.ysize) return subj;
            //all OK, so zero each pixel in subject where the relative aligned image is also zero
            imageptr subj_blanked = CloneImagePtr(subj);
            for(int i = 0; i < aligned.data.Length; i++){
                if (aligned.data[i] == 0) subj_blanked.data[i] = 0;
            }
            return subj_blanked;
        }


        /// <summary>
        /// Opens the fits file pointed by pathfitsfile and reads the card by the field name cardField
        /// and it returns it as a string after removing the T character (e.g. the T in 2016-10-12T10:22)
        /// </summary>
        /// <param name="pathfitsfile">the filename of the fits file to read</param>
        /// <param name="cardField">the name of the card to read</param>
        /// <returns></returns>
        public static string ReadCardFieldToDate(string pathfitsfile, string cardField)
        {
            try{
                nom.tam.fits.Fits f = new nom.tam.fits.Fits(pathfitsfile);
                BasicHDU basic_hdu = f.GetHDU(0);
                if (basic_hdu.Header.ContainsKey(cardField)){
                    return basic_hdu.Header.GetStringValue(cardField).Replace("T"," ");
                }else{
                    return "";
                }
            }catch (Exception) { return ""; }
        }

        /// <summary>
        /// Opens the fits file pointed by pathfitsfile and reads the card by the field name cardField
        /// and it returns it as a string
        /// </summary>
        /// <param name="pathfitsfile">the filename of the fits file to read</param>
        /// <param name="cardField">the name of the card to read</param>
        /// <returns></returns>
        public static string ReadCardFieldToString(string  pathfitsfile, string cardField)
        {
            try{
                nom.tam.fits.Fits f = new nom.tam.fits.Fits(pathfitsfile);
                BasicHDU basic_hdu = f.GetHDU(0);
                if (basic_hdu.Header.ContainsKey(cardField)){
                    return basic_hdu.Header.GetStringValue(cardField);
                }else{
                    return "";
                }
            }catch (Exception) { return ""; }
        }

        /// <summary>
        /// Opens the fits file pointed by pathfitsfile and reads the card by the field name cardField
        /// and it returns it as integer
        /// </summary>
        /// <param name="pathfitsfile">the filename of the fits file to read</param>
        /// <param name="cardField">the name of the card to read</param>
        /// <returns></returns>
        public static int ReadCardFieldToInt(string pathfitsfile, string cardField)
        {
            try {
                nom.tam.fits.Fits f = new nom.tam.fits.Fits(pathfitsfile);
                BasicHDU basic_hdu = f.GetHDU(0);
                if (basic_hdu.Header.ContainsKey(cardField)){
                    return basic_hdu.Header.GetIntValue(cardField);
                }else{
                    return 0;
                }
            }catch (Exception) { return 0; }
        }

        /// <summary>
        /// Opens the fits file pointed by pathfitsfile and reads the card by the field name cardField
        /// and it returns it as long
        /// </summary>
        /// <param name="pathfitsfile">the filename of the fits file to read</param>
        /// <param name="cardField">the name of the card to read</param>
        /// <returns></returns>
        public static long ReadCardFieldToLong(string pathfitsfile, string cardField)
        {
            try { 
                nom.tam.fits.Fits f = new nom.tam.fits.Fits(pathfitsfile);
                BasicHDU basic_hdu = f.GetHDU(0);
                if (basic_hdu.Header.ContainsKey(cardField)){
                        return basic_hdu.Header.GetLongValue(cardField);
                }else{
                    return 0L;
                }
            }catch (Exception) { return 0L; }
        }

        /// <summary>
        /// Opens the fits file pointed by pathfitsfile and reads the card by the field name cardField
        /// and it returns it as double
        /// </summary>
        /// <param name="pathfitsfile">the filename of the fits file to read</param>
        /// <param name="cardField">the name of the card to read</param>
        /// <returns></returns>
        public static double ReadCardFieldToDouble(string pathfitsfile, string cardField)
        {
            try{
                nom.tam.fits.Fits f = new nom.tam.fits.Fits(pathfitsfile);
                BasicHDU basic_hdu = f.GetHDU(0);
                if (basic_hdu.Header.ContainsKey(cardField)){
                    return basic_hdu.Header.GetDoubleValue(cardField);
                }else{
                    return 0.0;
                }
            }catch (Exception) { return 0.0; }
        }

        /// <summary>
        /// Opens the fits file pointed by pathfitsfile and reads the card by the field name cardField
        /// and it returns it as float
        /// </summary>
        /// <param name="pathfitsfile">the filename of the fits file to read</param>
        /// <param name="cardField">the name of the card to read</param>
        /// <returns></returns>
        public static float ReadCardFieldToFloat(string pathfitsfile, string cardField)
        {
            try { 
                nom.tam.fits.Fits f = new nom.tam.fits.Fits(pathfitsfile);
                BasicHDU basic_hdu = f.GetHDU(0);
                if (basic_hdu.Header.ContainsKey(cardField)){
                    return basic_hdu.Header.GetFloatValue(cardField);
                }else{
                    return 0f;
                }
            }catch (Exception) { return 0f; }
        }

        public static List<string> readHDUFields(string pathfitsfile)
        {
            nom.tam.fits.Fits fits = new nom.tam.fits.Fits(pathfitsfile);
            nom.tam.fits.BasicHDU basichdu = null;
            List<string> result = new List<string>();
            //int k = 0;
            do{
                basichdu = fits.ReadHDU();
            }while (basichdu != null);
            //Console.Out.WriteLine("NumberOfHDUs = {0}", fits.NumberOfHDUs);
            for (int i = 0; i < fits.NumberOfHDUs; i++){
                basichdu = fits.GetHDU(i);
                try{
                    //Console.Out.WriteLine("HDU{0} NumberOfCards={1}", i, basichdu.Header.NumberOfCards);
                    for (int j = 0; j < basichdu.Header.NumberOfCards; j++){
                        try{
                            string ans = basichdu.Header.GetCard(j);
                            if (ans != null){
                                result.Add(ans);
                                //Console.Out.WriteLine("{0}. {1}", k++, ans);
                            }
                        }catch(Exception exx){
                            //Console.Out.WriteLine(exx.Message.ToString());
                            Form1.frm.LogError("readHDUFields", "HDU " + i + " Card " + j + " returned exception: " + exx.Message.ToString());
                        }
                    }
                }catch (Exception ex){
                    Console.Out.WriteLine("HDU "+i+" returned exception: "+ex.Message.ToString());
                    Form1.frm.LogError("readHDUFields", "HDU " + i + " returned exception: " + ex.Message.ToString());
                }
            }
            fits.Stream.Close();
            //PrintAllFields(pathfitsfile);
            return result;
        }

        public static void PrintAllFields(string fn)
        {
            nom.tam.fits.Fits fits = new nom.tam.fits.Fits(fn);
            nom.tam.fits.BasicHDU basichdu = fits.ReadHDU();
            Header hdr = basichdu.Header;
            nom.tam.util.Cursor iter = hdr.GetCursor();
            //The above GetCursor returns at Console.Out:
            //------------------------------------------
            //Cursor Started from position: #-1

            while (iter.MoveNext()){
                //DictionaryEntry de = (DictionaryEntry)iter.Current;
                //Console.WriteLine("{0} = {1}", de.Key, de.Value);
                if (Form1.iLogLevel > 2) Form1.frm.LogEntry(String.Format("{0}", ((DictionaryEntry)iter.Current).Value.ToString()));
            }
        }

        public static bool SaveFITSFromDataGridView(int idx, string fitsPath, DataGridView dgv)//idx=0-Template, 1-Subject
        {
            try{
                imageptr img = Form1.image[idx];
                //convert img.data array into a 2D array
                Int16[][] data = new Int16[img.ysize][];
                for (int i = 0; i < img.ysize; i++){
                    data[i] = new Int16[img.xsize];
                    for (int j = 0; j < img.xsize; j++){
                        data[i][j] = (Int16)(img.data[i * img.xsize + j] - Int16.MaxValue);
                    }
                }
                nom.tam.fits.Fits f = new nom.tam.fits.Fits();
                nom.tam.fits.BasicHDU hdu = (BasicHDU)nom.tam.fits.FitsFactory.HDUFactory(data);
                hdu.AddValue("BITPIX", 16, "8 unsigned int, 16 & 32 int, -32 & -64 real");
                //we need he next two fields in order the 'grepnova-align.bin.exe' to allign images
                hdu.Header.AddValue("BSCALE", 1.0000000000000000f, "/ physical = BZERO + BSCALE * array_value");
                hdu.Header.AddValue("BZERO", 32768.000000000000, "/ physical = BZERO + BSCALE * array_value");
                for (int i=0; i < dgv.RowCount; i++){
                    try{
                        if (dgv.Rows[i].Cells[1].Value == null) dgv.Rows[i].Cells[1].Value = "";
                        if (dgv.Rows[i].Cells[2].Value == null) dgv.Rows[i].Cells[2].Value = "";
                        Console.Out.WriteLine("Item " + i + ": " + dgv.Rows[i].Cells[0].Value.ToString());
                        if (IsNumber(dgv.Rows[i].Cells[1].Value.ToString())){//if it's integer add it as integer
                            hdu.AddValue(dgv.Rows[i].Cells[0].Value.ToString(), int.Parse(dgv.Rows[i].Cells[1].Value.ToString()), dgv.Rows[i].Cells[2].Value.ToString());
                        }else if (IsDouble(dgv.Rows[i].Cells[1].Value.ToString())){//if it's double add it as double
                            hdu.AddValue(dgv.Rows[i].Cells[0].Value.ToString(), double.Parse(dgv.Rows[i].Cells[1].Value.ToString()), dgv.Rows[i].Cells[2].Value.ToString());
                        }else{//or else add it as string
                            hdu.AddValue(dgv.Rows[i].Cells[0].Value.ToString(), dgv.Rows[i].Cells[1].Value.ToString(), dgv.Rows[i].Cells[2].Value.ToString());
                        }
                    }catch(Exception ex){
                        Console.Out.WriteLine("Error in " + i + ": " + ex.Message.ToString());
                        Form1.frm.LogError("SaveFITSFromDataGridView", "Error in " + i + ": " + ex.Message.ToString());
                    }
                }
                //let us know that it is modified by Grepnova2
                hdu.Header.AddHistory("'Modified by Grepnova2'");
                
                f.AddHDU(hdu);
                BufferedFile bf = new BufferedFile(fitsPath, FileAccess.ReadWrite, FileShare.ReadWrite);
                f.Write(bf);
                bf.Flush();
                bf.Close();
                bf.Dispose();
                //f.Close();
            }
            catch(Exception e){
                if (idx == 0){
                    if (Form1.iLogLevel > 0) Form1.frm.LogEntry("Exception in saving Template: " + e.Message.ToString());
                    Form1.frm.LogError("SaveFITSFromDataGridView","Exception in saving Template: " + e.Message.ToString());
                }else{
                    if (Form1.iLogLevel > 0) Form1.frm.LogEntry("Exception in saving Subject: " + e.Message.ToString());
                    Form1.frm.LogError("SaveFITSFromDataGridView", "Exception in saving Subject: " + e.Message.ToString());
                }
                return false;
            }
            return true;
        }

        /*
        /// <summary>
        /// writes the header to fits file
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="filename"></param>
        /// <param name="writeMode"></param>
        /// <param name="stringTruncationLength"></param>
        /// <param name="padStringsLeft"></param>
        /// <param name="stringPadChar"></param>
        public static void Write(IDataReader reader, String filename,
                                StreamedBinaryTableHDU.StringWriteMode writeMode, int stringTruncationLength,
                                bool padStringsLeft, char stringPadChar)
        {
            Header header = new Header();
            header.Simple = true;
            header.Bitpix = 16;
            header.Naxes = 2;

            nom.tam.util.Cursor c = header.GetCursor();
            // move to the end of the header cards
            for (c.MoveNext(); c.MoveNext();) ;
            // we know EXTEND isn't there yet
            c.Add("EXTEND", new HeaderCard("EXTEND", true, null));

            ImageHDU hdu1 = new ImageHDU(header, null);

            StreamedBinaryTableHDU hdu2 =
                          new StreamedBinaryTableHDU(new DataReaderAdapter(reader), 4096,
                          writeMode, stringTruncationLength, padStringsLeft, stringPadChar);

            nom.tam.fits.Fits fits = new nom.tam.fits.Fits();
            fits.AddHDU(hdu1);
            fits.AddHDU(hdu2);

            Stream s = null;
            try
            {
                s = new FileStream(filename, FileMode.Create);
                fits.Write(s);
                s.Close();
            }
            catch (Exception e)
            {
                s.Close();
                throw (e);
            }
        }
        */

        public static imageptr import_fits(string pathfitsfile)
        {
            imageptr output = new imageptr();
            long[] inc = { 1, 1 }; /* Pixel increment when reading FITS data */
            long[] box_bottom = { 0, 0 };
            long[] box_top = { 1, 1 };
            nom.tam.fits.Fits fits = null;
            //nom.tam.fits.Fits(pathfitsfile) throws IOException if header is malformed, but it still executes. 
            try{
                fits = new nom.tam.fits.Fits(pathfitsfile);//does not really reads the file but just associates the fits variable to that file.
            }
            catch (System.IO.IOException fe) {Console.Out.WriteLine("Malformed FITS header: " + fe.Message.ToString());}
            catch (System.Exception e) {Console.Out.WriteLine("Exception in opening Fits: " + e.Message.ToString());}
            
            //BasicHDU basichdu=null;
            //here we really get the HDU data
            BasicHDU basichdu = fits.ReadHDU();//produces Exception EndOfStream
            /*do{
            basichdu = fits.readHDU();//produces Exception EndOfStream
            if (basichdu != null){
                    basichdu.Info();
                }
            }while (basichdu != null);*/

            //loop through the Header Data Units (commented for now)
            //for (int i = 0; i < 1; i++){fits.NumberOfHDUs; i++)
            //or just read HDU0
            //int i = 0;
            //basichdu = fits.GetHDU(i);
            if(basichdu != null){
                output.bitpix = basichdu.BitPix;
                double bzero = basichdu.BZero;
                double bscale = basichdu.BScale;
                //Console.Out.WriteLine("BPix={0} BZero={1} BScale={2}", output.bitpix, bzero, bscale);
                if (basichdu.GetType().FullName == "nom.tam.fits.ImageHDU") {
                    try {
                        nom.tam.fits.ImageHDU imghdu = (nom.tam.fits.ImageHDU)basichdu;
                        output.xsize = imghdu.Axes[1];
                        output.ysize = imghdu.Axes[0];
                        output.data = new int[output.xsize * output.ysize];
                        //get out the pixel array and the dimensions
                        Array[] array = (Array[])imghdu.Data.DataArray;
                        for (int y1 = 0; y1 < output.ysize; y1++) {
                            Int16[] temp16 = (Int16[])array[y1];
                            for (int x1 = 0; x1 < output.xsize; x1++) {
                                //turn temp16[x1] into unsigned Int16 adding Int16.MaxValue
                                output.data[y1 * output.xsize + x1] = (int)(Convert.ToInt16(temp16[x1]) + Int16.MaxValue);//(int)((temp16[x1]) * bscale + bzero);
                            }
                        }
                        output.bitpix = imghdu.BitPix;
                        output.object_name = imghdu.Object;
                        try{
                            output.date_numeric = imghdu.GetTrimmedString("DATE-OBS").Replace("T", " ");
                        }catch (Exception) { output.date_numeric = "no-date"; }
                        output.datapeak = (int)imghdu.MaximumValue;
                        output.datamin = (int)imghdu.MinimumValue;
                        float exptime = 0F;
                        if (imghdu.Header.ContainsKey("EXPTIME")){
                            try{
                                string expval = imghdu.Header.GetStringValueWhatever("EXPTIME");
                                exptime = float.Parse(expval, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                            }catch (Exception) { exptime = 0F; }
                        }
                        output.exposure = exptime;
                        //version 1.3 @ 20190430
                        float xpixsize = 0F;
                        string pixsize = "";
                        if (imghdu.Header.ContainsKey("XPIXSZ")) pixsize = imghdu.Header.GetStringValueWhatever("XPIXSZ");
                        if (imghdu.Header.ContainsKey("XPIXELSZ")) pixsize = imghdu.Header.GetStringValueWhatever("XPIXELSZ");
                        if (!pixsize.Equals("")) { 
                            try {
                                xpixsize = float.Parse(pixsize.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                            }catch (Exception) { xpixsize = 0F; }
                        }
                        output.pixelSizeX=xpixsize;
                        float ypixsize = 0F;
                        pixsize = "";
                        if (imghdu.Header.ContainsKey("YPIXSZ")) pixsize = imghdu.Header.GetStringValueWhatever("YPIXSZ");
                        if (imghdu.Header.ContainsKey("YPIXELSZ")) pixsize = imghdu.Header.GetStringValueWhatever("YPIXELSZ");
                        if (!pixsize.Equals("")){
                            try{
                                ypixsize = float.Parse(pixsize.Replace(",","."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                            }catch (Exception) { ypixsize = 0F; }
                        }
                        output.pixelSizeY = ypixsize;
                        float focal = 0F;
                        if (imghdu.Header.ContainsKey("FOCALLEN")){
                            try{
                                string focalval = imghdu.Header.GetStringValueWhatever("FOCALLEN");
                                focal = float.Parse(focalval, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                            }catch (Exception) { focal = 0F; }
                        }
                        output.focalLength = focal;
                        //version 1.3 @ 20190430 end
                        //Console.Out.WriteLine("Exposure {0}: {1} {2}", output.object_name, output.exposure, imghdu.Header.GetStringValueWhatever("EXPTIME"));
                        output = image_minmax_update(output);
                        //Console.Out.WriteLine("import_fits after image_minmax_update for {0}: datamin={1} datapeak={2}", output.object_name, output.datamin, output.datapeak);
                    } catch (Exception ex) {
                        string error = ex.Message;
                        Form1.frm.LogError("import_fits", error);
                    }
                }
            }else{
                output = CreateBlackImage(100, 100);
            }
            //}/*end of for loop fits.NumberOfHDUs*/
            fits.Stream.Close();
            fits = null;
            return output;
        }

        
        /// <summary>
        /// Dumps the data of a fits image to the Log area and to Console
        /// </summary>
        /// <param name="fn">the filename of FITS file</param>
        /// <param name="toConsole">if true then dump goes also to console</param>
        /// <returns></returns>
        public static string fitsdata2console(string fn, bool toConsole=false)
        {
            string output = "";
            nom.tam.fits.Fits f = new nom.tam.fits.Fits(fn);
            ImageHDU h = (ImageHDU)f.ReadHDU();
            System.Array img = (System.Array[])h.Kernel;
            output += "Dumping image data:\r\n";
            int x = 0, y = 0;//for pixel position tracking
            foreach (short[] collection in img){
                if (toConsole) Console.WriteLine("LineNo: " + y);
                output += "LineNo " + y + ":\r\n";
                foreach (UInt16 pixVal in collection){
                    if (toConsole) Console.Write(pixVal + ",");
                    output += "" + pixVal + ",";
                    x++;
                }
                y++;
                if (toConsole) Console.WriteLine("");
                output += "\r\n";
            }
            return output;
        }

        /// <summary>
        /// Save the imageptr structure to a FITS file. Currently only a BasicHDU is foreseen for this file
        /// </summary>
        /// <param name="img">the imageptr structure to save</param>
        /// <param name="fn">the FITS filename where we save the structure</param>
        public static void save_fits(imageptr img, string fn)
        {
            //convert img.data array into a 2D array 
            //int[] dim= { img.ysize, img.xsize };
            //Array data = nom.tam.util.ArrayFuncs.Curl(img.data.ToArray<int>(), dim);
            
            Int16[][] data = new Int16[img.ysize][];
            for (int i = img.ysize-1; i >=0; i--){
                data[i] = new Int16[img.xsize];
                for (int j = 0; j < img.xsize; j++){
                    data[i][j] = (Int16)(img.data[i * img.xsize + j] - Int16.MaxValue);// - Int16.MaxValue);//img.data[i * img.xsize + j];
                }
            }
            nom.tam.fits.Fits f = new nom.tam.fits.Fits();
            nom.tam.fits.BasicHDU hdu = nom.tam.fits.Fits.MakeHDU(data); //(BasicHDU)nom.tam.fits.FitsFactory.HDUFactory(data);
            //let us know that it is created by Grepnova2
            hdu.Header.AddHistory("'Created by Grepnova2'");
            hdu.Header.AddValue("BITPIX", 16, "8 unsigned int, 16 & 32 int, -32 & -64 real");
            hdu.Header.AddValue("OBJECT", img.object_name, "Object name");
            if(img.date_numeric.Length > 18) hdu.Header.AddValue("TIME-OBS", img.date_numeric.Substring(11,8), "Observation time");
            if(img.date_numeric.Length >= 10) hdu.Header.AddValue("DATE-OBS", img.date_numeric.Replace(" ","T"), "Observation date");
            hdu.Header.AddValue("EXPOSURE", img.exposure, "Exposure time");
            //we need he next two fields in order the 'grepnova-align.bin.exe' to allign images
            hdu.Header.AddValue("BSCALE", 1.0000000000000000f,"/ physical = BZERO + BSCALE * array_value");
            hdu.Header.AddValue("BZERO", 32768.000000000000, "/ physical = BZERO + BSCALE * array_value");
            f.AddHDU(hdu);
            BufferedFile bf = new BufferedFile(fn, FileAccess.ReadWrite, FileShare.ReadWrite);
            f.Write(bf);
            bf.Close();
        }

        public static int[] calc_new_contrast(imageptr img, int min, int max)
        {
            int img_length = img.xsize * img.ysize;
            int[] data = new int[img_length];
            double f = (max-min)/(img.datapeak-img.datamin);
            for(int i = 0; i < img_length; i++)
            {
                data[i] = (int)((double)(img.data[i] - img.datamin) * f + (double)img.datamin);
            }
            return data;
        }

        public static int[] calc_histogram_total(imageptr image)
        {
            int i;
            int no_pixels = 0;
            int min = 9999, max = -9999;
            int[] image_scan = image.data;
            int id = 0;
            double sum_brightness = 0;
            int[] histogram = new int[GREPNOVA_DATAMAX - GREPNOVA_DATAMIN + 1];

            for (i = 0; i <= (GREPNOVA_DATAMAX - GREPNOVA_DATAMIN); i++) histogram[i] = 0; // Zero histogram

            for (i = 0; i < (image.xsize * image.ysize); i++, id++)//image_scan++) // Find min/max brightnesses in the image 
                if (((image_scan[id]) != NULLVAL) && (image_scan[id] >= GREPNOVA_DATAMIN) && (image_scan[id] <= GREPNOVA_DATAMAX))
                {
                    if ((image_scan[id]) < min) min = image_scan[id];
                    if ((image_scan[id]) > max) max = image_scan[id];
                }
            image.datapeak = max;
            image.datamin = min;

            image_scan = image.data; // Construct histogram of brightnesses 
            id = 0;
            for (i = 0; i < (image.xsize * image.ysize); i++, id++)
            {
                if ((image_scan[id] >= GREPNOVA_DATAMIN) && (image_scan[id] <= GREPNOVA_DATAMAX))
                {
                    histogram[(image_scan[id]) - GREPNOVA_DATAMIN]++;
                    no_pixels++;
                    sum_brightness += (image_scan[id]);
                }
                else if (image_scan[id] != NULLVAL)
                {
                    try
                    {
                        if (image_scan[id] < GREPNOVA_DATAMIN) image_scan[id] = GREPNOVA_DATAMIN;
                        if (image_scan[id] > GREPNOVA_DATAMAX) image_scan[id] = GREPNOVA_DATAMAX;
                        histogram[(image_scan[id]) - GREPNOVA_DATAMIN]++;
                        no_pixels++;
                        sum_brightness += (image_scan[id]);
                    }
                    catch (FormatException ex)
                    {
                        string error = ex.Message;
                        Form1.frm.LogError("calc_histogram_total", error);
                    }
                }
            }
            return histogram;
        }

        /// <summary>
        /// Creates an array of bin elements (float) and calculates the elements
        /// of this array to contain the values of image.data array in each bin
        /// </summary>
        /// <param name="image">imageptr structure that we need to calculate histogram</param>
        /// <param name="bin">the number of bins to divide the data. Default is 256</param>
        /// <param name="isLogarithmic">if true calculates the values as logarithms. Default is true</param>
        /// <returns>Returns a floats array.</returns>
        public static float[] calc_histogram(imageptr image, int bin = 256, bool isLogarithmic=true)
        {
            int i;
            int no_pixels = 0;
            int min = 9999, max = -9999;
            int[] image_scan = image.data;
            int id = 0;
            double sum_brightness = 0;
            int intPerBin = (int)((GREPNOVA_DATAMAX - GREPNOVA_DATAMIN) / bin + 1);
            int[] histogram = new int[bin + 2];//GREPNOVA_DATAMAX - GREPNOVA_DATAMIN + 1];

            for (i = 0; i <= bin /*(GREPNOVA_DATAMAX - GREPNOVA_DATAMIN)*/; i++) histogram[(int)(i / intPerBin)] = 0; /* Zero histogram */

            for (i = 0; i < (image.xsize * image.ysize); i++, id++)//image_scan++) /* Find min/max brightnesses in the image */
                if (((image_scan[id]) != NULLVAL) && (image_scan[id] >= GREPNOVA_DATAMIN) && (image_scan[id] <= GREPNOVA_DATAMAX))
                {
                    if ((image_scan[id]) < min) min = image_scan[id];
                    if ((image_scan[id]) > max) max = image_scan[id];
                }
            image.datapeak = max;
            image.datamin = min;

            image_scan = image.data; /* Construct histogram of brightnesses */
            id = 0;
            for (i = 0; i < (image.xsize * image.ysize); i++, id++)
            {
                if ((image_scan[id] >= GREPNOVA_DATAMIN) && (image_scan[id] <= GREPNOVA_DATAMAX))
                {
                    histogram[(int)(((image_scan[id]) - GREPNOVA_DATAMIN) / intPerBin)]++;
                    no_pixels++;
                    sum_brightness += (image_scan[id]);
                }
                else if (image_scan[id] != NULLVAL)
                {
                    try
                    {
                        if (image_scan[id] < GREPNOVA_DATAMIN) image_scan[id] = GREPNOVA_DATAMIN;
                        if (image_scan[id] > GREPNOVA_DATAMAX) image_scan[id] = GREPNOVA_DATAMAX;
                        histogram[(int)((image_scan[id] - GREPNOVA_DATAMIN) / intPerBin)]++;
                        no_pixels++;
                        sum_brightness += (image_scan[id]);
                    }
                    catch (FormatException ex)
                    {
                        string error = ex.Message;
                        Form1.frm.LogError("calc_histogram", error);
                    }
                }
            }
            float[] myHist = new float[bin];
            if (isLogarithmic)
                for (int j = 0; j < bin; j++) if (histogram[j] > 0) myHist[j] = (float)Math.Log10(histogram[j]); else myHist[j] = 0.0f;
            else
                for (int j = 0; j < bin; j++) myHist[j] = (float)histogram[j];
            return myHist;
        }

        public static imageptr image_minmax_update(imageptr image)
        {
            if (image.xsize == 0 || image.ysize == 0) return image;
            int i, accumulator;
            int no_pixels = 0;
            int min = 9999;
            int max = -9999;
            int[] image_scan = image.data;
            int id = 0;
            double sum_brightness = 0;
            int[] histogram = new int[GREPNOVA_DATAMAX - GREPNOVA_DATAMIN + 1];

            for (i = 0; i <= (GREPNOVA_DATAMAX - GREPNOVA_DATAMIN); i++) histogram[i] = 0; /* Zero histogram */
            //Console.Out.WriteLine(String.Format("Dimensions: x={0} y={1} xy={2} image_scan={3}\n", image.xsize, image.ysize, image.xsize * image.ysize, image_scan.Length));
            try{
                for (i = 0; i < (image.xsize * image.ysize); i++){ /* Find min/max brightnesses in the image */
                    if (((image_scan[id]) != NULLVAL) 
                        && (image_scan[id] >= GREPNOVA_DATAMIN) 
                        && (image_scan[id] <= GREPNOVA_DATAMAX)){
                        if ((image_scan[id]) < min) min = image_scan[id];
                        if ((image_scan[id]) > max) max = image_scan[id];
                    }
                    id++;
                }
            }catch(Exception) { /*Console.Out.WriteLine(String.Format("WARNING: Error in min-max brightness at i={0} id={1}. Error {1}\n", i, id, eex.Message));*/ }
            image.datapeak = max;
            image.datamin = min;

            //image_scan = image.data; /* Construct histogram of brightnesses */
            id = 0;
            for (i = 0; i < (image.xsize * image.ysize); i++, id++) { 
                if ((image_scan[id] >= GREPNOVA_DATAMIN) && (image_scan[id] <= GREPNOVA_DATAMAX)) {
                    try{
                        histogram[(image_scan[id]) - GREPNOVA_DATAMIN]++;
                        no_pixels++;
                        sum_brightness += (image_scan[id]);
                    }catch (Exception ex) { Form1.frm.LogEntry(String.Format("WARNING: Error at id={0}. Error {1}\n", id, ex.Message)); }
                } else if (image_scan[id] != NULLVAL) {
                    try {
                        if (image_scan[id] < GREPNOVA_DATAMIN) image_scan[id] = GREPNOVA_DATAMIN;
                        if (image_scan[id] > GREPNOVA_DATAMAX) image_scan[id] = GREPNOVA_DATAMAX;
                        histogram[(image_scan[id]) - GREPNOVA_DATAMIN]++;
                        no_pixels++;
                        sum_brightness += (image_scan[id]);
                        //Form1.frm.LogEntry(String.Format("WARNING: Pixel outside {0}-{1} range at ({2},{3}), value = {4}.\n", GREPNOVA_DATAMIN, GREPNOVA_DATAMAX, (int)(i % image.xsize), (int)(i / image.xsize), image_scan[id]));
                    } catch (Exception ex) {
                        string error = ex.Message;
                        Form1.frm.LogError("Error in image_minmax_update", error);
                    }
                }
            }
            try{
                i = 0; accumulator = 0; /* Find lower decile brightness */
                while (accumulator < (no_pixels / 20)) accumulator += histogram[i++];
                image.data_lower_decile = i + GREPNOVA_DATAMIN;
                image.mean_ld_excess = ((double)sum_brightness / (double)no_pixels) - (double)(image.data_lower_decile);
            }catch (Exception) { Console.Out.WriteLine("Cannot calculate lower decile brightness"); }
            try { 
                i = GREPNOVA_DATAMAX - GREPNOVA_DATAMIN; accumulator = 0; /* Find upper decile brightness */
                while (accumulator < (no_pixels / 20)) accumulator += histogram[i--];
                image.data_upper_decile = i + GREPNOVA_DATAMIN;
            }catch (Exception) { Console.Out.WriteLine("Cannot calculate upper decile brightness"); }

            return image;
        }

        public static imageptr image_add_brightness(imageptr image_in, int brightness)
        {
            imageptr image = CloneImagePtr(image_in);

            //Bitmap temp = (Bitmap)img;
            //Bitmap bmap = (Bitmap)temp.Clone();
            if (brightness < -255) brightness = -255;
            if (brightness > 255) brightness = 255;
            int c;
            for (int i = 0; i < image.xsize; i++)
            {
                for (int j = 0; j < image.ysize; j++)
                {

                    c = image.data[j*image.xsize + i] + brightness;

                    if (c < GREPNOVA_DATAMIN) c = GREPNOVA_DATAMIN;
                    if (c > GREPNOVA_DATAMAX) c = GREPNOVA_DATAMAX;
                    image.data[j * image.xsize + i]=c;
                }
            }
            return image;
        }

        public static transform_config transform_init(imageptr image, 
                                                      double gamma, 
                                                      transform_type transform,
                                                      int iMin=-1,
                                                      int iMax=-1)
        {
            transform_config output = new transform_config();
            double max;

            output.gamma = gamma;
            output.transform = transform;
            if(iMin>-1)
                output.min = iMin;// image.data_lower_decile - (image.data_upper_decile - image.data_lower_decile) / 2.0;
            else
                output.min = image.data_lower_decile - (image.data_upper_decile - image.data_lower_decile) / 2.0;
            if (iMax > -1)
                max = iMax;
            else
                max = image.data_upper_decile + (image.data_upper_decile - image.data_lower_decile) / 2.0;
            //double bitPixRatio = image.bitpix / 16; 
            switch ((int)transform)
            {
                case (TRANSFORM_LIN):
                    output.normalisation = (double)256.0 / ((max - output.min)) / (gamma * 3.75);
                    break;
                case (TRANSFORM_LOG):
                    output.normalisation = (double)256.0 / Math.Log((max - output.min)) / (gamma * 3.9);
                    break;
                case (TRANSFORM_GAM): /* Line below is because maximum for gamma transfer really is unchangable maximum, so needs to be a little higher */
                    max = image.data_upper_decile + (image.data_upper_decile - image.data_lower_decile) * 2.0;
                    output.normalisation = (double)256.0 / Math.Pow((max - output.min), gamma * 1.8);
                    break;
                default:
                    //Form1.LogEntry("transform_init(): Undefined transform type.\n");
                    output.normalisation = 0;
                    break;
            }
            //Console.Out.WriteLine("Object={0} datamin={1} datamax={2}: norm={3} max={4} min={5}", image.object_name, image.datamin, image.datapeak, output.normalisation,max,output.min);
            return (output);
        }

        public static int transform_do(transform_config transform, double input)
        {
            double output;
            switch ((int)transform.transform)
            {
                case (TRANSFORM_LIN):
                    if (input <= transform.min) return (0);
                    output = (input - transform.min) * transform.normalisation;
                    break;
                case (TRANSFORM_LOG):
                    if (input <= transform.min) return (0);
                    output = Math.Log(input - transform.min) * transform.normalisation;
                    break;
                case (TRANSFORM_GAM):
                    if (input <= transform.min) return (0);
                    output = Math.Pow(input - transform.min, transform.gamma * 1.8) * transform.normalisation;
                    break;
                default:
                    //Form1.LogEntry("transform_do(): Undefined transform type.\n");
                    output = 0.0;
                    break;
            }
            //if (output < 0.0) output = 0.0; /* Clip data to 0-255 range */
            //if (output > 255.0) output = 255.0;
            output = (output < 0.0 ? 0.0 : (output > 255.0 ? 255.0 : output));
            return ((int)output);
        }

        //concept from http://fits.gsfc.nasa.gov/fits_libraries.html#CSharpFITS
        public static System.Drawing.Image GetFITSImage(imageptr image, double gamma, transform_type trans, int iMin=-1, int iMax=-1, int wavelength=0)
        {
            System.Drawing.Image result = null;
            transform_config transformer = transform_init(image, gamma, trans, iMin, iMax);// transform_type.LIN);
            try
            {
                int x = image.xsize;// imghdu.Axes[1];
                int y = image.ysize;// imghdu.Axes[0];

                //get out the pixel array and the dimensions
                int[] array = (int[])image.data;// imghdu.Data.DataArray;
                result = new System.Drawing.Bitmap(x, y);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(result);
                        
                for (int j = 0; j < y ; j++)//(y - 1)
                {
                    //pixels are NOT RGB values so here we convert it to an RGB value.
                    for (int k = 0; k < x; k++)//x - 1
                    {
                        double val = transform_do(transformer, (double)(image.data[j * x + k]));
                        System.Drawing.Color c = System.Drawing.Color.FromArgb(255, (int)val, (int)val, (int)val);
                        if(wavelength>0)c = WeighRGBValue(c, wavelength);
                        ((System.Drawing.Bitmap)result).SetPixel(k, j, c);
                    }
                }
            }catch (Exception ex){
                string error = ex.Message;
                Form1.frm.LogError("GetFITSImage", error);
            }                                        
            return result;
        }

        public static int[] ParseIntegerFile(string filename, int minVal=0)
        {
            string fileContent = File.ReadAllText(filename);
            string[] integerStrings = fileContent.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int[] integers = new int[integerStrings.Length];
            int i = 0;
            for (int n = 0; n < integerStrings.Length; n++){
                i = int.Parse(integerStrings[n]);
                integers[n] = (i < 0 ? 0 : i);
            }
            return integers;
        }


        public static System.Drawing.Color WeighRGBValue(System.Drawing.Color color, int wavelength)
        {
            //Tried to get a similar color to the original site.
            System.Drawing.Color c;
            int alpha, red, green, blue;
            red = green = blue = 255;
            alpha = color.A;
            switch (wavelength)
            {
                case 94:    //green : 00FF00
                    red = 0;
                    green = color.G;
                    blue = 0;
                    break;
                case 131:    //teal : 008080
                    red = 0;
                    green = color.G;
                    blue = color.B;
                    break;
                case 304:    //red : FF0000
                    red = color.R;
                    green = color.G / 5;
                    blue = color.B / 5;
                    break;
                case 335:    //blue : 0000FF
                    red = (color.R / 5);
                    green = (color.G / 5);
                    blue = color.B;
                    break;
                case 171:    //gold : FFD700
                    red = color.R;
                    green = (int)((double)color.G / 255.0 * 215.0);
                    blue = 0;
                    break;
                case 193:    //copper : B87333
                    red = color.R;                                    //184 mapped to 255
                    green = (int)((double)color.G / 255.0 * 115.0);    //115 mapped to 255
                    blue = (int)((double)color.B / 255.0 * 51.0);    //50 mapped to 255
                    break;
                case 211:    //purple : 800080
                    red = color.R;
                    green = 0;
                    blue = color.B;
                    break;
                case 1600:    //ocher : BBBB00
                    red = (int)((double)color.R / 255.0 * 187.0);    //BB mapped to 255
                    green = (int)((double)color.G / 255.0 * 187.0);    //BB mapped to 255
                    blue = 0;
                    break;
                case 1700:    //pink : FFC0CB
                    red = color.R;
                    green = (int)((double)color.G / 255.0 * 192.0);
                    blue = (int)((double)color.B / 255.0 * 203.0);
                    break;
                case 4500:    //silver : C0C0C0
                    red = color.R;
                    green = color.G;
                    blue = color.B;
                    break;
                default:
                    red = color.R;//0
                    green = color.G;//0
                    blue = color.B;//0
                    break;
            }                            //end switch
            c = System.Drawing.Color.FromArgb(alpha, red, green, blue);
            return c;
        }

        public static imageptr ImageSharpen(imageptr img, float k, int dscan = 1) {
            imageptr ret_img = CloneImagePtr(img);
            int V = dscan;
            for (int y = 0; y < img.ysize; y++)
            {
                for(int x = 0; x < img.xsize; x++)
                {
                    //Lsharp(j) = (L(j) - ksharp / 2 * (L(j - V) + L(j + V))) / (1 - ksharp)
                    int j = x + y * img.xsize;
                    int dd = 0;
                    int dd1 = 0;
                    if (j - V < 0 || j - V >= img.ysize * img.xsize) dd = 0; else dd = img.data[j - V];
                    if (j + V < 0 || j + V >= img.ysize * img.xsize) dd1 = 0; else dd1 = img.data[j + V];
                    ret_img.data[j] = (int)((img.data[j] - (k / 2f) * (dd + dd1)) / (1 - k));
                }
            }
            return ret_img;
        }

        public static List<Point> CheckForSNCandidates(imageptr subj, imageptr al_tmpl, int num)
        {
            List<Point> candidates = new List<Point>();
            //create the starlist of Subject
            Align.starlist s_subj = Align.source_extract(subj, num);
            //create the starlist of Aligned Template
            Align.starlist s_temp = Align.source_extract(al_tmpl, num);
            //check if any star in Subject starlist is not included in Aligned Template starlist
            for (int i = 0; i < s_subj.list_size; i++){
                if(!PointContainedInArray(new Point(s_subj.x_pos[i], s_subj.y_pos[i]), s_temp))
                    candidates.Add(new Point(s_subj.x_pos[i], s_subj.y_pos[i]));
            }
            //Console.Out.WriteLine("Candidates found: " + candidates.Count);
            return candidates;//if no cantidate, then cantidate.Count should be 0
        }

        private static bool PointContainedInArray(Point p, Align.starlist arr)//, bPrint)
        {
            for (int i=0; i<arr.list_size; i++){
                //if (bPrint) Console.WriteLine("Subj-ATemp {0}: x={1}-{2} y={3}-{4}", i, p.X, arr.x_pos[i], p.Y, arr.y_pos[i]);
                if (Math.Abs(p.X - arr.x_pos[i]) < iPointContainedLimit && Math.Abs(p.Y - arr.y_pos[i]) < iPointContainedLimit)
                {
                    //if(bPrint)Console.WriteLine("Subj-ATemp: x={0}-{1} y={2}-{3}", p.X, arr.x_pos[i], p.Y, arr.y_pos[i]);
                    return true;
                }
            }
            return false;
        }

        // A Simple Peak Detector Algorithm
        // Copyleft (L) 1998 Kenneth J. Mighell (Kitt Peak National Observatory)
        public static PointValue[] Peaker(imageptr img, int winWidth){//, int NX, int NY, int PEAKMIN, int PEAKMAX) {
            int maxPoints = 20;
            PointValue[] pp = new PointValue[maxPoints];
            int NX = img.xsize;
            int NY = img.ysize;
            int PEAKMIN = img.datamin;
            int PEAKMAX = img.datapeak;
            int X;
            int Y;
            int XX;
            int YY;
            int iCounter = 0;
            int[] iArr = new int[NX * NY];
            int[] tempArr;// = new int[NX * NY];
            int PIXEL;
            int NEIGHBOR;
            bool BINGO = true;
            int I;
            int j;
            int rectSize;
            COORD cen;
            int[] va = new int[maxPoints];
            //int winWidth = 1;
            iArr = img.data;
            //Form1.LogEntry(String.Format("Min={0} Max={1} Upper={2}", PEAKMIN, PEAKMAX, img.data_upper_decile));
            for (Y = winWidth; Y <= (NY - winWidth); Y++) {
                for (X = winWidth; X < (NX - winWidth); X++) {
                    PIXEL = iArr[X + Y * NX];// (NX - winWidth)];
                    if (PIXEL >= img.data_upper_decile){//(PIXEL >= PEAKMIN) && (PIXEL <= PEAKMAX)) {
                        BINGO = true;
                        for (YY = (Y - winWidth); YY < (Y + winWidth); YY++) {
                            for (XX = (X - winWidth); XX < (X + winWidth); XX++) {
                                if (BINGO == true) {
                                    NEIGHBOR = iArr[XX + YY * NX];// (X + winWidth)];
                                    if (NEIGHBOR > PIXEL) {
                                        BINGO = false;
                                    } else if (NEIGHBOR == PIXEL) {
                                        if ((XX != X) || (YY != Y)) {
                                            if (((XX <= X) && (YY <= Y)) || ((XX > X) && (YY < Y))) BINGO = false;
                                        }
                                    }
                                }
                            }
                        }
                        if (BINGO==true) {
                            rectSize = winWidth;
                            tempArr = new int[(rectSize + 1) * (rectSize + 1)];
                            for (I = 0; I < rectSize; I++) {
                                for (j = 0; j < rectSize; j++) {
                                    tempArr[I * rectSize + j] = iArr[X+j+(I+Y)*NX];//[X + I + (Y + j) * rectSize];
                                }
                            }
                            cen = floatCentroid(tempArr, X, Y, rectSize);
                            //GetCirclePosition(va[], val(x,y)) 'find the index of ranking current point (x,y) value
                            iCounter = GetCirclePosition(va, iArr[X + Y * NX]);
                            if (iCounter < maxPoints)
                            {
                                for (int i = maxPoints-1; i > iCounter; i--)
                                {
                                    pp[i] = pp[i - 1];
                                    va[i] = va[i - 1];
                                }
                                pp[iCounter].p.X = cen.X;
                                pp[iCounter].p.Y = cen.Y;
                                pp[iCounter].v = iArr[X + Y * NX];
                                va[iCounter]= iArr[X + Y * NX];
                            }
                        }
                    }
                }
            }
            return pp;
        }

        public static COORD floatCentroid(int[] iArr, int X, int Y, int rectSize) {
            int xcen;
            int ycen;
            long sumX;
            long sumY;
            double sumI;
            int I;
            int j;
            COORD ret=new COORD();
            sumX = 0;
            sumY = 0;
            sumI = 0;
            for (I = 0; I <= rectSize; I++) {
                for (j = 0; j <= rectSize; j++) {
                    sumX = sumX + iArr[I*rectSize+ j] * I;
                    sumY = sumY + iArr[I * rectSize + j] * j;
                    sumI = sumI + iArr[I * rectSize + j];
                }
            }
            xcen = X + (int)(sumX / sumI);
            ycen = Y + (int)(sumY / sumI);
            ret.X = xcen;
            ret.Y = ycen;
            return ret;
        }

        public static int GetCirclePosition(int[] va, int val)
        {
            for(int i = 0; i < va.Length; i++)
            {
                if (val > va[i]) return i;
            }
            return va.Length+1;
        }

        public static imageptr GetDataFromImage(Image img, imageptr image, double gamma, transform_type trans, int iMin, int iMax)
        {
            imageptr result = image;
            transform_config transformer = transform_init(image, gamma, trans, iMin, iMax);
            result.data = new int[image.xsize * image.ysize];
            int maxVal = image.datapeak;
            for (int y = 0; y < image.ysize; y++)
            {
                for (int x = 0; x < image.xsize; x++)
                {
                    int val = (int)((double)maxVal * (double)((System.Drawing.Bitmap)img).GetPixel(x, y).GetBrightness());
                    result.data[y*image.xsize+x]=val;
                }
            }
            //for (int i = 0; i < image.ysize*image.xsize; i++)
            //{
            //    int val = transform_do(transformer, result.data[i]);
            //    result.data[i] = val;
            //}
            result = image_minmax_update(result);
            return result;
        }

        public static int[,] ConvertImagePtrToIntArray(int[] img, int w, int h)
        {
            int[,] ret = new int[w, h];
            for(int y=0; y < h; y++)
            {
                for(int x = 0; x < w; x++)
                {
                    ret[x, y] = img[y * w + x];
                }
            }
            return ret;
        }

        public static int[] ConvertIntArrayToImagePtr(int[,] data, int w, int h)
        {
            int[] ret = new int[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    ret[y * w + x] = data[x,y];
                }
            }
            return ret;
        }

        public static int[] getHistValues(int[] data, int maxX, int maxY,int maxVal)
        {
            int[] ret = new int[256];//*256
            //int thisMax = 0;
            for (int y = 0; y < maxY; y++)
                for (int x = 0; x < maxX; x++)
                {
                    ret[(int)(data[y * maxX + x]/256)] += 1;
                    //if (ret[data[y * maxX + x]] > maxVal) thisMax = data[y * maxX + x];
                }
            //Console.Out.WriteLine("getHistValues Max val={0}", thisMax);
            //int[] retout = new int[thisMax + 1000];
            //for (int i = 0; i < thisMax; i++) retout[i] = ret[i];
            return ret;//retout
        }

        public static imageptr TransformImagePtr(imageptr img, double rads, double dx, double dy, double sca=1.0)
        {
            imageptr retimage = new imageptr();
            int w = img.xsize;
            int h = img.ysize;
            retimage.bitpix = img.bitpix;
            retimage.datamin = img.datamin;
            retimage.datapeak = img.datapeak;
            retimage.data_lower_decile = img.data_lower_decile;
            retimage.data_upper_decile = img.data_upper_decile;
            retimage.mean_ld_excess = img.mean_ld_excess;
            retimage.object_name = img.object_name;
            retimage.xsize = w;
            retimage.ysize = h;
            retimage.data = new int[w * h];
            for (int i = 0; i < w * h; i++) retimage.data[i] = 0;
            //convert int[] array to a [x,y] array to use it in the transformation
            //    int[,] arr = ConvertImagePtrToIntArray(img.data, w, h);
            //    int[,] retarr = new int[w, h];
            //int idx = (int)dx;
            //int idy = (int)dy;
            //  double offsetX = (w) / 2 + dx;
            //  double offsetY = (h) / 2 + dy;

            double cost = Math.Cos(rads);
            double sint = Math.Sin(rads);
            for(int y=0;y<img.ysize;y++)
                for(int x = 0; x < img.xsize; x++)
                {
                    int newX = (int)(sca * x * cost - sca * y * sint + dx);
                    int newY = (int)(sca * x * sint + sca * y * cost + dy);
                    if (newX >= 0 && newX < retimage.xsize && newY >= 0 && newY < retimage.ysize)
                        retimage.data[newX + newY * retimage.xsize] = img.data[x + y * img.xsize];
                }
            

            //rotate arr by angle rads(in rads) and translation dx,dy
            //https://softwarebydefault.com/2013/06/16/image-transform-rotate/
            //above link uses only rotation
            /*for (int x=0; x<w; x++)
            {
                for(int y = 0; y < h; y++)
                {
                    double X = x + dx;
                    double Y = y + dy;
                    int XX = (int)(Math.Round((X - offsetX) * Math.Cos(rads) - (Y - offsetY) * Math.Sin(rads) + offsetX ));//-16
                    int YY = (int)(Math.Round((X - offsetX) * Math.Sin(rads) + (Y - offsetY) * Math.Cos(rads) + offsetY ));//+ 22
                    if (XX > 0 && XX < w && YY > 0 && YY < h) retarr[XX, YY] = arr[x, y];
                }
            }*/
            //convert int[x,y] array to int[] array
            //  retimage.data = ConvertIntArrayToImagePtr(retarr, w, h);
            image_minmax_update(retimage);
            return retimage;
        }

        /// <summary>
        /// Adds a brightness value in a given value (val)
        /// </summary>
        /// <param name="val">the value where to add the brightness</param>
        /// <param name="brightness">the brightness value</param>
        /// <returns></returns>
        public static int SetBrightness(int val, int brightness)
        {
            return val + brightness;
        }

        /// <summary>
        /// Sets the contrast of a value in a given value (given a max value)
        /// </summary>
        /// <param name="val">the pixel value which will be changed by contrast</param>
        /// <param name="contrast">th contrast value</param>
        /// <param name="max">a value to be used for contrast calculation</param>
        /// <returns></returns>
        public static int SetContrast(int val, int contrast, int max = 2 * Int16.MaxValue)
        {
            float F = (max + 4) * (contrast + max) / (max * (max + 4f - contrast));
            return (int)(F * (val - max / 2.0) + max / 2.0);
        }


        //iValue in the range 0-255
        public static Bitmap AdjustBrightness(Bitmap img, int iValue)
        {
            Bitmap TempBitmap = img;
            Bitmap NewBitmap = new Bitmap(img.Width, img.Height);
            Graphics NewGraphics = Graphics.FromImage(NewBitmap);
            float FinalValue = (float)iValue / 255.0f;
            float[][] FloatColorMatrix ={
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 0, 0},
                    new float[] {0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {FinalValue, FinalValue, FinalValue, 1, 1}
            };
            ColorMatrix NewColorMatrix = new ColorMatrix(FloatColorMatrix);
            ImageAttributes Attributes = new ImageAttributes();
            Attributes.SetColorMatrix(NewColorMatrix);
            NewGraphics.DrawImage(TempBitmap, new Rectangle(0, 0, TempBitmap.Width, TempBitmap.Height), 0, 0, TempBitmap.Width, TempBitmap.Height, GraphicsUnit.Pixel, Attributes);
            Attributes.Dispose();
            NewGraphics.Dispose();
            return NewBitmap;
        }

        //threshold in the range -100 to +100
        public static Bitmap AdjustContrast(Bitmap sourceBitmap, int threshold)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
                                        sourceBitmap.Width, sourceBitmap.Height),
                                        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);
            double contrastLevel = Math.Pow((100.0 + threshold) / 100.0, 2);
            double blue = 0;
            double green = 0;
            double red = 0;
            for (int k = 0; k + 4 < pixelBuffer.Length; k += 4)
            {
                blue  = ((((pixelBuffer[k    ] / 255.0) - 0.5) * contrastLevel) + 0.5) * 255.0;
                green = ((((pixelBuffer[k + 1] / 255.0) - 0.5) * contrastLevel) + 0.5) * 255.0;
                red   = ((((pixelBuffer[k + 2] / 255.0) - 0.5) * contrastLevel) + 0.5) * 255.0;
                if (blue > 255){ blue = 255; }
                else if (blue < 0){ blue = 0; }
                if (green > 255){ green = 255; }
                else if (green < 0){ green = 0; }
                if (red > 255){ red = 255; }
                else if (red < 0){ red = 0; }
                pixelBuffer[k] = (byte)blue;
                pixelBuffer[k + 1] = (byte)green;
                pixelBuffer[k + 2] = (byte)red;
            }
            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0,
                                        resultBitmap.Width, resultBitmap.Height),
                                        ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(pixelBuffer, 0, resultData.Scan0, pixelBuffer.Length);
            resultBitmap.UnlockBits(resultData);
            return resultBitmap;
        }

        public static Bitmap AdjustBrightContrastGamma(Bitmap img, float brightness, float contrast, float gamma)
        {
            //Bitmap img;
            Bitmap adjustedImage=new Bitmap(img.Width, img.Height);
            //float brightness = 1.0f; // no change in brightness
            //float contrast = 2.0f; // twice the contrast
            //float gamma = 1.0f; // no change in gamma

            float adjustedBrightness = brightness - 1.0f;
            // create matrix that will brighten and contrast the image
            float[][] ptsArray ={
                                 new float[] {contrast, 0, 0, 0, 0}, // scale red
                                 new float[] {0, contrast, 0, 0, 0}, // scale green
                                 new float[] {0, 0, contrast, 0, 0}, // scale blue
                                 new float[] {0, 0, 0, 1.0f, 0}, // don't scale alpha
                                 new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}
                                };
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
            Graphics g = Graphics.FromImage(adjustedImage);
            g.DrawImage(img, new Rectangle(0, 0, adjustedImage.Width, adjustedImage.Height),
                0, 0, img.Width, img.Height,
                GraphicsUnit.Pixel, imageAttributes);
            return adjustedImage;
        }

        /// <summary>
        /// Resizes template by either neibour or bi-linear algorithm
        /// </summary>
        /// <param name="tmp">imageptr structure of template</param>
        /// <param name="subj">imageptr structure of subject</param>
        /// <param name="scale_type">the type of scaling (0=neibour, 1=bi-linear)</param>
        /// <returns>returns the imageptr structure of the scaled image</returns>
        public static imageptr ResizeImageptr(imageptr tmp, imageptr subj, int scale_type)
        {
            imageptr ret = new imageptr();
            switch (scale_type)
            {
                case 0://neibour algorithm
                    ret = scale_fits_neibour(tmp, subj);
                    break;
                case 1://bi-linear algorithm
                    float scaleX = subj.xsize / tmp.xsize;
                    float scaleY = subj.ysize / tmp.ysize;
                    float scale = Math.Min(scaleX, scaleY);
                    ret = scale_fits_bilinear(tmp, (int)(scale*tmp.xsize), (int)(scale*tmp.ysize));
                    break;
            }
            return ret;
        }

        /// <summary>
        /// Performs a basic 'pixel' enlarging re-sampling.
        /// </summary>
        /// <param name="tmp">imageptr structure of template</param>
        /// <param name="subj">imageptr structure of subject</param>
        /// <returns>returns the imageptr structure of the scaled image</returns>
        static imageptr scale_fits_neibour(imageptr tmp, imageptr subj)
        {
            //if one of images is null then return NULL
            if (tmp.data == null) return CreateBlackImage(subj.xsize, subj.ysize);
            if (subj.data == null) return CreateBlackImage(tmp.xsize, tmp.ysize);
            //if both images are of the same size there is nothing to scale, so return tmp
            if (tmp.xsize == subj.xsize && tmp.ysize == subj.ysize)
            {
                Console.Out.WriteLine("Nothing to scale. Template and Subject are of the same size\n");
                return tmp;
            }
            //We have images of different sizes so start with getting sizes
            int _width = tmp.xsize;
            int _height = tmp.ysize;
            int newWidth = subj.xsize;
            int newHeight = subj.ysize;
            // Get a new buffer to interpolate into
            int[] newData = new int[newWidth * newHeight * sizeof(int)];
            // set the width, height scale values
            double scaleWidth = (double)newWidth / (double)_width;
            double scaleHeight = (double)newHeight / (double)_height;
            for (int cy = 0; cy < newHeight; cy++)
            {
                for (int cx = 0; cx < newWidth; cx++)
                {
                    int pixel = (cy * (newWidth)) + (cx);
                    int nearestMatch = (((int)(cy / scaleHeight) * (_width)) + ((int)(cx / scaleWidth)));
                    newData[pixel] = tmp.data[nearestMatch];
                }
            }
            //reset new values in tmp pointer
            tmp.data = newData;
            tmp.xsize = newWidth;
            tmp.ysize = newHeight;
            
            Console.Out.WriteLine("{0} Template scaled from {1} x {2} to {3} x {4} using nearest neibour algorithm\n", friendly_timestring(), _width, _height, newWidth, newHeight);    
            //if(newData)free(newData);
            return tmp;
        }

        static float lerp(float s, float e, float t) { return s + (e - s) * t; }

        static float blerp(float c00, float c10, float c01, float c11, float tx, float ty)
        {
            return lerp(lerp(c00, c10, tx), lerp(c01, c11, tx), ty);
        }

        /// <summary>
        /// Performs a Bilinear Interpolation 'pixel' enlarging re-sampling (http://rosettacode.org)
        /// </summary>
        /// <param name="src">the imageptr structure of source</param>
        /// <param name="wi">the width of the finally scaled source</param>
        /// <param name="hi">the height of the finally scaled source</param>
        /// <returns>returns the scaled imageptr structure</returns>
        static imageptr scale_fits_bilinear(imageptr src, int wi, int hi)
        {
            //if image data is null then return NULL
            if (src.data == null) return CreateBlackImage(wi, hi);
            //if image size is of wi X hi there is nothing to scale, so return tmp
            if (src.xsize == wi && src.ysize == hi)
            {
                Console.Out.WriteLine("Nothing to scale. Template and Subject are of the same size\n");
                return src;
            }
            //We have images of different sizes so start with getting sizes
            int newWidth = wi;
            int newHeight = hi;
            int w = src.xsize;
            int h = src.ysize;
            // Get a new buffer to interpolate into
            int[] newData = new int[newWidth * newHeight * sizeof(int)];
            int x, y;
            float gx = 0;
            float gy = 0;
            int gxi = 0, gyi = 0, result = 0, c00 = 0, c10 = 0, c01 = 0, c11 = 0;
            for (y = 0; y < newHeight; y++){
                for (x = 0; x < newWidth; x++){
                    gx = (w - 1) * x / (float)(newWidth);
                    gy = (h - 1) * y / (float)(newHeight);
                    gxi = (int)gx;
                    gyi = (int)gy;
                    result = 0;
                    c00 = src.data[(gyi * w) + gxi];
                    c10 = src.data[(gyi * w) + gxi + 1];
                    c01 = src.data[((gyi + 1) * w) + gxi];
                    c11 = src.data[((gyi + 1) * w) + gxi + 1];
                    result |= (int)blerp(c00, c10, c01, c11, gx - gxi, gy - gyi);
                    newData[(y * newWidth) + x] = result;
                }
            }
            src.data = newData;
            src.xsize = newWidth;
            src.ysize = newHeight;
            Console.Out.WriteLine("{0} Template scaled from {1} x {2} to {3} x {4} using bilinear algorithm\n", friendly_timestring(), w, h, newWidth, newHeight);
            //if(newData)free(newData);
            return src;
        }

        /// <summary>
        /// Method checks if passed string is double
        /// </summary>
        /// <param name="text">string text for checking</param>
        /// <returns>bool - if text is double return true, else return false</returns>
        public static bool IsDouble(string text)
        {
            //double num = 0;
            bool isDouble = false;
            // Check for empty string.
            if (string.IsNullOrEmpty(text)){
                return false;
            }
            isDouble = double.TryParse(text, out double num);
            return isDouble;
        }

        /// <summary>
        /// Method checks if passed string is integer
        /// </summary>
        /// <param name="text">string text for checking</param>
        /// <returns>bool - if text is integer return true, else return false</returns>
        public static bool IsNumber(string text)
        {
            //int num = 0;
            bool isNum = false;
            // Check for empty string.
            if (string.IsNullOrEmpty(text)){
                return false;
            }
            isNum = int.TryParse(text, out int num);
            return isNum;
        }

        public static double CalcImageEntropy(imageptr img, double ibase=2.0)
        {
            double entropy = 0;
            double sum = 0.0;
            int[] data = calc_histogram_total(img);
            double mmax = 0.0;// GREPNOVA_DATAMAX - GREPNOVA_DATAMIN+1;
            for (int i = 0; i < data.Length; i++) mmax += data[i];
            for (int i = 0; i < data.Length; i++){
                if (data[i] > 1) sum += ((double)data[i] / mmax) * Math.Log((double)data[i] / mmax, ibase);
            }
            entropy = -sum;
            return entropy;
        }


    }//End of Fits class
    ///////////////////////////////////////////////////////////////////////////////////////
    //*************************************************************************************
    ///////////////////////////////////////////////////////////////////////////////////////
    //*************************************************************************************
    ///////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// class Filters for filtering Bitmap images
    /// </summary>
    public static class Filters
    {
        private static Bitmap GetArgbCopy(this Image sourceImage)
        {
            Bitmap bmpNew = new Bitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bmpNew))
            {
                graphics.DrawImage(sourceImage, new Rectangle(0, 0, bmpNew.Width, bmpNew.Height), new Rectangle(0, 0, bmpNew.Width, bmpNew.Height), GraphicsUnit.Pixel);
                graphics.Flush();
            }
            return bmpNew;
        }

        //Negative of an image
        /// <summary>
        /// Returns the negative image of the calling image
        /// </summary>
        /// <param name="sourceImage">the calling image</param>
        /// <returns></returns>
        public static Bitmap CopyAsNegative(this Image sourceImage)
        {
            Bitmap bmpNew = GetArgbCopy(sourceImage);
            BitmapData bmpData = bmpNew.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            IntPtr ptr = bmpData.Scan0;
            byte[] byteBuffer = new byte[bmpData.Stride * bmpNew.Height];
            Marshal.Copy(ptr, byteBuffer, 0, byteBuffer.Length);
            byte[] pixelBuffer = null;
            int pixel = 0;
            for (int k = 0; k < byteBuffer.Length; k += 4){
                pixel = ~BitConverter.ToInt32(byteBuffer, k);
                pixelBuffer = BitConverter.GetBytes(pixel);
                byteBuffer[k] = pixelBuffer[0];
                byteBuffer[k + 1] = pixelBuffer[1];
                byteBuffer[k + 2] = pixelBuffer[2];
            }
            Marshal.Copy(byteBuffer, 0, ptr, byteBuffer.Length);
            bmpNew.UnlockBits(bmpData);
            bmpData = null;
            byteBuffer = null;
            return bmpNew;
        }

        //Median Filter for a Bitmap given a matrix size (e.g. 3x3 or 5x5)
        /// <summary>
        /// Returns the Median filtered image of the calling image with the given
        /// matrixSize (e.g 3x3 or 9x9)
        /// </summary>
        /// <param name="sourceBitmap">the bitmap where we perform the Filter</param>
        /// <param name="matrixSize">the matrix of the Filter</param>
        /// <param name="bias">a bias value for the Filter. Default is 0</param>
        /// <param name="grayscale">if convert the output to grayscale. Default is true</param>
        /// <returns></returns>
        public static Bitmap MedianFilter(this Image sourceBitmap, int matrixSize, int bias = 0, bool grayscale = true)
        {
            BitmapData sourceData = ((Bitmap)sourceBitmap).LockBits(new Rectangle(0, 0,
                                    sourceBitmap.Width, sourceBitmap.Height),
                                    ImageLockMode.ReadOnly,
                                    PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];
            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            ((Bitmap)sourceBitmap).UnlockBits(sourceData);
            if (grayscale == true){
                float rgb = 0;
                for (int k = 0; k < pixelBuffer.Length; k += 4){
                    rgb = pixelBuffer[k] * 0.11f;
                    rgb += pixelBuffer[k + 1] * 0.59f;
                    rgb += pixelBuffer[k + 2] * 0.3f;
                    pixelBuffer[k] = (byte)rgb;
                    pixelBuffer[k + 1] = pixelBuffer[k];
                    pixelBuffer[k + 2] = pixelBuffer[k];
                    pixelBuffer[k + 3] = 255;
                }
            }
            int filterOffset = (matrixSize - 1) / 2;
            int calcOffset = 0;
            int byteOffset = 0;

            List<int> neighbourPixels = new List<int>();
            byte[] middlePixel;

            for (int offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++){
                for (int offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++){
                    byteOffset = offsetY * sourceData.Stride + offsetX * 4;
                    neighbourPixels.Clear();
                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++){
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++){
                            calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);
                            neighbourPixels.Add(BitConverter.ToInt32(pixelBuffer, calcOffset));
                        }
                    }
                    neighbourPixels.Sort();
                    middlePixel = BitConverter.GetBytes(neighbourPixels[filterOffset]);
                    resultBuffer[byteOffset] = middlePixel[0];
                    resultBuffer[byteOffset + 1] = middlePixel[1];
                    resultBuffer[byteOffset + 2] = middlePixel[2];
                    resultBuffer[byteOffset + 3] = middlePixel[3];
                }
            }
            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            BitmapData resultData =
                        resultBitmap.LockBits(new Rectangle(0, 0,
                        resultBitmap.Width, resultBitmap.Height),
                        ImageLockMode.WriteOnly,
                        PixelFormat.Format32bppArgb);

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);
            return resultBitmap;
        }

        /// <summary>
        /// Returns the ColorBalanced image of the calling image or the reversed Balanced one 
        /// (depending on parameter reverse) by the value of red-blue-green level (0-255)
        /// </summary>
        /// <param name="sourceBitmap"></param>
        /// <param name="blueLevel"></param>
        /// <param name="greenLevel"></param>
        /// <param name="redLevel"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static Bitmap ColorBalance(this Image sourceBitmap, byte blueLevel, byte greenLevel, byte redLevel, bool reverse = false)
        {
            BitmapData sourceData = ((Bitmap)sourceBitmap).LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                                        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            ((Bitmap)sourceBitmap).UnlockBits(sourceData);
            float blue = 0;
            float green = 0;
            float red = 0;
            float blueLevelFloat = blueLevel;
            float greenLevelFloat = greenLevel;
            float redLevelFloat = redLevel;
            if (!reverse){
                for (int k = 0; k + 4 < pixelBuffer.Length; k += 4){
                    blue = 255.0f / blueLevelFloat * (float)pixelBuffer[k];
                    green = 255.0f / greenLevelFloat * (float)pixelBuffer[k + 1];
                    red = 255.0f / redLevelFloat * (float)pixelBuffer[k + 2];
                    if (blue > 255) { blue = 255; }
                    else if (blue < 0) { blue = 0; }
                    if (green > 255) { green = 255; }
                    else if (green < 0) { green = 0; }
                    if (red > 255) { red = 255; }
                    else if (red < 0) { red = 0; }
                    pixelBuffer[k] = (byte)blue;
                    pixelBuffer[k + 1] = (byte)green;
                    pixelBuffer[k + 2] = (byte)red;
                }
            }else{
                for (int k = 0; k + 4 < pixelBuffer.Length; k += 4){
                    blue = blueLevelFloat / 255.0f * (float)pixelBuffer[k];
                    green = greenLevelFloat / 255.0f * (float)pixelBuffer[k + 1];
                    red = redLevelFloat / 255.0f * (float)pixelBuffer[k + 2];
                    if (blue > 255) { blue = 255; }
                    else if (blue < 0) { blue = 0; }
                    if (green > 255) { green = 255; }
                    else if (green < 0) { green = 0; }
                    if (red > 255) { red = 255; }
                    else if (red < 0) { red = 0; }
                    pixelBuffer[k] = (byte)blue;
                    pixelBuffer[k + 1] = (byte)green;
                    pixelBuffer[k + 2] = (byte)red;
                }
            }
            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0,
                                        resultBitmap.Width, resultBitmap.Height),
                                       ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            Marshal.Copy(pixelBuffer, 0, resultData.Scan0, pixelBuffer.Length);
            resultBitmap.UnlockBits(resultData);
            return resultBitmap;
        }

        public static Bitmap NoiseRemoval(this Image IntensityImage)
        {
            /*It removes the pixel that is stood alone any where in the vicinity.
			 *It is supposed to be accurate 4 our System.*/

            Bitmap b2 = (Bitmap)IntensityImage.Clone();
            byte val;
            // GDI+ still lies to us - the return format is BGR, NOT RGIntensityImage.
            BitmapData bmData = ((Bitmap)IntensityImage).LockBits(new Rectangle(0, 0, IntensityImage.Width, IntensityImage.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bmData2 = b2.LockBits(new Rectangle(0, 0, IntensityImage.Width, IntensityImage.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            System.IntPtr Scan02 = bmData2.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                byte* p2 = (byte*)(void*)Scan02;

                int nOffset = stride - IntensityImage.Width * 3;
                int nWidth = IntensityImage.Width * 3;

                //int nPixel=0;

                p += stride;
                p2 += stride;
                //int val;
                for (int y = 1; y < IntensityImage.Height - 1; ++y)
                {
                    p += 3;
                    p2 += 3;

                    for (int x = 3; x < nWidth - 3; ++x)
                    {
                        val = p2[0];
                        if (val == 0)
                            if ((p2 + 3)[0] == 0 || (p2 - 3)[0] == 0 || (p2 + stride)[0] == 0 || (p2 - stride)[0] == 0 || (p2 + stride + 3)[0] == val || (p2 + stride - 3)[0] == 0 || (p2 - stride - 3)[0] == 0 || (p2 + stride + 3)[0] == 0)
                                p[0] = val;
                            else
                                p[0] = 0;//255

                        ++p;
                        ++p2;
                    }

                    p += nOffset + 3;
                    p2 += nOffset + 3;
                }
            }

            ((Bitmap)IntensityImage).UnlockBits(bmData);
            b2.UnlockBits(bmData2);
            return (Bitmap)IntensityImage;
        }

        public static System.Drawing.Image getHistogramWithSetSize(int width, int height, int[] pixelFreqs)
        {
            // Builds you a histogram of canvas size width*height. Takes ~2ms on a 300*300, 15ms on a 2000*1000
            // Bigger ones look cleaner, because the UI does a nice job of scaling them down, and it means that
            // the graphics libraries have a nicer time finding a path through the points.

            // We want to be able to fit at least one 256 (w) by 100 (h) histogram into the space.
            // If the user gave us bad parameters, we won't complain to them, we will just make it
            // bigger than they want it. If their UI is set up to do so, it will scale it down for them.
            if (width < 280) { width = 280; }
            if (height < 120) { height = 130; }
            int widthPerFrequency = width / 256;
            int histWidth = widthPerFrequency * 256; //Width of actual hist we will center horizontally.
            int spaceLeft = width - histWidth;
            int startX = spaceLeft / 2;
            // Define the brushes/pens we will use:
            var axisPen = new System.Drawing.Pen(System.Drawing.Brushes.DarkGray, 2);
            var gridPen = new System.Drawing.Pen(System.Drawing.Brushes.DarkGray, 0.5f);
            var linGrBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Point(startX, height - 10),
                new Point(startX + histWidth, height - 10),
                System.Drawing.Color.FromArgb(0, 0, 0),
                System.Drawing.Color.FromArgb(255, 255, 255)
                );
            var bluePen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(51, 153, 255), 2);
            var redPen = new System.Drawing.Pen(System.Drawing.Color.OrangeRed, 2);
            var polyBrush = new SolidBrush(System.Drawing.Color.FromArgb(120, 51, 153, 255));
            // Make canvas and get bitmap gfx handler g
            Bitmap hist = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(hist);
            // Paint the canvas white, add gradient bar
            g.FillRectangle(System.Drawing.Brushes.White, new Rectangle(0, 0, width, height));
            g.FillRectangle(linGrBrush, startX, height - 20, histWidth, 10);
            // Draw the axis
            g.DrawLine(axisPen, startX, 10, startX, height - 30); //y
            g.DrawLine(axisPen, startX, height - 30, startX + histWidth, height - 30); //x
            // Draw meaningless grid marks just beacause it looks nice
            // Draw them every 10th of an x length
            int seperatingPixel = histWidth / 10;
            for (int x = startX; x < startX + histWidth; x += seperatingPixel)
            {
                g.DrawLine(gridPen, x, 10, x, height - 30);
            }
            for (int y = height - 30; y > 10; y -= seperatingPixel)
            {
                g.DrawLine(gridPen, startX, y, startX + histWidth, y);
            }
            int biggestValue = pixelFreqs.Max();
            Point[] polygon = new Point[258];
            // Add a point for each and every pixel frequency
            for (int i = 0; i < 256; i++)
            {
                float percent = (float)pixelFreqs[i] / (float)biggestValue;
                percent = percent * (height - 40);
                int percInt = (int)percent;
                Point addedPoint = new Point((startX + widthPerFrequency * i), (height - 30 - percInt));
                polygon[i] = addedPoint;
            }
            polygon[256] = new Point(widthPerFrequency * 256, height - 30); //Add a line taking us back to the axis
            polygon[257] = new Point(startX, height - 30); //and back to the origin
            g.DrawLines(bluePen, polygon);
            g.FillPolygon(polyBrush, polygon);
            // Draw a line between the min and max values we are showing - insert your resampled values here if you have them
            g.DrawLine(redPen, startX + pixelFreqs.Min() * widthPerFrequency, height - 30, startX + pixelFreqs.Max() * widthPerFrequency, 10);
            return hist;
        }

        public static Bitmap HistEq(this Image img)
        {
            int w = img.Width;
            int h = img.Height;
            BitmapData sd = ((Bitmap)img).LockBits(new Rectangle(0, 0, w, h),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int bytes = sd.Stride * sd.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(sd.Scan0, buffer, 0, bytes);
            ((Bitmap)img).UnlockBits(sd);
            int current = 0;
            double[] pn = new double[256];
            for (int p = 0; p < bytes; p += 4)
            {
                pn[buffer[p]]++;
            }
            for (int prob = 0; prob < pn.Length; prob++)
            {
                pn[prob] /= (w * h);
            }
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    current = y * sd.Stride + x * 4;
                    double sum = 0;
                    for (int i = 0; i < buffer[current]; i++)
                    {
                        sum += pn[i];
                    }
                    for (int c = 0; c < 3; c++)
                    {
                        result[current + c] = (byte)Math.Floor(255 * sum);
                    }
                    result[current + 3] = 255;
                }
            }
            Bitmap res = new Bitmap(w, h);
            BitmapData rd = res.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(result, 0, rd.Scan0, bytes);
            res.UnlockBits(rd);
            return res;
        }

        public static Bitmap Normalization(this Image img, byte min=0, byte max=255 )
        {
            int w = img.Width;
            int h = img.Height;
            Bitmap img1 = (Bitmap)img.Clone();

            BitmapData sd = ((Bitmap)img1).LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, img1.PixelFormat);// PixelFormat.Format32bppArgb);
            int bytes = sd.Stride * sd.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(sd.Scan0, buffer, 0, bytes);
            ((Bitmap)img1).UnlockBits(sd);
            int current = 0;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    current = y * sd.Stride + x * 4;
                    for (int i = 0; i < 3; i++)
                    {
                        if (max != min)
                        {
                            result[current + i] = (byte)((buffer[current + i] - min) * 100 / (max - min));
                        }
                        else
                        {
                            result[current + i] = 255;
                        }
                    }
                    result[current + 3] = 255;
                }
            }
            Bitmap resimg = new Bitmap(w, h);
            BitmapData rd = resimg.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly, img1.PixelFormat);// PixelFormat.Format32bppArgb);
            Marshal.Copy(result, 0, rd.Scan0, bytes);
            resimg.UnlockBits(rd);
            return resimg;
        }

        public static Bitmap Stretch(this Image img, int realMin, int realMax, int min = 0, int max = 65535)
        {
            int w = img.Width;
            int h = img.Height;
            Bitmap img1 = (Bitmap)img.Clone();

            BitmapData sd = ((Bitmap)img1).LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, img1.PixelFormat);// PixelFormat.Format32bppArgb);
            int bytes = sd.Stride * sd.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(sd.Scan0, buffer, 0, bytes);
            ((Bitmap)img1).UnlockBits(sd);
            int current = 0;
            double f = (double)(max - min) / (double)(realMax - realMin);
            Console.Out.WriteLine("f = "+f+" RealMax="+realMax+" max="+max);

            double new_min = (double)min * 255.0 / (double)(realMax - realMin);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    current = y * sd.Stride + x * 4;
                    for (int i = 0; i < 3; i++)
                    {
                        if (max != min)
                        {
                            result[current + i]  =  (byte)(((double)buffer[current + i] - new_min) * f + (double)new_min);
                            //data[i] = (int)((double)(img.data[i] - img.datamin) * f + (double)img.datamin);
                        }
                        else
                        {
                            result[current + i] = 255;
                        }
                    }
                    result[current + 3] = 255;
                }
            }
            Bitmap resimg = new Bitmap(w, h);
            BitmapData rd = resimg.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly, img1.PixelFormat);// PixelFormat.Format32bppArgb);
            Marshal.Copy(result, 0, rd.Scan0, bytes);
            resimg.UnlockBits(rd);
            return resimg;
        }

        static bool isPowerOfTwo(int v)
        {

            return false;
        }
        public static Bitmap LowHighPassFilter(this Image img, int low, int high)
        {
            Bitmap img1 = (Bitmap)img.Clone();
            if (isPowerOfTwo(img.Width)==true && isPowerOfTwo(img.Height)==true)
            {
                // create complex image
                AForge.Imaging.ComplexImage complexImage = AForge.Imaging.ComplexImage.FromBitmap(img1);
                // do forward Fourier transformation
                complexImage.ForwardFourierTransform();
                // create filter
                AForge.Imaging.ComplexFilters.FrequencyFilter filter = new AForge.Imaging.ComplexFilters.FrequencyFilter(new AForge.IntRange(low, high));
                // apply filter
                filter.Apply(complexImage);
                // do backward Fourier transformation
                complexImage.BackwardFourierTransform();
                // get complex image as bitmat
                Bitmap fourierImage = complexImage.ToBitmap();

                // get frequency view
                return complexImage.ToBitmap();
            }
            else
            {
                return img1;
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////

    public class ImageManipulation : IDisposable
    {
        /*use it this way:
        
        using (ImageManipulation cls = new ImageManipulation(_bitmap))
        {
            ImageBox.Image.Dispose();
            ImageBox.Image = (Bitmap) cls.AdjustBrightContrastGamma((float) nudContrast.Value, 
                                                                    (float) nudBrightness.Value, 
                                                                    (float) nudGamma.Value).Clone();
        }*/


        Bitmap _internalBitmapMemory;

        public ImageManipulation(Bitmap bitmap)
        {
            _internalBitmapMemory = new Bitmap(bitmap);
        }

        public Bitmap AdjustBrightContrastGamma(float Contrast = 1, float Brightness = 1, float Gamma = 1)
        {
            float brightness = Brightness;      // no change in brightness
            float contrast = Contrast;          // twice the contrast
            float gamma = Gamma;                // no change in gamma

            float adjustedBrightness = brightness - 1.0f;
            // create matrix that will brighten and contrast the image
            float[][] ptsArray = {
                new float[] {contrast, 0, 0, 0, 0},     // scale red
                new float[] {0, contrast, 0, 0, 0},     // scale green
                new float[] {0, 0, contrast, 0, 0},     // scale blue
                new float[] {0, 0, 0, 1.0f, 0},     // don't scale alpha
                new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}
            };

            var imageAttributes = new ImageAttributes();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);

            Graphics g = Graphics.FromImage(_internalBitmapMemory);

            g.DrawImage(_internalBitmapMemory, 
                new Rectangle(0, 0, _internalBitmapMemory.Width, _internalBitmapMemory.Height)
                , 0, 0, _internalBitmapMemory.Width, _internalBitmapMemory.Height,
                GraphicsUnit.Pixel, imageAttributes);
            g.Dispose();

            return _internalBitmapMemory;
        }

        public void Dispose()
        {
            _internalBitmapMemory.Dispose();
            _internalBitmapMemory = null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////
    ///////                           Binary Image                          ////////
    ////////////////////////////////////////////////////////////////////////////////

    public class BinaryImage
    {
        //idx=y*maxx+x
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int[] Pixels { get; set; }
        public BinaryImage(int width, int height) {
            Pixels = new int[width * height];
            Width = width;
            Height = height;
        }
        public BinaryImage(int[] pixels, int width, int height) {
            Pixels = pixels;
            Width = width;// pixels.GetLength(0);
            Height = height;// pixels.GetLength(1);
        }
        public void Flush(int value) {
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                    Pixels[i*Width+j] = value;
        }
        public int[] Erode(int[,] structureElement) {
            int[] ret = new int[Width * Height];
            for (int y = 0; y < Height; y++){
                for (int x = 0; x < Width; x++){
                    bool keep = true;
                    for (int m = 0; m < structureElement.GetLength(1); m++){
                        for (int n = 0; n < structureElement.GetLength(0); n++){
                            if (Pixels[Math.Min(Math.Max(0, x + n - structureElement.GetLength(0) / 2), Width - 1) +
                                        Width*Math.Min(Math.Max(0, y + m - structureElement.GetLength(1) / 2), Height - 1)] <= structureElement[n, m] && structureElement[n, m] != 0)
                            {
                                keep = false;
                                break;
                            }
                        }
                    }
                    ret[y * Width + x] = keep ? Pixels[y * Width + x] : 0;
                }
            }
            return ret;
        }
        public int[] Boundary(int[,] structureElement) {
            int[] result = Erode(structureElement);
            return Minus(new BinaryImage(result, Width, Height));
        }
        public int[] Minus(BinaryImage image) {
            int[] minus = new int[Width * Height];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    minus[y*Width+x] = Pixels[y * Width + x] - image.Pixels[y * Width + x];
            return minus;
        }
    }
}
