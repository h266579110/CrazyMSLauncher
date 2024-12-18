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

namespace Launcher_2016
{

    public partial class MainWindow : Window
    {

        public string server_ip = "192.99.188.44";
        public int port = 8484;

        private bool verificarExe = false; // Variável para controlar se o arquivo MapleCustom.exe será verificado ou não
        public Boolean atualizacaoAndamento = false;

        private static string _xmlURL = "http://lauche.maplecustom.com.br:8085/update/downloads.xml";
        public static string xmlURL { get { return _xmlURL; } set { _xmlURL = value; } }

        public List<string> downloadLinks_;
        string[] updateFiles = { "Base.wz",
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
                                "MapleCustom.exe",};

        [DllImport("kernel32.dll")]
        private static extern int ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In] [Out] byte[] buffer, uint size, out IntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In] [Out] byte[] buffer, uint size, out IntPtr lpNumberOfBytesWritten);

        public MainWindow(): this(OverlayStyle.WinForms)
        {
        }

        public MainWindow(OverlayStyle style)
        {
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
            if (!windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Por favor, execute como Administrador!", "Atenção");
                Close();
            }

            InitializeComponent();
        }


        public enum OverlayStyle { 
            WPF, // Can't use opacity.
            WinForms // Flicker on resize when opacity is used without DWM composition
        };

        private List<string> checkForUpdates()
        {
            string xmlResponse = "";
            string currentDirectory = Directory.GetCurrentDirectory();
            try
            {
                // Baixa o XML de atualização
                xmlResponse = new WebClient().DownloadString(xmlURL);
            }
            catch
            {
                // Se não conseguir baixar o XML, retornamos uma lista vazia
                Console.WriteLine("Erro ao baixar o XML de atualização.");
                return null;
            }

            List<string> downloadLinks = new List<string>();
            // Carregar o XML para processamento
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlResponse);

            // Verifica atualização dos arquivos WZ
            foreach (string wzName in updateFiles)
            {
                string temp = wzName.Split('.')[0].ToLower();

                // Buscar o elemento <file> correspondente ao wzName
                XmlNode fileNode = xmlDoc.SelectSingleNode($"//file[file_name='{wzName}']");

                if (fileNode != null)
                {
                    string wzDownload = fileNode["file_link"]?.InnerText;
                    string wzHash = fileNode["file_hash"]?.InnerText;
                    string wzSize = fileNode["file_size"]?.InnerText;

                    if (File.Exists(System.IO.Path.Combine(currentDirectory, wzName)))
                    {
                        // Calcula o hash MD5 do arquivo WZ local
                        string wzHashOnComputer = getFileHash(System.IO.Path.Combine(currentDirectory, wzName));

                        // Se o hash do arquivo local for diferente do hash do XML, adiciona à lista de downloads
                        if (wzHash != wzHashOnComputer)
                        {
                            downloadLinks.Add(wzName + "*" + wzHash + "*" + wzDownload + "*" + wzSize);
                        }
                    }
                    else
                    {
                        // Se o arquivo WZ não existir, adiciona à lista de downloads
                        downloadLinks.Add(wzName + "*" + wzHash + "*" + wzDownload + "*" + wzSize);
                    }
                }
                else
                {
                    Console.WriteLine($"Arquivo {wzName} não encontrado no XML.");
                }
            }

            Console.WriteLine(downloadLinks);

            downloadLinks_ = downloadLinks;

            return downloadLinks;
        }

        // Função para calcular o hash MD5 de um arquivo
        private string getFileHash(string filePath)
        {
            if (!File.Exists(filePath)) return string.Empty;

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan))
            {
                byte[] hash = MD5.Create().ComputeHash(stream);
                return BitConverter.ToString(hash).ToLowerInvariant().Replace("-", string.Empty);
            }
        }

        // Função auxiliar para extrair o conteúdo entre duas tags no XML
        private string getBetween(string source, string startTag, string endTag, int startIndex)
        {
            int startPos = source.IndexOf(startTag, startIndex);
            int endPos = source.IndexOf(endTag, startPos + startTag.Length);
            if (startPos == -1 || endPos == -1) return string.Empty;
            return source.Substring(startPos + startTag.Length, endPos - startPos - startTag.Length);
        }

        private string[] getBetweenAll(string strSource, string strStart, string strEnd)
        {
            List<string> Matches = new List<string>();

            for (int pos = strSource.IndexOf(strStart, 0),
                end = pos >= 0 ? strSource.IndexOf(strEnd, pos) : -1;
                pos >= 0 && end >= 0;
                pos = strSource.IndexOf(strStart, end),
                end = pos >= 0 ? strSource.IndexOf(strEnd, pos) : -1)
            {
                Matches.Add(strSource.Substring(pos + strStart.Length, end - (pos + strStart.Length)));
            }

            return Matches.ToArray();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

       private void Download_Load()
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            label6.Content = downloadLinks_.Count.ToString();
            label2.Content = downloadLinks_[0].Split('*')[0];

            string fileName = downloadLinks_[0].Split('*')[0];
            string fileUrl = downloadLinks_[0].Split('*')[2]; // A URL de download

            // Verifique a URL antes de tentar o download
            Console.WriteLine("URL de Download: " + fileUrl); // Adicione um log para verificar a URL

            // Verifique se o arquivo já existe no diretório e renomeie ou delete
            if (File.Exists(System.IO.Path.Combine(currentDirectory, fileName)))
            {
                if (fileName.Contains(".wz"))
                {
                    Boolean complete = false;
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
                Uri downloadUri = new Uri(fileUrl); // Converte a URL para Uri, se falhar, você terá uma exceção
                client.DownloadFileAsync(downloadUri, System.IO.Path.Combine(currentDirectory, fileName));
            }
            catch (UriFormatException ex)
            {
                // Se a URL for inválida, exiba um erro
                MessageBox.Show("Erro de URL: " + ex.Message, "Erro ao baixar arquivo");
            }
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            downloadLinks_.RemoveAt(0); // Remove o arquivo atual da lista

            label4.Content = Convert.ToInt32(label4.Content.ToString()) + 1;

            // Verifique se todos os arquivos foram baixados
            if (downloadLinks_.Count == 0)
            {
                label2.Content = "Concluído, já podemos iniciar!";
                atualizacaoAndamento = false;
                label4.Content = "0";
            }
            else
            {
                pbStatus.Value = 0;
                label2.Content = downloadLinks_[0].Split('*')[0];

                // Verifique se o arquivo existe antes de deletar
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
                    MessageBox.Show("Erro de URL: " + ex.Message, "Erro ao baixar arquivo");
                }
            }
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Verifique o valor de BytesReceived antes de usar
            double bytesIn = double.Parse(e.BytesReceived.ToString());

            // Verifique se o valor da totalBytes está correto
            string totalBytesString = downloadLinks_[0].Split('*')[3];
            double totalBytes = 0;

            // Use TryParse para garantir que a conversão seja segura
            if (!double.TryParse(totalBytesString, out totalBytes))
            {
                // Se falhar na conversão, exiba uma mensagem ou defina um valor padrão
                MessageBox.Show("Erro ao ler o tamanho do arquivo. Valor inválido para o tamanho total: " + totalBytesString, "Erro");
                return; // Se não for possível converter, pare o processamento
            }

            double percentage = bytesIn / totalBytes * 100;

            label7.Content = string.Format("{0} MB / {1} MB", 
                (e.BytesReceived / 1024d / 1024d).ToString("0.00"), 
                (totalBytes / 1024d / 1024d).ToString("0.00"));
    
            pbStatus.Value = (int)Math.Truncate(percentage);
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
             IniciarVerificacoes();
         }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink source = sender as Hyperlink;

            if (source != null)
            {
                System.Diagnostics.Process.Start(source.NavigateUri.ToString());

            }

        }

        private void IniciarVerificacoes()
        {
            // Verifica se já estamos no processo de atualização
            if (atualizacaoAndamento)
            {
                MessageBox.Show("O launcher já está atualizando seus arquivos.", "Atenção");
                return;
            }

            // Verifica se há atualizações a serem feitas
            List<string> downloadLinks = checkForUpdates();

            if (downloadLinks == null)
            {
                MessageBox.Show("Não foi possivel obter lista de atualizações!\n\rBaixe o novo launcher diretamente no site do MapleCustom\n\rLink - https://maplecustom.com.br/MapleCustom/", "Atenção");
                return;
            }

            // Se não houver atualizações
            if (downloadLinks.Count == 0)
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                string exePath = System.IO.Path.Combine(currentDirectory, "MapleCustom.exe");

                // Se a variável verificarExe for verdadeira, realiza a verificação do arquivo .exe
                if (verificarExe)
                {
                    if (File.Exists(exePath))
                    {
                        // Prepara para iniciar o jogo
                        ProcessStartInfo pInfo = new ProcessStartInfo
                        {
                            FileName = exePath,
                            Arguments = server_ip + " " + port // Passando os parâmetros de conexão para o jogo
                        };

                        // Inicia o processo do jogo e fecha o launcher
                        try
                        {
                            using (Process exeProcess = Process.Start(pInfo))
                            {
                                this.Close();  // Fecha o launcher
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Erro ao tentar iniciar o jogo: " + ex.Message, "Erro");
                        }
                    }
                    else
                    {
                        // Caso o arquivo MapleStory.exe não seja encontrado
                        MessageBox.Show("Não conseguimos localizar o MapleStory.exe na pasta!", "Info");
                    }
                }
                else
                {
                    // Se a verificação do exe não for necessária, simplesmente inicia o jogo sem verificações
                    ProcessStartInfo pInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = server_ip + " " + port // Passando os parâmetros de conexão para o jogo
                    };

                    // Inicia o processo do jogo e fecha o launcher
                    try
                    {
                        using (Process exeProcess = Process.Start(pInfo))
                        {
                            this.Close();  // Fecha o launcher
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro ao tentar iniciar o jogo: " + ex.Message, "Erro");
                    }
                }
            }
            // Se houver arquivos para atualizar
            else if (downloadLinks != null && downloadLinks.Count > 0)
            {
                // Inicia o processo de download dos arquivos
                Download_Load();
                atualizacaoAndamento = true;  // Marca que a atualização está em andamento
            }
            else
            {
                // Caso haja algum erro na obtenção dos links de download
                MessageBox.Show("Erro ao verificar atualizações. Tente novamente mais tarde.", "Erro");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Launcher criado especialmente para MapleCustom.\n\rCréditos - GabrielSin (https://github.com/albinosin/)", "Sobre");
        }
    }
}
