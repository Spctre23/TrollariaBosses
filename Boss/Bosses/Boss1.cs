using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TrollariaBosses.Helper;
using TShockAPI;

namespace TrollariaBosses.Boss.Bosses;

public class Boss1 : Boss
{
    public Boss1() : base("Boss1", NPCID.Retinazer, NPCID.DungeonGuardian, 1000, ItemID.SpicyPepper, ["The Twins", "Retinazer", "Skeletron Prime"]) { }

    protected override void NPC_AI(NPC boss)
    {
        boss.aiStyle = -1;
        boss.defense = 800;

        SetVelocity(boss, player, 13, 200);

        Vector2 projDir = Vector2.Normalize(player.Center - boss.Center);

        boss.localAI[0]++;

        if (boss.life >= (boss.lifeMax / 2))
        {
            Vector2 projSpeed = projDir * 9;

            if (boss.localAI[0] % 20 == 0)
            {
                SpawnProjectile(boss, boss.Center, projSpeed, ProjectileID.DemonSickle, 50, 30, 3, 20);
            }

            if (boss.localAI[0] % 300 == 0)
            {
                SpawnProjectile(boss, boss.Center, projSpeed, ProjectileID.EyeLaser, 50, 30, 5, 0, 0, 120);
            }

            if (boss.localAI[0] % 500 == 0)
            {
                Vector2 spawnPos = VectorUtils.GetRandomVectorWithinRange(player.Center, 1000);

                int minionId = NPC.NewNPC(new EntitySource_Sync(), (int)spawnPos.X, (int)spawnPos.Y, NPCID.DungeonGuardian);
                NPC minion = Main.npc[minionId];
                minion.life = 100;
            }

            if (boss.localAI[0] % 700 == 0)
            {
                ProjectileBurst(boss, Vector2.Zero, ProjectileID.CultistBossIceMist, 75, 30, 5, 500, 200);
            }
        }
        else
        {
            Vector2 velocity = new(0, 30);
            Vector2 pos = player.Center + new Vector2(0, -500);

            if (boss.localAI[0] % 60 == 0)
            {
                SpawnProjectile(boss, pos, velocity, ProjectileID.CultistBossFireBall, 50, 30, 13, 0, 150, 0);
            }

            if (boss.localAI[0] % 40 == 0)
            {
                SpawnProjectile(boss, boss.Center, projDir * 9f, ProjectileID.EyeBeam, 50, 30);
            }

            if (boss.localAI[0] % 500 == 0)
            {
                Vector2 spawnPos = VectorUtils.GetRandomVectorWithinRange(player.Center, 1000);

                NPC.NewNPC(new EntitySource_Sync(), (int)spawnPos.X, (int)spawnPos.Y, NPCID.SkeletronPrime);
            }

            if (boss.localAI[0] % 600 == 0)
            {
                ProjectileBurst(boss, Vector2.Zero, ProjectileID.CultistBossIceMist, 75, 30, 10, 200, 500);
            }
        }

        boss.netUpdate = true;
        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, boss.whoAmI);
    }

    protected override void MinionAI(NPC minion)
    {
        if (player == null) return;

        SetVelocity(minion, player, 14, 200);

        minion.netUpdate = true;
        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, minion.whoAmI);
    }

    protected override void ClearMinions()
    {
        for (int i = 0; i < Main.npc.Length; i++)
        {
            if (Main.npc[i].active && (Main.npc[i].type == NPCID.DungeonGuardian || Main.npc[i].type == NPCID.SkeletronPrime))
            {
                TSPlayer.Server.StrikeNPC(i, 1000000, 0, 0);
            }
        }
    }
}
