using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    class DSS
    {
        public static string[] ImageOrigin = {
                                "poss2ukstu_red","poss2ukstu_blue", "poss2ukstu_ir",
                                "poss1_red",      "poss1_blue",     "quickv",
                                "phase2_gsc2",     "phase2_gsc1" };

        public static string[] ImageOriginName = {
                                "POSS2/UKSTU Red", "POSS2 / UKSTU Blue", "POSS2 / UKSTU IR",
                                "POSS1 Red",       "POSS1 Blue",         "Quick - V",
                                "HST Phase 2 (GSC2)", "HST Phase 2 (GSC1)" };

        public static string[] ImageFormat = { "fits", "gif" };

        public static string imageOrigin = ImageOrigin[0];
        public static string imageFormat = ImageFormat[0];

        public static void PostDSSrequest(string RA, string DEC, string filename, string width="12.8787", string height="8.5672") {
            string strURL = "http://archive.stsci.edu/cgi-bin/dss_search";
            Console.Out.WriteLine("Calling " + strURL + " with RA=" + RA + " DEC=" + DEC);
            Form1.frm.LogEntry("DSS search with RA=" + RA + " DEC=" + DEC);
            using (var client = new WebClient())
            {
                var values = new NameValueCollection{
                    ["r"] = RA,
                    ["d"] = DEC,
                    ["h"] = height,
                    ["w"] = width,
                    ["v"] = imageOrigin,
                    ["f"] = imageFormat
                };
                try{
                    byte[] result = client.UploadValues(strURL, values);
                    File.WriteAllBytes(filename, result);
                    Console.Out.WriteLine("" + result.Length + " bytes saved in file '" + filename + "'");
                    Form1.frm.LogEntry("" + result.Length + " bytes saved in file '" + filename + "'");
                }
                catch(Exception ex) { Console.Out.WriteLine("Error" + ex.Message.ToString()); Form1.frm.LogEntry("Error: "+ex.Message.ToString()) ; return; }
            }
            Form1.frm.LogEntry("DSS search ended successfully!!!");
        }

    }
}
