using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Auth.Microsoft;
using CmlLib.Core.ProcessBuilder;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ModLauncher
{
    static class MC_Game
    {
        static MainWindow main = new MainWindow();
        static MSession SESSION;

        public static void XBoxLogin()
        {
            var loginHandler = JELoginHandlerBuilder.BuildDefault();
            var session = loginHandler.Authenticate().Result;
            if (session != null)
            {
                SESSION = session;
                _ = LaunchGameAsync();
            }
        }

        public static void XBoxLogout()
        {
            var loginHandler = JELoginHandlerBuilder.BuildDefault();
            loginHandler.Signout();
        }

        public static async Task LaunchGameAsync()
        {
            try
            {
                var path = new MinecraftPath(Directory.GetCurrentDirectory() + $@"\Launcher\{FileManager.Pack.Name}");
                var launcher = new MinecraftLauncher(path);


                // more options : https://github.com/AlphaBs/CmlLib.Core/wiki/MLaunchOption
                var launchOption = new MLaunchOption
                {
                    MaximumRamMb = SettingsWindow.G_Settings.Ram,
                    Session = SESSION
                };

                string jar = File.ReadAllText($"Launcher/{FileManager.Pack.Name}/Forge Version.txt");
                var process = await launcher.CreateProcessAsync(jar, launchOption);
                process.Start();
            } catch (Exception)
            {
                main.GetError($"Failed Launching {FileManager.Pack.DisplayName}!");
            }
        }
    }
}
