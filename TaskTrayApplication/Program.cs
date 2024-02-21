using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

namespace TaskTrayApplication
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (PriorProcess() != null)
            {
                MessageBox.Show("Another instance of the app is already running.\nPlease close the previous app on the taskbar.");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            TaskTrayApplicationContext tta = new TaskTrayApplicationContext();

            // Subscribe to WM_CLIPBOARDUPDATE event
            ClipboardNotification.ClipboardUpdate += tta.OnClipboardUpdate;
            
            // Instead of running a form, we run an ApplicationContext.
            Application.Run(tta);
        }

        public static Process PriorProcess()
        // Returns a System.Diagnostics.Process pointing to
        // a pre-existing process with the same name as the
        // current one, if any; or null if the current process
        // is unique.
        {
            Process curr = Process.GetCurrentProcess();
            Process[] procs = Process.GetProcessesByName(curr.ProcessName);
            foreach (Process p in procs)
            {
                if ((p.Id != curr.Id) &&
                    (p.MainModule.FileName == curr.MainModule.FileName))
                    return p;
            }
            return null;
        }
    }
}