using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MC_Launcher_Server
{
    internal class Server
    {
        private Socket? socket;
        private Modpack? Pack;


        public void Start()
        {
            while (Connect() == null)
                Thread.Sleep(3000);

            Listen();
        }

        private Socket? Connect()
        {
            try
            {
                Console.WriteLine("Starting server..");
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var endPoint = new IPEndPoint(IPAddress.Any, 5001);
                socket.Bind(endPoint);
                Console.WriteLine("Server started.");

                return socket;
            } catch (Exception)
            {
                Console.WriteLine("Failed to start server.");
                return null;
            }
        }

        private void Listen()
        {
            while (true)
            {
                Thread.Sleep(200);
                socket!.Listen(100);
                Task.Run(new Action(HandleTcpClient));
            }
        }

        private void HandleTcpClient()
        {
            if (!File.Exists(Modpack.PATH))
            {
                Console.WriteLine("modpack.json doesn't exist, edit the newly created one");
                File.WriteAllText(Modpack.PATH, JsonConvert.SerializeObject(new Modpack(), Formatting.Indented));
                Environment.Exit(0);
            }
            string json = File.ReadAllText(Modpack.PATH);
            Pack = JsonConvert.DeserializeObject<Modpack>(json);

            Socket client = socket!.Accept();
            Console.WriteLine("Connected with {0}", client.RemoteEndPoint);

            byte[] buffer = new byte[client.SendBufferSize];
            int length = client.Receive(buffer);
            byte[] bytes = new byte[length];
            for (int index = 0; index < length; index++)
                bytes[index] = buffer[index];

            string data = Encoding.ASCII.GetString(bytes);
            Console.WriteLine(string.Format("Command \"{0}\" from {1}", data, client.RemoteEndPoint));

            GetCommand(data, client);
        }

        private void GetCommand(string data, Socket client)
        {
            try
            {
                switch (data)
                {
                    case "VER":
                        GetVersion(client);
                        break;

                    case "NEW":
                        ModInstall(client);
                        break;

                    case "UPD":
                        ModUpdate(client);
                        break;

                    case "C-VER":
                        GetLauncherVersion(client);
                        break;

                    case "C-UPD":
                        LauncherUpdate(client);
                        break;
                }
            } catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }

        #region Commands
        private static void GetVersion(Socket client)
        {
            Console.WriteLine("{0} requested from {1}", Modpack.PATH, client.RemoteEndPoint);

            byte[] bytes = Encoding.UTF8.GetBytes(File.ReadAllText(Modpack.PATH));
            client.Send(bytes);
            Console.WriteLine("Data sent!");
        }

        private void ModUpdate(Socket client)
        {
            string path = $"Modpack/{Pack!.Name}/{Pack!.Name}.7z";
            Console.WriteLine("{0} requested from {1}", path, client.RemoteEndPoint);

            Console.WriteLine("Sending {0} update to {1}", Pack.Name, client.RemoteEndPoint);
            client.SendFile(path);
            Console.WriteLine("{0} send!", Pack.Name);

            client.Close();
        }

        private void ModInstall(Socket client)
        {
            string path = $"Modpack/{Pack!.Name}/{Pack!.Name}.7z";
            Console.WriteLine("{0} requested from {1}", path, client.RemoteEndPoint);

            Console.WriteLine("Sending {0} to {1}", Pack.Name, client.RemoteEndPoint);
            client.SendFile(path);
            Console.WriteLine("{0} send!", Pack.Name);

            client.Close();
        }

        private static void GetLauncherVersion(Socket client)
        {
            string path = "Launcher/Version.txt";
            Console.WriteLine("{0} requested from {1}", path, client.RemoteEndPoint);

            byte[] bytes = Encoding.ASCII.GetBytes(File.ReadAllText(path));
            client.Send(bytes);
            Console.WriteLine("Data sent!");
        }

        private static void LauncherUpdate(Socket client)
        {
            string path = $"Launcher/launcher.7z";
            Console.WriteLine("{0} requested from {1}", path, client.RemoteEndPoint);

            Console.WriteLine("Sending launcher update to {0}", client.RemoteEndPoint);
            client.SendFile(path);
            Console.WriteLine("launcher send!");

            client.Close();
        }

        #endregion
    }
}
