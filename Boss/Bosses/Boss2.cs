using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TrollariaBosses.Helper;

namespace TrollariaBosses.Boss.Bosses;

public class Boss2 : Boss
{
    public Boss2() : base("Boss2", NPCID.SkeletronPrime, 300, 300, [NPCID.LunarTowerSolar], ["Skeletron Prime", "Lunar Tower Solar"]) { }

    protected override void BossAI(NPC boss)
    {
        SetVelocity(boss, player, 16, 200);

        if (boss.life >= (boss.lifeMax / 2))
        {
            if (boss.localAI[0] % 100 == 0)
            {
                Vector2 spawnPos = VectorUtils.GetRandomVectorWithinRange(player.Center, 1000);

                int minionId = NPC.NewNPC(new EntitySource_Sync(), (int)spawnPos.X, (int)spawnPos.Y, NPCID.LunarTowerSolar);
                NPC minion = Main.npc[minionId];
                minion.life = 100;

                minionIds.Add(minionId);
            }
        }
    }

    protected override void MinionAI(NPC minion)
    {
        Vector2 direction = Vector2.Normalize(player.Center - minion.Center);
        minion.SimpleFlyMovement(direction, 20);
    }
}
