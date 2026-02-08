
using System;
using System.Net;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Web.Script.Serialization; // Requires System.Web.Extensions

namespace PiBotWebBackend {
    
    class Program {
        static string WEB_ROOT = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web");
        static string MULTIPASS_PATH = @"C:\Program Files\Multipass\bin\multipass.exe";
        static PerformanceCounter _ramCounter;

        static void Main(string[] args) {
            Console.WriteLine("🤖 PIBOT Web Core Initiating...");
            
            if (!Directory.Exists(WEB_ROOT)) {
                Console.WriteLine("❌ ERROR: 'web' folder not found!");
                return;
            }

            try {
                _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            } catch { Console.WriteLine("⚠️ Active RAM monitoring disabled."); }

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();
            
            Console.WriteLine("✅ Server Listening at http://localhost:8080/");
            Console.WriteLine("🌍 Open your browser to access the Control Center.");

            // Open Browser Automatically
            Process.Start("http://localhost:8080/");

            while (true) {
                try {
                    HttpListenerContext context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem(o => HandleRequest(context));
                } catch (Exception ex) { Console.WriteLine("Request Error: " + ex.Message); }
            }
        }

        static void HandleRequest(HttpListenerContext context) {
            HttpListenerRequest req = context.Request;
            HttpListenerResponse res = context.Response;
            
            // CORS (Allow any origin for dev)
            res.AddHeader("Access-Control-Allow-Origin", "*");
            
            string path = req.Url.AbsolutePath;
            string method = req.HttpMethod;

            Console.WriteLine("[" + DateTime.Now.ToShortTimeString() + "] " + method + " " + path);

            if (path.StartsWith("/api/")) {
                HandleApi(path, req, res);
            } else {
                ServeStaticFile(path, res);
            }
        }

        static void HandleApi(string path, HttpListenerRequest req, HttpListenerResponse res) {
            try {
                string jsonResponse = "{}";
                
                if (path == "/api/status") {
                    var status = new {
                        ramFree = _ramCounter != null ? _ramCounter.NextValue() : 0,
                        bots = GetBots()
                    };
                    jsonResponse = new JavaScriptSerializer().Serialize(status);
                
                } else if (path == "/api/launch" && req.HttpMethod == "POST") {
                    RunMultipass("launch lts --name pibot-" + DateTime.Now.ToString("HHmmss") + " --memory 1G --disk 10G --cloud-init Data/cloud-init.yaml");
                    jsonResponse = "{\"msg\": \"Launch Initiated\"}";
                
                } else if (path == "/api/start" && req.HttpMethod == "POST") {
                    string name = req.QueryString["name"];
                    if (!string.IsNullOrEmpty(name)) RunMultipass("start " + name);
                    jsonResponse = "{\"msg\": \"Starting " + name + "\"}";
                
                } else if (path == "/api/stop" && req.HttpMethod == "POST") {
                    string name = req.QueryString["name"];
                    if (!string.IsNullOrEmpty(name)) RunMultipass("stop " + name);
                    jsonResponse = "{\"msg\": \"Stopping " + name + "\"}";
                }

                byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);
                res.ContentType = "application/json";
                res.ContentLength64 = buffer.Length;
                res.OutputStream.Write(buffer, 0, buffer.Length);
                res.Close();
            } catch (Exception ex) {
                res.StatusCode = 500;
                res.Close();
                Console.WriteLine("API Error: " + ex.Message);
            }
        }

        static void ServeStaticFile(string path, HttpListenerResponse res) {
            if (path == "/") path = "/index.html";
            string filePath = Path.Combine(WEB_ROOT, path.TrimStart('/'));
            
            if (File.Exists(filePath)) {
                byte[] buffer = File.ReadAllBytes(filePath);
                res.ContentLength64 = buffer.Length;
                
                if (path.EndsWith(".html")) res.ContentType = "text/html";
                else if (path.EndsWith(".css")) res.ContentType = "text/css";
                else if (path.EndsWith(".js")) res.ContentType = "application/javascript";
                else if (path.EndsWith(".png")) res.ContentType = "image/png";
                else if (path.EndsWith(".mp4")) res.ContentType = "video/mp4";
                
                res.OutputStream.Write(buffer, 0, buffer.Length);
            } else {
                res.StatusCode = 404;
            }
            res.Close();
        }

        // --- Core Logic Duplicated from WPF App ---

        static List<object> GetBots() {
            var list = new List<object>();
            try {
                Process p = new Process();
                p.StartInfo.FileName = MULTIPASS_PATH;
                p.StartInfo.Arguments = "list --format csv";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                string[] lines = output.Split('\n');
                foreach (var line in lines) {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Name")) continue;
                    var parts = line.Split(',');
                    if (parts.Length >= 3 && parts[0].Contains("pibot")) {
                        string bName = parts[0].Trim();
                        string state = parts[1].Trim();
                        list.Add(new {
                            name = bName,
                            status = state,
                            ip = parts[2].Trim(),
                            ram = "1G",
                            uptime = state == "Running" ? GetUptime(bName) : "0m"
                        });
                    }
                }
            } catch { }
            return list;
        }

        static string GetUptime(string name) {
            try {
                Process p = new Process();
                p.StartInfo.FileName = MULTIPASS_PATH;
                p.StartInfo.Arguments = "exec " + name + " -- uptime -p";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                string output = p.StandardOutput.ReadToEnd().Trim();
                p.WaitForExit();
                return output.Replace("up ", "").Replace(" hours", "h").Replace(" hour", "h").Replace(" minutes", "m").Replace(" minute", "m").Replace(",", "");
            } catch { return "0m"; }
        }

        static void RunMultipass(string args) {
            // Run in background thread so API doesn't hang
            ThreadPool.QueueUserWorkItem(o => {
                Console.WriteLine("⚡ Running: multipass " + args);
                // Adjust paths for cloud-init if needed
                if (args.Contains("Data/cloud-init.yaml")) {
                     args = args.Replace("Data/cloud-init.yaml", "\"" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "cloud-init.yaml") + "\"");
                }
                
                Process p = new Process();
                p.StartInfo.FileName = MULTIPASS_PATH;
                p.StartInfo.Arguments = args;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                p.WaitForExit();
                Console.WriteLine("✅ Command Finished: " + args);
            });
        }
    }
}
