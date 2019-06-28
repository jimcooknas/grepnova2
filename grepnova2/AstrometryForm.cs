using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using software.elendil.AstrometryNet;
using software.elendil.AstrometryNet.Enum;
using software.elendil.AstrometryNet.Json;

namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public partial class AstrometryForm : Form
    {
        const double WIDTH_MIN = 8.3;
        const double HEIGHT_MIN = 5.6;

        Client client = null;
        string objName = "";
        Fits.imageptr image;
        int solveMethod = 1;
        string exePlateSolver = Form1.plateSolverPath;//@"C:\Program Files (x86)\PlateSolver\PlateSolver.exe";
        System.Diagnostics.Process process = null;
        Task<LoginResponse> login = null;
        Task<UploadResponse> upload = null;
        Task<SubmissionStatusResponse> tasks = null;
        Task<JobStatusResponse> tasks2 = null;
        Task<AstrometryResult> tasks3 = null;
        bool bFormClosing = false;
        bool bASPSClosing = false;
        CancellationTokenSource tokenSource;
        CancellationToken token;
        Astrometry.TICPOS_struct ticpos = new Astrometry.TICPOS_struct();
        bool bTicposSet = false;


        public struct AstrometryResult
        {
            /// <summary>
            /// Exit keyword (text): OK/ERROR (Plate solving performed/Unable to perform plate solving)
            /// </summary>
            public string resExit;

            /// <summary>
            /// Right ascension (numeric): Reports the resulting Right Ascension equatorial J2000 coordinate
            /// of center of image "FileName", expressed in decimal degrees.
            /// </summary>
            public double resRA;

            /// <summary>
            /// Declination (numeric): Reports the resulting Declination equatorial J2000 coordinate
            /// of center of image "FileName", expressed in decimal degrees.
            /// </summary>
            public double resDec;

            /// <summary>
            /// Radius of 
            /// </summary>
            public double resRadius;

            /// <summary>
            /// FOV width (numeric): Image field width(horizontal axis) of image "FileName", 
            /// expressed in arcminutes.
            /// </summary>
            public double resFOVwidth;

            /// <summary>
            /// FOV height (numeric): Image field height(vertical axis) of image "FileName",
            /// expressed in arcminutes.
            /// </summary>
            public double resFOVheight;

            /// <summary>
            /// Image rotation (numeric): Camera rotation of image "FileName", expressed in decimal degrees.
            /// Data convention CROTA2 of .wcs format (world coordinate system).
            /// </summary>
            public double resOrientation;

            /// <summary>
            /// Image scale (numeric): Scale of image "FileName", expressed in arcseconds per pixel.
            /// </summary>
            public double resPixelscale;

            /// <summary>
            /// Focal length (numeric): Real focal length of optics used, in millimeters.
            /// It depends by the pixel size declared in input.
            /// </summary>
            public double resFocalLength;

            /// <summary>
            /// Number of Objects returned in plate
            /// </summary>
            public int resObjectNum;

            /// <summary>
            /// Object names returned in plate
            /// </summary>
            public string[] resObject;
        }

        public AstrometryResult astrometryResult;

        public bool astrometryResultSet = false;

        /// <summary>
        /// Initialize AstrometryForm
        /// </summary>
        /// <param name="oName">the filename of the plate to solve</param>
        /// <param name="img">the FITS image to upload</param>
        public AstrometryForm(string oName, Fits.imageptr img) {
            InitializeComponent();
            this.objName = oName;
            this.image = img;
            lblObject.Text = this.objName;
            pictureBox1.Image = Fits.GetFITSImage(this.image, 1.0, Fits.transform_type.GAM);
            cbSolveMethod.SelectedIndex = solveMethod;
        }

        private void AstrometryForm_Shown(object sender, EventArgs e) {
            bFormClosing = false;
            Application.DoEvents();
            switch (cbSolveMethod.SelectedIndex){
                case 0://Astrometry.net direct
                    astrometryPlateSolve(objName);
                    break;
                case 1://All Sky Plate Solver
                    btnSolve_Click(sender, e);
                    break;
            }
            
        }

        private void btnSolve_Click(object sender, EventArgs e) {
            switch (cbSolveMethod.SelectedIndex){
                case 0://Astrometry.net direct
                    if (btnSolve.Text.Equals("Solve Plate")){
                        btnSolve.Text = "Stop Solving";
                        bASPSClosing = false;
                        astrometryPlateSolve(objName);
                    }else{
                        tokenSource.Cancel();
                        bASPSClosing = true;
                        lblStatus.Text = "Aborting Solving...";
                        btnSolve.Text = "Solve Plate";
                    } 
                    break;
                case 1://All Sky Plate Solver
                    if (btnSolve.Text.Equals("Solve Plate")){
                        btnSolve.Text = "Stop Solving";
                        bASPSClosing = false;
                        AllSkyPlateSolve();
                    }else{
                        bASPSClosing = true;
                        btnSolve.Text = "Solve Plate";
                        lblStatus.Text = "Aborting Solving...";
                    }
                    break;
            }
        }

        private void btnExit_Click(object sender, EventArgs e){
            this.Close();
        }

        public static string friendly_time() {
            return DateTime.Now.ToString("HH:mm:ss.fff")+": ";
        }

        private void cbSolveMethod_SelectedIndexChanged(object sender, EventArgs e) {
            if (cbSolveMethod.SelectedIndex == 0){
                btnRunSolver.Visible = false;
                btnConfigureSolver.Visible = false;
                btnIndexWizz.Visible = false;
                groupBox1.Width = btnSolve.Left + btnSolve.Width + 10;
            }else{
                btnRunSolver.Visible = true;
                btnConfigureSolver.Visible = true;
                btnIndexWizz.Visible = true;
                groupBox1.Width = btnConfigureSolver.Left + btnConfigureSolver.Width + 10;
            }
        }

        private bool CheckIfASDSExists() {
            try{
                if (Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Classes").OpenSubKey("AllSkyPlateSolver.PlateSolver").ValueCount > 0) return true;
            }catch (Exception){
                return false;
            }
            return false;
        }

        private void btnSavePlate_Click(object sender, EventArgs e)
        {
            try{
                // Create bitmap with graphis...
                Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                Graphics g = Graphics.FromImage(bmp);
                Rectangle rect = pictureBox1.RectangleToScreen(pictureBox1.ClientRectangle);
                g.CopyFromScreen(rect.Location, Point.Empty, pictureBox1.Size);
                // ...select file to save bitmap...
                saveFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.Title = "Select Image Filename to Save";
                saveFileDialog1.DefaultExt = "jpg";
                saveFileDialog1.Filter = "jpg files (*.jpg)|*.jpg|All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.CheckFileExists = false;
                saveFileDialog1.CheckPathExists = true;
                string fn = "";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK){
                    fn = saveFileDialog1.FileName;
                    // ...and save it.
                    bmp.Save(fn, System.Drawing.Imaging.ImageFormat.Jpeg);
                    txt.Text += friendly_time() + "File '" + fn + "' saved successfully.\r\n";
                }else{
                    return;
                }
            }
            catch (Exception ex) { txt.Text += friendly_time() + "Nothing saved.\r\nError: " + ex.Message+"\r\n"; }

        }

        private void btnConfigureSolver_Click(object sender, EventArgs e)
        {
            if (!CheckIfASDSExists()) { MessageBox.Show("The AllSkyPlateSolver is not installed on this machine."); return; }
            if (!System.IO.File.Exists(exePlateSolver)) { MessageBox.Show("Cannot find AllSkyPlateSolver executable on this machine."); return; }
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = exePlateSolver;
            process.StartInfo.Arguments = "/sfsettings";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            txt.Text += process.StandardOutput.ReadToEnd();
            txt.Text += "\r\n";
        }

        private void btnRunSolver_Click(object sender, EventArgs e)
        {
            if (!CheckIfASDSExists()) { MessageBox.Show("The AllSkyPlateSolver is not installed on this machine."); return; }
            if (!System.IO.File.Exists(exePlateSolver)) { MessageBox.Show("Cannot find AllSkyPlateSolver executable on this machine."); return; }
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = exePlateSolver;
            process.StartInfo.Arguments = "/solve";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            txt.Text += process.StandardOutput.ReadToEnd();
            txt.Text += "\r\n";
        }

        private void btnIndexWizz_Click(object sender, EventArgs e)
        {
            if (!CheckIfASDSExists()) { MessageBox.Show("The AllSkyPlateSolver is not installed on this machine."); return; }
            if (!System.IO.File.Exists(exePlateSolver)) { MessageBox.Show("Cannot find AllSkyPlateSolver executable on this machine."); return; }
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = exePlateSolver;
            process.StartInfo.Arguments = "/sfindex";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            txt.Text += process.StandardOutput.ReadToEnd();
            txt.Text += "\r\n";
        }

        private void AstrometryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bFormClosing = true;
            if(process!=null)
            if (!process.HasExited){
                Console.Out.WriteLine("Killing ASPS process...");
                txt.Text += friendly_time() + "Killing ASPS process...\r\n";
                Application.DoEvents();
                process.Kill();
                while (!process.HasExited){
                    Application.DoEvents();
                }
                process.Close();
                process.Dispose();
                Console.Out.WriteLine("ASPS process has been ended by user.");
            }
            if (tokenSource != null){
                tokenSource.Cancel();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //// Astrometry.net direct method  ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        // version 1.2.0   @ 20190317
        // Astrometry PlateSolver using Astrometry.net APIs
        // It takes about 30-60 seconds to solve a plate
        /// <summary>
        /// Solves Plate by using astrometry.net API's
        /// </summary>
        /// <param name="fileName">The filename to upload for solving</param>
        /// <returns></returns>
        public string astrometryPlateSolve(string fileName)
        {
            //btnSolve.Enabled = false;
            bTicposSet = false;
            const string apiKey = Astrometry.ASTROMETRY_NET_PASS;
            string file = fileName;
            string ret = "";
            txt.Text += friendly_time() + "Starting plate solving by direct contact of 'astrometry.net'\r\n";
            Application.DoEvents();
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
            token.ThrowIfCancellationRequested();
            try {
                progressIndicator1.Visible = true;
                progressIndicator1.Start();
                lblStatus.Text = "Login...";
                Application.DoEvents();
                client = new Client(apiKey);
                login = SubmitLoginAsync(apiKey, token);
                while (!login.IsCompleted){
                    Application.DoEvents();
                }
                progressIndicator1.Stop();
                Console.WriteLine("Login: " + login.Status);
                lblStatus.Text = "Login: " + login.Status;
                txt.Text += friendly_time() + "Login : " + login.Status + "\r\n";
                Application.DoEvents();

                Console.WriteLine("Uploading file...");
                txt.Text += friendly_time() + "Uploading file '" + objName + "\r\n";
                lblStatus.Text = "Uploading file...";
                progressIndicator1.Start();
                Application.DoEvents();
                upload = SubmitUploadAsync(fileName, token);
                while (!upload.IsCompleted){
                    Application.DoEvents();
                }
                progressIndicator1.Stop();
                Console.WriteLine("File uploaded. Starting Submission...");
                lblStatus.Text = "File uploaded. Starting Submission...";
                txt.Text += friendly_time() + "File uploaded.\r\n"+ friendly_time() + "Starting Submission...\r\n";
                progressIndicator1.Start();
                Application.DoEvents();
                tasks = SubmitTasksAsync(client, upload.Result, token);
                while (!tasks.IsCompleted){
                    Application.DoEvents();
                }
                txt.Text += friendly_time() + "Submission completed.\r\n"+ friendly_time() + "Starting Main Job\r\n";//tasks.Result.ToString();
                lblStatus.Text = "Starting Main Job";
                Application.DoEvents();
                tasks2 = SubmitTasksAsync2(client, tasks.Result, token);
                while (!tasks2.IsCompleted){
                    Application.DoEvents();
                }
                txt.Text += friendly_time() + "Main Job completed.\r\n"+ friendly_time() + "Starting Calibration\r\n";// tasks2.Result.ToString();
                lblStatus.Text = "Main Job completed.Starting Calibration";
                Application.DoEvents();

                tasks3 = SubmitTasksAsync3(client, tasks2.Result, tasks.Result, token);
                while (!tasks3.IsCompleted){
                    Application.DoEvents();
                }
                txt.Text += friendly_time() + "Calibration completed.\r\n";// tasks2.Result.ToString();
                lblStatus.Text = "Calibration completed.";
                Application.DoEvents();

                astrometryResult = new AstrometryResult
                {
                    resRA = tasks3.Result.resRA,
                    resDec = tasks3.Result.resDec,
                    resRadius = tasks3.Result.resRadius,
                    resOrientation = tasks3.Result.resOrientation,
                    resPixelscale = tasks3.Result.resPixelscale
                };
                txt.Text += "\tRA : " + astrometryResult.resRA + "(" + RADegreesToSexagesimal(astrometryResult.resRA) + ")\r\n";
                txt.Text += "\tDec : " + astrometryResult.resDec + "(" + DecDegreesToSexagesimal(astrometryResult.resDec) + ")\r\n";
                txt.Text += "\tRadius : " + astrometryResult.resRadius + " (arcmin)\r\n";
                txt.Text += "\tOrientation : " + astrometryResult.resOrientation + " (degrees)\r\n";
                txt.Text += "\tPixelscale  : " + astrometryResult.resPixelscale + " (arcsec/pixel)\r\n";
                txt.Text += "\tObjects (" + astrometryResult.resObjectNum + "):\r\n";
                astrometryResultSet = true;
                astrometryResult.resObjectNum = tasks3.Result.resObjectNum;
                astrometryResult.resObject = new string[astrometryResult.resObjectNum];
                int i = 0;
                foreach (string obj in tasks3.Result.resObject) astrometryResult.resObject[i++] = obj;
                foreach (string obj in astrometryResult.resObject) txt.Text += "\t-" + obj + "\r\n";
                progressIndicator1.Stop();
                //ticpos = new Astrometry.TICPOS_struct{
                //    ticsize = 75,
                //    pixlen = image.xsize,
                //    deglen = astrometryResult.resPixelscale * image.xsize / 3600
                //};
                //ticpos = Astrometry.TicPos(ticpos);
                bTicposSet = true;
                List<VisieRCatalog.Catalog> vi;
                if (chkVisier.Checked)
                    vi = GetVisieRCatalog(astrometryResult.resRA, astrometryResult.resDec, astrometryResult.resPixelscale * image.xsize / 60, astrometryResult.resPixelscale * image.ysize / 60);// WIDTH_MIN, HEIGHT_MIN);
                else
                    vi = null;
                SetGrid(vi);
            }catch (OperationCanceledException){
                Console.WriteLine("astrometry.net aborted ...");
                txt.Text += friendly_time() + "astrometry.net aborted.\r\n";
            }
            catch (Exception e){
                Console.WriteLine(e.Message);
                txt.Text += friendly_time() + e.Message;
            }finally{
                Console.WriteLine("SolvePlate finished ...");
                tokenSource.Dispose();
                txt.Text += friendly_time() + "SolvePlate finished ...\r\n";
                tokenSource = null;
                btnSolve.Text = "Solve Plate";
            }
            progressIndicator1.Visible = false;
            btnSolve.Enabled = true;
            return ret;
        }


        Task<LoginResponse> SubmitLoginAsync(string apikey, CancellationToken token) {
            return Task.Run(() => SubmitLogin(apikey, token));
        }

        private LoginResponse SubmitLogin(string apiKey, CancellationToken token) {
            if(client==null) client = new Client(apiKey);
            var res = client.Login();
            return res;
        }


        Task<UploadResponse> SubmitUploadAsync(string fn, CancellationToken token) {
            return Task.Run(() => SubmitUpload(fn, token));
        }

        private UploadResponse SubmitUpload(string fn, CancellationToken token) {
            var uploadArguments = new UploadArgs { publicly_visible = Visibility.n };
            var uploadResponse = client.Upload(fn, uploadArguments);
            return uploadResponse;
        }


        Task<SubmissionStatusResponse> SubmitTasksAsync(Client client, UploadResponse uploadResponse, CancellationToken token) {
            return Task.Run(() => SubmitTasks(client, uploadResponse, token));
        }

        private Task<SubmissionStatusResponse> SubmitTasks(Client client, UploadResponse uploadResponse, CancellationToken token) {
            string txt1 = "";
            Task<SubmissionStatusResponse> submissionStatusResponse = null;
            try{
                submissionStatusResponse = client.GetSubmissionStatus(uploadResponse.subid, token);
            } catch (Exception e){
                Console.WriteLine(e.Message);
                txt1 += friendly_time() + e.Message;
            }finally{
                //tokenSource.Dispose();
            }
            return submissionStatusResponse;
        }

        Task<JobStatusResponse> SubmitTasksAsync2(Client client, SubmissionStatusResponse submissionStatusResponse, CancellationToken token)
        {
            return Task.Run(() => SubmitTasks2(client, submissionStatusResponse, token));
        }

        private Task<JobStatusResponse> SubmitTasks2(Client client, SubmissionStatusResponse submissionStatusResponse, CancellationToken token)
        {
            Task<JobStatusResponse> jobStatusResponse = null;
            string txt1 = "";
            try{
                jobStatusResponse = client.GetJobStatus(submissionStatusResponse.jobs[0], token);
                Console.WriteLine("Tasks returned...");
            }catch (Exception e){
                Console.WriteLine(e.Message);
                txt1 += friendly_time() + e.Message;
            }finally{
                //tokenSource.Dispose();
            }
            return jobStatusResponse;
        }

        Task<AstrometryResult> SubmitTasksAsync3(Client client, JobStatusResponse jobStatusResponse, SubmissionStatusResponse submissionStatusResponse, CancellationToken token)
        {
            return Task.Run(() => SubmitTasks3(client, jobStatusResponse, submissionStatusResponse, token));
        }

        private AstrometryResult SubmitTasks3(Client client, JobStatusResponse jobStatusResponse, SubmissionStatusResponse submissionStatusResponse, CancellationToken token)
        {
            AstrometryResult astroRes = new AstrometryResult();
            try{
                if (jobStatusResponse.status.Equals(ResponseJobStatus.success)){
                    var calibrationResponse = client.GetCalibration(submissionStatusResponse.jobs[0]);
                    astroRes.resRA = calibrationResponse.ra;
                    astroRes.resDec = calibrationResponse.dec;
                    astroRes.resRadius = calibrationResponse.radius;
                    astroRes.resFOVwidth = 2*calibrationResponse.radius;
                    astroRes.resFOVheight = 2*calibrationResponse.radius;
                    astroRes.resOrientation = calibrationResponse.orientation;
                    astroRes.resPixelscale = calibrationResponse.pixscale;
                    Console.WriteLine("RA         : " + calibrationResponse.ra);
                    Console.WriteLine("Dec         : " + calibrationResponse.dec);
                    Console.WriteLine("Radius      : " + calibrationResponse.radius);
                    Console.WriteLine("Orientation (degrees) : " + calibrationResponse.orientation);
                    Console.WriteLine("Pixelscale (arcsec/pixel): " + calibrationResponse.pixscale);
                    Application.DoEvents();
                    var objectsInFieldResponse = client.GetObjectsInField(submissionStatusResponse.jobs[0]);
                    Console.WriteLine("");
                    astroRes.resObjectNum = objectsInFieldResponse.objects_in_field.Length;
                    astroRes.resObject = new string[astroRes.resObjectNum];
                    int i = 0;
                    foreach (string obj in objectsInFieldResponse.objects_in_field){
                        Console.WriteLine(obj);
                        astroRes.resObject[i++] = obj;
                    }
                    Application.DoEvents();
                }else{
                    Console.WriteLine("Status : " + jobStatusResponse.status);
                }
            }catch (Exception e){
                Console.WriteLine(e.Message);
            }finally{
                //tokenSource.Dispose();
            }
            return astroRes;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //// END of Astrometry.net direct method  /////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //// All Sky Plate Solve method  ///////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void AllSkyPlateSolve()
        {
            if(!CheckIfASDSExists()) { MessageBox.Show("The AllSkyPlateSolver is not installed on this machine."); return; }
            if (!System.IO.File.Exists(exePlateSolver)) { MessageBox.Show("Cannot find AllSkyPlateSolver executable on this machine.");return; }
            bTicposSet = false;
            lblStatus.Text = "Solving with All Sky PlateSolver";
            progressIndicator1.Visible = true;
            progressIndicator1.Start();
            string outFile = Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "\\asps_test.txt";
            process = new System.Diagnostics.Process();
            if (System.IO.File.Exists(outFile))System.IO.File.Delete(outFile);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = exePlateSolver;
            process.StartInfo.Arguments = "/solvefile "+objName+" "+outFile;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            txt.Text += friendly_time() + "Starting PlateSolver.exe\r\n";
            process.Start();
            while (!System.IO.File.Exists(outFile)) {
                if (bFormClosing || bASPSClosing){
                    if (!process.HasExited) process.Kill();
                    while (!process.HasExited) Application.DoEvents();
                    Console.Out.WriteLine("Exiting process...");
                    txt.Text += friendly_time() + "Exiting process...\r\n";
                    progressIndicator1.Stop();
                    progressIndicator1.Visible = false;
                    btnSolve.Text = "Solve Plate";
                    return;
                } 
                Application.DoEvents();
            }
            txt.Text += process.StandardOutput.ReadToEnd();
            txt.Text += "\r\n";
            process.Close();
            process = null;
            progressIndicator1.Stop();
            progressIndicator1.Visible = false;
            txt.Text += friendly_time() + "Finished Solving\r\n";
            astrometryResult = new AstrometryResult();
            if (System.IO.File.Exists(outFile))
            {
                string[] lines = System.IO.File.ReadAllLines(outFile);
                string decSeparator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
                string thousandSeparator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberGroupSeparator;
                //Exit keyword <text> OK/ ERROR
                if (lines[0].Equals("OK") && lines.Length>=8) {
                    //Right ascension
                    astrometryResult.resRA = double.Parse(lines[1].Replace(thousandSeparator, decSeparator));
                    //Declination
                    astrometryResult.resDec = double.Parse(lines[2].Replace(thousandSeparator, decSeparator));
                    //FOV width 
                    astrometryResult.resFOVwidth = double.Parse(lines[3].Replace(thousandSeparator, decSeparator));
                    //FOV height
                    astrometryResult.resFOVheight = double.Parse(lines[4].Replace(thousandSeparator, decSeparator));
                    //Radius
                    astrometryResult.resRadius = (astrometryResult.resFOVwidth + astrometryResult.resFOVheight) / 2;
                    //Image scale
                    astrometryResult.resPixelscale = double.Parse(lines[5].Replace(thousandSeparator, decSeparator));
                    //Image rotation
                    astrometryResult.resOrientation = double.Parse(lines[6].Replace(thousandSeparator, decSeparator));
                    //Focal length
                    astrometryResult.resFocalLength = double.Parse(lines[7].Replace(thousandSeparator, decSeparator));
                    txt.Text += "Plate Soving Results:\r\n\tRA : " + astrometryResult.resRA + " ("+RADegreesToSexagesimal(astrometryResult.resRA)+")\r\n";
                    txt.Text += "\tDec : " + astrometryResult.resDec + " ("+DecDegreesToSexagesimal(astrometryResult.resDec)+")\r\n";
                    txt.Text += "\tFOVwidth (arcmin): " + astrometryResult.resFOVwidth + "\r\n";
                    txt.Text += "\tFOVheight (arcmin): " + astrometryResult.resFOVheight + "\r\n";
                    txt.Text += "\tOrientation (degrees): " + astrometryResult.resOrientation + "\r\n";
                    txt.Text += "\tPixelscale  (arcsec/pixel): " + astrometryResult.resPixelscale + "\r\n";
                    txt.Text += "\tFocal length (mm): " + astrometryResult.resFocalLength + "\r\n";
                    astrometryResultSet = true;
                    //ticpos = new Astrometry.TICPOS_struct{
                    //    ticsize = 75,
                    //    pixlen = image.xsize,
                    //    deglen = astrometryResult.resPixelscale * image.xsize / 3600
                    //};
                    //ticpos = Astrometry.TicPos(ticpos);
                    bTicposSet = true;
                    List<VisieRCatalog.Catalog> vi;
                    if (chkVisier.Checked)
                        vi = GetVisieRCatalog(astrometryResult.resRA, astrometryResult.resDec, astrometryResult.resPixelscale * image.xsize / 60, astrometryResult.resPixelscale * image.ysize / 60);// WIDTH_MIN, HEIGHT_MIN);
                    else
                        vi = null;
                    SetGrid(vi);
                    lblStatus.Text = "All Sky Plate Solver finished";
                }
                else{
                    txt.Text += friendly_time() + "All Sky Plate Solver returned ERROR.\r\n";
                    lblStatus.Text = "All Sky Plate Solver ERROR";
                }
                System.IO.File.Delete(outFile);
            }else{
                txt.Text += friendly_time() + "File " + outFile + " could not be created.\r\n";
                lblStatus.Text = "File could not be created";
            }
            btnSolve.Text = "Solve Plate";
            bASPSClosing = false;
        }



        //////////////////////////////////////////////////////////////////////////////
        //  Graphics and grids
        // 
        //////////////////////////////////////////////////////////////////////////////

        private List<VisieRCatalog.Catalog> GetVisieRCatalog(double RA, double DEC, double widthmin, double heightmin) {
            Console.Out.WriteLine("GetVisieRCatalog with RA={0} DEC={1}", RA, DEC);
            string[] ra = RADegreesToSexagesimal(RA,2).Replace(',', '.').Split('.');
            string[] dec = DecDegreesToSexagesimal(DEC,2).Replace(',', '.').Split('.');
            Console.Out.WriteLine("GetVisieRCatalog with RA={0} DEC={1} RADIUS={2}/{3} ra={4} dec={5}", RA, DEC, widthmin, heightmin, ra[0],dec[0]);
            string res = HttpHelper.Unso(ra[0], dec[0], (float)widthmin, (float)heightmin, 100);
            string[] lines = res.Split(new string[] { "\n" }, StringSplitOptions.None);
            List<VisieRCatalog.Catalog> visier = new List<VisieRCatalog.Catalog>();
            //int i = 0;
            foreach (string s in lines){
                if (s.StartsWith("#")){
                    //ignore comment lines
                }else if (s.Length > 1 && IsDigit(s.Trim()[0])){
                    VisieRCatalog.Catalog vi = new VisieRCatalog.Catalog(s);
                    if (!vi.USNO.Equals("Error"))
                        visier.Add(vi);
                }
            }
            Console.Out.WriteLine("GetVisieRCatalog Objects found: {0}", visier.Count);
            return visier;
        }

        private bool IsDigit(char c) {
            if (c.Equals('1') || c.Equals('2') || c.Equals('3') || c.Equals('4')
                || c.Equals('5') || c.Equals('6') || c.Equals('7') || c.Equals('8')
                || c.Equals('9') || c.Equals('0')) return true;
            else return false;
        }

        private void SetGrid(List<VisieRCatalog.Catalog> vi = null) {
            if (!bTicposSet) return;
            //reload the image
            pictureBox1.Image = Fits.GetFITSImage(image, 1.0, Fits.transform_type.GAM);
            //Astrometry.TICPOS_struct ticpos = new Astrometry.TICPOS_struct();
            Matrix m = new Matrix();
            Bitmap bm = (Bitmap)pictureBox1.Image;
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);// (image.xsize, image.ysize);
            m.Rotate((float)astrometryResult.resOrientation, MatrixOrder.Append);
            Graphics gr = Graphics.FromImage(pictureBox1.Image);
            GraphicsPath gp = new GraphicsPath(FillMode.Winding);
            System.Drawing.Image imgpic = (System.Drawing.Image)bm;// pictureBox1.Image.Clone();
            //the coordinate of the polygon must be: point 1 = left-top corner, 
            //                                       point 2 = right-top corner, 
            //                                       point 3 = left-bottom
            gp.AddPolygon(new Point[]{new Point(0,0),
                new Point(imgpic.Width,0),
                new Point(0,imgpic.Height)});
            gp.Transform(m);
            PointF[] pts = gp.PathPoints;
            gr.DrawImage(imgpic, pts);
            pictureBox1.Refresh();
            
            //Setting vertical grid-lines
            ticpos = new Astrometry.TICPOS_struct{
                ticsize = 75,
                pixlen = image.xsize,
                deglen = astrometryResult.resPixelscale * image.xsize / 3600
            };
            ticpos = Astrometry.TicPos(ticpos);
            double xstep = ticpos.ticsize;
            double sc = 1.0;
            if (ticpos.units.ToUpper().Equals("ARC MINUTES")) sc = 60.0;
            if (ticpos.units.ToUpper().Equals("DEGREES")) sc = 3600.0;
            double xincr = sc * ticpos.incr / 3600;
            Console.Out.WriteLine("X: ticsize={0} incr={1} {2} xincr={3}", ticpos.ticsize, ticpos.incr, ticpos.units, xincr);
            //txt.Text += "RA units: " + ticpos.units;
            Graphics g = pictureBox1.CreateGraphics();
            int maxsteps = (int)(image.xsize / xstep / 2) + 1;
            float cx0 = image.xsize / 2;
            float cy0 = image.ysize / 2;
            Font font = new Font("Calibri", 8f);
            for (int i=0; i < maxsteps; i++)
            {
                g.DrawLine(new Pen(Color.LightGray, 0.5f), (float)(cx0 + i * xstep), 16, (float)(cx0 + i * xstep), image.ysize-16);
                string s = String.Format("{0}", DecDegreesToSexagesimal(astrometryResult.resRA + i*xincr, 0));
                SizeF size = g.MeasureString(s, font);
                g.DrawString(s, font, new SolidBrush(Color.LightGray), (float)(cx0 + i * xstep)-size.Width/2,0f);
                
                g.DrawLine(new Pen(Color.LightGray, 0.5f), (float)(cx0 - i * xstep), 16, (float)(cx0 - i * xstep), image.ysize-16);
                s = String.Format("{0}", DecDegreesToSexagesimal(astrometryResult.resRA - i*xincr, 0));
                size = g.MeasureString(s, font);
                g.DrawString(s, font, new SolidBrush(Color.LightGray), (float)(cx0 - i * xstep)-size.Width/2, 0f);
            }
            //Setting horiontal grid-lines
            ticpos = new Astrometry.TICPOS_struct{
                ticsize = 75,
                pixlen = image.ysize,
                deglen = astrometryResult.resPixelscale * image.ysize / 3600
            };
            ticpos = Astrometry.TicPos(ticpos);
            double ystep = ticpos.ticsize;
            sc = 1.0;
            if (ticpos.units.ToUpper().Equals("ARC MINUTES"))sc = 60.0;
            if (ticpos.units.ToUpper().Equals("DEGREES")) sc = 3600.0;
            double yincr = sc * ticpos.incr / 3600;
            Console.Out.WriteLine("Y: ticsize={0}px incr={1} {2} yincr={3}", ticpos.ticsize, ticpos.incr, ticpos.units, yincr);
            //txt.Text += "Dec units: " + ticpos.units;
            int maxstepy = (int)(image.ysize / ystep / 2) + 1;
            for (int i = 0; i < maxstepy; i++)
            {
                string s = String.Format("{0}", DecDegreesToSexagesimal(astrometryResult.resDec + i*yincr, 0));
                SizeF size = g.MeasureString(s, font);
                g.DrawLine(new Pen(Color.WhiteSmoke, 0.5f), size.Width, (float)(cy0 + i * ystep), image.xsize, (float)(cy0 + i * ystep));
                g.DrawString(s, font, new SolidBrush(Color.LightGray), 0f, (float)(cy0 + i * ystep) - size.Height / 2);

                s = String.Format("{0}", DecDegreesToSexagesimal(astrometryResult.resDec - i*yincr, 0));
                size = g.MeasureString(s, font);
                g.DrawLine(new Pen(Color.WhiteSmoke, 0.5f), size.Width, (float)(cy0 - i * ystep), image.xsize, (float)(cy0 - i * ystep));
                g.DrawString(s, font, new SolidBrush(Color.LightGray), 0f, (float)(cy0 - i * ystep) - size.Height / 2);
            }
            // draw Visier catalog if not null
            Console.Out.WriteLine("xincr={0} yincr={1} deglen={2}", xincr, yincr, ticpos.deglen);
            if (vi != null){
                if (vi.Count > 0){
                    int i = 0;
                    foreach (VisieRCatalog.Catalog v in vi){
                        //WCSUtil.worldpix
                        int ans = WCSUtils.xypix(v.RAJ2000, v.DEJ2000,
                                        astrometryResult.resRA, astrometryResult.resDec,
                                        image.xsize/2, image.ysize/2, xincr, yincr, //astrometryResult.resRadius/60, astrometryResult.resRadius / 60,
                                        astrometryResult.resOrientation, "-SIN",
                                        out double xpix, out double ypix);
                        if (ans == 0){
                            //PointF p = RADECtoPIX(cx0, cy0, astrometryResult.resRA, astrometryResult.resDec, v.RAJ2000, v.DEJ2000, ticpos.ticsize, yincr);// ticpos.incr);
                            float rad = 6f;
                            g.DrawEllipse(new Pen(Color.Red, 0.5f), (float)xpix, (float)ypix, rad, rad);
                            //g.DrawEllipse(new Pen(Color.Yellow, 0.5f), p.X, p.Y, rad, rad);
                        }
                        //Console.Out.WriteLine("{0}. ra={1} dec={2} x={3} y={4}", i++, RADegreesToSexagesimal(v.RAJ2000), DecDegreesToSexagesimal(v.DEJ2000), p.X, p.Y);
                        Console.Out.WriteLine("{0}. ans={1} x={2} y={3} RA={4} Dec={5}", i++, ans, xpix, ypix, v.RAJ2000, v.DEJ2000);
                    }
                }else{
                    Console.Out.WriteLine("VisieR list is Empty");
                }
            }else{
                Console.Out.WriteLine("VisieR list is Null");
            }
        }

        private PointF RADECtoPIX(float cx0, float cy0, double ra0, double dec0, double ra, double dec, double ticsize, double incr) {
            double ix = ra0 - ra;
            double iy = dec0 - dec;
            float y = (float)(cy0 - iy * ticsize / incr);
            float x = (float)(cx0 - ix * ticsize / incr); //Math.Cos(dec0 * Math.PI / 180) *
            Console.Out.WriteLine("RADECtoPIX point {0}, {1}", x, y);
            return new PointF(x, y);
        }
        //////////////////////////////////////////////////////////////////////////////
        // Converters  for RA and Dec ////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////

        public double DecSexagesimalToDegrees(string sexa) {
            string[] ss = sexa.Split(' ');
            if (ss.Length != 3) return 0;
            double deg = 0;
            double min = 0;
            double sec = 0;
            string decSeparator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            string thousandSeparator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberGroupSeparator;
            try
            {
                deg = double.Parse(ss[0]);
                min = double.Parse(ss[1]);
                sec = double.Parse(ss[2].Replace(thousandSeparator, decSeparator));
                return deg + min / 60.0 + sec / 3600.0;
            }catch (Exception) { return 0; }
        }

        public string DecDegreesToSexagesimal(double dd, int secDigits=3) {
            int deg = (int)dd;
            int min = (int)((dd - deg) * 60);
            double sec = ((dd - deg) * 60 - min) * 60;
            string sign = "+";
            if (dd < 0) sign = "-";
            string formatSec = "";
            switch (secDigits){
                case 0:
                    formatSec = "{0:00}";
                    break;
                case 1:
                    formatSec = "{0:00.0}";
                    break;
                case 2:
                    formatSec = "{0:00.00}";
                    break;
                case 3:
                    formatSec = "{0:00.000}";
                    break;
            }
            return sign + String.Format("{0:D2}", deg) + " " + String.Format("{0:D2}", min) + " " + String.Format(formatSec, sec);
        }

        public double RASexagesimalToDegrees(string sexa) {
            string[] ss = sexa.Split(' ');
            if (ss.Length != 3) return 0;
            double h = 0;
            double m = 0;
            double s = 0;
            string decSeparator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            string thousandSeparator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberGroupSeparator;
            try
            {
                h = double.Parse(ss[0]);
                m = double.Parse(ss[1]);
                s = double.Parse(ss[2].Replace(thousandSeparator, decSeparator));
                return h * 15.0 + m / 4.0 + s / 240.0;
            }catch (Exception) { return 0; }
        }

        public string RADegreesToSexagesimal(double dd, int secDigits=3) {
            int h = (int)(dd/15.0);
            int m = (int)((dd - h * 15.0) * 4.0);
            double s = ((dd - h * 15.0) * 4.0 - m) * 60.0;
            string formatSec = "";
            switch (secDigits){
                case 0:
                    formatSec = "{0:00}";
                    break;
                case 1:
                    formatSec = "{0:00.0}";
                    break;
                case 2:
                    formatSec = "{0:00.00}";
                    break;
                case 3:
                    formatSec = "{0:00.000}";
                    break;
            }
            return "" + String.Format("{0:D2}", h) + " " + String.Format("{0:D2}", m) + " " + String.Format(formatSec, s);
        }

        private void AstrometryForm_Resize(object sender, EventArgs e) {
            return;
            if (astrometryResult.resPixelscale > 0)
            {
                ticpos = new Astrometry.TICPOS_struct{
                    ticsize = 75,
                    pixlen = pictureBox1.Width,// image.xsize;
                    deglen = astrometryResult.resPixelscale * pictureBox1.Height / 3600// image.xsize / 3600;
                };
                ticpos = Astrometry.TicPos(ticpos);
                bTicposSet = true;
                SetGrid();
            }
        }

        private void chkVisier_CheckedChanged(object sender, EventArgs e) {
            if (!astrometryResultSet) return;
            progressIndicator2.Visible = true;
            progressIndicator2.Start();
            Application.DoEvents();
            List<VisieRCatalog.Catalog> vi;
            if (chkVisier.Checked)
                vi = GetVisieRCatalog(astrometryResult.resRA, astrometryResult.resDec, astrometryResult.resPixelscale * image.xsize / 60, astrometryResult.resPixelscale * image.ysize / 60);
            else
                vi = null;
            SetGrid(vi);
            progressIndicator2.Stop();
            progressIndicator2.Visible = false;
        }
    }
}
