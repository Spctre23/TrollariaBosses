using Terraria;
using TShockAPI;

namespace TrollariaBosses;

public class ChatCommands
{
    public void RegisterCommands()
    {
        Commands.ChatCommands.Add(new Command("taddons.commands", HandleCommands, "ta"));
    }

    private void HandleCommands(CommandArgs args)
    {
        if (args.Parameters.Count == 0)
        {
            args.Player.SendMessage("Invalid syntax! Get subcommands with /taddons help.", 255, 0, 0);
            return;
        }

        switch (args.Parameters[0])
        {
            case "help":
                CommandHelp(args);
                break;
            case "reload":
                ReloadConfig(args);
                break;
            case "mechdusa":
                SpawnMechdusa(args);
                break;
            case "legendarymode":
                LegendaryMode(args);
                break;
            case "boss":
                SpawnCustomBoss(args);
                break;
            default:
                args.Player.SendMessage("Invalid syntax! Get subcommands with /taddons help.", 255, 0, 0);
                break;
        }
    }

    private void SpawnMechdusa(CommandArgs args)
    {
        TSPlayer player = TShock.Players[0];
        if (player == null) return;

        Main.getGoodWorld = true;
        Terraria.NPC.SpawnMechQueen(player.Index);
        Main.getGoodWorld = false;
    }

    private void LegendaryMode(CommandArgs args)
    {
        Main.getGoodWorld = !Main.getGoodWorld;

        args.Player.SendMessage($"Get Fixed Boi = {Main.getGoodWorld}.", 255, 255, 0);
    }

    private void SpawnCustomBoss(CommandArgs args)
    {
        int amount = (args.Parameters.Count > 2 && int.TryParse(args.Parameters[2], out int result) == true) ? result : 1;
        string bossName = args.Parameters[1];

        if (!TrollariaBosses.Instance.bossManager.TrySpawnBoss(bossName, args.Player.LastNetPosition, amount))
            args.Player.SendMessage($"Unknown boss '{bossName}'.", 255, 0, 0);
    }

    private void CommandHelp(CommandArgs args)
    {
        args.Player.SendMessage("TrollariaAddons subcommands:" +
            "\n- help" +
            "\n- mechdusa", 255, 255, 0);
    }

    private void ReloadConfig(CommandArgs args)
    {
        Configuration.Reload();
        args.Player.SendMessage("Reloaded TrollariaAddons configuration.", 0, 255, 0);
    }
}
