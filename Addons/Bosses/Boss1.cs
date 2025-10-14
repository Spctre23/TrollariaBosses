using Google.Protobuf.WellKnownTypes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using TrollariaAddons.Addons.Helpers;
using TShockAPI;

namespace TrollariaAddons.Addons.Bosses
{
    public class Boss1 : Boss
    {
        Player? player;
        public int damageMultiplier = 1000;

        public void BossAI(NPC boss)
        {
            boss.aiStyle = -1;
            boss.defense = 800;

            int target = Player.FindClosest(boss.Center, boss.width, boss.height);
            player = Main.player[target];

            SetVelocity(boss, player, 2.5f, 13, 200, 12, 0.25f);

            Vector2 projDir = player.Center - boss.Center;
            if (projDir.LengthSquared() > 1f) 
                projDir.Normalize();

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
                    SpawnProjectile(boss, pos, velocity, ProjectileID.CultistBossFireBall, 50, 30, 10, 0, 150, 0);
                }

                if (boss.localAI[0] % 40 == 0)
                {
                    SpawnProjectile(boss, projDir * 9f, ProjectileID.EyeBeam, 50, 30, 4, 20);
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

        public void PrimeMinionAI(NPC primeMinion)
        {
            if (player == null) return;

            SetVelocity(primeMinion, player, 2.5f, 14, 200, 12, 0.25f);

            primeMinion.netUpdate = true;
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, primeMinion.whoAmI);
        }

        public void ClearMinions()
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
}
