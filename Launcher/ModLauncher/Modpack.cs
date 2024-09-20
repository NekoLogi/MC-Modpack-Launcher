namespace ModLauncher
{
    public class Modpack
    {
        public const string PATH = "Launcher/modpack.json";
        public string DisplayName = "Kein Name";
        public string Name = "Kein Modpack";
        public string Description = "Server Verbindung wird benötigt!";
        public string Version = "N/a";
        public string Log = "";
        public string Hash = "";
        public string[] Actions = null;

        public Modpack()
        {
        }

        public Modpack(string displayName, string name, string description, string version, string log, string[] actions)
        {
            DisplayName = displayName;
            Name = name;
            Description = description;
            Version = version;
            Log = log;
            Actions = actions;
        }
    }
}
