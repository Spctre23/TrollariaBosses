using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TrollariaBosses.Boss;
using TrollariaBosses.Boss.Bosses;
using TShockAPI;

public class BossManager
{
    private readonly List<Boss> activeBosses = [];

    private readonly Dictionary<string, Func<Boss>> bossRegistry = new()
    {
        { "boss1", () => new Boss1() },
        { "boss2", () => new Boss2() }
    };

    private readonly Dictionary<int, Func<Boss>> summonItemRegistry = new()
    {
        { ItemID.SpicyPepper, () => new Boss1() },
        { ItemID.Honeyfin,    () => new Boss2() }
    };

    public void Initialize()
    {
        On.Terraria.Player.ItemCheck += Player_ItemCheck;
    }

    public void Dispose()
    {
        On.Terraria.Player.ItemCheck -= Player_ItemCheck;

        foreach (Boss boss in activeBosses)
            boss.Dispose();

        activeBosses.Clear();
    }

    private void Player_ItemCheck(On.Terraria.Player.orig_ItemCheck orig, Player self)
    {
        Item item = self.inventory[self.selectedItem];

        if (self.controlUseItem && summonItemRegistry.TryGetValue(item.type, out Func<Boss>? factory))
        {
            bool alreadyActive = activeBosses.Any(b => b.GetType() == factory().GetType());

            if (!alreadyActive) 
                SpawnBoss(factory, self.Center);
        }

        orig(self);
    }

    public bool TrySpawnBoss(string name, Vector2 pos, int amount)
    {
        if (!bossRegistry.TryGetValue(name.ToLower(), out Func<Boss>? factory))
            return false;

        for (int i = 0; i < amount; i++)
            SpawnBoss(factory, pos);

        return true;
    }

    private void SpawnBoss(Func<Boss> factory, Vector2 pos)
    {
        Boss boss = factory();
        boss.OnDeath += HandleDeath;
        boss.Initialize();
        boss.SpawnBoss(pos);
        activeBosses.Add(boss);
    }

    private void HandleDeath(Boss boss)
    {
        activeBosses.Remove(boss);
        Task.Run(async () =>
        {
            await Task.Delay(1);
            boss.Dispose();
        });
    }
}