using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TrollariaBosses.Helper;

namespace TrollariaBosses.Boss.Bosses;

public class Boss1 : Boss
{
    public Boss1() : base("Boss1", NPCID.Retinazer, 700, 300, [NPCID.DungeonGuardian, NPCID.SkeletronPrime], ["The Twins", "Retinazer", "Skeletron Prime"]) { }

    private int mainAttackInterval = 60;

    protected override void BossAI(NPC boss)
    {
        SetVelocity(boss, player, 13, 200);

        if (boss.life >= (boss.lifeMax / 2))
            FirstPhase(boss);
        else
            SecondPhase(boss);
    }

    private void MainAttack(NPC boss, int interval, int damage)
    {
        if (boss.localAI[0] % interval == 0)
        {
            Vector2 projVelocity = Vector2.Normalize(player.Center - boss.Center);
            SpawnProjectile(boss, boss.Center, projVelocity * 9, ProjectileID.DemonSickle, damage: damage, knockback: 30, count: 3, spread: 20, lifespan: 70);
        }
    }

    private void FirstPhase(NPC boss)
    {
        mainAttackInterval = MathUtils.ScaleRange(boss.lifeMax / 2, boss.lifeMax, 20, 60, boss.life);
        MainAttack(boss, mainAttackInterval, 25);

        if (boss.localAI[0] % 300 == 0)
        {
            Vector2 projVelocity = Vector2.Normalize(player.Center - boss.Center);
            SpawnProjectile(boss, boss.Center, projVelocity * 15, ProjectileID.EyeLaser, damage: 50, knockback: 30, count: 5, repeatSpacingY: 180);
        }

        if (boss.localAI[0] % 400 == 0)
        {
            Vector2 projSpawnPos = VectorUtils.GetRandomVectorWithinRange(player.Center, 1000);
            Vector2 projVelocity = Vector2.Normalize(player.Center - projSpawnPos);

            SpawnProjectile(boss, projSpawnPos, projVelocity * 20, ProjectileID.FlamingScythe, damage: 50, knockback: 30, count: 5, spread: 50);
        }

        if (boss.localAI[0] % 500 == 0)
        {
            Vector2 minionSpawnPos = VectorUtils.GetRandomVectorWithinRange(player.Center, 1000);

            int minionId = NPC.NewNPC(new EntitySource_Sync(), (int)minionSpawnPos.X, (int)minionSpawnPos.Y, NPCID.DungeonGuardian);
            NPC minion = Main.npc[minionId];
            minion.life = 100;

            minionIds.Add(minionId);
        }

        if (boss.localAI[0] % 700 == 0)
        {
            ProjectileBurst(boss, ProjectileID.CultistBossIceMist, damage: 30, knockback: 30, repeatCount: 5, delay: 500, range: 200);
        }
    }

    private void SecondPhase(NPC boss)
    {
        int direction = new Random().Next(0, 2) * 2 - 1;

        MainAttack(boss, 200, 50);

        if (boss.localAI[0] % 40 == 0)
        {
            Vector2 projSpawnPos = player.Center + new Vector2(-500 * direction, 0);
            Vector2 projVelocity = new(30 * direction, 0);

            SpawnProjectile(boss, projSpawnPos, projVelocity, ProjectileID.FlamingScythe, damage: 25, knockback: 30, count: 6, repeatSpacingY: 250);
        }

        if (boss.localAI[0] % 60 == 0)
        {
            Vector2 projSpawnPos = player.Center + new Vector2(0, -500 * direction);
            Vector2 projVelocity = new(0, 30 * direction);

            SpawnProjectile(boss, projSpawnPos, projVelocity, ProjectileID.CultistBossFireBall, damage: 20, knockback: 30, count: 13, repeatSpacingX: 250);
        }

        if (boss.localAI[0] % 30 == 0)
        {
            ProjectileBurst(boss, ProjectileID.StormLightning, damage: 200, knockback: 30, repeatCount: 16, delay: 100, range: 500);
        }

        if (boss.localAI[0] % 500 == 0)
        {
            Vector2 minionSpawnPos = VectorUtils.GetRandomVectorWithinRange(player.Center, 1000);
            int minionId = NPC.NewNPC(new EntitySource_Sync(), (int)minionSpawnPos.X, (int)minionSpawnPos.Y, NPCID.SkeletronPrime);

            minionIds.Add(minionId);
        }

        if (boss.localAI[0] % 600 == 0)
        {
            ProjectileBurst(boss, ProjectileID.CultistBossIceMist, damage: 75, knockback: 30, repeatCount: 10, delay: 200, range: 500);
        }
    }

    protected override void MinionAI(NPC minion)
    {
        SetVelocity(minion, player, 14, 200);
    }
}
