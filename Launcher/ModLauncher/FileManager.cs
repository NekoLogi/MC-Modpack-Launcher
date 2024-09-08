using Newtonsoft.Json;
using SevenZipExtractor;
using System.IO;
using System.Windows;

namespace ModLauncher
{
    static class FileManager
    {
        public static Modpack Pack = new Modpack();
        private static string extractpath = $"Launcher/{FileServer.Pack.Name}";
        private static string cache = $"Cache/{FileServer.Pack.Name}";


        public static void ModInstall()
        {
            try
            {
                string zipPath = $"Cache/{FileServer.Pack.Name.ToLower()}.7z";

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
            const char separator = '';
            try
            {
                if (!File.Exists($"Cache/{FileServer.Pack.Name.ToLower()}_update.7z"))
                {
                    MainWindow.CurrentWindow.GetError("Modpack was not found, after downloading from server!");
                    return;
                }
                string zipPath = $"Cache/{FileServer.Pack.Name.ToLower()}_update.7z";

                using (ArchiveFile archiveFile = new ArchiveFile(zipPath))
                {
                    archiveFile.Extract(cache, true);
                }
                string failedChanges = "";
                foreach (string item in FileServer.Pack.Actions)
                {
                    string[] steps = item.Split(separator);
                    switch (steps[0])
                    {
                        case "ADD":
                            if (!AddChange(steps[1], steps[2]))
                                failedChanges += $"- Failed to delete: {(steps[1] == "DIR" ? "DIRECTORY" : "FILE")} {steps[2]}\n";
                            break;
                        case "DEL":
                            if (!RemoveChange(steps[1], steps[2]))
                                failedChanges += $"- Failed to add: {(steps[1] == "DIR" ? "DIRECTORY" : "FILE")} {steps[2]}\n";
                            break;
                        case "MOD":
                            if ((!RemoveChange(steps[1], steps[2])) || AddChange(steps[1], steps[2]))
                                failedChanges += $"- Failed to modify: {(steps[1] == "DIR" ? "DIRECTORY" : "FILE")} {steps[2]} \n";
                            break;
                    }
                }
                if (failedChanges != "")
                    MessageBox.Show("Failed Changes:\n" + failedChanges);
                File.Delete(zipPath);
                Directory.Delete(cache, true);
                SetVersion();
            } catch (System.Exception err)
            {
                MainWindow.CurrentWindow.GetError(err.Message);
            }
        }

        private static bool RemoveChange(string format, string path)
        {
            if (!Directory.Exists(extractpath))
                return false;

            try
            {
                switch (format)
                {
                    case "DIR":
                        Directory.Delete(extractpath + "/" + path, true);
                        break;
                    case "FILE":
                        File.Delete(extractpath + "/" + path);
                        break;
                }
                return true;
            } catch (System.Exception err) { MainWindow.CurrentWindow.GetError(err.Message); }

            return false;
        }

        private static bool AddChange(string format, string path)
        {
            if (!Directory.Exists(extractpath))
                return false;

            try
            {
                switch (format)
                {
                    case "DIR":
                        Directory.Move(cache + "/" + path, extractpath + "/" + path);
                        break;
                    case "FILE":
                        File.Move(cache + "/" + path, extractpath + "/" + path);
                        break;
                }
                return true;
            } catch (System.Exception err) { MainWindow.CurrentWindow.GetError(err.Message); }

            return false;
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