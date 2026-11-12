using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MotionHighlighter.Streaming
{
    public class ConfiguredCamera
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = "My Cameras";
        public string Url { get; set; } = string.Empty;
        public StreamFormat Format { get; set; } = StreamFormat.Unknown;
        public AuthType AuthType { get; set; } = AuthType.None;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string BearerToken { get; set; } = string.Empty;
        public Dictionary<string, string> CustomHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> QueryParameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public string UserAgent { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        public override string ToString()
        {
            string catIcon = Category switch
            {
                "My Cameras" => "📹",
                "Camera Portals" => "📷",
                "Public Test Streams" => "🎬",
                _ => "🎥"
            };
            return $"{catIcon} [{Category}] {Name} ({Url})";
        }
    }

    public class StreamConfigFile
    {
        public string Description { get; set; } = "Motion Highlighter Camera & Stream Configuration";
        public List<ConfiguredCamera> Cameras { get; set; } = new();
    }

    public class StreamHistoryItem
    {
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public StreamFormat Format { get; set; } = StreamFormat.Unknown;
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Title) && Title != Url)
            {
                return $"🕒 [{Format}] {Title} ({Url})";
            }
            return $"🕒 [{Format}] {Url}";
        }
    }

    public static class StreamHistoryManager
    {
        private static readonly string HistoryDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MotionHighlighter");

        private static readonly string HistoryFilePath = Path.Combine(HistoryDir, "stream_history.json");
        private static readonly string LocalConfigPath = Path.Combine(HistoryDir, "streams.config.json");
        private static readonly string BaseDirConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "streams.config.json");

        private static readonly object FileLock = new();

        public static string GetConfigFilePath()
        {
            lock (FileLock)
            {
                if (File.Exists(BaseDirConfigPath))
                {
                    return BaseDirConfigPath;
                }

                if (File.Exists(LocalConfigPath))
                {
                    return LocalConfigPath;
                }

                // If not created yet, create in LocalAppData with default template
                try
                {
                    Directory.CreateDirectory(HistoryDir);
                    StreamConfigFile initialConfig = CreateDefaultConfigFile();
                    JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    string json = JsonSerializer.Serialize(initialConfig, options);
                    File.WriteAllText(LocalConfigPath, json);
                }
                catch { }

                return LocalConfigPath;
            }
        }

        public static List<ConfiguredCamera> LoadConfiguredCameras()
        {
            lock (FileLock)
            {
                string configPath = GetConfigFilePath();
                try
                {
                    if (File.Exists(configPath))
                    {
                        string json = File.ReadAllText(configPath);
                        JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        StreamConfigFile? config = JsonSerializer.Deserialize<StreamConfigFile>(json, options);
                        if (config?.Cameras != null && config.Cameras.Count > 0)
                        {
                            return config.Cameras;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[StreamHistoryManager] Error reading {configPath}: {ex.Message}");
                }

                // Fallback to defaults
                return CreateDefaultConfigFile().Cameras;
            }
        }

        public static void OpenConfigFileInEditor()
        {
            string configPath = GetConfigFilePath();
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = configPath,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch
            {
                // Fallback to notepad
                try
                {
                    Process.Start("notepad.exe", configPath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[StreamHistoryManager] Could not open editor: {ex.Message}");
                }
            }
        }

        public static IReadOnlyList<(string Category, string Name, string Url, StreamFormat Format)> BuiltInPresets
        {
            get
            {
                List<ConfiguredCamera> cameras = LoadConfiguredCameras();
                return cameras.Select(c => (c.Category, c.Name, c.Url, c.Format)).ToList();
            }
        }

        private static StreamConfigFile CreateDefaultConfigFile()
        {
            return new StreamConfigFile
            {
                Description = "Motion Highlighter Camera & Stream Configuration. Users can add custom cameras, portals, RTSP links, and credentials here.",
                Cameras = new List<ConfiguredCamera>
                {
                    new ConfiguredCamera
                    {
                        Name = "Sample RTSP IP Camera (Basic Auth Example)",
                        Category = "My Cameras",
                        Url = "rtsp://192.168.1.100:554/stream1",
                        Format = StreamFormat.Rtsp,
                        AuthType = AuthType.BasicAuth,
                        Username = "admin",
                        Password = "password123",
                        Notes = "Example showing how to configure credentials for an RTSP camera stream."
                    },
                    new ConfiguredCamera
                    {
                        Name = "Sample MJPEG IP Camera (Custom Headers Example)",
                        Category = "My Cameras",
                        Url = "http://192.168.1.100:8080/video.mjpg",
                        Format = StreamFormat.Mjpeg,
                        AuthType = AuthType.CustomHeaders,
                        CustomHeaders = new Dictionary<string, string>
                        {
                            ["Authorization"] = "Bearer sample_token_here",
                            ["Cookie"] = "session_id=your_cookie_here"
                        },
                        Notes = "Example showing custom headers or cookies for protected HTTP camera streams."
                    },
                    new ConfiguredCamera
                    {
                        Name = "Google Home Live Camera",
                        Category = "Camera Portals",
                        Url = "https://home.google.com",
                        Format = StreamFormat.WebLiveFeed,
                        AuthType = AuthType.None,
                        Notes = "Use 'Sign In & Capture...' to log in with your Google account. You can also replace this URL with your direct camera page URL."
                    },
                    new ConfiguredCamera
                    {
                        Name = "Google Nest Web Portal",
                        Category = "Camera Portals",
                        Url = "https://home.nest.com",
                        Format = StreamFormat.WebLiveFeed,
                        AuthType = AuthType.None,
                        Notes = "Nest camera live web view."
                    },
                    new ConfiguredCamera
                    {
                        Name = "Ring Protect Live View",
                        Category = "Camera Portals",
                        Url = "https://ring.com/users/sign_in",
                        Format = StreamFormat.WebLiveFeed,
                        AuthType = AuthType.None,
                        Notes = "Ring dashboard login."
                    },
                    new ConfiguredCamera
                    {
                        Name = "Wyze Web View Live",
                        Category = "Camera Portals",
                        Url = "https://view.wyze.com/live",
                        Format = StreamFormat.WebLiveFeed,
                        AuthType = AuthType.None,
                        Notes = "Wyze live camera feed."
                    },
                    new ConfiguredCamera
                    {
                        Name = "Arlo Web Portal",
                        Category = "Camera Portals",
                        Url = "https://my.arlo.com",
                        Format = StreamFormat.WebLiveFeed,
                        AuthType = AuthType.None,
                        Notes = "Arlo camera portal."
                    },
                    new ConfiguredCamera
                    {
                        Name = "Eufy Security Portal",
                        Category = "Camera Portals",
                        Url = "https://mysecurity.eufylife.com",
                        Format = StreamFormat.WebLiveFeed,
                        AuthType = AuthType.None
                    },
                    new ConfiguredCamera
                    {
                        Name = "UniFi Protect",
                        Category = "Camera Portals",
                        Url = "https://unifi.ui.com",
                        Format = StreamFormat.WebLiveFeed,
                        AuthType = AuthType.None
                    },
                    new ConfiguredCamera
                    {
                        Name = "Big Buck Bunny (DASH Stream - Verified 200 OK)",
                        Category = "Public Test Streams",
                        Url = "https://dash.akamaized.net/akamai/bbb_30fps/bbb_30fps.mpd",
                        Format = StreamFormat.DashMpd,
                        AuthType = AuthType.None
                    },
                    new ConfiguredCamera
                    {
                        Name = "Big Buck Bunny (HLS Stream - Verified 200 OK)",
                        Category = "Public Test Streams",
                        Url = "https://test-streams.mux.dev/x36xhzz/x36xhzz.m3u8",
                        Format = StreamFormat.HlsM3u8,
                        AuthType = AuthType.None
                    },
                    new ConfiguredCamera
                    {
                        Name = "Tears of Steel (HLS Stream - Verified 200 OK)",
                        Category = "Public Test Streams",
                        Url = "https://demo.unified-streaming.com/k8s/features/stable/video/tears-of-steel/tears-of-steel.ism/.m3u8",
                        Format = StreamFormat.HlsM3u8,
                        AuthType = AuthType.None
                    }
                }
            };
        }

        public static List<StreamHistoryItem> LoadHistory()
        {
            lock (FileLock)
            {
                try
                {
                    if (!File.Exists(HistoryFilePath))
                    {
                        return new List<StreamHistoryItem>();
                    }

                    string json = File.ReadAllText(HistoryFilePath);
                    List<StreamHistoryItem>? items = JsonSerializer.Deserialize<List<StreamHistoryItem>>(json);
                    return items?.OrderByDescending(i => i.LastUsed).ToList() ?? new List<StreamHistoryItem>();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[StreamHistoryManager] Error loading history: {ex.Message}");
                    return new List<StreamHistoryItem>();
                }
            }
        }

        public static void SaveHistoryItem(string url, StreamFormat format, string? title = null)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            lock (FileLock)
            {
                try
                {
                    Directory.CreateDirectory(HistoryDir);

                    List<StreamHistoryItem> history = LoadHistory();
                    StreamHistoryItem? existing = history.FirstOrDefault(h => string.Equals(h.Url.Trim(), url.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        existing.LastUsed = DateTime.UtcNow;
                        if (!string.IsNullOrWhiteSpace(title)) existing.Title = title;
                        if (format != StreamFormat.Unknown) existing.Format = format;
                    }
                    else
                    {
                        history.Insert(0, new StreamHistoryItem
                        {
                            Url = url.Trim(),
                            Format = format,
                            Title = !string.IsNullOrWhiteSpace(title) ? title : url.Trim(),
                            LastUsed = DateTime.UtcNow
                        });
                    }

                    // Keep at most 30 recent items
                    if (history.Count > 30)
                    {
                        history = history.Take(30).ToList();
                    }

                    JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(history, options);
                    File.WriteAllText(HistoryFilePath, json);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[StreamHistoryManager] Error saving history: {ex.Message}");
                }
            }
        }
    }
}
