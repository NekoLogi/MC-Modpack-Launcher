namespace MC_Launcher_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Minecraft File-Server";

            var server = new Server();
            server.Start();
        }
    }
}
