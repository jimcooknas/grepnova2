using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
using System.Windows.Forms;

namespace grepnova2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            try
            {
                Application.SetCompatibleTextRenderingDefault(false);
            }catch (InvalidOperationException ioe) { Console.Out.WriteLine("Error in Main() - SetCompatibleTextRenderingDefault to false: " + ioe.Message.ToString()); }
            try{
                Application.Run(new Form1());
            }catch (InvalidOperationException ioe1) { Console.Out.WriteLine("Error in Main() - Run (new Form1()): " + ioe1.Message.ToString()); }
        }
    }
}
