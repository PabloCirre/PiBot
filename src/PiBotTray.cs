using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Globalization;

namespace PiBotManager
{
    public class PiBotTray : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private string multipassPath = @"C:\Program Files\Multipass\bin\multipass.exe";
        private string vmName = "pibot-env";
        private string lang = "en";
        private Dictionary<string, string> t;

        [STAThread]
        public static void Main()
        {
            Application.Run(new PiBotTray());
        }

        public PiBotTray()
        {
            DetectLanguage();
            InitializeTranslations();
            InitializeComponent();
            CheckStatus();
        }

        private void DetectLanguage()
        {
            lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower();
            if (lang != "es") lang = "en";
        }

        private void InitializeTranslations()
        {
            t = new Dictionary<string, string>();
            if (lang == "es") {
                t["tray_text"] = "PIBOT Pro Control";
                t["header"] = "🔹 PIBOT Pro Beta";
                t["loading"] = "Cargando...";
                t["finding_ip"] = "Buscando IP...";
                t["view"] = "👁️ Ver PIBOT (Escritorio)";
                t["start"] = "🔥 Encender PIBOT";
                t["stop"] = "❄️ Apagar PIBOT";
                t["help"] = "📖 Ayuda y Manual PIBOT";
                t["exit"] = "🚪 Cerrar Administrador";
                t["ip_label"] = "IP: {0}";
                t["status_label"] = "Estado: {0}";
                t["balloon_title"] = "PIBOT Pro Beta";
                t["balloon_msg"] = "Control PIBOT preparado. Clic derecho en BMO.";
            } else {
                t["tray_text"] = "PIBOT Pro Control";
                t["header"] = "🔹 PIBOT Pro Beta";
                t["loading"] = "Loading...";
                t["finding_ip"] = "Finding IP...";
                t["view"] = "👁️ View PIBOT (Desktop)";
                t["start"] = "🔥 Start PIBOT";
                t["stop"] = "❄️ Stop PIBOT";
                t["help"] = "📖 PIBOT Help & Manual";
                t["exit"] = "🚪 Exit Manager";
                t["ip_label"] = "IP: {0}";
                t["status_label"] = "Status: {0}";
                t["balloon_title"] = "PIBOT Pro Beta";
                t["balloon_msg"] = "PIBOT Control ready. Right-click BMO.";
            }
        }

        private void InitializeComponent()
        {
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add(t["header"]).Enabled = false;
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add(t["view"], OnView);
            trayMenu.MenuItems.Add(t["start"], OnStart);
            trayMenu.MenuItems.Add(t["stop"], OnStop);
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add(t["help"], (s, e) => Process.Start("https://github.com/PIBOT"));
            trayMenu.MenuItems.Add(t["exit"], OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = t["tray_text"];
            trayIcon.Icon = CreateBmoIcon();
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;

            trayIcon.BalloonTipTitle = t["balloon_title"];
            trayIcon.BalloonTipText = t["balloon_msg"];
            trayIcon.ShowBalloonTip(3000);
        }

        private Icon CreateBmoIcon()
        {
            Bitmap bmp = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(bmp)) {
                g.Clear(Color.FromArgb(181, 226, 213));
                g.FillEllipse(Brushes.Black, 6, 10, 4, 4);
                g.FillEllipse(Brushes.Black, 22, 10, 4, 4);
                g.DrawArc(new Pen(Color.Black, 2), 8, 15, 16, 8, 0, 180);
            }
            return Icon.FromHandle(bmp.GetHicon());
        }

        private void OnView(object sender, EventArgs e)
        {
            string ip = GetIP();
            if (ip != "") Process.Start(string.Format("http://{0}:6080", ip));
        }

        private void OnStart(object sender, EventArgs e) { RunMultipass("start"); }
        private void OnStop(object sender, EventArgs e) { RunMultipass("stop"); }

        private void RunMultipass(string cmd)
        {
            ThreadPool.QueueUserWorkItem(s => {
                ProcessStartInfo psi = new ProcessStartInfo(multipassPath, cmd + " " + vmName);
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(psi).WaitForExit();
                CheckStatus();
            });
        }

        private string GetIP()
        {
            try {
                ProcessStartInfo psi = new ProcessStartInfo(multipassPath, "info " + vmName + " --format csv");
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                using (Process p = Process.Start(psi)) {
                    string output = p.StandardOutput.ReadToEnd();
                    if (output.Contains(",")) return output.Split('\n')[1].Split(',')[2];
                }
            } catch { }
            return "";
        }

        private void CheckStatus()
        {
            string ip = GetIP();
            trayIcon.Text = string.Format("{0}\n{1}", t["tray_text"], ip == "" ? "Offline" : ip);
        }

        private void OnExit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }
    }
}
