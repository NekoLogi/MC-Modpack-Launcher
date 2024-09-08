using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace ModLauncher
{
    class FileServer
    {
        public static Modpack Pack;
        public static bool isConnected = true;
        static string lastError;
        static Socket socket;

        public static void Connect(int index)
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(SettingsWindow.G_Settings.Address), SettingsWindow.G_Settings.Port);
                socket.Connect(endPoint);
                HandleData(index);
            } catch (Exception e)
            {
                if (lastError != e.Message || lastError == null)
                {
                    lastError = $"Connection failed: {e.Message}";
                    MainWindow.CurrentWindow.GetError($"Connection failed: {e.Message}");
                }
                isConnected = false;
            }
            socket.Close();
        }

        static void HandleData(int index)
        {
            try
            {
                string[] input = { "VER", "NEW", "UPD" };
                byte[] data = Encoding.ASCII.GetBytes(input[index]);
                socket.Send(data);

                switch (input[index])
                {
                    case "VER":
                        GetVersion();
                        break;
                    case "NEW":
                        GetModpack();
                        break;
                    case "UPD":
                        GetUpdate();
                        break;
                }
            } catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                socket.Close();
            }
        }

        static byte[] ReceiveData()
        {
            byte[] buffer = new byte[socket.SendBufferSize];
            int bytesRead = socket.Receive(buffer);
            byte[] formatted = new byte[bytesRead];

            for (int i = 0; i < bytesRead; i++)
            {
                formatted[i] = buffer[i];
            }
            return formatted;
        }

        static void GetVersion()
        {
            byte[] formatted = ReceiveData();
            string data = Encoding.UTF8.GetString(formatted);
            Pack = JsonConvert.DeserializeObject<Modpack>(data);
        }

        static void GetModpack()
        {
            if (File.Exists($"Cache/{Pack.Name.ToLower()}.7z"))
                File.Delete($"Cache/{Pack.Name.ToLower()}.7z");
            if (Directory.Exists($"Launcher/{Pack.Name}"))
                Directory.Delete($"Launcher/{Pack.Name}", true);

            MainWindow.CurrentWindow.ChangeInstallStatus($"Downloading modpack: {Pack.Name}...");
            DownloadFile(0);
            MainWindow.CurrentWindow.ChangeInstallStatus($"Generating checksum...");
            GenerateHash($"Cache/{Pack.Name.ToLower()}.7z");
            if (FileManager.Pack.Hash != Pack.Hash)
            {
                MainWindow.CurrentWindow.GetError("Installation failed: checksum is invalid.");
                Environment.Exit(0);
            }
            MainWindow.CurrentWindow.ChangeInstallStatus($"Extracting files...");
            FileManager.ModInstall();
            MainWindow.CurrentWindow.ChangeInstallStatus($"Modpack is installed!");
        }

        private static void GenerateHash(string file)
        {
            SHA256 Sha256 = SHA256.Create();
            byte[] bytes;
            using (FileStream stream = File.OpenRead(file))
            {
                bytes = Sha256.ComputeHash(stream);
            }
            string result = "";
            foreach (byte b in bytes)
                result += b.ToString("x2");
            FileManager.Pack.Hash = result;
            string json = JsonConvert.SerializeObject(FileManager.Pack, Formatting.Indented);
            File.WriteAllText(Modpack.PATH, json);
        }

        static void GetUpdate()
        {
            DownloadFile(1);
            FileManager.ModUpdate();
        }

        static void DownloadFile(int index)
        {
            string[] fileNames = { $"{Pack.Name.ToLower()}.7z", $"{Pack.Name.ToLower()}_update.7z" };

            if (File.Exists($"Cache/" + fileNames[index]))
                return;

            byte[] formatted = ReceiveData();
            while (formatted.Length != 0)
            {
                using (var stream = new FileStream("Cache/" + fileNames[index], FileMode.Append))
                {
                    stream.Write(formatted, 0, formatted.Length);
                }
                formatted = ReceiveData();
            }
        }
    }
}
