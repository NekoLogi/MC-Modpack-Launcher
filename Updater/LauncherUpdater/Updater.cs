using SevenZipExtractor;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LauncherUpdater
{
    internal class Updater
    {
        public static string? VERSION_CLIENT { get; set; }
        public static string NO_VERSION = "N/a";
        public static Settings G_Settings = new Settings();

        private const string VERSION_FILE = "Launcher/Launcher_Version.txt";


        public bool Start()
        {
            Init();

            if (!IsCurrentVersion())
                return false;

            StartLauncher();
            return true;
        }

        private void Init()
        {
            if (!G_Settings.Load())
                G_Settings.Save();

            Task task1 = Task.Run(()
                => VERSION_CLIENT = GetClientVersion());

            Server server = new Server();
            Task task2 = Task.Run(()
                => server.Start(Server.Status.Version));

            task1.Wait();
            task2.Wait();

            if (!Directory.Exists("Cache"))
                Directory.CreateDirectory("Cache");
        }

        // Get Client (VERSION_FILE) for current version.
        private string GetClientVersion()
        {
            if (!File.Exists(VERSION_FILE))
                return NO_VERSION;

            return File.ReadAllText(VERSION_FILE);
        }

        // Compare current client and server version.
        private bool IsCurrentVersion()
        {
            if (VERSION_CLIENT == Server.VERSION_SERVER)
                return true;
            return false;
        }

        // Update client launcher.
        public void UpdateClient()
        {
            Server server = new Server();

            Task downloader = Task.Run(()
                => server.Start(Server.Status.Update));

            downloader.Wait();


            InstallClient();
            SetVersion();
            StartLauncher();
        }

        // Install the downloaded client launcher.
        private void InstallClient()
        {
            if (File.Exists("Cache/launcher.7z"))
            {
                string zipPath = "Cache/launcher.7z";
                string extractpath = "Launcher";
                //string cache = "Cache/Launcher";

                if (!Directory.Exists(extractpath))
                    Directory.CreateDirectory(extractpath);

                using (ArchiveFile archiveFile = new ArchiveFile(zipPath))
                {
                    archiveFile.Extract(extractpath, true);
                }
                File.Delete(zipPath);
            }
        }

        // Start launcher and close updater.
        private void StartLauncher()
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(Directory.GetCurrentDirectory() + "\\Launcher\\ModLauncher.exe") { CreateNoWindow = true });
            Environment.Exit(0);
        }

        // Set new launcher version.
        private void SetVersion()
        {
            File.WriteAllText(VERSION_FILE, Server.VERSION_SERVER!);
        }
    }
}
