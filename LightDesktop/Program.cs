using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;

namespace Onyeyiri2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SetAutoRun();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Launchy.Form1());
        }

        static void SetAutoRun()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);//打开注册表子项  
            string szStartFile = Application.ProductName.Replace("/", "\\") + ".exe";
            szStartFile = Path.GetFullPath(szStartFile);
            key.SetValue("MyDesktop", szStartFile);
        }
    }
}
