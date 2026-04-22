using Newtonsoft.Json;
using TShockAPI;

namespace TrollariaBosses;

public class Configuration
{
    public static string ConfigPath = Path.Combine(TShock.SavePath, "TrollariaBosses.json");

    public static Configuration Reload()
    {
        Configuration? c = null;

        if (File.Exists(ConfigPath))
        {
            c = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(ConfigPath));
        }

        if (c == null)
        {
            c = new Configuration();
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(c, Formatting.Indented));
        }
        
        return c;
    }
    
    public void Write()
    {
        File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
