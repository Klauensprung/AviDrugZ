using System;
using System.IO;
using System.Reflection;
using WindowsShortcutFactory;

namespace AviDrugZ.Lib
{
    public class ShortcutHelper
    {
        public static void CreateOrUpdateStartMenuShortcut(string appName, string description)
        {
            try
            {
                string startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
                string shortcutPath = Path.Combine(startMenuPath, $"{appName}.lnk");

                string executablePath = GetExecutablePath();
                if (string.IsNullOrEmpty(executablePath) || !File.Exists(executablePath))
                    throw new FileNotFoundException("Executable path could not be resolved or does not exist.");

                string workingDirectory = Path.GetDirectoryName(executablePath);
                string iconPath = Path.Combine(workingDirectory, "Resources", "splash.ico");
                if (!File.Exists(iconPath))
                    iconPath = executablePath; // fallback to exe icon

                using var shortcut = new WindowsShortcut
                {
                    Path = executablePath,
                    WorkingDirectory = workingDirectory,
                    IconLocation = iconPath,
                    Description = description
                };

                shortcut.Save(shortcutPath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Shortcut creation failed: {ex}");
            }
        }

        private static string GetExecutablePath()
        {
            string path = Assembly.GetEntryAssembly()?.Location;

            if (string.IsNullOrEmpty(path))
            {
                try
                {
                    path = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                }
                catch { }
            }

            return path;
        }
    }
}
