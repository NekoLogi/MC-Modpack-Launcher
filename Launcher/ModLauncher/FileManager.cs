using Newtonsoft.Json;
using SevenZipExtractor;
using System.IO;
using System.Windows;

namespace ModLauncher
{
    static class FileManager
    {
        public static Modpack Pack = new Modpack();
        private static string extractPath = $"Launcher/";
        private static string cache = $"Cache/";


        public static void ModInstall()
        {
            try
            {
                string zipPath = $"Cache/{FileServer.Pack.Name.ToLower()}.7z";

                using (ArchiveFile archiveFile = new ArchiveFile(zipPath))
                {
                    archiveFile.Extract(extractPath, true);
                }
                File.Delete(zipPath);
                SetVersion();
            } catch (System.Exception err)
            {
                MainWindow.CurrentWindow.GetError(err.Message);
            }
        }

        public static void ModUpdate()
        {
            const char separator = '^';
            try
            {
                string zipPath = $"Cache/{FileServer.Pack.Name.ToLower()}.7z";
                if (!File.Exists(zipPath))
                {
                    MainWindow.CurrentWindow.GetError("Modpack was not found, after downloading from server!");
                    return;
                }

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
                            MainWindow.CurrentWindow.ChangeInstallStatus($"Adding {steps[2].Split('/')[1]}...");
                            if (!AddChange(steps[1], steps[2]))
                                failedChanges += $"- Failed to add: {(steps[1] == "DIR" ? "DIRECTORY" : "FILE")} {steps[2]}\n";
                            break;
                        case "DEL":
                            MainWindow.CurrentWindow.ChangeInstallStatus($"Removing {steps[2].Split('/')[1]}...");
                            if (!RemoveChange(steps[1], steps[2]))
                                failedChanges += $"- Failed to remove: {(steps[1] == "DIR" ? "DIRECTORY" : "FILE")} {steps[2]}\n";
                            break;
                        case "MOD":
                            MainWindow.CurrentWindow.ChangeInstallStatus($"Changing {steps[2].Split('/')[1]}...");
                            if ((!RemoveChange(steps[1], steps[2])) || !AddChange(steps[1], steps[2]))
                                failedChanges += $"- Failed to modify: {(steps[1] == "DIR" ? "DIRECTORY" : "FILE")} {steps[2]} \n";
                            break;
                    }
                }
                if (failedChanges != "")
                    MessageBox.Show("Failed Changes:\n" + failedChanges);
                File.Delete(zipPath);
                Directory.Delete(cache + FileServer.Pack.Name, true);
                SetVersion();
            } catch (System.Exception err)
            {
                MainWindow.CurrentWindow.GetError(err.Message);
            }
        }

        private static bool RemoveChange(string format, string path)
        {
            try
            {
                switch (format)
                {
                    case "DIR":
                        if (!Directory.Exists(extractPath + $"{FileServer.Pack.Name}/" + path))
                            return false;
                        Directory.Delete(extractPath + $"{FileServer.Pack.Name}/" + path, true);
                        break;
                    case "FILE":
                        if (!File.Exists(extractPath + $"{FileServer.Pack.Name}/" + path))
                            return false;
                        File.Delete(extractPath + $"{FileServer.Pack.Name}/" + path);
                        break;
                }
                return true;
            } catch (System.Exception err) { MainWindow.CurrentWindow.GetError(err.Message); }

            return false;
        }

        private static bool AddChange(string format, string path)
        {
            try
            {

                switch (format)
                {
                    case "DIR":
                        if (Directory.Exists(extractPath + $"{FileServer.Pack.Name}/" + path))
                            return false;
                        Directory.Move(cache + $"{FileServer.Pack.Name}/" + path, extractPath + $"{FileServer.Pack.Name}/" + path);
                        break;
                    case "FILE":
                        if (File.Exists(extractPath + $"{FileServer.Pack.Name}/" + path))
                            return false;
                        File.Move(cache + $"{FileServer.Pack.Name}/" + path, extractPath + $"{FileServer.Pack.Name}/" + path);
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