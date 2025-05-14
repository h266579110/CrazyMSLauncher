using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace CalculateHash {



    class Program {
        //Specify download link for .wz files
        static readonly string _wzURL = "https://ms.fz-game.club/update/";

        static int Main(string[] args) {
            try {
                Run(args);
                return 0;
            } catch (Exception e) {
                Console.Error.WriteLine(e.Message);
                return Environment.ExitCode != 0 ? Environment.ExitCode : 0xbad;
            }
        }

        static void Run(IEnumerable<string> args) {
            string updateFolder = "file";
            string outputFilePath = "downloads.xml";

            if (!Directory.Exists(updateFolder)) {
                throw new Exception("The 'file' folder was not found.");
            }

            var files = Directory.GetFiles(updateFolder, "*", SearchOption.AllDirectories);

            XElement rootElement = new XElement("downloads");

            foreach (var file in files) {
                string hash = BitConverter.ToString(MD5Hash(file)).ToLowerInvariant().Replace("-", string.Empty);

                string fileName = Path.GetFileName(file);
                string filePath = Path.GetDirectoryName(file).Split(new[] { "file" }, StringSplitOptions.None)[1];
                long fileSize = new FileInfo(file).Length;

                XElement fileElement = new XElement("file",
                    new XElement("file_link", _wzURL + file.Replace("\\", "/")),
                    new XElement("file_size", fileSize),
                    new XElement("file_path", filePath == string.Empty ? "" : filePath.Substring(1)),
                    new XElement("file_name", fileName),
                    new XElement("file_hash", hash)
                );

                rootElement.Add(fileElement);
            }

            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                rootElement
            );

            doc.Save(outputFilePath);

            Console.WriteLine("Successfully generated XML file in 'downloads.xml'.");
        }

        static byte[] MD5Hash(string path) {
            using (var stream = new FileStream(path, FileMode.Open,
                                                 FileAccess.Read,
                                                 FileShare.Read,
                                                 4096,
                                                 FileOptions.SequentialScan)) {
                return MD5.Create().ComputeHash(stream);
            }
        }
    }
}
