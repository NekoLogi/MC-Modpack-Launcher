using Newtonsoft.Json;
using System.IO;

namespace ModLauncher
{
    public class Settings
    {
        public const string PATH = "Settings.json";
        public string Address = "127.0.0.1";
        public int Port = 5001;
        public int Ram = 4000;

        public bool Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(PATH, json);
                return true;
            } catch (System.Exception)
            {
                return false;
            }
        }

        public bool Load()
        {
            try
            {
                if (!File.Exists(PATH))
                    return false;
                string json = File.ReadAllText(PATH);
                Settings settings = JsonConvert.DeserializeObject<Settings>(json);
                Ram = settings.Ram;
                Address = settings.Address;
                Port = settings.Port;
                return true;
            } catch (System.Exception)
            {
                return false;
            }
        }
    }
}
