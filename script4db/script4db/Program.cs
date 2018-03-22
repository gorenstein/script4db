using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace script4db
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        private const int ATTACH_PARENT_PROCESS = -1;
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // redirect console output to parent process;
            // must be before any calls to Console.WriteLine()
            AttachConsole(ATTACH_PARENT_PROCESS);
            Console.WriteLine("\nStart");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());

            Console.WriteLine("End\n");
            FreeConsole(); // detach console
            SendKeys.SendWait("^(c)"); // get command prompt back
        }
    }
}
