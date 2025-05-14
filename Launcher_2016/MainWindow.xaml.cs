using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Launcher_2016 {

    public partial class MainWindow : Window {
        public string localhost_name = "MapleStory.exe";
        public string server_name = "瘋汁谷更新程式";
        public Uri website_url = new Uri("https://ms.fz-game.club/");
        public Uri discord_url = new Uri("https://discord.gg/KUnS3eG6AD");
        public bool force_admin = false;

        private readonly bool checkExe = true;
        public Boolean updateInProgress = false;

        public static string XmlURL { get; set; } = "https://ms.fz-game.club/update/downloads.xml";

        public List<string> downloadLinks_;


        [DllImport("kernel32.dll")]
        private static extern int ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In][Out] byte[] buffer, uint size, out IntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In][Out] byte[] buffer, uint size, out IntPtr lpNumberOfBytesWritten);


        public MainWindow() : this(OverlayStyle.WinForms) { }

        public MainWindow(OverlayStyle style) {
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
            if (!windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator) && force_admin) {
                MessageBox.Show("Please run as Administrator!", "Warning");
                Close();
            }

            InitializeComponent();

            IniFile reader = new IniFile("config.ini");
            string screen_width = reader.Read("width", "general");
            string screen_height = reader.Read("height", "general");

            ComboBox resCB = this.FindName("resComboBox") as ComboBox;
            if (screen_width == "1024" && screen_height == "768") {
                resCB.SelectedItem = resCB.Items[0]; // 1024*768
            } else if (screen_width == "1280" && screen_height == "1024") {
                resCB.SelectedItem = resCB.Items[1]; // 1280*1024
            } else if (screen_width == "1280" && screen_height == "720") {
                resCB.SelectedItem = resCB.Items[2]; // 1280*720
            } else if (screen_width == "1366" && screen_height == "768") {
                resCB.SelectedItem = resCB.Items[3]; // 1366*768
            } else if (screen_width == "1600" && screen_height == "900") {
                resCB.SelectedItem = resCB.Items[4]; // 1600*900
            } else if (screen_width == "1920" && screen_height == "1080") {
                resCB.SelectedItem = resCB.Items[5]; // 1920*1080
            }

            label5.Content = server_name;
            ws.NavigateUri = website_url;
            disc.NavigateUri = discord_url;
        }

        private void ResolutionComboBoxChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox resCB = this.FindName("resComboBox") as ComboBox;
            ComboBoxItem item = (ComboBoxItem)resCB.SelectedItem;
            string selectedResolution = item.Content.ToString(); // Get the text of the selected item
            string[] resolutionParts = selectedResolution.Split('*');
            if (resolutionParts.Length == 2) {
                string width = resolutionParts[0];
                string height = resolutionParts[1];
                IniFile reader = new IniFile("config.ini");
                reader.Write("width", width, "general");
                reader.Write("height", height, "general");
            }
        }


        public enum OverlayStyle {
            WPF,
            WinForms
        };

        private async Task<List<string>> CheckForUpdates() {
            string currentDirectory = Directory.GetCurrentDirectory();
            List<string> downloadLinks = new List<string>();
            XDocument doc = await Task.Run(() => XDocument.Load(XmlURL));

            var results = doc.Descendants("file").Select(x => new {
                link = x.Element("file_link").Value,
                size = x.Element("file_size").Value,
                path = x.Element("file_path").Value,
                name = x.Element("file_name").Value,
                hash = x.Element("file_hash").Value
            }).ToList();

            foreach (var file in results) {
                if (File.Exists(Path.Combine(currentDirectory + "\\" + file.path, file.name))) {
                    string wzHashOnComputer = GetFileHash(Path.Combine(currentDirectory + "\\" + file.path, file.name));

                    if (file.hash != wzHashOnComputer) {
                        downloadLinks.Add(file.path + "*" + file.name + "*" + file.hash + "*" + file.link + "*" + file.size);
                    }
                } else {
                    downloadLinks.Add(file.path + "*" + file.name + "*" + file.hash + "*" + file.link + "*" + file.size);
                }
            }

            // Check if the file is not in the database but in local directory.
            List<string> dbFiles = results.Select(x => Path.Combine(x.path, x.name)).ToList();
            IEnumerable<string> allFiles = Directory.EnumerateFiles("Data", "*", SearchOption.AllDirectories);
            foreach (string file in allFiles.Except(dbFiles, StringComparer.OrdinalIgnoreCase)) {
                try {
                    File.Delete(file);
                } catch (IOException) {
                    // handle exception here
                }
            }

            Console.WriteLine(downloadLinks);
            downloadLinks_ = downloadLinks;
            return downloadLinks;
        }

        private string GetFileHash(string filePath) {
            if (!File.Exists(filePath)) return string.Empty;

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan)) {
                byte[] hash = MD5.Create().ComputeHash(stream);
                return BitConverter.ToString(hash).ToLowerInvariant().Replace("-", string.Empty);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            this.DragMove();
        }

        private void Download_Load() {
            string currentDirectory = Directory.GetCurrentDirectory();
            string filePath = downloadLinks_[0].Split('*')[0];
            string fileName = downloadLinks_[0].Split('*')[1];
            string fileUrl = downloadLinks_[0].Split('*')[3];

            label6.Content = downloadLinks_.Count.ToString();
            label2.Content = filePath + fileName;


            Console.WriteLine("下載連結：" + fileUrl);

            if (File.Exists(Path.Combine(currentDirectory, fileName)))
                File.Delete(Path.Combine(currentDirectory, fileName));

            WebClient client = new WebClient();

            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(Client_DownloadFileCompleted);

            try {
                Uri downloadUri = new Uri(fileUrl);
                client.DownloadFileAsync(downloadUri, Path.Combine(currentDirectory + "\\" + filePath, fileName));
            } catch (UriFormatException ex) {
                MessageBox.Show("連結錯誤：" + ex.Message, "下載錯誤");
            }
        }

        void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
            string currentDirectory = Directory.GetCurrentDirectory();
            downloadLinks_.RemoveAt(0);

            label4.Content = Convert.ToInt32(label4.Content.ToString()) + 1;

            if (downloadLinks_.Count == 0) {
                label2.Content = "更新完成, 可以開始遊戲!";
                updateInProgress = false;
                button1.IsEnabled = true;

                ImageBrush imageBrush = new ImageBrush {
                    ImageSource = new BitmapImage(new Uri("pack://application:,,,/Imagens/play.png"))
                };
                button1.Background = imageBrush;

                label4.Content = "0";
            } else {
                pbStatus.Value = 0;
                label2.Content = downloadLinks_[0].Split('*')[0] + "\\" + downloadLinks_[0].Split('*')[1];

                if (File.Exists(Path.Combine(currentDirectory, downloadLinks_[0].Split('*')[1])))
                    File.Delete(Path.Combine(currentDirectory, downloadLinks_[0].Split('*')[1]));

                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(Client_DownloadFileCompleted);

                try {
                    if (downloadLinks_[0].Split('*')[0] != string.Empty) {
                        if (!Directory.Exists(currentDirectory + "\\" + downloadLinks_[0].Split('*')[0])) {
                            Directory.CreateDirectory(currentDirectory + "\\" + downloadLinks_[0].Split('*')[0]);
                        }
                    }
                    Uri downloadUri = new Uri(downloadLinks_[0].Split('*')[3]);
                    client.DownloadFileAsync(downloadUri, Path.Combine(currentDirectory + "\\" + downloadLinks_[0].Split('*')[0], downloadLinks_[0].Split('*')[1]));
                } catch (UriFormatException ex) {
                    MessageBox.Show("連結錯誤：" + ex.Message, "下載錯誤");
                }
            }
        }

        void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            string totalBytesString = downloadLinks_[0].Split('*')[4];

            if (!double.TryParse(totalBytesString, out double totalBytes)) {
                MessageBox.Show("讀取檔案大小錯誤。檔案大小無效：" + totalBytesString, "錯誤");
                return;
            }

            double percentage = bytesIn / totalBytes * 100;
            label7.Content = string.Format("{0} MB / {1} MB", (e.BytesReceived / 1024d / 1024d).ToString("0.00"), (totalBytes / 1024d / 1024d).ToString("0.00"));
            pbStatus.Value = (int)Math.Truncate(percentage);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e) {
            StartChecks();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e) {
            if (sender is Hyperlink source) {
                Process.Start(source.NavigateUri.ToString());
            }
        }

        private async void StartChecks() {
            if (updateInProgress) {
                MessageBox.Show("更新程式正在更新檔案。", "警告");
                return;
            }

            label2.Content = "檔案驗證中, 請稍後...";
            button1.IsEnabled = false;
            ImageBrush imageBrush = new ImageBrush {
                ImageSource = new BitmapImage(new Uri("pack://application:,,,/Imagens/play-off.png"))
            };
            button1.Background = imageBrush;

            List<string> downloadLinks = await CheckForUpdates();
            if (downloadLinks == null) {
                label2.Content = "請更新啟動器";
                MessageBoxResult result = MessageBox.Show(
                    "無法獲得更新列表！\n\r請從網站下載新版遊戲檔案",
                    "警告",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.OK) {
                    try {
                        Process.Start(new ProcessStartInfo {
                            FileName = website_url.ToString(),
                            UseShellExecute = true
                        });
                    } catch (Exception ex) {
                        MessageBox.Show("打開瀏覽器失敗：" + ex.Message, "錯誤");
                    }
                }

                return;
            }

            if (downloadLinks.Count == 0) {
                string currentDirectory = Directory.GetCurrentDirectory();
                string exePath = Path.Combine(currentDirectory, localhost_name);

                if (checkExe) {
                    if (File.Exists(exePath)) {
                        ProcessStartInfo pInfo = new ProcessStartInfo {
                            FileName = exePath
                        };


                        try {
                            using (Process exeProcess = Process.Start(pInfo)) {
                                this.Close();
                            }
                        } catch (Exception ex) {
                            MessageBox.Show("啟動遊戲時發生錯誤：" + ex.Message, "錯誤");
                        }
                    } else {
                        MessageBox.Show("在資料夾中找不到遊戲主程式！", "資訊");
                    }
                } else {
                    ProcessStartInfo pInfo = new ProcessStartInfo {
                        FileName = exePath
                    };

                    try {
                        using (Process exeProcess = Process.Start(pInfo)) {
                            this.Close();
                        }
                    } catch (Exception ex) {
                        MessageBox.Show("啟動遊戲時發生錯誤：" + ex.Message, "錯誤");
                    }
                }
            } else if (downloadLinks != null && downloadLinks.Count > 0) {
                Download_Load();
                updateInProgress = true;
            } else {
                MessageBox.Show("檢查更新時發生錯誤，請稍後再試。", "錯誤");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (updateInProgress) {
                MessageBoxResult result = MessageBox.Show(
                    "更新進行中，確定要關閉程式？",
                    "確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.No) return;

            }

            Application.Current.Shutdown();
        }
    }
}
