namespace MC_Launcher_Server
{
    public class Modpack
    {
        public const string PATH = "modpack.json";
        public string DisplayName = "Guter Anzeigename";
        public string Name = "Gutes_Modpack";
        public string Description = "Das ist ein gutes Modpack!";
        public string Version = "N/a";
        public string Log = "";
        public string Hash = "";
        public string[] Actions = [
            "ADD&%FILE_WITH_PATH",
            "DEL&%FILE_WITH_PATH",
        ];
    }
}
