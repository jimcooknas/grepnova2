using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace grepnova2
{
    partial class AboutBox1 : Form
    {
        public AboutBox1(int idx)
        {
            InitializeComponent();
            switch (idx)
            {
                case 1:
                    aboutGrepnova();
                    break;
                case 2:
                    aboutGrepnova2();
                    break;
            }
        }

        public void aboutGrepnova()
        {
            string descr = "";
            descr = "<font face='Calibri' size='2' color='black'/>" + "<b>GREPNOVA</b> Version 0.6.0<br><br>";
            descr += "GrepNova is an image manipulation tool which aids the image comparison tasks which are required for amateur nova-supernova hunting. " +
                "It is operated by supplying pairs of images, each consisting of a reference library image of an object and a newly obtained image of the same object.<br>" +
                "GrepNova will align the two images into a common orientation and then blink them, allowing easy visual comparison, as is necessary when " +
                "visually searching images of galaxies for new sources which might be nova-supernova candidates. Because such patrolling, by its very nature, " +
                "involves searching large numbers of images, GrepNova includes features to ease the processing many pairs of images in quick succession.<br>";
            descr += "<a href='https://in-the-sky.org/software.php' target='_blank'>GrepNova 0.6.0</a> is free software, distributed under the Gnu General Public License(GPL). For the precise legal terms of this license, " +
                "see the file COPYING.txt. In summary, you are at liberty to redistribute and / or modify this software, so long as its original author " +
                "is credited, and the source code is distributed with the any executable version of the program.However, the author would be very pleased" +
                "to hear from any users who successfully make astronomical discoveries using this software.<br>";
            descr += "<br><a href='https://in-the-sky.org/about.php#dcf21' target='_blank'>Dominic Ford</a><br>" + "Trinity College, Cambridge, CB2 1TQ<br>" + "1st November 2005<br>";
            this.Text = String.Format("About {0}", "grepnova 0.6.0");
            this.labelProductName.Text = "grepnova";
            this.labelVersion.Text = String.Format("Version {0}", "0.6.0");
            this.labelCopyright.Text = "Licensed under GNU GPL";
            this.labelCompanyName.Text = "coder: Dominic Ford";
            this.webBrowser1.DocumentText = descr;
        }

        public void aboutGrepnova2()
        {
            string descr = "";
            descr += "<font face='Calibri' size='2' color='black'/>" + "<b>GrepNova2</b> Version " + AssemblyVersion + "<br><br>";
            descr += "GrepNova2 is coded by Cookn@s (2017 - 2018) as a tool for <b>Greek Supernovae Search Team</b>, based on the beautiful alignment code that <a href='https://in-the-sky.org/about.php' target='_blank'>Dominic Ford</a> wrote back in 2005<br>";
            descr += "Code re-written in C# from scratch using <b>CSharpFits</b> and <b>AForge</b> packages.<br>";
            descr += "<b><a href='https://www.nuget.org/packages/CSharpFITS/' target='_blank'>CSharpFITS</a></b> package (version 1.1.0) is a pure C# .NET port of <b>Tom McGlynn's nom.tam.fits</b> Java package. It provides native C# support for reading and writing FITS files</b><br>";
            descr += "<b><a href='http://www.aforgenet.com/framework/' target='_blank'>AForge.NET</a></b> package (version 2.2.5) is a C# framework designed for developers and researchers in the fields of Computer Vision and Artificial Intelligence - image processing, neural networks, genetic algorithms, machine learning, robotics, etc<br>";
            descr += "<b><a href='https://in-the-sky.org/software.php' target='_blank'>grepnova</a></b> (version 0.6.0) is a platform indepentent application written in C by Dominic Ford at 2005 for image blinking. From this package the <b>likelihood.c</b> file modified just to output alignment parameters (displacement and rotation), which are ued by Grepnova2.<br>";
            descr += "<br>Besides, GrepNova2 uses some custom controls as:<br>";
            descr += "<b><a href='https://www.codeproject.com/Articles/30625/Circular-Progress-Indicator' target='_blank'>Circular Progress Indicator</a></b> is a control written by <b>Nitoc3</b> (Emilio Simões) in Code Project under CPOL (The Code Project Open License)<br>";
            descr += "<br><br>Cooknas (2018)<br><a href='mailto:cooknas@gmail.com'>cooknas@gmail.com</a><br>Kozani, Greece<br>";
            this.Text = String.Format("About {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0}", AssemblyVersion);
            this.labelCopyright.Text = AssemblyCopyright;
            this.labelCompanyName.Text = AssemblyCompany;
            this.webBrowser1.DocumentText = descr;
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
