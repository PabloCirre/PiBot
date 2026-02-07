using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

namespace PiBotInstaller
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            string installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PIBOT");
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string startupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup));

            try {
                if (!Directory.Exists(installDir)) Directory.CreateDirectory(installDir);

                string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                
                // Copy executables and docs from root
                string[] rootFiles = { "PiBotTray.exe", "PiBotControlCenter.exe", "README.md" };
                foreach (string file in rootFiles) {
                    string src = Path.Combine(currentDir, file);
                    string target = Path.Combine(installDir, file);
                    if (File.Exists(src)) File.Copy(src, target, true);
                }

                // Copy script from scripts folder
                string scriptSource = Path.Combine(currentDir, "scripts", "install-pibot-env.sh");
                if (File.Exists(scriptSource)) File.Copy(scriptSource, Path.Combine(installDir, "install-pibot-env.sh"), true);

                CreateShortcut(desktopPath, Path.Combine(installDir, "PiBotTray.exe"), "PIBOT Pro Manager");
                CreateShortcut(desktopPath, Path.Combine(installDir, "PiBotControlCenter.exe"), "PIBOT Control Center");
                CreateShortcut(startupPath, Path.Combine(installDir, "PiBotTray.exe"), "PIBOT Startup");

                MessageBox.Show("PIBOT Pro Beta installed successfully.\nCheck the shortcuts on your desktop.", "PIBOT Beta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Process.Start(Path.Combine(installDir, "PiBotTray.exe"));

            } catch (Exception ex) {
                MessageBox.Show("Installation Error: " + ex.Message, "PIBOT Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void CreateShortcut(string folder, string target, string name)
        {
            string linkPath = Path.Combine(folder, name + ".lnk");
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            dynamic shell = Activator.CreateInstance(shellType);
            dynamic shortcut = shell.CreateShortcut(linkPath);
            shortcut.TargetPath = target;
            shortcut.WorkingDirectory = Path.GetDirectoryName(target);
            shortcut.Save();
        }
    }
}
