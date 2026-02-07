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

namespace PiBotControlCenter
{
    public partial class App : Application {
        private static Mutex _mutex = null;

        [STAThread]
        public static void Main() {
            const string appName = "PiBotControlCenterSingleInstance";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew) {
                // If already running, notify and exit
                MessageBox.Show("PIBOT Control Center ya está en ejecución.", "PIBOT System");
                return;
            }

            App app = new App();
            
            // Splash Screen Logic
            app.Startup += (s, e) => {
                app.ShutdownMode = ShutdownMode.OnExplicitShutdown; // Prevent exit when splash closes explicitly
                
                SplashScreenWindow splash = new SplashScreenWindow();
                splash.Show();

                // 3-second timer for simulated loading
                System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                timer.Tick += (sender, args) => {
                    timer.Stop();
                    splash.Close();
                    
                    // Launch Main Window
                    PiBotCenterWpf mainWin = new PiBotCenterWpf();
                    app.MainWindow = mainWin;
                    mainWin.Show();
                    app.ShutdownMode = ShutdownMode.OnMainWindowClose; // Revert to normal shutdown mode
                };
                timer.Start();
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

            string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media", "Pantalla de bienvenida al programa.png");
            
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
        private Random _rnd = new Random();
        
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
            Grid header = new Grid() { Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)) }; // Soft Grey
            header.Effect = new DropShadowEffect() { Color = Colors.Black, Direction = 270, ShadowDepth = 2, Opacity = 0.1 };
            
            // Define 3 columns preventing overlap: Left (Title), Center (Face), Right (Controls)
            header.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) }); // Center space for Face
            header.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

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
            titleBadge.Child = new TextBlock() { Text = "PIBOT", FontWeight = FontWeights.Bold, Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center, FontSize = 14, FontFamily = new FontFamily("Segoe UI Black") };
            header.Children.Add(titleBadge);

            // BMO Face (Center)
            Canvas bmoFace = CreateBmoFace(100, 60);
            Grid.SetColumn(bmoFace, 1);
            header.Children.Add(bmoFace);
            
            // Window Controls (Right)
            StackPanel winControls = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 15, 0) };
            winControls.Children.Add(CreateControlBtn("—", (s, e) => this.WindowState = WindowState.Minimized));
            winControls.Children.Add(CreateControlBtn("✕", (s, e) => this.Close()));
            Grid.SetColumn(winControls, 2);
            header.Children.Add(winControls);

            header.MouseDown += (s, e) => { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); };

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
            sidebar.Children.Add(new TextBlock() { Text = "PIBOT COMMAND", FontWeight = FontWeights.Bold, Margin = new Thickness(15, 20, 15, 5), Opacity = 0.5, FontSize = 10 });
            
            Button btnNew = new Button() { Content = "LAUNCH PIBOT", Height = 45, Margin = new Thickness(15), Background = accentColor, Foreground = Brushes.White, FontWeight = FontWeights.Bold };
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

            mainGrid.Children.Add(header);
            mainGrid.Children.Add(globalProgress);
            mainGrid.Children.Add(content); Grid.SetRow(content, 2);
            mainGrid.Children.Add(consoleLog); Grid.SetRow(consoleLog, 3);

            this.Content = mainGrid;

            // Load Naming Data
            LoadNamingData();

            Log("PIBOT Pro Beta System Online.");
            RefreshBots();
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
            if (dataNames.Count == 0) return "pibot-legacy-" + DateTime.Now.ToString("ss");

            string n = dataNames[_rnd.Next(dataNames.Count)].Trim();
            string e = dataEntities.Count > 0 ? dataEntities[_rnd.Next(dataEntities.Count)].Trim() : "Void";
            string c = dataColors.Count > 0 ? dataColors[_rnd.Next(dataColors.Count)].Trim() : "White";

            // Format: pibot-name-entity-color (e.g., pibot-aaron-andromeda-mint)
            return String.Format("pibot-{0}-{1}-{2}", n, e, c).ToLower().Replace(" ", "");
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

        private void Log(string msg) {
            // Clean ANSI and Multipass specific codes for cleaner UI
            msg = Regex.Replace(msg, @"\x1B\[[^@-~]*[@-~]", "");
            msg = Regex.Replace(msg, @"\[[0-9A-Z]{2,}", ""); 
            msg = msg.Replace("[2K[0A[0E", "").Replace("[0A[0E", "").Trim();
            
            if (string.IsNullOrEmpty(msg)) return;
            if (msg.Contains("retrieving image") || msg.Contains("Checking for")) return;
            
            this.Dispatcher.Invoke(() => {
                consoleLog.AppendText(DateTime.Now.ToString("[HH:mm:ss] ") + msg + "\n");
                consoleLog.ScrollToEnd();
            });
        }

        private void RefreshBots() {
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
                if (MessageBox.Show("Seguro que quieres eliminar a " + name + "?", "TERMINATE PIBOT", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
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
            // Advanced Name Generation
            string name = GenerateProName();
            
            Log("✨ Initiating New PIBOT: " + name);
            Log("⏳ Requesting resources (2CPU, 1.5GB RAM, LTS Image)...");
            globalProgress.IsIndeterminate = true;
            
            ThreadPool.QueueUserWorkItem(s => {
                try {
                    // Robust Launch Command: Explicit LTS, Disk Limit, Cloud-Init
                    string cloudInitPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "cloud-init.yaml");
                    string cmd = "launch lts --name " + name + " --cpus 2 --memory 1536M --disk 10G";
                    
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
                            if (p.ExitCode == 0) Log("✅ " + name + " system ready!");
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
