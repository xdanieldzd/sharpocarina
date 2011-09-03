using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SharpOcarina
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();
        
        public static string ApplicationTitle;
        public static int ApplicationVersion = 0x0040;

        public static MainForm MF;
        public static bool QuitProgram = false;

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MF = new MainForm();
#if DEBUG
            AllocConsole();
            ApplicationTitle = MF.GetType().Namespace + GetVerString(ApplicationVersion) + " (Debug)";
#else
            ApplicationTitle = MF.GetType().Namespace + GetVerString(ApplicationVersion);
#endif           
            MF.Text = ApplicationTitle;

            MF.FormClosed += new FormClosedEventHandler(MF_FormClosed);
            MF.Show();

            do
            {
                MF.ProgramMainLoop();
                Application.DoEvents();

                System.Threading.Thread.Sleep(2);
            }
            while (QuitProgram == false);
        }

        static void MF_FormClosed(object sender, FormClosedEventArgs e)
        {
            QuitProgram = true;
        }

        public static string GetVerString(int Version)
        {
            string VerString = "";

            VerString = " v" +
                (Version >> 8).ToString() + "." +
                ((Version & 0xF0) >> 4).ToString();

            if ((Version & 0xF) != 0)
                VerString += "." + (Version & 0xF).ToString();

            return VerString;
        }
    }
}
