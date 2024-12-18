using System;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace CalculateHash {



class Program
{
        static string _wzURL = "http://lauche.maplecustom.com.br:8085/update/wz/";

        static int Main(string[] args)
        {
            try
            {
                Run(args);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return Environment.ExitCode != 0 ? Environment.ExitCode : 0xbad;
            }
        }

        static void Run(IEnumerable<string> args) 
        {
            string updateFolder = "update"; // Caminho da pasta "update"
            string outputFilePath = "downloads.xml"; // Caminho do arquivo de saída XML

            // Verifica se a pasta "update" existe
            if (!Directory.Exists(updateFolder))
            {
                throw new Exception("A pasta 'update' não foi encontrada.");
            }

            // Obtém todos os arquivos dentro da pasta "update"
            var files = Directory.GetFiles(updateFolder);

            // Criação do documento XML
            XElement rootElement = new XElement("downloads");

            foreach (var file in files)
            {
                // Calcula o hash MD5 do arquivo
                string hash = BitConverter.ToString(MD5Hash(file))
                    .ToLowerInvariant()
                    .Replace("-", string.Empty);

                // Obtem o nome do arquivo e o tamanho em bytes
                string fileName = Path.GetFileName(file);
                long fileSize = new FileInfo(file).Length; // Obtém o tamanho do arquivo em bytes

                // Criação do elemento XML para o arquivo
                XElement fileElement = new XElement("file",
                    new XElement("file_link", _wzURL + fileName), // Pode preencher com a URL mais tarde
                    new XElement("file_size", fileSize),
                    new XElement("file_name", fileName),
                    new XElement("file_hash", hash) // Adiciona o hash do arquivo
                );

                // Adiciona o elemento de arquivo ao elemento raiz
                rootElement.Add(fileElement);
            }

            // Cria o arquivo XML e grava os dados nele
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                rootElement
            );

            // Salva o XML no arquivo de saída
            doc.Save(outputFilePath);

            Console.WriteLine("Arquivo XML gerado com sucesso em 'downloads.xml'.");
        }

        static byte[] MD5Hash(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open,
                                                 FileAccess.Read,
                                                 FileShare.Read,
                                                 4096,
                                                 FileOptions.SequentialScan))
            {
                return MD5.Create().ComputeHash(stream);
            }
        }
    }
}
