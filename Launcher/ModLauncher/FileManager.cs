using Newtonsoft.Json;
using SevenZipExtractor;
using System.IO;

namespace ModLauncher
{
    static class FileManager
    {
        public static Modpack Pack = new Modpack();

        public static void ModInstall()
        {
            try
            {
                string zipPath = $"Cache/{FileServer.Pack.Name.ToLower()}.7z";
                string extractpath = $"Launcher/{FileServer.Pack.Name}";

                using (ArchiveFile archiveFile = new ArchiveFile(zipPath))
                {
                    archiveFile.Extract(extractpath, true);
                }
                File.Delete($"Cache/{FileServer.Pack.Name.ToLower()}.7z");
                SetVersion();
            } catch (System.Exception err)
            {
                MainWindow.CurrentWindow.GetError(err.Message);
            }
        }

        public static void ModUpdate()
        {
            try
            {
                if (!File.Exists($"Cache/{FileServer.Pack.Name.ToLower()}_update.7z"))
                {
                    MainWindow.CurrentWindow.GetError("Modpack was not found, after downloading from server!");
                    return;
                }
                string zipPath = $"Cache/{FileServer.Pack.Name.ToLower()}_update.7z";
                string extractpath = $"Launcher/{FileServer.Pack.Name}";
                string cache = $"Cache/{FileServer.Pack.Name.ToLower()}";

                using (ArchiveFile archiveFile = new ArchiveFile(zipPath))
                {
                    archiveFile.Extract(extractpath, true);
                }
                File.Delete(zipPath);
                SetVersion();
            } catch (System.Exception err)
            {
                MainWindow.CurrentWindow.GetError(err.Message);
            }
        }

        public static bool CheckVersion()
        {
            string json;
            if (!File.Exists(Modpack.PATH))
            {
                json = JsonConvert.SerializeObject(Pack, Formatting.Indented);
                File.WriteAllText(Modpack.PATH, json);
            }

            json = File.ReadAllText(Modpack.PATH);
            Pack = JsonConvert.DeserializeObject<Modpack>(json);
            if (FileServer.isConnected)
            {
                if (Pack.Version == FileServer.Pack.Version)
                    return true;
            }
            return false;
        }

        public static bool SetVersion()
        {
            try
            {
                string json;
                json = JsonConvert.SerializeObject(FileServer.Pack, Formatting.Indented);
                File.WriteAllText(Modpack.PATH, json);

                json = File.ReadAllText(Modpack.PATH);
                Pack = JsonConvert.DeserializeObject<Modpack>(json);
                return true;

            } catch (System.Exception)
            {
                return false;
            }
        }
    }
}