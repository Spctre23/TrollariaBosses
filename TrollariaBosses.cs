using TerrariaApi.Server;
using TrollariaBosses.Boss;
using TShockAPI;

namespace TrollariaBosses
{
    [ApiVersion(2, 1)]
    public class TrollariaBosses(Terraria.Main game) : TerrariaPlugin(game)
    {
        public override string Name => "TrollariaBosses";
        public override string Author => "Spctre";
        public override string Description => "Custom bosses for the Trollaria server";
        public override Version Version => new(1, 0, 0);
        public static TrollariaBosses Instance { get; private set; }

        public ChatCommands commands = new();
        public BossManager bossManager = new();

        public override void Initialize()
        {
            Instance = this;
            bossManager.Initialize();
            commands.RegisterCommands();
            
            TShock.Log.ConsoleInfo("======= TrollariaBosses Plugin Initialized =======");
        }

        protected override void Dispose(bool disposing) 
        {
            if (disposing) 
            {
            }
        }
    }
}

