using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace Syria_Transfer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static List<Transfer> Transfers = new List<Transfer>();
        public const string transfersPath = @"D:\Dropbox\Text Files\SyriaTransfers.txt";
        public static string Username, Password;


        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var currentProcess = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(currentProcess.ProcessName);
            var process = processes.FirstOrDefault(p => p.Id != currentProcess.Id);
            if (process != null)
            {
                SetForegroundWindow(process.MainWindowHandle);
                this.Shutdown();
            }
            string pathRes = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\syria";
            string[] dataRes = File.ReadAllLines(pathRes);
            Username = dataRes[0];
            Password = dataRes[1];

            Transfer.GetTransfers();
        }
    }
}
