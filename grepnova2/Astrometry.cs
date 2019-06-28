using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using AllSkyPlateSolver;
using System.Diagnostics;
using System.Threading;
using software.elendil.AstrometryNet;
using software.elendil.AstrometryNet.Enum;
using software.elendil.AstrometryNet.Json;


namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
    }

    public class PlateSolver1
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PlateSolve(string fileName, int focalLength , double pixelSize, 
                                        double currentRA, double currentDec, double nearRadius);

        const int LOAD_LIBRARY_AS_DATAFILE = 0x00000002;

        public static void runPlateSolve(string fn) {
            IntPtr pDll = NativeMethods.LoadLibraryEx(@"ASPS.dll", IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
            if (pDll == IntPtr.Zero) { Console.Out.WriteLine("Could not load ASPS.dll"); Form1.frm.LogEntry("Could not load ASPS.dll"); return; }

            IntPtr pAddressOfFunctionToCall = NativeMethods.GetProcAddress(pDll, "PlateSolver");
            if(pAddressOfFunctionToCall == IntPtr.Zero) { Console.Out.WriteLine("Could not find PlateSolver"); Form1.frm.LogEntry("Could not find PlateSolver"); return; }


            PlateSolve plateSolve = (PlateSolve)Marshal.GetDelegateForFunctionPointer(
                                        pAddressOfFunctionToCall,
                                        typeof(PlateSolve));

            plateSolve(fn, 1217, 9, 0, 0, 0);


            bool result = NativeMethods.FreeLibrary(pDll);
            //remaining code here

            Console.WriteLine("Exiting runPlateSolve");
        }

        //version 1.2.0   @ 20190317
        //Astrometry PlateSolver using Astrometry.net APIs
        //It takes about 10-20 seconds to solve a plate
        public static string astrometryPlateSolve(string fileName) //, int focalLength, double pixelSize,
                                        //double currentRA, double currentDec, double nearRadius)
        {
            const string apiKey = Astrometry.ASTROMETRY_NET_PASS;
            string file = fileName;
            string ret = "";
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            try
            {
                var client = new Client(apiKey);
                var res = client.Login();
                Console.WriteLine("Login : " + res.status);
                ret += "Login : " + res.status + "\r\n";
                //Form1.frm.LogEntry("Login : " + res.status);
                Console.WriteLine("Uploading file...");
                var uploadArguments = new UploadArgs { publicly_visible = Visibility.n };
                var uploadResponse = client.Upload(file, uploadArguments);
                Console.WriteLine("File uploaded. Starting tasks...");
                Task<SubmissionStatusResponse> submissionStatusResponse = client.GetSubmissionStatus(uploadResponse.subid, token);
                Task<JobStatusResponse> jobStatusResponse = client.GetJobStatus(submissionStatusResponse.Result.jobs[0], token);
                Console.WriteLine("Tasks returned...");
                if (jobStatusResponse.Result.status.Equals(ResponseJobStatus.success)){
                    var calibrationResponse = client.GetCalibration(submissionStatusResponse.Result.jobs[0]);
                    var objectsInFieldResponse = client.GetObjectsInField(submissionStatusResponse.Result.jobs[0]);

                    Console.WriteLine("\nRA         : " + calibrationResponse.ra);
                    Console.WriteLine("Dec         : " + calibrationResponse.dec);
                    Console.WriteLine("Radius      : " + calibrationResponse.radius);
                    Console.WriteLine("Orientation : " + calibrationResponse.orientation);
                    Console.WriteLine("Pixelscale  : " + calibrationResponse.pixscale);
                    ret += "RA : " + calibrationResponse.ra;
                    ret += "Dec : " + calibrationResponse.dec;
                    ret += "radius : " + calibrationResponse.radius;
                    //Form1.frm.LogEntry("RA : " + calibrationResponse.ra);
                    //Form1.frm.LogEntry("Dec : " + calibrationResponse.dec);
                    //Form1.frm.LogEntry("radius : " + calibrationResponse.radius);

                    Console.WriteLine("");
                    foreach (string obj in objectsInFieldResponse.objects_in_field){
                        Console.WriteLine(obj);
                    }
                }else{
                    Console.WriteLine("Status : " + jobStatusResponse.Result.status);
                }
            }catch (Exception e){
                Console.WriteLine(e.Message);
            }finally{
                tokenSource.Dispose();
                //Form1.frm.LogEntry("SolvePlate finished...");
                Console.WriteLine("Finished ...");    
            }
            return ret;
        }

    }


    class Astrometry
    {
        public const string ASTROMETRY_NET_PASS = "ayfpuzfiajaejgyw";

        [DataContract]
        public class AuthResponse
        {
            [DataMember]
            private string status;
            [DataMember]
            private string message;
            [DataMember]
            private string session;

            internal string Status { get => status; set => status = value; }
            internal string Message { get => message; set => message = value; }
            internal string Session { get => session; set => session = value; }

            /*public AuthResponse() {
                this.status = "";
                this.Message = "";
                this.session = "";
            }*/
        }

        [DataContract]
        public class UploadResponse
        {
            [DataMember]
            internal string status;
            [DataMember]
            internal string subid;
            [DataMember]
            internal string hash;

            public UploadResponse() {
                this.status = "";
                this.subid = "";
                this.hash = "";
            }
        }

        public AuthResponse AstrometryLogin() {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://nova.astrometry.net/api/login");
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Method = "POST";
            
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())){
                string json = "request-json=" + WebUtility.UrlEncode("{\"apikey\": \"" + ASTROMETRY_NET_PASS + "\"}");
                //string json = "request-json=%7B%22apikey%22%3A+%22" + ASTROMETRY_NET_PASS + "%22%7D";
                Console.Out.WriteLine("json = " + json);
                //Console.Out.WriteLine("json = " + "request-json=%7B%22apikey%22%3A+%22" + ASTROMETRY_NET_PASS + "%22%7D");
                streamWriter.Write(json);// (byte1, 0, byte1.Length);
                streamWriter.Flush();
                streamWriter.Close();
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string result="";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream())){
                result = streamReader.ReadToEnd();
            }
            return ReadToObject(result.ToString());
        }

        // Deserialize a JSON stream to a AuthResponse object.  
        public static AuthResponse ReadToObject(string json) {
            AuthResponse deserializedUser = new AuthResponse();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(deserializedUser.GetType());
            deserializedUser = ser.ReadObject(ms) as AuthResponse;
            ms.Close();
            return deserializedUser;
        }

        public UploadResponse AstrometryUloadFile(string filePath, string session) {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://nova.astrometry.net/api/upload");
            httpWebRequest.ContentType = "multipart/form-data";
            httpWebRequest.Method = "POST";
            MultipartFormDataContent form = new MultipartFormDataContent();
            //"request-json=" + 
            string json = WebUtility.UrlEncode("{\"session\": \"" + session + "\"}");
            form.Add(new StringContent(json, Encoding.ASCII, "text/plain"), "\"request-json\"");
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            form.Add(new StreamContent(fileStream), "\"file\"", "\"17288.fts\"");
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())){
                streamWriter.Write(form);
                streamWriter.Flush();
                streamWriter.Close();
            }
            string result = "No Response";
            try{
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                try{
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream())){
                        result = streamReader.ReadToEnd();
                    }
                }catch(Exception ee) { Console.Out.WriteLine("Error in httpResponse.GetResponseStream: " + ee.Message); }
            }catch(WebException ex) { Console.Out.WriteLine("Error in httpWebRequest.GetResponse: " + ex.Message); }
            if (result.Equals("No Response")) return null;
            else return ReadToObjectUpload(result.ToString());
        }

        // Deserialize a JSON stream to a UploadResponse object.  
        public static UploadResponse ReadToObjectUpload(string json) {
            UploadResponse deserializedUload = new UploadResponse();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(deserializedUload.GetType());
            deserializedUload = ser.ReadObject(ms) as UploadResponse;
            ms.Close();
            return deserializedUload;
        }

        public struct TICPOS_struct
        {
            /// <summary>
            /// Double (in): length of axis in DEGREES
            /// </summary>
            public double deglen;
            /// <summary>
            /// Integer (in): length of axis in plotting units(pixels)
            /// </summary>
            public int pixlen;
            /// <summary>
            /// Double (in/out): distance between tic marks(pixels), positive scalar
            /// </summary>
            public double ticsize;
            /// <summary>
            /// Double (out): incremental value for tic marks in round units given by the UNITS parameter
            /// </summary>
            public double incr;
            /// <summary>
            /// String (out): giving units of ticsize, either 'ARC SECONDS', 'ARC MINUTES', or 'DEGREES'
            /// </summary>
            public string units;
        }

        public TICPOS_struct TicPos(double deglen, int pixlen, out double ticsize, out double incr, out string units) {
            TICPOS_struct ticpos = new TICPOS_struct{
                deglen = deglen,
                pixlen = pixlen
            };
            ticpos = TicPos(ticpos);
            ticsize = ticpos.ticsize;
            incr = ticpos.incr;
            units = ticpos.units;
            return ticpos;
        }

        public static TICPOS_struct TicPos(TICPOS_struct ticpos) {
            // NAME:
            //       TicPos
            // PURPOSE:
            //       Specify distance between tic marks for astronomical coordinate overlays
            // EXPLANATION:
            //       User inputs number an approximate distance
            //       between tic marks, and the axis length in degrees.TicPos will return 
            //       a distance between tic marks such that the separation is a round
            //       multiple in arc seconds, arc minutes, or degrees
            //
            // CALLING SEQUENCE:
            //       TicPos(double deglen, int pixlen, out double ticsize, out double incr, out string units)
            //
            // INPUTS:
            //       deglen - length of axis in DEGREES
            //       pixlen - length of axis in plotting units(pixels)
            //       ticsize - distance between tic marks(pixels).  This value will be
            //               adjusted by TICPOS such that the distance corresponds to
            //               a round multiple in the astronomical coordinate.
            //
            // OUTPUTS:
            //       ticsize - distance between tic marks(pixels), positive scalar
            //       incr    - incremental value for tic marks in round units given 
            //               by the UNITS parameter
            //       units - string giving units of ticsize, either 'ARC SECONDS',
            //               'ARC MINUTES', or 'DEGREES'
            //
            // EXAMPLE:
            //       Suppose a 512 x 512 image array corresponds to 0.2 x 0.2 degrees on
            //       the sky.A tic mark is desired in round angular units, approximately
            //       every 75 pixels.
            //
            //       ticpos= new TicPos();
            //       ticpos.ticsize = 75;
            //       ticpos.pixlen = 512;
            //       ticpos.deglen = 0.2;
            //       ticpos = TicPos(ticpos);
            //       ==> ticpos.ticsize = 85.333, ticpos.incr = 2. ticpos.units = 'Arc Minutes'
            //       i.e.a good tic mark spacing is every 2 arc minutes, corresponding to 85.333 pixels.
            //
            // HISTORY:
            //      Converted in C# by Cooknas (2019) from IDL Astronomy User's Library of NASA
            //      Initially written by W.Landsman November, 1988
            //      Converted to IDL V5.0 W.Landsman September 1997
            //      Fix case where incr crosses deg/min or min/deg boundary A.Mortier/W.Landsman April 2005
            double minpix = ticpos.deglen * 60.0 / ticpos.pixlen; //Arc minute per pixel
            ticpos.incr = minpix * ticpos.ticsize; // Arc minutes between tics
            int sgn = 1;
            if (ticpos.incr < 0.0) sgn = -1; else sgn = 1;
            ticpos.incr = Math.Abs(ticpos.incr);
            if (ticpos.incr >= 30.0) ticpos.units = "Degrees";
            else if (ticpos.incr <= 0.5) ticpos.units = "Arc Seconds";
            else ticpos.units = "Arc Minutes";
            // determine increment
            if (ticpos.incr >= 120.0) ticpos.incr = 4.0;// degrees
            else if (ticpos.incr >= 60.0) ticpos.incr = 2.0; // degrees
            else if (ticpos.incr >= 30.0) ticpos.incr = 1.0; // degrees
            else if (ticpos.incr >  15.0) ticpos.incr = 30.0; // minutes
            else if (ticpos.incr >= 10.0) ticpos.incr = 15.0; // minutes
            else if (ticpos.incr >= 5.0) ticpos.incr = 10.0; // minutes
            else if (ticpos.incr >= 2.0) ticpos.incr = 5.0; // minutes
            else if (ticpos.incr >= 1.0) ticpos.incr = 2.0; // minutes
            else if (ticpos.incr >  0.5) ticpos.incr = 1.0; // minutes
            else if (ticpos.incr >= 0.25) ticpos.incr = 30.0; // seconds
            else if (ticpos.incr >= 0.16) ticpos.incr = 15.0; // seconds
            else if (ticpos.incr >= 0.08) ticpos.incr = 10.0; // seconds
            else if (ticpos.incr >= 0.04) ticpos.incr = 5.0; // seconds
            else if (ticpos.incr >= 0.02) ticpos.incr = 2.0; // seconds
            else if (ticpos.incr <  0.02) ticpos.incr = 1.0; // seconds

            if (ticpos.units.Equals("Arc Seconds")) minpix = minpix * 60.0;
            else if (ticpos.units.Equals("Degrees")) minpix = minpix / 60.0;

            ticpos.ticsize = ticpos.incr / Math.Abs(minpix); //determine ticsize
            ticpos.incr = ticpos.incr * sgn;
            return ticpos;
        }
    }

    /*  worldpos.c -- WCS Algorithms from Classic AIPS.
        Copyright (C) 1994
        Associated Universities, Inc. Washington DC, USA.
   
        This library is free software; you can redistribute it and/or modify it
        under the terms of the GNU Library General Public License as published by
        the Free Software Foundation; either version 2 of the License, or (at your
        option) any later version.
   
        This library is distributed in the hope that it will be useful, but WITHOUT
        ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
        FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Library General Public
        License for more details.
   
        You should have received a copy of the GNU Library General Public License
        along with this library; if not, write to the Free Software Foundation,
        Inc., 675 Massachusetts Ave, Cambridge, MA 02139, USA.
   
        Correspondence concerning AIPS should be addressed as follows:
               Internet email: aipsmail@nrao.edu
               Postal address: AIPS Group
                               National Radio Astronomy Observatory
                               520 Edgemont Road
                               Charlottesville, VA 22903-2475 USA

                     -=-=-=-=-=-=-

        These two ANSI C functions, worldpos() and xypix(), perform
        forward and reverse WCS computations for 8 types of projective
        geometries ("-SIN", "-TAN", "-ARC", "-NCP", "-GLS", "-MER", "-AIT"
        and "-STG"):

            worldpos() converts from pixel location to RA,Dec 
            xypix()    converts from RA,Dec         to pixel location   

        where "(RA,Dec)" are more generically (long,lat). These functions
        are based on the WCS implementation of Classic AIPS, an
        implementation which has been in production use for more than ten
        years. See the two memos by Eric Greisen

            ftp://fits.cv.nrao.edu/fits/documents/wcs/aips27.ps.Z
        ftp://fits.cv.nrao.edu/fits/documents/wcs/aips46.ps.Z

        for descriptions of the 8 projective geometries and the
        algorithms.  Footnotes in these two documents describe the
        differences between these algorithms and the 1993-94 WCS draft
        proposal (see URL below). In particular, these algorithms support
        ordinary field rotation, but not skew geometries (CD or PC matrix
        cases). Also, the MER and AIT algorithms work correctly only for
        CRVALi=(0,0). Users should note that GLS projections with yref!=0
        will behave differently in this code than in the draft WCS
        proposal.  The NCP projection is now obsolete (it is a special
        case of SIN).  WCS syntax and semantics for various advanced
        features is discussed in the draft WCS proposal by Greisen and
        Calabretta at:
    
            ftp://fits.cv.nrao.edu/fits/documents/wcs/wcs.all.ps.Z
    
                    -=-=-=-

        The original version of this code was Emailed to D.Wells on
        Friday, 23 September by Bill Cotton <bcotton@gorilla.cv.nrao.edu>,
        who described it as a "..more or less.. exact translation from the
        AIPSish..". Changes were made by Don Wells <dwells@nrao.edu>
        during the period October 11-13, 1994:
        1) added GNU license and header comments
        2) added testpos.c program to perform extensive circularity tests
        3) changed float-->double to get more than 7 significant figures
        4) testpos.c circularity test failed on MER and AIT. B.Cotton
           found that "..there were a couple of lines of code [in] the wrong
           place as a result of merging several Fortran routines." 
        5) testpos.c found 0h wraparound in xypix() and worldpos().
        6) E.Greisen recommended removal of various redundant if-statements,
           and addition of a 360d difference test to MER case of worldpos(). 
     */

    /// <summary>
    /// World Coordinate System Utilities
    /// </summary>
    public class WCSUtil
    {
        /// <summary>
        /// routine to determine accurate position for pixel coordinates          
        /// returns 0 if successful otherwise:                                    
        /// 1 = angle too large for projection;                                   
        /// (WDP 1/97: changed the return value to 501 instead of 1)              
        /// does: -SIN, -TAN, -ARC, -NCP, -GLS, -MER, -AIT projections            
        /// anything else is linear (== -CAR)                                     
        /// </summary>
        /// <param name="xpix">x pixel number  (RA or longtitude without rotation)</param>
        /// <param name="ypix">y pixel number  (dec or latitude without rotation)</param>
        /// <param name="xref">x reference coordinate value (deg)</param>
        /// <param name="yref">y reference coordinate value (deg)</param>
        /// <param name="xrefpix">x reference pixel</param>
        /// <param name="yrefpix">y reference pixel</param>
        /// <param name="xinc">x coordinate increment (deg)</param>
        /// <param name="yinc">y coordinate increment (deg)</param>
        /// <param name="rot">rotation (deg)  (from N through E)</param>
        /// <param name="type">projection type code e.g. "-SIN";</param>
        /// <param name="xpos">x (RA) coordinate (deg)</param>
        /// <param name="ypos">y (dec) coordinate (deg)  </param>
        /// <returns></returns>
        public static int worldpos(double xpix, double ypix, double xref, double yref,
            double xrefpix, double yrefpix, double xinc, double yinc, double rot,
            string type, out double xpos, out double ypos) {
            double cosr, sinr, dx, dy, dz, temp, x, y, z;
            double sins, coss, dect, rat, dt, l, m, mg, da, dd, cos0, sin0;
            double dec0, ra0, decout, raout;
            double geo1, geo2, geo3;
            double cond2r = 1.745329252e-2;
            double twopi = 6.28318530717959, deps = 1.0e-5;
            int i, itype;
            string[] types = { "-CAR", "-SIN", "-TAN", "-ARC", "-NCP", "-GLS", "-MER", "-AIT", "-STG" };


            /*   Offset from ref pixel  */
            dx = (xpix - xrefpix) * xinc;
            dy = (ypix - yrefpix) * yinc;
            /*   Take out rotation  */
            cosr = Math.Cos(rot * cond2r);
            sinr = Math.Sin(rot * cond2r);
            if (rot != 0.0)
            {
                temp = dx * cosr - dy * sinr;
                dy = dy * cosr + dx * sinr;
                dx = temp;
            }
            /*  find type  */
            /* WDP 1/97: removed support for default type for better error checking */
            /*  itype = 0;   default type is linear */
            itype = -1; /* no default type */
            for (i = 0; i < 9; i++)
            {
                if (type == types[i])
                {
                    itype = i;
                }
            }

            /* default, linear result for error return  */
            xpos = xref + dx;
            ypos = yref + dy;
            /* convert to radians  */
            ra0 = xref * cond2r;
            dec0 = yref * cond2r;
            l = dx * cond2r;
            m = dy * cond2r;
            sins = l * l + m * m;
            cos0 = Math.Cos(dec0);
            sin0 = Math.Sin(dec0);

            /* process by case  */
            switch (itype)
            {
                case 0: /* linear -CAR */
                    rat = ra0 + l;
                    dect = dec0 + m;
                    break;
                case 1: /* -SIN sin*/
                    if (sins > 1.0) return 501;
                    coss = Math.Sqrt(1.0 - sins);
                    dt = sin0 * coss + cos0 * m;
                    if ((dt > 1.0) || (dt < -1.0)) return 501;
                    dect = Math.Asin(dt);
                    rat = cos0 * coss - sin0 * m;
                    if ((rat == 0.0) && (l == 0.0)) return 501;

                    rat = Math.Atan2(l, rat) + ra0;
                    break;
                case 2: /* -TAN tan */
                    x = cos0 * Math.Cos(ra0) - l * Math.Sin(ra0) - m * Math.Cos(ra0) * sin0;
                    y = cos0 * Math.Sin(ra0) + l * Math.Cos(ra0) - m * Math.Sin(ra0) * sin0;
                    z = sin0 + m * cos0;
                    rat = Math.Atan2(y, x);
                    dect = Math.Atan(z / Math.Sqrt(x * x + y * y));
                    break;
                case 3: /* -ARC Arc*/
                    if (sins >= twopi * twopi / 4.0) return 501;
                    sins = Math.Sqrt(sins);
                    coss = Math.Cos(sins);
                    if (sins != 0.0) sins = Math.Sin(sins) / sins;
                    else
                        sins = 1.0;
                    dt = m * cos0 * sins + sin0 * coss;
                    if ((dt > 1.0) || (dt < -1.0)) return 501;
                    dect = Math.Asin(dt);
                    da = coss - dt * sin0;
                    dt = l * sins * cos0;
                    if ((da == 0.0) && (dt == 0.0)) return 501;
                    rat = ra0 + Math.Atan2(dt, da);
                    break;
                case 4: /* -NCP North celestial pole*/
                    dect = cos0 - m * sin0;
                    if (dect == 0.0) return 501;
                    rat = ra0 + Math.Atan2(l, dect);
                    dt = Math.Cos(rat - ra0);
                    if (dt == 0.0) return 501;
                    dect = dect / dt;
                    if ((dect > 1.0) || (dect < -1.0)) return 501;
                    dect = Math.Acos(dect);
                    if (dec0 < 0.0) dect = -dect;
                    break;
                case 5: /* -GLS global sinusoid */
                    dect = dec0 + m;
                    if (Math.Abs(dect) > twopi / 4.0) return 501;
                    coss = Math.Cos(dect);
                    if (Math.Abs(l) > twopi * coss / 2.0) return 501;
                    rat = ra0;
                    if (coss > deps) rat = rat + l / coss;
                    break;
                case 6: /* -MER mercator*/
                    dt = yinc * cosr + xinc * sinr;
                    if (dt == 0.0) dt = 1.0;
                    dy = (yref / 2.0 + 45.0) * cond2r;
                    dx = dy + dt / 2.0 * cond2r;
                    dy = Math.Log(Math.Tan(dy));
                    dx = Math.Log(Math.Tan(dx));
                    geo2 = dt * cond2r / (dx - dy);
                    geo3 = geo2 * dy;
                    geo1 = Math.Cos(yref * cond2r);
                    if (geo1 <= 0.0) geo1 = 1.0;
                    rat = l / geo1 + ra0;
                    if (Math.Abs(rat - ra0) > twopi) return 501; /* added 10/13/94 DCW/EWG */
                    dt = 0.0;
                    if (geo2 != 0.0) dt = (m + geo3) / geo2;
                    dt = Math.Exp(dt);
                    dect = 2.0 * Math.Atan(dt) - twopi / 4.0;
                    break;
                case 7: /* -AIT Aitoff*/
                    dt = yinc * cosr + xinc * sinr;
                    if (dt == 0.0) dt = 1.0;
                    dt = dt * cond2r;
                    dy = yref * cond2r;
                    dx = Math.Sin(dy + dt) / Math.Sqrt((1.0 + Math.Cos(dy + dt)) / 2.0) -
                         Math.Sin(dy) / Math.Sqrt((1.0 + Math.Cos(dy)) / 2.0);
                    if (dx == 0.0) dx = 1.0;
                    geo2 = dt / dx;
                    dt = xinc * cosr - yinc * sinr;
                    if (dt == 0.0) dt = 1.0;
                    dt = dt * cond2r;
                    dx = 2.0 * Math.Cos(dy) * Math.Sin(dt / 2.0);
                    if (dx == 0.0) dx = 1.0;
                    geo1 = dt * Math.Sqrt((1.0 + Math.Cos(dy) * Math.Cos(dt / 2.0)) / 2.0) / dx;
                    geo3 = geo2 * Math.Sin(dy) / Math.Sqrt((1.0 + Math.Cos(dy)) / 2.0);
                    rat = ra0;
                    dect = dec0;
                    if ((l == 0.0) && (m == 0.0)) break;
                    dz = 4.0 - l * l / (4.0 * geo1 * geo1) - ((m + geo3) / geo2) * ((m + geo3) / geo2);
                    if ((dz > 4.0) || (dz < 2.0)) return 501;
                    dz = 0.5 * Math.Sqrt(dz);
                    dd = (m + geo3) * dz / geo2;
                    if (Math.Abs(dd) > 1.0) return 501;
                    dd = Math.Asin(dd);
                    if (Math.Abs(Math.Cos(dd)) < deps) return 501;
                    da = l * dz / (2.0 * geo1 * Math.Cos(dd));
                    if (Math.Abs(da) > 1.0) return 501;
                    da = Math.Asin(da);
                    rat = ra0 + 2.0 * da;
                    dect = dd;
                    break;
                case 8: /* -STG Sterographic*/
                    dz = (4.0 - sins) / (4.0 + sins);
                    if (Math.Abs(dz) > 1.0) return 501;
                    dect = dz * sin0 + m * cos0 * (1.0 + dz) / 2.0;
                    if (Math.Abs(dect) > 1.0) return 501;
                    dect = Math.Asin(dect);
                    rat = Math.Cos(dect);
                    if (Math.Abs(rat) < deps) return 501;
                    rat = l * (1.0 + dz) / (2.0 * rat);
                    if (Math.Abs(rat) > 1.0) return 501;
                    rat = Math.Asin(rat);
                    mg = 1.0 + Math.Sin(dect) * sin0 + Math.Cos(dect) * cos0 * Math.Cos(rat);
                    if (Math.Abs(mg) < deps) return 501;
                    mg = 2.0 * (Math.Sin(dect) * cos0 - Math.Cos(dect) * sin0 * Math.Cos(rat)) / mg;
                    if (Math.Abs(mg - m) > deps) rat = twopi / 2.0 - rat;
                    rat = ra0 + rat;
                    break;

                default:
                    /* fall through to here on error */
                    return 504;
            }

            /*  return ra in range  */
            raout = rat;
            decout = dect;
            if (raout - ra0 > twopi / 2.0) raout = raout - twopi;
            if (raout - ra0 < -twopi / 2.0) raout = raout + twopi;
            if (raout < 0.0) raout += twopi; /* added by DCW 10/12/94 */

            /*  correct units back to degrees  */
            xpos = raout / cond2r;
            ypos = decout / cond2r;
            return 0;
        }

        /// <summary>
        ///  routine to determine accurate pixel coordinates for an RA and Dec     */
        ///  returns 0 if successful otherwise:                                    */
        ///  1 = angle too large for projection;                                   */
        ///  2 = bad values                                                        */
        ///  WDP 1/97: changed the return values to 501 and 502 instead of 1 and 2 */
        ///  does: -SIN, -TAN, -ARC, -NCP, -GLS, -MER, -AIT projections            */
        ///  anything else is linear                                               */
        /// </summary>
        /// <param name="xpos">x (RA) coordinate (deg)</param>
        /// <param name="ypos">y (dec) coordinate (deg)</param>
        /// <param name="xref">x reference coordinate value (deg)</param>
        /// <param name="yref">y reference coordinate value (deg)</param>
        /// <param name="xrefpix">x reference pixel</param>
        /// <param name="yrefpix">y reference pixel</param>
        /// <param name="xinc">x coordinate increment (deg)</param>
        /// <param name="yinc">y coordinate increment (deg)</param>
        /// <param name="rot">rotation (deg)  (from N through E)</param>
        /// <param name="type">projection type code e.g. "-SIN";</param>
        /// <param name="xpix"> x pixel number  (RA or long without rotation)</param>
        /// <param name="ypix">y pixel number  (dec or lat without rotation)</param>
        /// <returns></returns>
        public static int worldpix(double xpos,    double ypos,    double xref,     double yref,
                                   double xrefpix, double yrefpix, double xinc,     double yinc, 
                                   double rot,     string type,    out double xpix, out double ypix) {

            double dx, dy, dz, r, ra0, dec0, ra, dec, coss, sins, dt, da, dd, sint;
            double l, m, geo1, geo2, geo3, sinr, cosr, cos0, sin0;
            double cond2r = 1.745329252e-2, deps = 1.0e-5, twopi = 6.28318530717959;
            int i, itype;
            string[] types = { "-CAR", "-SIN", "-TAN", "-ARC", "-NCP", "-GLS", "-MER", "-AIT", "-STG" };

            /* 0h wrap-around tests added by D.Wells 10/12/94: */
            dt = (xpos - xref);
            if (dt > 180) xpos -= 360;
            if (dt < -180) xpos += 360;
            /* NOTE: changing input argument xpos is OK (call-by-value in C!) */

            /* default values - linear */
            dx = xpos - xref;
            dy = ypos - yref;
            /*  dz = 0.0; */
            /*  Correct for rotation */
            r = rot * cond2r;
            cosr = Math.Cos(r);
            sinr = Math.Sin(r);
            dz = dx * cosr + dy * sinr;
            dy = dy * cosr - dx * sinr;
            dx = dz;
            /*     check axis increments - bail out if either 0 */
            if ((xinc == 0.0) || (yinc == 0.0)) { xpix = 0.0; ypix = 0.0; return 502; }
            /*     convert to pixels  */
            xpix = dx / xinc + xrefpix;
            ypix = dy / yinc + yrefpix;

            /*  find type  */
            /* WDP 1/97: removed support for default type for better error checking */
            /*  itype = 0;   default type is linear */
            itype = -1;  /* no default type */
            for (i = 0; i < 9; i++)
            {
                if (type == types[i])
                {
                    itype = i;
                }
            }

            /* Non linear position */
            ra0 = xref * cond2r;
            dec0 = yref * cond2r;
            ra = xpos * cond2r;
            dec = ypos * cond2r;

            /* compute direction cosine */
            coss = Math.Cos(dec);
            sins = Math.Sin(dec);
            cos0 = Math.Cos(dec0);
            sin0 = Math.Sin(dec0);
            l = Math.Sin(ra - ra0) * coss;
            sint = sins * sin0 + coss * cos0 * Math.Cos(ra - ra0);

            /* process by case  */
            switch (itype)
            {
                case 1:   /* -SIN sin*/
                    if (sint < 0.0) return 501;
                    m = sins * Math.Cos(dec0) - coss * Math.Sin(dec0) * Math.Cos(ra - ra0);
                    break;
                case 2:   /* -TAN tan */
                    if (sint <= 0.0) return 501;
                    if (cos0 < 0.001)
                    {
                        /* Do a first order expansion around pole */
                        m = (coss * Math.Cos(ra - ra0)) / (sins * sin0);
                        m = (-m + cos0 * (1.0 + m * m)) / sin0;
                    }
                    else
                    {
                        m = (sins / sint - sin0) / cos0;
                    }
                    if (Math.Abs(Math.Sin(ra0)) < 0.3)
                    {
                        l = coss * Math.Sin(ra) / sint - cos0 * Math.Sin(ra0) + m * Math.Sin(ra0) * sin0;
                        l /= Math.Cos(ra0);
                    }
                    else
                    {
                        l = coss * Math.Cos(ra) / sint - cos0 * Math.Cos(ra0) + m * Math.Cos(ra0) * sin0;
                        l /= -Math.Sin(ra0);
                    }
                    break;
                case 3:   /* -ARC Arc*/
                    m = sins * Math.Sin(dec0) + coss * Math.Cos(dec0) * Math.Cos(ra - ra0);
                    if (m < -1.0) m = -1.0;
                    if (m > 1.0) m = 1.0;
                    m = Math.Acos(m);
                    if (m != 0)
                        m = m / Math.Sin(m);
                    else
                        m = 1.0;
                    l = l * m;
                    m = (sins * Math.Cos(dec0) - coss * Math.Sin(dec0) * Math.Cos(ra - ra0)) * m;
                    break;
                case 4:   /* -NCP North celestial pole*/
                    if (dec0 == 0.0)
                        return 501;  /* can't stand the equator */
                    else
                        m = (Math.Cos(dec0) - coss * Math.Cos(ra - ra0)) / Math.Sin(dec0);
                    break;
                case 5:   /* -GLS global sinusoid */
                    dt = ra - ra0;
                    if (Math.Abs(dec) > twopi / 4.0) return 501;
                    if (Math.Abs(dec0) > twopi / 4.0) return 501;
                    m = dec - dec0;
                    l = dt * coss;
                    break;
                case 6:   /* -MER mercator*/
                    dt = yinc * cosr + xinc * sinr;
                    if (dt == 0.0) dt = 1.0;
                    dy = (yref / 2.0 + 45.0) * cond2r;
                    dx = dy + dt / 2.0 * cond2r;
                    dy = Math.Log(Math.Tan(dy));
                    dx = Math.Log(Math.Tan(dx));
                    geo2 = dt * cond2r / (dx - dy);
                    geo3 = geo2 * dy;
                    geo1 = Math.Cos(yref * cond2r);
                    if (geo1 <= 0.0) geo1 = 1.0;
                    dt = ra - ra0;
                    l = geo1 * dt;
                    dt = dec / 2.0 + twopi / 8.0;
                    dt = Math.Tan(dt);
                    if (dt < deps) return 502;
                    m = geo2 * Math.Log(dt) - geo3;
                    break;
                case 7:   /* -AIT Aitoff*/
                    da = (ra - ra0) / 2.0;
                    if (Math.Abs(da) > twopi / 4.0) return 501;
                    dt = yinc * cosr + xinc * sinr;
                    if (dt == 0.0) dt = 1.0;
                    dt = dt * cond2r;
                    dy = yref * cond2r;
                    dx = Math.Sin(dy + dt) / Math.Sqrt((1.0 + Math.Cos(dy + dt)) / 2.0) -
                        Math.Sin(dy) / Math.Sqrt((1.0 + Math.Cos(dy)) / 2.0);
                    if (dx == 0.0) dx = 1.0;
                    geo2 = dt / dx;
                    dt = xinc * cosr - yinc * sinr;
                    if (dt == 0.0) dt = 1.0;
                    dt = dt * cond2r;
                    dx = 2.0 * Math.Cos(dy) * Math.Sin(dt / 2.0);
                    if (dx == 0.0) dx = 1.0;
                    geo1 = dt * Math.Sqrt((1.0 + Math.Cos(dy) * Math.Cos(dt / 2.0)) / 2.0) / dx;
                    geo3 = geo2 * Math.Sin(dy) / Math.Sqrt((1.0 + Math.Cos(dy)) / 2.0);
                    dt = Math.Sqrt((1.0 + Math.Cos(dec) * Math.Cos(da)) / 2.0);
                    if (Math.Abs(dt) < deps) return 503;
                    l = 2.0 * geo1 * Math.Cos(dec) * Math.Sin(da) / dt;
                    m = geo2 * Math.Sin(dec) / dt - geo3;
                    break;
                case 8:   /* -STG Sterographic*/
                    da = ra - ra0;
                    if (Math.Abs(dec) > twopi / 4.0) return 501;
                    dd = 1.0 + sins * Math.Sin(dec0) + coss * Math.Cos(dec0) * Math.Cos(da);
                    if (Math.Abs(dd) < deps) return 501;
                    dd = 2.0 / dd;
                    l = l * dd;
                    m = dd * (sins * Math.Cos(dec0) - coss * Math.Sin(dec0) * Math.Cos(da));
                    break;

                default:
                    /* fall through to here on error */
                    return 504;

            }  /* end of itype switch */

            /*   back to degrees  */
            dx = l / cond2r;
            dy = m / cond2r;
            /*  Correct for rotation */
            dz = dx * cosr + dy * sinr;
            dy = dy * cosr - dx * sinr;
            dx = dz;
            /*     convert to pixels  */
            xpix = dx / xinc + xrefpix;
            ypix = dy / yinc + yrefpix;
            return 0;
        }

        /*//////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////*/

        /// <summary>
        /// Function to determine accurate position for pixel coordinates which 
        /// returns 0 if successful otherwise: 1 = angle too large for projection;                                  
        /// does: -SIN, -TAN, -ARC, -NCP, -GLS, -MER, -AIT projections            
        /// anything else is linear
        /// </summary>
        /// <param name="xpix">x pixel number  (RA or long without rotation)</param>
        /// <param name="ypix">y pixel number  (dec or lat without rotation)</param>
        /// <param name="xref">x reference coordinate value (deg)</param>
        /// <param name="yref">y reference coordinate value (deg)</param>
        /// <param name="xrefpix">x reference pixel</param>
        /// <param name="yrefpix">y reference pixel</param>
        /// <param name="xinc">x coordinate increment (deg)</param>
        /// <param name="yinc">y coordinate increment (deg)</param>
        /// <param name="rot">rotation (deg) (from N through E)</param>
        /// <param name="type">projection type code e.g. "-SIN"</param>
        /// <param name="xpos">output: x (RA) coordinate (deg)</param>
        /// <param name="ypos">output: y (dec) coordinate (deg)</param>
        /// <returns></returns>
        public static int WorldPos(double xpix,    double ypix,    double xref,     double yref, 
                            double xrefpix, double yrefpix, double xinc,     double yinc, 
                            double rot,     string type,    out double xpos, out double ypos) 
            {
            double cosr, sinr, dx, dy, dz, temp;
            double sins, coss, dect = 0, rat = 0, dt, l, m, mg, da, dd, cos0, sin0;
            double dec0, ra0, decout, raout;
            double geo1, geo2, geo3;
            double cond2r = 1.745329252e-2;
            double twopi = 6.28318530717959, deps = 1.0e-5;
            int i, itype;
            string[] ctypes ={"-SIN","-TAN","-ARC","-NCP", "-GLS", "-MER", "-AIT", "-STG"};
            /*   Offset from ref pixel  */
            dx = (xpix-xrefpix) * xinc;
            dy = (ypix-yrefpix) * yinc;
            /*   Take out rotation  */
            cosr = Math.Cos(rot* cond2r);
            sinr = Math.Sin(rot* cond2r);
            if (rot!=0.0){
                temp = dx* cosr - dy* sinr;
                dy = dy* cosr + dx* sinr;
                dx = temp;
            }
            /*  find type  */
            itype = 0;  /* default type is linear */
            for (i=0;i<8;i++) if (type.Equals(ctypes[i])) itype = i+1;
            /* default, linear result for error return  */
            xpos = xref + dx;
            ypos = yref + dy;
            /* convert to radians  */
            ra0 = xref* cond2r;
            dec0 = yref* cond2r;
            l = dx* cond2r;
            m = dy* cond2r;
            sins = l* l + m* m;
            decout = 0.0;
            raout = 0.0;
            cos0 = Math.Cos(dec0);
            sin0 = Math.Sin(dec0);
            /* process by case  */
            switch (itype) {
                case 0:   /* linear */
                    rat =  ra0 + l;
                    dect = dec0 + m;
                    break;
                case 1:   /* -SIN sin*/ 
                    if (sins>1.0) return 1;
                    coss = Math.Sqrt(1.0 - sins);
                    dt = sin0* coss + cos0* m;
                    if ((dt>1.0) || (dt<-1.0)) return 1;
                    dect = Math.Asin(dt);
                rat = cos0* coss - sin0* m;
                    if ((rat==0.0) && (l==0.0)) return 1;
                    rat = Math.Atan2(l, rat) + ra0;
                    break;
                case 2:   /* -TAN tan */
                    if (sins>1.0) return 1;
                    dect = cos0 - m* sin0;
                    if (dect==0.0) return 1;
                    rat = ra0 + Math.Atan2(l, dect);
                    dect = Math.Atan(Math.Cos(rat-ra0) * (m* cos0 + sin0) / dect);
                    break;
                case 3:   /* -ARC Arc*/
                    if (sins>=twopi* twopi/4.0) return 1;
                    sins = Math.Sqrt(sins);
                    coss = Math.Cos(sins);
                    if (sins!=0.0) sins = Math.Sin(sins) / sins;
                    else sins = 1.0;
                    dt = m* cos0 * sins + sin0* coss;
                    if ((dt>1.0) || (dt<-1.0)) return 1;
                    dect = Math.Asin(dt);
                    da = coss - dt* sin0;
                    dt = l* sins * cos0;
                    if ((da==0.0) && (dt==0.0)) return 1;
                    rat = ra0 + Math.Atan2(dt, da);
                    break;
                case 4:   /* -NCP North celestial pole*/
                    dect = cos0 - m* sin0;
                    if (dect==0.0) return 1;
                    rat = ra0 + Math.Atan2(l, dect);
                dt = Math.Cos(rat-ra0);
                    if (dt==0.0) return 1;
                    dect = dect / dt;
                    if ((dect>1.0) || (dect<-1.0)) return 1;
                    dect = Math.Acos(dect);
                    if (dec0<0.0) dect = -dect;
                    break;
                case 5:   /* -GLS global sinusoid */
                    dect = dec0 + m;
                    if (Math.Abs(dect)>twopi/4.0) return 1;
                    coss = Math.Cos(dect);
                    if (Math.Abs(l)>twopi* coss/2.0) return 1;
                    rat = ra0;
                    if (coss>deps) rat = rat + l / coss;
                    break;
                case 6:   /* -MER mercator*/
                    dt = yinc* cosr + xinc* sinr;
                    if (dt==0.0) dt = 1.0;
                    dy = (yref/2.0 + 45.0) * cond2r;
                    dx = dy + dt / 2.0 * cond2r;
                    dy = Math.Log(Math.Tan (dy));
                    dx = Math.Log(Math.Tan(dx));
                    geo2 = dt* cond2r / (dx - dy);
                    geo3 = geo2* dy;
                    geo1 = Math.Cos(yref* cond2r);
                    if (geo1<=0.0) geo1 = 1.0;
                    rat = l / geo1 + ra0;
                    if (Math.Abs(rat - ra0) > twopi) return 1; /* added 10/13/94 DCW/EWG */
                    dt = 0.0;
                    if (geo2!=0.0) dt = (m + geo3) / geo2;
                    dt = Math.Exp(dt);
                    dect = 2.0 * Math.Atan(dt) - twopi / 4.0;
                    break;
                case 7:   /* -AIT Aitoff*/
                    dt = yinc* cosr + xinc* sinr;
                    if (dt==0.0) dt = 1.0;
                    dt = dt* cond2r;
                    dy = yref* cond2r;
                    dx = Math.Sin(dy+dt)/ Math.Sqrt((1.0+ Math.Cos(dy+dt))/2.0) -
                    Math.Sin(dy)/ Math.Sqrt((1.0+ Math.Cos(dy))/2.0);
                    if (dx==0.0) dx = 1.0;
                    geo2 = dt / dx;
                    dt = xinc* cosr - yinc* sinr;
                    if (dt==0.0) dt = 1.0;
                    dt = dt* cond2r;
                    dx = 2.0 * Math.Cos(dy) * Math.Sin(dt/2.0);
                    if (dx==0.0) dx = 1.0;
                    geo1 = dt* Math.Sqrt((1.0+ Math.Cos(dy) * Math.Cos(dt/2.0))/2.0) / dx;
                    geo3 = geo2* Math.Sin(dy) / Math.Sqrt((1.0+ Math.Cos(dy))/2.0);
                    rat = ra0;
                    dect = dec0;
                    if ((l==0.0) && (m==0.0)) break;
                    dz = 4.0 - l* l/(4.0* geo1* geo1) - ((m+geo3)/geo2)* ((m+geo3)/geo2) ;
                    if ((dz>4.0) || (dz<2.0)) return 1;
                    dz = 0.5 * Math.Sqrt(dz);
                    dd = (m+geo3) * dz / geo2;
                    if (Math.Abs(dd)>1.0) return 1;
                    dd = Math.Asin(dd);
                    if (Math.Abs(Math.Cos(dd))<deps) return 1;
                    da = l* dz / (2.0 * geo1 * Math.Cos(dd));
                    if (Math.Abs(da)>1.0) return 1;
                    da = Math.Asin(da);
                    rat = ra0 + 2.0 * da;
                    dect = dd;
                    break;
                case 8:   /* -STG Sterographic*/
                    dz = (4.0 - sins) / (4.0 + sins);
                    if (Math.Abs(dz)>1.0) return 1;
                    dect = dz* sin0 + m* cos0 * (1.0+dz) / 2.0;
                    if (Math.Abs(dect)>1.0) return 1;
                    dect = Math.Asin(dect);
                    rat = Math.Cos(dect);
                    if (Math.Abs(rat)<deps) return 1;
                    rat = l* (1.0+dz) / (2.0 * rat);
                    if (Math.Abs(rat)>1.0) return 1;
                    rat = Math.Asin(rat);
                    mg = 1.0 + Math.Sin(dect) * sin0 + Math.Cos(dect) * cos0 * Math.Cos(rat);
                    if (Math.Abs(mg)<deps) return 1;
                    mg = 2.0 * (Math.Sin(dect) * cos0 - Math.Cos(dect) * sin0 * Math.Cos(rat)) / mg;
                    if (Math.Abs(mg-m)>deps) rat = twopi/2.0 - rat;
                    rat = ra0 + rat;
                    break;
            }
            /*  return ra in range  */
            raout = rat;
            decout = dect;
            if (raout-ra0>twopi/2.0) raout = raout - twopi;
            if (raout-ra0<-twopi/2.0) raout = raout + twopi;
            if (raout< 0.0) raout += twopi; /* added by DCW 10/12/94 */
            /*  correct units back to degrees  */
            xpos = raout / cond2r;
            ypos = decout / cond2r;
            return 0;
        }  /* End of WorldPos */


        /// <summary>
        /// Routine to determine accurate pixel coordinates for an RA and Dec which    
        /// returns 0 if successful, otherwise:                                    
        ///     1 = angle too large for projection;
        ///     2 = bad values.
        /// It does: -SIN, -TAN, -ARC, -NCP, -GLS, -MER, -AIT projections,
        /// anything else is linear
        /// </summary>                                 
        /// <param name="xpos">x (RA) coordinate (deg)</param>
        /// <param name="ypos">y(dec) coordinate(deg)</param>
        /// <param name="xref">x reference coordinate value(deg)</param>
        /// <param name="yref">y reference coordinate value(deg)</param>
        /// <param name="xrefpix">x reference pixel</param>
        /// <param name="yrefpix">y reference pixel</param>
        /// <param name="xinc">x coordinate increment(deg)</param>
        /// <param name="yinc">y coordinate increment(deg)</param>
        /// <param name="rot">rotation(deg)  (from N through E)</param>
        /// <param name="type">projection type code e.g. "-SIN";</param>
        /// <param name="xpix">x pixel number(RA or long without rotation)</param>
        /// <param name="ypix">y pixel number(dec or lat without rotation)</param>
        /// <returns></returns>
        public static int XYPix(double xpos,    double ypos,    double xref,     double yref,
                         double xrefpix, double yrefpix, double xinc,     double yinc,
                         double rot,     string type,    out double xpix, out double ypix) {

            double dx, dy, dz, r, ra0, dec0, ra, dec, coss, sins, dt, da, dd, sint;
            double l, m = 0, geo1, geo2, geo3, sinr, cosr;
            double cond2r = 1.745329252e-2, deps = 1.0e-5, twopi = 6.28318530717959;
            int i, itype;
            string[] ctypes ={"-SIN","-TAN","-ARC","-NCP", "-GLS", "-MER", "-AIT", "-STG"};

            /* 0h wrap-around tests added by D.Wells 10/12/94: */
            dt = (xpos - xref);
            if (dt >  180) xpos -= 360;
            if (dt< -180) xpos += 360;
            /* NOTE: changing input argument xpos is OK (call-by-value in C!) */

            /* default values - linear */
            dx = xpos - xref;
            dy = ypos - yref;
            dz = 0.0;
            /*  Correct for rotation */
            r = rot* cond2r;
            cosr = Math.Cos(r);
            sinr = Math.Sin(r);
            dz = dx* cosr + dy* sinr;
            dy = dy* cosr - dx* sinr;
            dx = dz;
            /*     check axis increments - bail out if either 0 */
            if ((xinc==0.0) || (yinc==0.0)) {xpix=0.0; ypix=0.0; return 2;}
            /*     convert to pixels  */
            xpix = dx / xinc + xrefpix;
            ypix = dy / yinc + yrefpix;

            /*  find type  */
            itype = 0;  /* default type is linear */
            for (i=0;i<8;i++) if (type.Equals(ctypes[i])) itype = i+1;
            if (itype==0) return 0;  /* done if linear */

            /* Non linear position */
            ra0 = xref* cond2r;
            dec0 = yref* cond2r;
            ra = xpos* cond2r;
            dec = ypos* cond2r;

            /* compute direction cosine */
            coss = Math.Cos(dec);
            sins = Math.Sin(dec);
            l = Math.Sin(ra-ra0) * coss;
            sint = sins* Math.Sin(dec0) + coss* Math.Cos(dec0) * Math.Cos(ra-ra0);
            /* process by case  */
            switch (itype) {
                case 1:   /* -SIN sin*/ 
                    if (sint<0.0) return 1;
                    m = sins* Math.Cos(dec0) - coss* Math.Sin(dec0) * Math.Cos(ra-ra0);
                    break;
                case 2:   /* -TAN tan */
                    if (sint<=0.0) return 1;
                    m = sins* Math.Sin(dec0) + coss* Math.Cos(dec0) * Math.Cos(ra-ra0);
                    l = l / m;
                    m = (sins* Math.Cos(dec0) - coss* Math.Sin(dec0) * Math.Cos(ra-ra0)) / m;
                    break;
                case 3:   /* -ARC Arc*/
                    m = sins* Math.Sin(dec0) + coss* Math.Cos(dec0) * Math.Cos(ra-ra0);
                    if (m<-1.0) m = -1.0;
                    if (m>1.0) m = 1.0;
                    m = Math.Acos(m);
                    if (m!=0)  m = m / Math.Sin(m);
                    else m = 1.0;
                    l = l* m;
                    m = (sins* Math.Cos(dec0) - coss* Math.Sin(dec0) * Math.Cos(ra-ra0)) * m;
                    break;
                case 4:   /* -NCP North celestial pole*/
                    if (dec0==0.0) return 1;  /* can't stand the equator */
                    else m = (Math.Cos(dec0) - coss* Math.Cos(ra-ra0)) / Math.Sin(dec0);
                    break;
                case 5:   /* -GLS global sinusoid */
                    dt = ra - ra0;
                    if (Math.Abs(dec)>twopi/4.0) return 1;
                    if (Math.Abs(dec0)>twopi/4.0) return 1;
                    m = dec - dec0;
                    l = dt* coss;
                    break;
                case 6:   /* -MER mercator*/
                    dt = yinc* cosr + xinc* sinr;
                    if (dt==0.0) dt = 1.0;
                    dy = (yref/2.0 + 45.0) * cond2r;
                    dx = dy + dt / 2.0 * cond2r;
                    dy = Math.Log(Math.Tan (dy));
                    dx = Math.Log(Math.Tan (dx));
                    geo2 = dt* cond2r / (dx - dy);
                    geo3 = geo2* dy;
                    geo1 = Math.Cos(yref* cond2r);
                    if (geo1<=0.0) geo1 = 1.0;
                    dt = ra - ra0;
                    l = geo1* dt;
                    dt = dec / 2.0 + twopi / 8.0;
                    dt = Math.Tan(dt);
                    if (dt<deps) return 2;
                    m = geo2* Math.Log(dt) - geo3;
                    break;
                case 7:   /* -AIT Aitoff*/
                    l = 0.0;
                    m = 0.0;
                    da = (ra - ra0) / 2.0;
                    if (Math.Abs(da)>twopi/4.0) return 1;
                    dt = yinc* cosr + xinc* sinr;
                    if (dt==0.0) dt = 1.0;
                    dt = dt* cond2r;
                    dy = yref* cond2r;
                    dx = Math.Sin(dy+dt)/ Math.Sqrt((1.0+ Math.Cos(dy+dt))/2.0) - Math.Sin(dy)/ Math.Sqrt((1.0+ Math.Cos(dy))/2.0);
                    if (dx==0.0) dx = 1.0;
                    geo2 = dt / dx;
                    dt = xinc* cosr - yinc* sinr;
                    if (dt==0.0) dt = 1.0;
                    dt = dt* cond2r;
                    dx = 2.0 * Math.Cos(dy) * Math.Sin(dt/2.0);
                    if (dx==0.0) dx = 1.0;
                    geo1 = dt* Math.Sqrt((1.0+ Math.Cos(dy) * Math.Cos(dt/2.0))/2.0) / dx;
                    geo3 = geo2* Math.Sin(dy) / Math.Sqrt((1.0+ Math.Cos(dy))/2.0);
                    dt = Math.Sqrt((1.0 + Math.Cos(dec) * Math.Cos(da))/2.0);
                    if (Math.Abs(dt)<deps) return 3;
                    l = 2.0 * geo1 * Math.Cos(dec) * Math.Sin(da) / dt;
                    m = geo2* Math.Sin(dec) / dt - geo3;
                    break;
                case 8:   /* -STG Sterographic*/
                    da = ra - ra0;
                    if (Math.Abs(dec)>twopi/4.0) return 1;
                    dd = 1.0 + sins* Math.Sin(dec0) + coss* Math.Cos(dec0) * Math.Cos(da);
                    if (Math.Abs(dd)<deps) return 1;
                    dd = 2.0 / dd;
                    l = l* dd;
                    m = dd* (sins* Math.Cos(dec0) - coss* Math.Sin(dec0) * Math.Cos(da));
                    break;
            }  /* end of itype switch */
            /*   back to degrees  */
            dx = l / cond2r;
            dy = m / cond2r;
            /*  Correct for rotation */
            dz = dx* cosr + dy* sinr;
            dy = dy* cosr - dx* sinr;
            dx = dz;
            /*     convert to pixels  */
            xpix = dx / xinc + xrefpix;
            ypix = dy / yinc + yrefpix;
            return 0;
        }  /* end xypix */


    }
}
