using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Timer = System.Timers.Timer;

namespace TaskTrayApplication
{
    public class TaskTrayApplicationContext : ApplicationContext
    {
        private Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
        private const string pattern = @"^(smb://.*|//[a-zA-Z].*)";

        private Timer cooldownTimer = new Timer(1500);
        private bool IsOnCooldown = false;

        NotifyIcon notifyIcon = new NotifyIcon();
        Configuration configWindow = new Configuration();

        public TaskTrayApplicationContext()
        {
            MenuItem configMenuItem = new MenuItem("Configuration", new EventHandler(ShowConfig));
            MenuItem exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));

            notifyIcon.Icon = TaskTrayApplication.Properties.Resources.AppIcon;
            notifyIcon.DoubleClick += new EventHandler(ShowMessage);
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] { configMenuItem, exitMenuItem });
            notifyIcon.Visible = true;

            // After 1000ms set IsOnCooldown to false
            cooldownTimer.Elapsed += (sender, e) => IsOnCooldown = false;
            cooldownTimer.Start();
        }

        void ShowMessage(object sender, EventArgs e)
        {
            // Only show the message if the settings say we can.
            if (TaskTrayApplication.Properties.Settings.Default.ShowMessage)
                MessageBox.Show("MacToWindowsLinks © Colin Senner 2018\n\nConverts smb:// mac links to windows formatted links to your clipboard.");
        }

        void ShowConfig(object sender, EventArgs e)
        {
            // If we are already showing the window meerly focus it.
            if (configWindow.Visible)
                configWindow.Focus();
            else
                configWindow.ShowDialog();
        }

        void Exit(object sender, EventArgs e)
        {
            // We must manually tidy up and remove the icon before we exit.
            // Otherwise it will be left behind until the user mouses over.
            notifyIcon.Visible = false;

            Application.Exit();
        }

        public void OnClipboardUpdate(object sender, EventArgs e)
        {
            // Ignore updates for 1500 ms after this is called
            if (IsOnCooldown)
                return;

            IsOnCooldown = true;

            string results = String.Empty;

            if (!Clipboard.ContainsText(TextDataFormat.UnicodeText))
                return;

            string clipboardText = Clipboard.GetText();
            string[] lines = clipboardText.Split('\n');

            foreach (var line in lines)
            {
                MatchCollection matches = rgx.Matches(line);

                if (matches.Count > 0)
                {
                    // This line contains a mac link
                    string s = matches[0].Value;
                    int idx = matches[0].Index;

                    // Strip off the smb: if it's there
                    string link = s.StartsWith("smb:") ? s.Substring(4) : s;

                    results += line.Substring(0, idx);  // Keep the beginning of the line up until we match the regex intact
                    results += link.Replace('/', '\\');  // Replace /'s with \'s
                }
                else
                {
                    // This line doesn't contain a mac link
                    results += line;
                }

                results += "\n";
            }

            // Trim off the last newline character
            results = results.TrimEnd('\n');

            Console.WriteLine();

            try
            {
                Clipboard.Clear();
                Clipboard.SetText(results, TextDataFormat.UnicodeText);
            }
            catch (Exception err)
            {
                string message = string.Format($"Problem setting the clipboard.\nError: {1}", err.Message);
                MessageBox.Show(message, "MacToWindowsLink", MessageBoxButtons.OK);
            }
        }
    }
}
