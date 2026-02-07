using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web.Script.Serialization; // Needs System.Web.Extensions
using System.Drawing; // For Icon
using System.Windows.Forms; // For NotifyIcon (need to add reference)
using Application = System.Windows.Application; // Resolve ambiguity
using MessageBox = System.Windows.MessageBox;
using Button = System.Windows.Controls.Button;
using Panel = System.Windows.Controls.Panel;
using Label = System.Windows.Controls.Label;
using TextBox = System.Windows.Controls.TextBox;
using CheckBox = System.Windows.Controls.CheckBox;
using ProgressBar = System.Windows.Controls.ProgressBar;
using Orientation = System.Windows.Controls.Orientation;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using VerticalAlignment = System.Windows.VerticalAlignment;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using Brush = System.Windows.Media.Brush; // Resolve ambiguity
using Color = System.Windows.Media.Color;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using Image = System.Windows.Controls.Image;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace PiBotControlCenter
{
    public class BotStatus {
        public string name { get; set; }
        public string status { get; set; }
        public string ip { get; set; }
        public int progress { get; set; }
    }

    public class SystemStatus {
        public float ramFree { get; set; }
        public List<BotStatus> bots { get; set; }
    }

    public partial class App : Application {
        private static Mutex _mutex = null;

        [STAThread]
        public static void Main() {
            const string appName = "PiBotControlCenterSingleInstance";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew) {
                // If already running, notify and exit
                MessageBox.Show("PIBOT Control Center is already running.", "PIBOT System");
                return;
            }

            App app = new App();
            
            // Splash Screen Logic
            // Headless Server Logic
            app.Startup += (s, e) => {
                app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                
                // Initialize Hidden Server
                PiBotCenterWpf server = new PiBotCenterWpf();
                // We do NOT show the window: server.Show();
                
                // Setup System Tray Icon
                NotifyIcon trayIcon = new NotifyIcon();
                try {
                	// Try load icon from assets or use default application icon if available
                	string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "pibot_icon.ico");
                	if (File.Exists(iconPath)) trayIcon.Icon = new Icon(iconPath);
                	else trayIcon.Icon = SystemIcons.Application;
                } catch { trayIcon.Icon = SystemIcons.Application; }
                
                trayIcon.Visible = true;
                trayIcon.Text = "PIBOT Control Center (Active)";
                
                // Context Menu
                ContextMenuStrip menu = new ContextMenuStrip();
                menu.Items.Add("Open Web Interface", null, (sender, args) => server.OpenBrowser());
                menu.Items.Add("-");
                menu.Items.Add("Exit PIBOT", null, (sender, args) => {
                    trayIcon.Visible = false;
                    Environment.Exit(0);
                });
                trayIcon.ContextMenuStrip = menu;
                
                trayIcon.DoubleClick += (sender, args) => server.OpenBrowser();

                // Initial Tasks
                server.RefreshBots();
                // Server starts automatically in constructor
            };
            
            app.Run();
        }
    }

    // Splash Screen Implementation
    public class SplashScreenWindow : Window {
        public SplashScreenWindow() {
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.AllowsTransparency = true;
            this.Background = Brushes.Transparent;
            this.Width = 600;
            this.Height = 400;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Topmost = true;
            this.ShowInTaskbar = false;

            string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media", "pibot_splash.png");
            
            Border border = new Border() { 
                CornerRadius = new CornerRadius(20), 
                ClipToBounds = true,
                Effect = new DropShadowEffect() { BlurRadius = 20, Opacity = 0.5, ShadowDepth = 5 }
            };

            if (File.Exists(imagePath)) {
                Image img = new Image();
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(imagePath);
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                img.Source = bi;
                img.Stretch = Stretch.Uniform;
                border.Child = img;
            } else {
                Grid fallback = new Grid() { Background = new SolidColorBrush(Color.FromRgb(45, 156, 219)) };
                TextBlock txt = new TextBlock() { 
                    Text = "PIBOT SYSTEM LOADING...", 
                    HorizontalAlignment = HorizontalAlignment.Center, 
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 24, FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White
                };
                fallback.Children.Add(txt);
                border.Child = fallback;
            }

            this.Content = border;
        }
    }

    public class PiBotCenterWpf : Window
    {
        private StackPanel botList;
        private TextBox consoleLog;
        private ProgressBar globalProgress;
        private string multipassPath = @"C:\Program Files\Multipass\bin\multipass.exe";
        private List<string> dataNames = new List<string>();
        private List<string> dataEntities = new List<string>();
        private List<string> dataColors = new List<string>();
        private TextBlock _ramLabel;
        private PerformanceCounter _ramCounter;
        private Random _rnd = new Random();
        private HttpListener _listener;
        private int _webPort = 8080;
        private object _cacheLock = new object();
        private string _cachedJson = "{}";
        private List<string> _webLogs = new List<string>();
        private int _maxWebLogs = 100;
        private System.Windows.Threading.DispatcherTimer _pollTimer;
        
        private Brush bmoTeal = new SolidColorBrush(Color.FromRgb(181, 226, 213));
        private Brush bmoDark = new SolidColorBrush(Color.FromRgb(30, 33, 39));
        private Brush accentColor = new SolidColorBrush(Color.FromRgb(45, 156, 219));

        public PiBotCenterWpf()
        {
            this.Title = "PIBOT Pro Beta";
            this.Width = 850;
            this.Height = 850;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Background = new SolidColorBrush(Color.FromRgb(240, 242, 245));
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;

            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(80) }); // Header
            mainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(4) });  // Global Progress
            mainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) }); // Content
            mainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(150) }); // Console

            // 1. Header
            // 1. Header - Nintendo Switch-like "Navbar"
            Grid titleBar = new Grid() { Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)) }; // Soft Grey
            titleBar.Effect = new DropShadowEffect() { Color = Colors.Black, Direction = 270, ShadowDepth = 2, Opacity = 0.1 };
            
            // Title Badge (Left)
            Border titleBadge = new Border() { 
                Background = new SolidColorBrush(Color.FromRgb(231, 0, 18)), // Nintendo Red
                CornerRadius = new CornerRadius(15), 
                Padding = new Thickness(15, 5, 15, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0),
                Height = 35
            };
            TextBlock appTitle = new TextBlock() { Text = "PIBOT", FontWeight = FontWeights.Bold, Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center, FontSize = 14, FontFamily = new FontFamily("Segoe UI Black") };
            titleBadge.Child = appTitle;

            StackPanel titleStack = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center };
            titleStack.Children.Add(titleBadge);

            // BMO Face (Center)
            Canvas bmoFace = CreateBmoFace(100, 60);
            
            // Window Controls (Right)
            StackPanel windowControls = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 15, 0) };
            windowControls.Children.Add(CreateControlBtn("—", (s, e) => this.WindowState = WindowState.Minimized));
            windowControls.Children.Add(CreateControlBtn("✕", (s, e) => this.Close()));

            titleBar.MouseDown += (s, e) => { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); };

            // --- RAM Monitor ---
            _ramLabel = new TextBlock() { Text = "🧠 RAM: calculating...", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Gray, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 140, 0) };
            
            // Re-order children because Grid doesn't respect order like StackPanel
            Grid headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // Title
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) }); // Face + RAM
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // Controls

            Grid.SetColumn(titleStack, 0);
            Grid.SetColumn(bmoFace, 1);
            Grid.SetColumn(windowControls, 2);
            
            // We need to add the RAM label to the middle column too, aligned right
            StackPanel centerStack = new StackPanel() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right };
            centerStack.Children.Add(_ramLabel);
            
            headerGrid.Children.Add(titleStack);
            headerGrid.Children.Add(bmoFace);
            headerGrid.Children.Add(centerStack); // Add RAM label to the center column
            headerGrid.Children.Add(windowControls);
            
            titleBar.Children.Add(headerGrid); // Add the new headerGrid to the titleBar

            // Life Support: Check on Shutdown
            this.Closing += OnWindowClosing;

            // 2. Global Progress
            globalProgress = new ProgressBar() { Height = 4, Background = Brushes.Transparent, Foreground = accentColor, BorderThickness = new Thickness(0), Minimum = 0, Maximum = 100 };
            Grid.SetRow(globalProgress, 1);

            // 3. Content
            Grid content = new Grid();
            content.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            content.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(220) });

            ScrollViewer scroll = new ScrollViewer() { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            botList = new StackPanel() { Margin = new Thickness(20) };
            scroll.Content = botList;
            content.Children.Add(scroll);

            StackPanel sidebar = new StackPanel() { Background = Brushes.White };
            sidebar.Children.Add(new TextBlock() { Text = "PIBOT CORE", FontWeight = FontWeights.Bold, Margin = new Thickness(15, 20, 15, 5), Opacity = 0.5, FontSize = 10 });
            
            Button btnNew = new Button() { Content = "GENERATE PIBOT", Height = 45, Margin = new Thickness(15), Background = accentColor, Foreground = Brushes.White, FontWeight = FontWeights.Bold };
            btnNew.Template = CreateBtnTemplate((SolidColorBrush)accentColor, new SolidColorBrush(Color.FromRgb(35, 126, 189)));
            btnNew.Click += OnNewBot;
            sidebar.Children.Add(btnNew);

            Grid.SetColumn(sidebar, 1);
            content.Children.Add(sidebar);

            // 4. Console
            consoleLog = new TextBox() { 
                Background = bmoDark, Foreground = Brushes.LimeGreen, BorderThickness = new Thickness(0, 1, 0, 0), BorderBrush = Brushes.Black,
                FontFamily = new FontFamily("Consolas"), FontSize = 11, IsReadOnly = true, Padding = new Thickness(10), VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            mainGrid.Children.Add(titleBar); Grid.SetRow(titleBar, 0);
            mainGrid.Children.Add(globalProgress);
            mainGrid.Children.Add(content); Grid.SetRow(content, 2);
            mainGrid.Children.Add(consoleLog); Grid.SetRow(consoleLog, 3);

            this.Content = mainGrid;

            // Load Naming Data
            LoadNamingData();

            // Initialize RAM Counter
            try {
                _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            } catch (Exception ex) {
                Log("⚠️ Error initializing RAM counter: " + ex.Message);
                _ramCounter = null;
            }

            Log("PIBOT Pro Beta System Online (Headless).");
            RefreshBots();
            
            // Start Web Server
            StartWebServer();

            // Background Polling for API Speed
            _pollTimer = new System.Windows.Threading.DispatcherTimer();
            _pollTimer.Interval = TimeSpan.FromSeconds(3);
            _pollTimer.Tick += (s, e) => UpdateStatusCache();
            _pollTimer.Start();
            UpdateStatusCache(); // Initial run
        }

        private void UpdateStatusCache() {
            ThreadPool.QueueUserWorkItem(o => {
                try {
                    var botList = new List<object>();
                    Process p = new Process();
                    p.StartInfo.FileName = multipassPath;
                    p.StartInfo.Arguments = "list --format csv";
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    
                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines) {
                        if (line.StartsWith("Name") || !line.Contains("pibot")) continue;
                        
                        // Robust CSV split (handle quotes)
                        var parts = line.Replace("\"", "").Split(',');
                        if (parts.Length >= 3) {
                            string bName = parts[0].Trim();
                            string state = parts[1].Trim();
                            string bIp = parts[2].Trim();
                            int prog = 0;

                            if (state == "Running") {
                                prog = EstimateProgress(bName);
                            } else if (state == "Starting") {
                                prog = 10;
                            } else if (state == "Stopped") {
                                prog = 0;
                            } else {
                                prog = 5; // Unknown/Transition
                            }

                            botList.Add(new { 
                                name = bName, 
                                status = state, 
                                ip = bIp, 
                                progress = prog 
                            });
                        }
                    }

                    float currentRam = 0;
                    if (_ramCounter != null) {
                        try { currentRam = _ramCounter.NextValue(); } catch { }
                    }

                    var sysStatus = new { 
                        ramFree = (int)currentRam, 
                        bots = botList,
                        logs = GetLogs()
                    };
                    
                    string json = new JavaScriptSerializer().Serialize(sysStatus);
                    // Ensure lowercase keys for JS stability (manual lowercase if needed, but Serializer usually keeps property names)
                    // We'll use anonymous objects which usually work well.

                    lock (_cacheLock) {
                        _cachedJson = json;
                    }
                } catch (Exception ex) { 
                    Log("Cache Update Error: " + ex.Message);
                }
            });
        }

        private List<string> GetLogs() {
            lock(_webLogs) {
                var copy = new List<string>(_webLogs);
                _webLogs.Clear(); // Consumed by the web UI for incrementals
                return copy;
            }
        }

        private int EstimateProgress(string name) {
            try {
                Process p = new Process();
                p.StartInfo.FileName = multipassPath;
                // Faster check: try to see if websockify is listening on 6080 using 'ss' or 'netstat'
                p.StartInfo.Arguments = "exec " + name + " -- bash -c \"(ss -ltn | grep -q :6080 && echo 100) || (grep -c '...' /var/log/cloud-init-output.log || echo 0)\"";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                string output = p.StandardOutput.ReadToEnd().Trim();
                p.WaitForExit();

                if (output == "100") return 100;
                
                // If it's not 100, we count lines to estimate. 
                // But cloud-init log might be short. Let's just use a conservative estimate based on time or just lines.
                int lineCount = 0;
                int.TryParse(output, out lineCount);
                
                // GUI Install is heavy. 
                int pct = 15 + (lineCount * 80 / 4000); 
                return pct > 98 ? 98 : (pct < 15 ? 15 : pct);
            } catch { return 15; }
        }

        public void OpenBrowser() {
            string url = "http://localhost:" + _webPort + "/";
            Log("🌐 Opening Browser: " + url);
            try {
                // Try to find Edge or Chrome for --app mode
                string edge = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
                string chrome = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
                
                if (File.Exists(edge)) {
                    Process.Start(edge, "--app=" + url + " --window-size=1200,800");
                } else if (File.Exists(chrome)) {
                    Process.Start(chrome, "--app=" + url + " --window-size=1200,800");
                } else {
                    // Fallback to default browser
                    Process.Start(url);
                }
            } catch { Process.Start(url); }
        }

        // Removed duplicate OpenBrowser

        private void StartWebServer()
        {
            ThreadPool.QueueUserWorkItem(o => {
                _webPort = GetFreePort(8080);
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://localhost:" + _webPort + "/");
                try {
                    _listener.Start();
                    this.Dispatcher.Invoke(() => Log("🌍 Web Interface listening on port " + _webPort));
                    
                    // Use the improved browser launcher
                    this.Dispatcher.BeginInvoke(new Action(() => OpenBrowser()));
                    
                    while (_listener.IsListening) {
                        try {
                            var context = _listener.GetContext();
                            ThreadPool.QueueUserWorkItem(c => HandleWebRequest((HttpListenerContext)c), context);
                        } catch { }
                    }
                } catch (Exception ex) {
                    this.Dispatcher.Invoke(() => Log("❌ Web Server Error: " + ex.Message));
                }
            });
        }

        private int GetFreePort(int startPort) {
            int port = startPort;
            while (true) {
                try {
                    TcpListener tcp = new TcpListener(IPAddress.Loopback, port);
                    tcp.Start();
                    tcp.Stop();
                    return port;
                } catch { port++; }
            }
        }

        private void HandleWebRequest(HttpListenerContext context) {
            try {
                var req = context.Request;
                var res = context.Response;
                string path = req.Url.AbsolutePath;
                
                if (path != "/api/status") {
                    Log("📥 Request: " + req.HttpMethod + " " + path);
                }

                // CORS
                res.AddHeader("Access-Control-Allow-Origin", "*");

                if (path.StartsWith("/api/")) {
                    HandleApi(path, req, res);
                } else {
                    ServeStaticFile(path, res);
                }
            } catch (Exception ex) {
                this.Dispatcher.Invoke(() => Log("Web Error: " + ex.Message));
            }
        }

        private void HandleApi(string path, HttpListenerRequest req, HttpListenerResponse res) {
            string jsonResponse = "{}";
            if (path == "/api/status") {
                // Return Cached Status Instant
                lock (_cacheLock) {
                    jsonResponse = _cachedJson;
                }

            } else if (path == "/api/start" || path == "/api/stop" || path == "/api/purge") {
                string name = req.QueryString["name"];
                string action = path.Replace("/api/", "");
                if (!string.IsNullOrEmpty(name)) {
                    if (action == "purge") {
                        this.Dispatcher.Invoke(() => RunMultipass("delete --purge", name));
                    } else {
                        this.Dispatcher.Invoke(() => RunMultipass(action, name));
                    }
                    jsonResponse = "{\"msg\": \"Command Sent\"}";
                }
            } else if (path == "/api/launch") {
                string ram = req.QueryString["ram"] ?? "1024M";
                string disk = req.QueryString["disk"] ?? "10G";

                // If it's a POST request, try to read from the body
                if (req.HttpMethod == "POST" && req.HasEntityBody) {
                    using (var reader = new StreamReader(req.InputStream, req.ContentEncoding)) {
                        string requestBody = reader.ReadToEnd();
                        // Assuming JSON body like {"ram": "2048M", "disk": "20G"}
                        var json = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(requestBody);
                        if (json.ContainsKey("ram")) ram = json["ram"];
                        if (json.ContainsKey("disk")) disk = json["disk"];
                    }
                }

                this.Dispatcher.Invoke(() => LaunchCustomBot(ram, disk));
                jsonResponse = "{\"msg\": \"Launch Initiated: " + ram + " RAM / " + disk + " Disk\"}";
            }

            byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);
            res.ContentType = "application/json";
            res.ContentEncoding = Encoding.UTF8;
            res.ContentLength64 = buffer.Length;
            using (var s = res.OutputStream) {
                s.Write(buffer, 0, buffer.Length);
            }
            res.Close();
        }

        private void ServeStaticFile(string path, HttpListenerResponse res) {
            if (path == "/") path = "/index.html";
            string webRoot = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web");
            string filePath = System.IO.Path.Combine(webRoot, path.TrimStart('/'));

            if (File.Exists(filePath)) {
                byte[] buffer = File.ReadAllBytes(filePath);
                string ext = System.IO.Path.GetExtension(filePath).ToLower();
                if (ext == ".html") res.ContentType = "text/html";
                else if (ext == ".css") res.ContentType = "text/css";
                else if (ext == ".js") res.ContentType = "application/javascript";
                else if (ext == ".png") res.ContentType = "image/png";
                else if (ext == ".mp4") res.ContentType = "video/mp4";
                
                res.ContentLength64 = buffer.Length;
                using (var s = res.OutputStream) {
                    s.Write(buffer, 0, buffer.Length);
                }
            } else {
                res.StatusCode = 404;
            }
            res.Close();
        }

        private void LoadNamingData() {
            try {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string namesFile = System.IO.Path.Combine(baseDir, "Data", "pibot_names_expanded.csv");
                string entitiesFile = System.IO.Path.Combine(baseDir, "Data", "pibot_entities_expanded.csv");
                string colorsFile = System.IO.Path.Combine(baseDir, "Data", "pibot_colors_expanded.csv");

                if (File.Exists(namesFile)) dataNames.AddRange(File.ReadAllLines(namesFile));
                if (File.Exists(entitiesFile)) dataEntities.AddRange(File.ReadAllLines(entitiesFile));
                if (File.Exists(colorsFile)) {
                    foreach (var line in File.ReadAllLines(colorsFile)) {
                        var parts = line.Split(',');
                        if (parts.Length > 0) dataColors.Add(parts[0]);
                    }
                }
                
                // Remove headers if present
                if (dataNames.Count > 0 && dataNames[0].ToLower().Contains("name")) dataNames.RemoveAt(0);
                if (dataEntities.Count > 0 && dataEntities[0].ToLower().Contains("entity")) dataEntities.RemoveAt(0);
                if (dataColors.Count > 0 && dataColors[0].ToLower().Contains("color")) dataColors.RemoveAt(0);

            } catch (Exception ex) { Log("⚠️ Error loading name data: " + ex.Message); }
        }

        private string GenerateProName() {
            // Default fallback
            if (dataNames.Count == 0) return "Alpha Bravo";

            string n = dataNames[_rnd.Next(dataNames.Count)].Trim();
            string e = dataEntities.Count > 0 ? dataEntities[_rnd.Next(dataEntities.Count)].Trim() : "";
            
            // Format: "Name Entity" (e.g., "Victor Eventide")
            string finalName = (n + " " + e).Trim();
            // Capitalize First letters
            finalName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(finalName.ToLower());
            return finalName;
        }

        private Canvas CreateBmoFace(double w, double h) {
            Canvas c = new Canvas() { Width = w, Height = h, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Ellipse eyeL = new Ellipse() { Fill = Brushes.Black, Width = 10, Height = 10 };
            Canvas.SetLeft(eyeL, 25); Canvas.SetTop(eyeL, 15);
            Ellipse eyeR = new Ellipse() { Fill = Brushes.Black, Width = 10, Height = 10 };
            Canvas.SetLeft(eyeR, 65); Canvas.SetTop(eyeR, 15);
            System.Windows.Shapes.Path mouth = new System.Windows.Shapes.Path() { Stroke = Brushes.Black, StrokeThickness = 3, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round, Data = Geometry.Parse("M 35,35 Q 50,45 65,35") };
            c.Children.Add(eyeL); c.Children.Add(eyeR); c.Children.Add(mouth);
            return c;
        }

        private void LaunchCustomBot(string ram, string disk) {
            string friendlyName = GenerateProName();
            // Multipass needs lowercase and no spaces for the instance name
            string instanceName = friendlyName.ToLower().Replace(" ", "-");
            if (!instanceName.StartsWith("pibot-")) instanceName = "pibot-" + instanceName;

            Log("✨ Initiating New Birth: " + friendlyName);
            Log("⏳ Config: " + ram + " RAM / " + disk + " Disk");
            
            ThreadPool.QueueUserWorkItem(s => {
                try {
                    string cloudInitPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "cloud-init.yaml");
                    string cmd = "launch lts --name " + instanceName + " --cpus 2 --memory " + ram + " --disk " + disk;
                    if (File.Exists(cloudInitPath)) cmd += " --cloud-init \"" + cloudInitPath + "\"";

                    ProcessStartInfo psi = new ProcessStartInfo(multipassPath, cmd);
                    psi.CreateNoWindow = true; psi.WindowStyle = ProcessWindowStyle.Hidden;
                    psi.RedirectStandardOutput = true; psi.RedirectStandardError = true;
                    psi.UseShellExecute = false;

                    using (Process p = Process.Start(psi)) {
                        p.OutputDataReceived += (proc, outLine) => { if (!string.IsNullOrEmpty(outLine.Data)) Log(outLine.Data); };
                        p.ErrorDataReceived += (proc, errLine) => { if (!string.IsNullOrEmpty(errLine.Data)) Log("❌ MP Error: " + errLine.Data); };
                        p.BeginOutputReadLine(); p.BeginErrorReadLine();
                        p.WaitForExit(900000); // 15 min
                    }
                    this.Dispatcher.Invoke(() => RefreshBots());
                } catch (Exception ex) { Log("💥 CUSTOM LAUNCH ERROR: " + ex.Message); }
            });
        }

        public void Log(string msg) {
            // Clean ANSI and Multipass specific codes for cleaner UI
            msg = Regex.Replace(msg, @"\x1B\[[^@-~]*[@-~]", "");
            msg = Regex.Replace(msg, @"\[[0-9A-Z]{2,}", ""); 
            msg = msg.Replace("[2K[0A[0E", "").Replace("[0A[0E", "").Trim();
            
            if (string.IsNullOrEmpty(msg)) return;
            string timestamp = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            string formatted = "[" + timestamp + "] " + msg;
            
            // Log to Web Buffer
            lock(_webLogs) {
                _webLogs.Add(formatted);
                if (_webLogs.Count > _maxWebLogs) _webLogs.RemoveAt(0);
            }

            // Log to file for headless debugging
            try {
                File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pibot_system.log"), formatted + Environment.NewLine);
            } catch { }

            if (consoleLog == null) return;
            this.Dispatcher.Invoke(() => {
                consoleLog.AppendText(formatted + Environment.NewLine);
                consoleLog.ScrollToEnd();
            });
        }

        public void UpdateRamStats() {
            if (_ramCounter != null) {
                this.Dispatcher.Invoke(() => {
                    float ram = _ramCounter.NextValue();
                    int mb = (int)ram;
                    int freeGB = mb / 1024;
                    int maxBots = mb / 1100; // conservative estimate
                    
                    if (maxBots < 1) _ramLabel.Foreground = Brushes.Red;
                    else if (maxBots < 2) _ramLabel.Foreground = Brushes.Orange;
                    else _ramLabel.Foreground = new SolidColorBrush(Color.FromRgb(50, 215, 75));

                    _ramLabel.Text = string.Format("🧠 RAM: {0:N0}MB FREE", mb);
                });
            }
        }

        public void RefreshBots() {
            if (botList == null) return; // Ensure botList is initialized before trying to access it
            ThreadPool.QueueUserWorkItem(s => {
                try {
                    ProcessStartInfo psi = new ProcessStartInfo(multipassPath, "list --format csv");
                    psi.RedirectStandardOutput = true;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    using (Process p = Process.Start(psi)) {
                        string output = p.StandardOutput.ReadToEnd();
                        this.Dispatcher.Invoke(() => {
                            botList.Children.Clear();
                            string[] lines = output.Split('\n');
                            foreach (string line in lines) {
                                if (string.IsNullOrEmpty(line) || line.StartsWith("Name")) continue;
                                string[] parts = line.Split(',');
                                if (parts[0].Contains("pibot")) AddBotCard(parts[0], parts[1], parts[2]);
                            }
                        });
                    }
                } catch { }
            });
        }

        private void AddBotCard(string name, string status, string ip) {
            Border card = new Border() { 
                Background = Brushes.White, 
                CornerRadius = new CornerRadius(15), 
                Margin = new Thickness(0, 0, 0, 15), 
                Padding = new Thickness(15), 
                Effect = new DropShadowEffect() { BlurRadius = 10, ShadowDepth = 4, Opacity = 0.1, Color = Colors.Black } // Soft "floating" card
            };
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) }); // Widened for kill button

            grid.Children.Add(CreateBmoFace(30, 30));

            StackPanel info = new StackPanel() { Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            
            // Format Display Name: Remove "pibot-" prefix and hyphens for UI
            string displayName = name.Replace("pibot-", "").Replace("-", " ");
            displayName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(displayName);
            string humanStatus = status;
            string statusColor = "#555555";
            string lowerStatus = status.ToLower();

            if (lowerStatus == "running") { humanStatus = "🟢 Running (Active)"; statusColor = "#27AE60"; }
            else if (lowerStatus == "stopped") { humanStatus = "🔴 Stopped (Asleep)"; statusColor = "#E74C3C"; }
            else if (lowerStatus.Contains("start")) { humanStatus = "🟡 Starting (Waking Up)"; statusColor = "#F1C40F"; }
            else if (lowerStatus.Contains("stop")) { humanStatus = "🟠 Stopping (Sleeping)"; statusColor = "#E67E22"; }
            else if (lowerStatus == "suspended") { humanStatus = "⏸️ Suspended (Hibernating)"; statusColor = "#3498DB"; }
            else if (lowerStatus == "deleted") { humanStatus = "💀 Deleted (Dead)"; statusColor = "#7F8C8D"; }
            else { humanStatus = "❓ Unknown (" + status + ")"; statusColor = "#95A5A6"; }

            info.Children.Add(new TextBlock() { Text = displayName, FontWeight = FontWeights.Bold, FontSize = 16 });
            info.Children.Add(new TextBlock() { 
                Text = humanStatus + " • " + (ip == "" ? "No IP" : ip), 
                FontSize = 11, 
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(statusColor)),
                FontWeight = FontWeights.SemiBold
            });
            
            ProgressBar pbar = new ProgressBar() { Height = 3, Margin = new Thickness(0, 10, 0, 0), Minimum = 0, Maximum = 100, Background = Brushes.Transparent, BorderThickness = new Thickness(0), Foreground = accentColor };
            if (status.ToLower().Contains("starting") || status.ToLower().Contains("stopping")) pbar.IsIndeterminate = true;
            info.Children.Add(pbar);
            
            Grid.SetColumn(info, 1);
            grid.Children.Add(info);

            StackPanel btnArea = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            Button btnView = CreateActionBtn("VIEW", new SolidColorBrush(Color.FromRgb(0, 184, 187))); // Nintendo Teal/Blue
            btnView.Click += (s, e) => { 
                if (ip != "" && ip != "No IP") {
                   // Point directly to vnc.html with autoconnect
                   LaunchAppBrowser("http://" + ip + ":6080/vnc.html?autoconnect=true&resize=scale");
                }
            };
            Button btnToggle = CreateActionBtn(status.ToLower() == "running" ? "STOP" : "START", status.ToLower() == "running" ? new SolidColorBrush(Color.FromRgb(255, 69, 58)) : new SolidColorBrush(Color.FromRgb(50, 215, 75))); // Neon Red / Green
            btnToggle.Click += (s, e) => {
                pbar.IsIndeterminate = true;
                RunMultipass(status.ToLower() == "running" ? "stop" : "start", name);
            };
            
            // Kill Button (Skull)
            Button btnKill = new Button() { 
                Content = "💀", Width = 30, Height = 30, Margin = new Thickness(5),
                Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                Foreground = Brushes.Salmon, FontWeight = FontWeights.Bold, FontSize = 16, ToolTip = "Assassinate PIBOT"
            };
            btnKill.Click += (s, e) => {
                if (MessageBox.Show("Are you sure you want to terminate " + name + "?", "TERMINATE PIBOT", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                    RunMultipass("delete --purge", name);
                }
            };

            btnArea.Children.Add(btnView); btnArea.Children.Add(btnToggle); btnArea.Children.Add(btnKill);
            Grid.SetColumn(btnArea, 2);
            grid.Children.Add(btnArea);

            card.Child = grid;
            botList.Children.Add(card);
        }

        private Button CreateActionBtn(string t, Brush bg) {
            Button b = new Button() { Content = t, Width = 80, Height = 35, Margin = new Thickness(5), Background = bg, BorderThickness = new Thickness(0), FontWeight = FontWeights.Bold, Foreground = Brushes.White, FontSize = 12, FontFamily = new FontFamily("Segoe UI Black") };
            b.Template = CreateBtnTemplate((SolidColorBrush)bg, new SolidColorBrush(Color.FromArgb(200, ((SolidColorBrush)bg).Color.R, ((SolidColorBrush)bg).Color.G, ((SolidColorBrush)bg).Color.B)));
            return b;
        }

        private ControlTemplate CreateBtnTemplate(SolidColorBrush n, SolidColorBrush h) {
            // Pills/Chunky Buttons common in Nintendo UI
            var ct = new ControlTemplate(typeof(Button));
            var b = new FrameworkElementFactory(typeof(Border));
            b.Name = "b"; 
            b.SetValue(Border.CornerRadiusProperty, new CornerRadius(17)); // Pill Shape
            b.SetValue(Border.BackgroundProperty, n);
            // Add a subtle "physical" border
            b.SetValue(Border.BorderBrushProperty, new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)));
            b.SetValue(Border.BorderThicknessProperty, new Thickness(0, 0, 0, 3)); 

            var cp = new FrameworkElementFactory(typeof(ContentPresenter));
            cp.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center); cp.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            b.AppendChild(cp); ct.VisualTree = b;
            
            var tr = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            tr.Setters.Add(new Setter(Border.BackgroundProperty, h, "b"));
            // "Press" effect visual
            tr.Setters.Add(new Setter(Border.MarginProperty, new Thickness(0, 2, 0, 0), "b"));
            tr.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(0, 0, 0, 1), "b"));
            
            ct.Triggers.Add(tr); return ct;
        }

        private Button CreateControlBtn(string c, RoutedEventHandler h) {
            Button b = new Button() { Content = c, Width = 30, Height = 30, Background = Brushes.Transparent, BorderThickness = new Thickness(0), FontWeight = FontWeights.Bold };
            b.Click += h; return b;
        }

        private void LaunchAppBrowser(string url) {
            try {
                // Try to find Edge or Chrome to launch in --app mode (no address bar, feels like native app)
                string edgePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft\Edge\Application\msedge.exe");
                string chromePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Google\Chrome\Application\chrome.exe");
                
                string browserPath = File.Exists(edgePath) ? edgePath : (File.Exists(chromePath) ? chromePath : null);

                if (browserPath != null) {
                    Process.Start(new ProcessStartInfo(browserPath, "--app=\"" + url + "\" --window-size=1024,768 --force-renderer-accessibility") { UseShellExecute = true });
                } else {
                    // Fallback to default
                    Process.Start("explorer", url);
                }
            } catch (Exception ex) {
                Log("⚠️ Browser launch error: " + ex.Message);
                Process.Start("explorer", url);
            }
        }

        private void RunMultipass(string cmd, string name) {
            string displayText = cmd == "delete --purge" ? "Terminating" : cmd; 
            Log("PIBOT Command: " + displayText + " " + name);
            ThreadPool.QueueUserWorkItem(s => {
                try {
                    Process.Start(new ProcessStartInfo(multipassPath, cmd + " " + name) { CreateNoWindow=true, WindowStyle=ProcessWindowStyle.Hidden }).WaitForExit();
                } catch { }
                this.Dispatcher.Invoke(() => RefreshBots());
            });
        }

        private void OnNewBot(object sender, RoutedEventArgs e) {
            string friendlyName = GenerateProName();
            string instanceName = friendlyName.ToLower().Replace(" ", "-");
            if (!instanceName.StartsWith("pibot-")) instanceName = "pibot-" + instanceName;

            Log("✨ Initiating New Deployment: " + friendlyName);
            Log("⏳ Requesting resources (2CPU, 1024MB RAM, LTS Image)...");
            globalProgress.IsIndeterminate = true;
            
            ThreadPool.QueueUserWorkItem(s => {
                try {
                    // Robust Launch Command: Explicit LTS, Disk Limit, Cloud-Init
                    string cloudInitPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "cloud-init.yaml");
                    string cmd = "launch lts --name " + instanceName + " --cpus 2 --memory 1024M --disk 10G";
                    
                    if (File.Exists(cloudInitPath)) {
                        cmd += " --cloud-init \"" + cloudInitPath + "\"";
                        Log("📜 Applied Cloud-Init Configuration.");
                    } else {
                        Log("⚠️ GUI Config missing! Bot will be headless.");
                    }

                    ProcessStartInfo psi = new ProcessStartInfo(multipassPath, cmd);
                    psi.CreateNoWindow = true;
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true; // CRITICAL: Capture errors
                    psi.UseShellExecute = false;

                    using (Process p = Process.Start(psi)) {
                        // Async read of output/error to prevent deadlocks
                        p.OutputDataReceived += (proc, outLine) => { if (!string.IsNullOrEmpty(outLine.Data)) Log(outLine.Data); };
                        p.ErrorDataReceived += (proc, errLine) => { if (!string.IsNullOrEmpty(errLine.Data)) Log("❌ MP Error: " + errLine.Data); };

                        p.BeginOutputReadLine();
                        p.BeginErrorReadLine();

                        // Cloud-Init installing XFCE takes 5-8 minutes. Increase timeout significantly.
                        Log("⏳ Installing Linux OS & GUI... (This might take ~5-10 mins)");
                        bool finished = p.WaitForExit(900000); // 15 minute timeout security
                        if (!finished) {
                            Log("⚠️ Launch taking too long! It might still be installing in background.");
                            // Do NOT kill the process, let it finish. Just stop waiting on UI.
                        } else {
                            if (p.ExitCode == 0) Log("✅ " + friendlyName + " system ready!");
                            else Log("⚠️ Launch finished with Exit Code: " + p.ExitCode);
                        }
                    }
                    
                    this.Dispatcher.Invoke(() => { globalProgress.IsIndeterminate = false; RefreshBots(); });
                } catch (Exception ex) { 
                    Log("💥 CRITICAL LAUNCH ERROR: " + ex.Message);
                    this.Dispatcher.Invoke(() => globalProgress.IsIndeterminate = false); 
                }
            });
        }
        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Count running bots via a quick check or assume active if we have listed them recently
            bool hasBots = false;
            foreach (UIElement child in botList.Children) {
                // If we have cards, we likely have bots. A stronger check would be multipass list, but ui-check is faster for prompt.
                hasBots = true; 
                break;
            }

            if (hasBots) {
                var result = MessageBox.Show(
                    "⚠️ WARNING: Closing Control Center will put all active PiBots to SLEEP (Stop).\n\nAre you sure you want to proceed?", 
                    "Confirm Shutdown", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.No) {
                    e.Cancel = true;
                    return;
                }

                // If Yes, initiating shutdown sequence
                e.Cancel = true; // Temporary cancel to run async
                this.Hide(); // Hide window to show we are working
                
                ThreadPool.QueueUserWorkItem(s => {
                    try {
                        Process.Start(new ProcessStartInfo(multipassPath, "stop --all") { CreateNoWindow=true, WindowStyle=ProcessWindowStyle.Hidden }).WaitForExit();
                    } catch { }
                    this.Dispatcher.Invoke(() => Application.Current.Shutdown());
                });
            }
        }
    }
}
