using System.Diagnostics;
using System.Xml;

namespace GetEPGs
{
    internal class GetEPGs
    {
        string outputDir {  get; set; }
        string sevenZipPath { get; set; }
        string sevenZipDllPath { get; set; }
        string mergedFile {  get; set; }
        string finalCompressedFile { get; set; }
        string dtdFilePath { get; set; }
        List<EPGobj> list { get; set; }

        internal GetEPGs(string sevenZipPath, string sevenZipDllPath, List<EPGobj> list)
        {
            this.outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            this.sevenZipPath = sevenZipPath;
            this.sevenZipDllPath = sevenZipDllPath;
            this.list = list;
            this.mergedFile = Path.Combine(this.outputDir, "epg.xml");
            this.finalCompressedFile = Path.Combine(this.outputDir, "epg.xml.gz");
            this.dtdFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "xmltv.dtd");
        }

        internal void Do()
        {
            if (this.sevenZipPath == null || this.sevenZipDllPath == null || this.list == null || this.list.Count() < 1) return;

            var xmlDocs = new List<XmlDocument>();
            foreach (var item in list)
            {
                var currentOutputDir = Path.Combine(this.outputDir, item.Id);
                if (!Directory.Exists(currentOutputDir)) Directory.CreateDirectory(currentOutputDir);

                var contentType = DownloadFile(item.Uri, Path.Combine(this.outputDir, item.Id, item.SourceFileName)).Result;
                if (contentType != null && !contentType.StartsWith("text/"))
                {
                    DecompressFile(Path.Combine(outputDir, item.Id, item.SourceFileName), Path.Combine(this.outputDir, item.Id, item.TargetFileName));
                }
                else
                {
                    item.TargetFileName = item.SourceFileName;
                }

                bool isValid = XmltvValidator.ValidateXmlAgainstDtd(Path.Combine(this.outputDir, item.Id, item.TargetFileName), dtdFilePath);
                if (!isValid)
                {
                    $"The XMLTV document is not valid! Skipping file {item.SourceFileName}.".Warn(true);
                    continue;
                }

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(Path.Combine(this.outputDir, item.Id, item.TargetFileName));
                xmlDocs.Add(xmlDoc);
                Console.WriteLine();
            }

            $"Processing {xmlDocs.Count} EPG files...".Info(true);

            for (int i = 1; i < xmlDocs.Count; i++)
            {
                foreach (XmlNode node in xmlDocs[i].DocumentElement.ChildNodes)
                {
                    XmlNode importedNode = xmlDocs[0].ImportNode(node, true);
                    xmlDocs[0].DocumentElement.AppendChild(importedNode);
                }
            }

            xmlDocs[0].Save(mergedFile);
            CompressGzipFile(mergedFile, finalCompressedFile);

            foreach (var item in list)
            {
                RemoveFolder(Path.Combine(this.outputDir, item.Id));
            }
            "Done!".Info(true);
        }

        async Task<string?> DownloadFile(Uri url, string outputPath)
        {
           $"Downloading {url}".Info();
#if DEBUG
            if (File.Exists(outputPath)) return "whatsoever";
#endif
            string? contentType = null;

            using var client = new HttpClient();
            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = File.Create(outputPath);
                await contentStream.CopyToAsync(fileStream);

                contentType = response.Content.Headers.ContentType?.MediaType;
                $"File downloaded to: {outputPath}".Info();
            }
            catch (HttpRequestException e)
            {
                $"Error downloading file: {e.Message}".Error(true);
            }

            return contentType;
        }

        void DecompressFile(string sourceFile, string destinationFile)
        {
            if (!File.Exists(sevenZipPath) || !File.Exists(sevenZipDllPath))
            {
                throw new FileNotFoundException("Please make sure that 7z.exe and 7z.dll exist in the application directory.");
            }

            $"Decompressing {sourceFile}...".Info();

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = sevenZipPath,
                Arguments = $"e \"{sourceFile}\" -o\"{Path.GetDirectoryName(destinationFile)}\" -y",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(processStartInfo))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    var exMsg = $"7-zip exited with code {process.ExitCode}";
                    exMsg.Error(true);
                    throw new InvalidOperationException(exMsg);
                }
            }
        }

        void CompressGzipFile(string sourceFile, string destinationFile)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = sevenZipPath,
                Arguments = $"a -tgzip \"{destinationFile}\" \"{sourceFile}\" -y",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(processStartInfo))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    var exMsg = $"7-zip exited with code {process.ExitCode}";
                    exMsg.Error(true);
                    throw new InvalidOperationException(exMsg);
                }
            }
        }

        void RemoveFolder(string folder)
        {
#if DEBUG
            return;
#endif
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
        }
    }
}
