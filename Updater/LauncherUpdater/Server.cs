using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace LauncherUpdater
{
    internal class Server
    {
        public static string? VERSION_SERVER { get; set; }
        public static bool IS_CONNECTED { get; set; }

        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private string[] STATUS_TEXT = new string[] { "VER", "NEW", "UPD", "C-VER", "C-UPD" };
        private string LAUNCHER_NAME = "launcher.7z";

        public enum Status
        {
            Version = 3,
            Update
        }


        public bool Start(Status status)
        {
            if (!Connect())
                return false;

            SendRequest(status);

            switch (status)
            {
                case Status.Version:
                    VERSION_SERVER = GetServerVersion();
                    break;

                case Status.Update:
                    UpdateClient();
                    break;
            }

            Disconnect();

            return true;
        }

        // Get server version of client launcher.
        private string GetServerVersion()
        {
            byte[] formatted = ReceiveData();
            return Encoding.ASCII.GetString(formatted);
        }

        // Connect to server.
        private bool Connect()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(Updater.G_Settings.Address), Updater.G_Settings.Port);
                socket.Connect(endPoint);

                IS_CONNECTED = true;
                return true;
            } catch (Exception)
            {
                Disconnect();
                return false;
            }
        }

        // Disconnect from server.
        private void Disconnect()
        {
            socket.Close();
        }

        // Receive incoming stream from server.
        private byte[] ReceiveData()
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

        // Download new client launcher from server.
        private void UpdateClient()
        {
            if (!File.Exists("Cache/" + LAUNCHER_NAME))
            {
                byte[] formatted = ReceiveData();
                while (formatted.Length != 0)
                {
                    using (var stream = new FileStream("Cache/" + LAUNCHER_NAME, FileMode.Append))
                    {
                        stream.Write(formatted, 0, formatted.Length);
                    }
                    formatted = ReceiveData();
                }
            }
            else
            {
                MessageBox.Show("File already exists!");
            }
        }

        // Send request to server.
        private void SendRequest(Status status)
        {
            byte[] data = Encoding.ASCII.GetBytes(STATUS_TEXT[(int)status]);
            socket.Send(data);
        }
    }
}
