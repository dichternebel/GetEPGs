using System.Reflection;

namespace GetEPGs
{
    internal class Program
    {
        static string appVersion;

        static void Main(string[] args)
        {
            // Initialize serilog logger
            Logging.Init();

            appVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            $"Running GetEPGs v{appVersion}".Info(true);
            Console.WriteLine();

            if (args.Length == 0) GetEPGs();
            else if (args.Length > 0)
            {
                if (args[0] == "7zpath")
                {
                    GetEPGs(args[0]);
                }
                else if (args[0] == "install-task")
                {
                    TaskManager.InstallTask();
                }
                else if (args[0] == "remove-task")
                {
                    TaskManager.RemoveTask();
                }
                else
                {
                    "Unknown argument. Known arguments are 7zpath, install-task and remove-task.".Warn();
                }
            }
            Thread.Sleep(1500);
        }

        static void GetEPGs(string? customPath = null)
        {
            var list = GetEPGsources().Result;
            if (list == null || list.Count == 0) return;

            var sevenZip = new SevenZip();
            sevenZip.Set7zPath(customPath);
            if (sevenZip.ExePath == null || sevenZip.DllPath == null) return;

            var getEPGs = new GetEPGs(sevenZip.ExePath, sevenZip.DllPath, list);
            getEPGs.Do();
        }

        static async Task<List<EPGobj>?> GetEPGsources()
        {
            string sourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "epg_sources.txt");

            if (!File.Exists(sourcesPath))
            {
                "File 'epg_sources.txt' does not exist! Aborting.".Warn(true);
                return null;
            }

            $"Checking sources in '{sourcesPath}'.".Info();
            "This might take some time...".Info();
            Console.WriteLine();

            var list = new List<EPGobj>();
            int i = 0;
            using (StreamReader reader = new StreamReader(sourcesPath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (!line.TrimStart().StartsWith("#"))
                    {
                        line = line.Trim();
                        var uri = UriValidator.IsValiUrl(line);
                        if (uri == null)
                        {
                            $"Not valid: {line}".Warn(true);
                            continue;
                        }
                        
                        var isAccessable = await UriValidator.IsUriAccessible(uri);
                        if (!isAccessable)
                        {
                            $"Not accessible: {line}".Warn(true);
                            continue;
                        }

                        var item = new EPGobj();
                        item.Id = i.ToString("D3");
                        item.Uri = uri;
                        item.SourceFileName = Path.GetFileName(uri.LocalPath);
                        if (string.IsNullOrEmpty(item.SourceFileName))
                        {
                            item.SourceFileName = Guid.NewGuid().ToString();
                            item.TargetFileName = item.SourceFileName + ".xml";
                        }
                        else
                        {
                            item.TargetFileName = Path.GetFileNameWithoutExtension(item.SourceFileName);
                        }

                        list.Add(item);
                        i++;
                    }
                }
            }

            if (list.Count == 0) "No valid sources found in 'epg_sources.txt'! Aborting.".Warn(true);
            return list;
        }
    }
}