using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.IO;
using System.Net;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Xml;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Launcher_2016
{

    public partial class MainWindow : Window
    {

        public string server_ip = "127.0.0.1";
        public int port = 8484;

        public string localhost_name = "MapleStory.exe";
        public string server_name = "MapleOrigins";
        public Uri website_url = new Uri("http://google.com/");
        public Uri discord_url = new Uri("http://google.com/");
        public string[] updateFiles;
        public int createBackup = 0; // 0 = false / 1 = true
        public bool force_admin = false;

        private bool checkExe = false; 
        public Boolean updateInProgress = false;

        public bool iniOutput = true; 

        private static string _xmlURL = "http://127.0.0.1/update/downloads.xml";
        public static string xmlURL { get { return _xmlURL; } set { _xmlURL = value; } }

        public List<string> downloadLinks_;


        [DllImport("kernel32.dll")]
        private static extern int ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In] [Out] byte[] buffer, uint size, out IntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In] [Out] byte[] buffer, uint size, out IntPtr lpNumberOfBytesWritten);


         public MainWindow(): this(OverlayStyle.WinForms){}

        public MainWindow(OverlayStyle style) {
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
            if (!windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator) && force_admin)
            {
                MessageBox.Show("Please run as Administrator!", "Warning");
                Close();
            }

            InitializeComponent();

            if (iniOutput)
            {
                LoadConfig("config.ini");
            }

            label5.Content = server_name;
            ws.NavigateUri = website_url;
            disc.NavigateUri = discord_url;


            updateFiles = new string[]
            {
                "Base.wz",
                "Character.wz",
                "Effect.wz",
                "Etc.wz",
                "Item.wz",
                "List.wz",
                "Map.wz",
                "Mob.wz",
                "Morph.wz",
                "Npc.wz",
                "Quest.wz",
                "Reactor.wz",
                "Skill.wz",
                "Sound.wz",
                "String.wz",
                "TamingMob.wz",
                "UI.wz",
                "ijl15.dll",
                localhost_name
            };
        }

        private void LoadConfig(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Configuration file not found: {filePath}. Using default values.");
                return;
            }

            try
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("["))
                        continue;

                    var parts = line.Split('=');
                    if (parts.Length != 2) continue; 

                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "server_ip":
                            server_ip = value;
                            break;
                        case "port":
                            if (int.TryParse(value, out int parsedPort))
                            {
                                port = parsedPort;
                            }
                            break;
                        case "localhost_name":
                            localhost_name = value;
                            break;
                        case "server_name":
                            server_name = value;
                            break;
                        case "create_backup":
                             if (int.TryParse(value, out int parsedBkp))
                             {
                                createBackup = parsedBkp;
                             }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration file: {ex.Message}");
            }
        }


        public enum OverlayStyle { 
            WPF, 
            WinForms 
        };

        private List<string> checkForUpdates() {

            string xmlResponse = "";
            string currentDirectory = Directory.GetCurrentDirectory();

            try {
                xmlResponse = new WebClient().DownloadString(xmlURL);
            } catch {
                Console.WriteLine("Error downloading update XML.");
                return null;
            }

            List<string> downloadLinks = new List<string>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlResponse);

            foreach (string wzName in updateFiles) {

                string temp = wzName.Split('.')[0].ToLower();

                XmlNode fileNode = xmlDoc.SelectSingleNode($"//file[file_name='{wzName}']");

                if (fileNode != null) {
                    string wzDownload = fileNode["file_link"]?.InnerText;
                    string wzHash = fileNode["file_hash"]?.InnerText;
                    string wzSize = fileNode["file_size"]?.InnerText;

                    if (File.Exists(System.IO.Path.Combine(currentDirectory, wzName)))  {
                        string wzHashOnComputer = getFileHash(System.IO.Path.Combine(currentDirectory, wzName));

                        if (wzHash != wzHashOnComputer) {
                            downloadLinks.Add(wzName + "*" + wzHash + "*" + wzDownload + "*" + wzSize);
                        }
                    } else {
                        downloadLinks.Add(wzName + "*" + wzHash + "*" + wzDownload + "*" + wzSize);
                    }
                }
                else
                {
                    Console.WriteLine($"File {wzName} not found in XML\r\n.");
                }
            }

            Console.WriteLine(downloadLinks);

            downloadLinks_ = downloadLinks;

            return downloadLinks;
        }

        private string getFileHash(string filePath)
        {
            if (!File.Exists(filePath)) return string.Empty;

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan))
            {
                byte[] hash = MD5.Create().ComputeHash(stream);
                return BitConverter.ToString(hash).ToLowerInvariant().Replace("-", string.Empty);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            this.DragMove();
        }

       private void Download_Load() {
            string currentDirectory = Directory.GetCurrentDirectory();

            label6.Content = downloadLinks_.Count.ToString();
            label2.Content = downloadLinks_[0].Split('*')[0];

            string fileName = downloadLinks_[0].Split('*')[0];
            string fileUrl = downloadLinks_[0].Split('*')[2];

            Console.WriteLine("Download URL: " + fileUrl);

            if (File.Exists(System.IO.Path.Combine(currentDirectory, fileName)))
            {
                if (fileName.Contains(".wz") && createBackup == 1)
                {
                    bool complete = false;
                    int i = 1;

                    while (!complete)
                    {
                        string backupFileName = fileName.Split('.')[0] + ".BAK" + i.ToString();
                        if (File.Exists(System.IO.Path.Combine(currentDirectory, backupFileName)))
                        {
                            i++;
                        }
                        else
                        {
                            complete = true;
                            File.Move(System.IO.Path.Combine(currentDirectory, fileName), System.IO.Path.Combine(currentDirectory, backupFileName));
                        }
                    }
                        }
                else
                {
                    File.Delete(System.IO.Path.Combine(currentDirectory, fileName));
                }
            }

            WebClient client = new WebClient();

            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);

            try
            {
                Uri downloadUri = new Uri(fileUrl);
                client.DownloadFileAsync(downloadUri, System.IO.Path.Combine(currentDirectory, fileName));
            }
            catch (UriFormatException ex)
            {
                MessageBox.Show("URL error: " + ex.Message, "Error downloading file");
            }
        }   

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            downloadLinks_.RemoveAt(0); 

            label4.Content = Convert.ToInt32(label4.Content.ToString()) + 1;

            if (downloadLinks_.Count == 0)
            {
                label2.Content = "Done, we can now start!";
                updateInProgress = false;
                button1.IsEnabled = true;

                ImageBrush imageBrush = new ImageBrush();
                imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Imagens/play.png"));
                button1.Background = imageBrush;

                label4.Content = "0";
            }
            else
            {
                pbStatus.Value = 0;
                label2.Content = downloadLinks_[0].Split('*')[0];

                if (File.Exists(System.IO.Path.Combine(currentDirectory, downloadLinks_[0].Split('*')[0])))
                {
                    File.Delete(System.IO.Path.Combine(currentDirectory, downloadLinks_[0].Split('*')[0]));
                }

                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);

                try
                {
                    Uri downloadUri = new Uri(downloadLinks_[0].Split('*')[2]);
                    client.DownloadFileAsync(downloadUri, System.IO.Path.Combine(currentDirectory, downloadLinks_[0].Split('*')[0]));
                }
                catch (UriFormatException ex)
                {
                    MessageBox.Show("URL error: " + ex.Message, "Error downloading file");
                }
            }
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());

            string totalBytesString = downloadLinks_[0].Split('*')[3];
            double totalBytes = 0;

            if (!double.TryParse(totalBytesString, out totalBytes))
            {
                MessageBox.Show("Error reading file size. Invalid value for total size: " + totalBytesString, "Error");
                return;
            }

            double percentage = bytesIn / totalBytes * 100;
            label7.Content = string.Format("{0} MB / {1} MB",   (e.BytesReceived / 1024d / 1024d).ToString("0.00"),  (totalBytes / 1024d / 1024d).ToString("0.00"));
            pbStatus.Value = (int)Math.Truncate(percentage);
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
             StartChecks();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink source = sender as Hyperlink;

            if (source != null)
            {
                System.Diagnostics.Process.Start(source.NavigateUri.ToString());
            }
        }

        private void StartChecks()
        {
            if (updateInProgress)
            {
                MessageBox.Show("The launcher is already updating its files.", "Warning");
                return;
            }

            List<string> downloadLinks = checkForUpdates();
            if (downloadLinks == null)
            {
                label2.Content = "Please, update launcher";
                var result = MessageBox.Show(
                    "Unable to get update list!\n\rDownload the new launcher directly from the website", 
                    "Warning", 
                    MessageBoxButton.OKCancel, 
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new ProcessStartInfo
                        {
                            FileName = website_url.ToString(), 
                            UseShellExecute = true 
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to open browser: " + ex.Message, "Error");
                    }
                }

                return;
            }

            if (downloadLinks.Count == 0)
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                string exePath = System.IO.Path.Combine(currentDirectory, localhost_name);

                if (checkExe)
                {
                    if (File.Exists(exePath))
                    {
                        ProcessStartInfo pInfo = new ProcessStartInfo
                        {
                            FileName = exePath,
                            Arguments = server_ip + " " + port 
                        };


                        try
                        {
                            using (Process exeProcess = Process.Start(pInfo))
                            {
                                this.Close();  
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error when trying to start the game: " + ex.Message, "Error");
                        }
                    }
                    else
                    {
                        MessageBox.Show("We are unable to locate " + localhost_name + " in the folder!", "Info");
                    }
                }
                else
                {
                    ProcessStartInfo pInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = server_ip + " " + port 
                    };

                    try
                    {
                        using (Process exeProcess = Process.Start(pInfo))
                        {
                            this.Close();  
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error when trying to start the game: " + ex.Message, "Error");
                    }
                }
            }
            else if (downloadLinks != null && downloadLinks.Count > 0)
            {
                Download_Load();
                updateInProgress = true;  
                button1.IsEnabled = false;

                ImageBrush imageBrush = new ImageBrush();
                imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Imagens/play-off.png"));

                button1.Background = imageBrush;
            }
            else
            {
                MessageBox.Show("Error checking for updates. Please try again later.", "Error");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (updateInProgress)
            {
                MessageBoxResult result = MessageBox.Show(
                    "An update is in progress. Are you sure you want to close the program?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.No) return;
                
            }

            Application.Current.Shutdown();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Developer by GabrielSin (https://github.com/albinosin/)", "About");
        }
    }
}
