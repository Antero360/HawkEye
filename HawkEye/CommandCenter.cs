using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace HawkEye
{
    class CommandCenter
    {
        private static readonly string unauthUseLog = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Hawkeye_Alert.txt";
        private static readonly string errorLog = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Hawkeye_ErrorLog.txt";
        private static Machine thisMachine = Security.GetMachineDetails();
        private static string lastPublicIP = string.Empty;     

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hwind, int showConsole);

        static void Main(string[] args)
        {
            IntPtr window = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(window, 0);
            while (true)
            {
                try
                {
                    if (Security.CheckInternetConnection())
                    {
                        thisMachine.RefreshDetails();
                        if (!thisMachine.PublicIP.Equals(lastPublicIP))
                        {
                            lastPublicIP = thisMachine.PublicIP;
                            
                            try
                            {
                                Security.EmailAdmin(errorLog, unauthUseLog, thisMachine);
                            }
                            catch (Exception exception)
                            {
                                Security.Log(errorLog, exception.StackTrace, unauthUseLog, thisMachine);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Security.Log(errorLog, exception.StackTrace, unauthUseLog, thisMachine);
                }
                
                //sleep for 15 minutes
                Thread.Sleep(900000);
            }            
        }
    }
}