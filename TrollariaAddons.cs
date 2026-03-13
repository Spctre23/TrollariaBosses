using IL.OTAPI;
using IL.Terraria.ID;
using IL.Terraria.Modules;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Drawing;
using System.IO.Streams;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Map;
using TerrariaApi.Server;
using TrollariaAddons.Addons;
using TrollariaAddons.Addons.Bosses;
using TShockAPI;
using TShockAPI.Configuration;
using TShockAPI.DB;
using TShockAPI.Hooks;

namespace TrollariaAddons
{
    [ApiVersion(2, 1)]
    public class TrollariaAddons : TerrariaPlugin
    {
        public override string Name => "TrollariaAddons";
        public override string Author => "Spctre";
        public override string Description => "Trollaria server addons";
        public override Version Version => new Version(1, 0, 0);
        public TrollariaAddons(Terraria.Main game) : base(game) { }

        public static Configuration Config = Configuration.Reload();
        public static ChatCommands ChatCommands = new();
        public static DatabaseManager DBManager = new DatabaseManager(new SqliteConnection("Data Source=" + DatabaseManager.DatabasePath));
        public static Boss boss = new();

        public override void Initialize()
        {
            Handlers.InitializeHandlers(this);
            boss.Initialize();

            ChatCommands.RegisterCommands();

            Configuration.Reload();                            
            TShock.Log.ConsoleInfo("======= TrollariaAddons Plugin Initialized =======");
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                Handlers.DisposeHandlers(this);
                boss.Dispose();
            }
        }
    }
}

