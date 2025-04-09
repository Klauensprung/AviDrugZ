using System;
using System.IO;
using System.Reflection;
using IWshRuntimeLibrary;
using File = System.IO.File;

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

                // Correct and reliable way to get the executable path
                string executablePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;

                if (string.IsNullOrEmpty(executablePath) || !File.Exists(executablePath))
                    throw new FileNotFoundException("Executable path could not be resolved or does not exist.");

                string iconPath = Path.Combine(Path.GetDirectoryName(executablePath), "Resources", "splash.ico");
                if (!File.Exists(iconPath))
                    iconPath = executablePath; // fallback to exe icon

                var shell = new WshShell();
                IWshShortcut shortcut;

                if (File.Exists(shortcutPath))
                {
                    shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                }
                else
                {
                    shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                }

                shortcut.Description = description;
                shortcut.TargetPath = executablePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(executablePath);
                shortcut.IconLocation = iconPath;
                shortcut.Save();
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
