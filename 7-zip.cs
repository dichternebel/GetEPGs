using Microsoft.Win32;

namespace GetEPGs
{
    internal class SevenZip
    {
        internal string? ExePath { get; set; }
        internal string? DllPath { get; set; }

        internal void Set7zPath(string? customPath = null)
        {
            if (customPath != null && !Directory.Exists(customPath))
            {
                "Given 7z-path does not exist!".Warn(true);
            }

            if (customPath != null && Directory.Exists(customPath))
            {
                string exePath = Path.Combine(customPath, "7z.exe");
                string dllPath = Path.Combine(customPath, "7z.dll");

                if (File.Exists(exePath) && File.Exists(dllPath))
                {
                    ExePath = exePath;
                    DllPath = dllPath;
                }
                else
                {
                    $"Given 7z-path exists, but 7z.exe or 7z.dll is missing in {customPath}".Warn(true);
                }
            }

            string? registryPath = GetSevenZipPathFromRegistry();

            if (registryPath != null)
            {
                string exePath = Path.Combine(registryPath, "7z.exe");
                string dllPath = Path.Combine(registryPath, "7z.dll");

                if (File.Exists(exePath) && File.Exists(dllPath))
                {
                    ExePath = exePath;
                    DllPath = dllPath;
                }
                else
                {
                    "7-Zip is installed, but 7z.exe or 7z.dll is missing. Please re-install!".Warn(true);
                }
            }
            else
            {
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "7z.exe");
                string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "7z.dll");

                if (File.Exists(exePath) && File.Exists(dllPath))
                {
                    ExePath = exePath;
                    DllPath = dllPath;
                }
                else
                {
                    "7z.exe or 7z.dll could not be found. Please put them into this folder or install 7-zip.".Warn(true);
                }
            }
        }

        string? GetSevenZipPathFromRegistry()
        {
            string[] registryKeys =
            {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\7-Zip",
            @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\7-Zip"
        };

            foreach (string keyPath in registryKeys)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key != null)
                    {
                        object installLocation = key.GetValue("InstallLocation");
                        if (installLocation != null)
                        {
                            return installLocation.ToString();
                        }
                    }
                }
            }
            return null;
        }
    }
}
