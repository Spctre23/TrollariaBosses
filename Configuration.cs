using Newtonsoft.Json;
using Terraria;
using TShockAPI;

namespace TrollariaAddons
{
    public class Configuration
    {
        public static string ConfigPath = Path.Combine(TShock.SavePath, "trollariaguard.json");

        public string ____________TILE_BAN_____________ = "";
        public Dictionary<int, int> BannedTiles { get; set; } = new();
        public bool BroadcastTileDebugInfo = false;
        public bool BannedTilePlacementWarnings = true;
        public bool LogBannedTilePlacements = true;

        public string ____________ANTI_SPAM_____________ = "";

        public bool DisableBossMessages = false;
        public bool DisableOrbMessages = false;

        public string SpamPenalty = "ignore";
        public double CapsRatio = 0.66;
        public double CapsWeight = 2.0;
        public double NormalWeight = 1.0;
        public int ShortLength = 4;
        public double ShortWeight = 1.5;

        public double Threshold = 5;
        public double Time = 1.5;

        public bool LimitUnicodeCharactersInChat = true;
        public int ChatUnicodeCharacterLimit = 60;

        public string ____________ANTI_BOTS_____________ = "";

        public bool LimitConnectionsPerIp = false;
        public double MaxConnectionsWithSameIP = 3;

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
}
