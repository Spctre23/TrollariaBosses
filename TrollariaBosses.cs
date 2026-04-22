using TerrariaApi.Server;
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

        public Configuration Config = Configuration.Reload();
        public ChatCommands ChatCommands = new();
        public BossManager bossManager = new();

        public override void Initialize()
        {
            Instance = this;
            Handlers.InitializeHandlers(this);
            bossManager.Initialize();

            ChatCommands.RegisterCommands();

            Configuration.Reload();                            
            TShock.Log.ConsoleInfo("======= TrollariaBosses Plugin Initialized =======");
        }

        protected override void Dispose(bool disposing) 
        {
            if (disposing) 
            {
                Handlers.DisposeHandlers(this);
            }
        }
    }
}

