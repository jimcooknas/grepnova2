using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Text;

namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public static class HttpHelper
    {
        public struct DirectoryItem
        {
            public Uri BaseUri;

            public string AbsolutePath
            {
                get
                {
                    return string.Format("{0}/{1}", BaseUri, Name);
                }
            }

            public DateTime DateCreated;
            public bool IsDirectory;
            public string Name;
            public List<DirectoryItem> Items;
        }

        public static List<DirectoryItem> GetDirectoryInformation(string address, string username, string password)
        {
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(address);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Credentials = new NetworkCredential(username, password);
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = false;
            List<DirectoryItem> returnValue = null;
            try
            {
                returnValue = new List<DirectoryItem>();
                string[] list = null;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    list = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                }

                foreach (string line in list)
                {
                    // Windows FTP Server Response Format
                    // DateCreated    IsDirectory    Name
                    string data = line;

                    // Parse date
                    string date = data.Substring(0, 17);
                    DateTime dateTime = DateTime.Parse(date, System.Globalization.CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat);
                    data = data.Remove(0, 24);

                    // Parse <DIR>
                    string dir = data.Substring(0, 5);
                    bool isDirectory = dir.Equals("<dir>", StringComparison.InvariantCultureIgnoreCase);
                    data = data.Remove(0, 5);
                    data = data.Remove(0, 10);

                    // Parse name
                    string name = data;

                    // Create directory info
                    DirectoryItem item = new DirectoryItem
                    {
                        BaseUri = new Uri(address),
                        DateCreated = dateTime,
                        IsDirectory = isDirectory,
                        Name = name
                    };

                    Debug.WriteLine(item.AbsolutePath);
                    //item.Items = item.IsDirectory ? GetDirectoryInformation(item.AbsolutePath, username, password) : null;

                    returnValue.Add(item);
                }
            }
            catch (Exception ex)
            {
                Form1.frm.LogError("GetDirectoryInformation", "Connection error" + ex.Message.ToString());
                returnValue = null;
                MyMessageBox msgBox = new MyMessageBox();
                if(msgBox.ShowDialog("Connection error",0,"Could not retreive server connection.\r\n\r\nError: " +ex.Message.ToString()+"\r\n\r\nPress Details for more information", 2, "","Ok","Details",3) == DialogResult.No)
                {
                    string mss = "<font face='Calibri' size=2 /><b>" + ex.GetBaseException().Message + "</b><br><b>Source:</b> " + ex.Source + "<br>" + "<b>Target:</b> " + ex.TargetSite + "<br><b>StackTrace:</b><br>" + ex.StackTrace.ToString().Replace("\r\n", "<br>");
                    msgBox.ShowDialog("Connection error details", 1, mss, 1, "Ok", "","", 1);
                }
            }
            return returnValue;
        }

        //public static async Task<int> ftpDownload(string filename, string address, string user, string pass)
        public static int ftpDownload(string filename, string address, string user, string pass)
        {
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(address);//"ftp://www.contoso.com/test.htm");
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.UsePassive = false;
            request.UseBinary = true;

            // Use FTP site credentials.
            request.Credentials = new NetworkCredential(user, pass);

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            
            using (FileStream writer = new FileStream(filename, FileMode.Create))
            {
                long length = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[2048];
                readCount = responseStream.Read(buffer, 0, bufferSize);
                while (readCount > 0){
                    writer.Write(buffer, 0, readCount);
                    readCount = responseStream.Read(buffer, 0, bufferSize);
                }
            }

            reader.Close();
            response.Close();
            return 1;
        }

        /// <summary>
        /// Specifies the URL to receive the request and contacts VisieR site and asks for stars' catalog
        /// </summary>
        /// <param name="radeg">Right Asension in degrees or sexagesimal form</param>
        /// <param name="decdeg">Declination in degrees or sexagesimal form</param>
        /// <param name="fovamw">width of view in arcminutes</param>
        /// <param name="fovamh">height of view in arcminutes</param>
        /// <param name="max">Maximum number of returned stars (0 means unlimited)</param>
        /// <returns>A string containing the List of results (in asu-tsv form)</returns>
        public static string Unso(string radeg, string decdeg, float fovamw, float fovamh, int max = 0) {
            string sMax = "";
            if (max == 0) sMax = "unlimited";
            else sMax = "" + max;
            string str1 = "http://webviz.u-strasbg.fr/viz-bin/asu-tsv/?-source=USNO-B1";
            string str2 = String.Format("&-c.ra={0}&-c.dec={1}&-c.bm={2}/{3}&-out.max={4}", radeg, decdeg, fovamw, fovamh, sMax);
            //string str1 = "http://webviz.u-strasbg.fr/viz-bin/asu-tsv/?-source=USNO-B1&-c.ra=05 40 44.00&-c.dec=+49 41 42.0&-c.bm=7,2/7,2&out.max=unlimited";
            //string str2 = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(str1+str2);
            Console.Out.WriteLine(str1 + str2);
            // Set some reasonable limits on resources used by this request
            request.MaximumAutomaticRedirections = 4;
            request.MaximumResponseHeadersLength = 4;
            request.Timeout = 20000;
            // Set credentials to use for this request.
            request.Credentials = CredentialCache.DefaultCredentials;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //Console.WriteLine("Content length is {0}", response.ContentLength);
                //Console.WriteLine("Content type is {0}", response.ContentType);

                // Get the stream associated with the response.
                Stream receiveStream = response.GetResponseStream();

                // Pipes the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

                //Console.WriteLine(readStream.ReadToEnd());
                string res = readStream.ReadToEnd();
                //Console.WriteLine("Response stream received ({0} characters)",res.Length);
                System.IO.File.WriteAllText("USNO-B1.txt", res, Encoding.UTF8);
                response.Close();
                readStream.Close();
                return res;
            }catch(WebException we) {
                Console.Out.WriteLine("Web Exception: " + we.Message);
                return "";
            }
        }

    }
}
