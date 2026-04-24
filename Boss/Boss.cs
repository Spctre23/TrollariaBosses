using Google.Protobuf.WellKnownTypes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using TrollariaBosses.Helper;
using TShockAPI;

namespace TrollariaBosses.Boss;

public class Boss(string name, int bossNpcType, int defense, int damage, HashSet<int> minionNpcTypes, HashSet<string> namesToReplace)
{
    private int bossId = -1;
    protected HashSet<int> minionIds = [];
    private readonly HashSet<int> projectileIds = [];
    private readonly Color bossMessageColor = new(175, 75, 255);
    private bool alive;
    private bool hasAnnounced;
    protected Player? player;

    public event Action<Boss>? OnDeath;

    public void Initialize()
    {
        On.Terraria.NPC.AI += NPC_AI;
        On.Terraria.NPC.CheckActive += NPC_CheckActive;
        On.Terraria.NPC.checkDead += NPC_checkDead;
        On.Terraria.Projectile.AI += Projectile_AI;
        On.Terraria.Chat.ChatHelper.BroadcastChatMessage += ChatHelper_BroadcastChatMessage;
    }

    public void Dispose()
    {
        On.Terraria.NPC.AI -= NPC_AI;
        On.Terraria.NPC.CheckActive -= NPC_CheckActive;
        On.Terraria.NPC.checkDead -= NPC_checkDead;
        On.Terraria.Projectile.AI -= Projectile_AI;
        On.Terraria.Chat.ChatHelper.BroadcastChatMessage -= ChatHelper_BroadcastChatMessage;
    }

    private void NPC_AI(On.Terraria.NPC.orig_AI orig, NPC npc)
    {
        if (!npc.active || !alive)
        {
            orig(npc);
            return;
        }

        if (npc.whoAmI == bossId && npc.type == bossNpcType)
        {
            npc.defense = defense;
            npc.localAI[0]++;

            int target = Player.FindClosest(npc.Center, npc.width, npc.height);
            player = Main.player[target];
            if (player != null)
            {
                BossAI(npc);
                HandleBossDamage(npc);
                Sync(npc);
            }
        }
        else if (minionIds.Contains(npc.whoAmI))
        {
            if (!minionNpcTypes.Contains(npc.type))
            {
                minionIds.Remove(npc.whoAmI);
                orig(npc);
                return;
            }

            MinionAI(npc);
            Sync(npc);
        }
        else orig(npc);
    }

    protected virtual void BossAI(NPC boss) { }

    protected virtual void MinionAI(NPC minion) { }

    private void NPC_CheckActive(On.Terraria.NPC.orig_CheckActive orig, NPC npc)
    {
        orig(npc);

        if (!npc.active) HandleDeath(npc);
    }

    private void NPC_checkDead(On.Terraria.NPC.orig_checkDead orig, NPC npc)
    {
        if (npc.life <= 0) HandleDeath(npc);

        orig(npc);
    }

    private void HandleDeath(NPC npc)
    {
        if (npc.whoAmI == bossId && alive)
        {
            ClearMinions();
            projectileIds.Clear();
            alive = false;
            bossId = -1;
            OnDeath?.Invoke(this);

            TShock.Utils.Broadcast($"{name} has been defeated!", bossMessageColor);
        }
        else minionIds.Remove(npc.whoAmI);
    }

    private void ClearMinions()
    {
        foreach (int npcId in minionIds)
        {
            TSPlayer.Server.StrikeNPC(npcId, 1000000, 0, 0);
        }
    }

    private void ChatHelper_BroadcastChatMessage(On.Terraria.Chat.ChatHelper.orig_BroadcastChatMessage orig, NetworkText text, Color color, int excludedPlayer)
    {
        string msg = text.ToString();

        foreach (string origName in namesToReplace)
        {
            if (msg.Contains(origName))
            {
                if (ContainsBossStatus(msg)) return;
                text._text = msg.Replace(origName, name);
            }
        }

        if (!hasAnnounced)
        {
            string status = GetBossStatus(msg);
            text._text = $"{name} {status}";       
            hasAnnounced = true;
        }

        orig(text, color, excludedPlayer);
    }

    private bool ContainsBossStatus(string msg)
    {
        return msg.Contains("awoken") || msg.Contains("been defeated!");
    }

    private string GetBossStatus(string msg)
    {
        return msg.Contains("awoken") ? "has awoken!" : "has been defeated!";
    }

    public void SpawnBoss(Vector2 pos)
    {
        alive = true;
        hasAnnounced = false;
        int id = NPC.NewNPC(new EntitySource_Sync(), (int)pos.X + 1000, (int)pos.Y, bossNpcType);
        bossId = id;

        TShock.Utils.Broadcast($"{name} has awoken!", bossMessageColor);
    }

    private void HandleBossDamage(NPC boss)
    {
        for (int i = 0; i < Main.player.Length; i++)
        {
            Player plr = Main.player[i];
            if (!plr.active || plr.dead) continue;

            if (boss.Hitbox.Intersects(plr.Hitbox))
            {
                TSPlayer tsPlayer = TShock.Players[i];
                tsPlayer.DamagePlayer(damage, PlayerDeathReason.ByNPC(boss.whoAmI));
            }
        }
    }

    public static void SetVelocity(NPC npc, Player player, float speed, float decelRange)
    {
        Vector2 direction = player.Center - npc.Center;
        float playerDistance = direction.Length();

        if (playerDistance <= decelRange)
        {
            speed *= playerDistance / decelRange;
        }

        Vector2 velocity = Vector2.Normalize(direction) * speed;
        npc.velocity = velocity;
    }

    private void Projectile_AI(On.Terraria.Projectile.orig_AI orig, Projectile self)
    {
        orig(self);

        HandleProjectileDamage(self);
    }

    protected void SpawnProjectile(NPC npc, Vector2 pos, Vector2 velocity, int type, int damage, float knockback, int count = 1, int spread = 0, int repeatSpacingX = 0, int repeatSpacingY = 0, int lifespan = 5000)
    {
        float step = 0;
        float start = 0;
        float speed = 0;

        if (velocity.Length() > 0)
        {
            speed = velocity.Length();
            if (speed <= 0.001f) return;

            velocity /= speed;
            step = MathHelper.ToRadians(spread) / Math.Max(1, count - 1);
            start = -0.5f * (count - 1) * step;
        }

        for (int i = 0; i < count; i++)
        {
            float angle = start + step * i;
            Vector2 newVelocity = velocity.Length() > 0 ? velocity.RotatedBy(angle) * speed : velocity;
            float centeredOffset = i - (count - 1) / 2f;
            Vector2 offset = new(repeatSpacingX * centeredOffset, repeatSpacingY * centeredOffset);

            int index = Projectile.NewProjectile(new EntitySource_BossSpawn(npc),
                pos - offset,
                newVelocity,
                type,
                damage,
                knockback);

            Projectile proj = Main.projectile[index];
            if (index < 1000 && proj.active)
            {
                proj.timeLeft = lifespan;
                proj.friendly = false;
                proj.hostile = false;
                proj.npcProj = false;
                projectileIds.Add(index);
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
            }
        }
    }

    protected async void ProjectileBurst(NPC npc, int repeatCount, int delay, Action action)
    {
        for (int i = 0; i < repeatCount; i++)
        {
            Main.QueueMainThreadAction(() =>
            {
                if (!npc.active) return;
                action.Invoke();
            });

            await Task.Delay(delay);
        }
    }

    protected async void ProjectileBurst(NPC npc, Vector2 velocity, Vector2 pos, int type, int damage, float knockback, int repeatCount, int delay)
    {
        ProjectileBurst(npc, repeatCount, delay, () => SpawnProjectile(npc, pos, velocity, type, damage, knockback));
    }

    protected async void ProjectileBurst(NPC npc, int type, int damage, float knockback, int repeatCount, int delay, int range)
    {
        ProjectileBurst(npc, repeatCount, delay, () =>
        {
            Vector2 pos = VectorUtils.GetRandomVectorWithinRange(player.Center, range);
            Vector2 velocity = Vector2.Normalize(player.Center - pos);
            SpawnProjectile(npc, pos, velocity, type, damage, knockback);
        });
    }

    private void HandleProjectileDamage(Projectile proj)
    {
        if (!projectileIds.Contains(proj.whoAmI)) return;

        for (int i = 0; i < Main.player.Length; i++)
        {
            Player plr = Main.player[i];
            if (!plr.active || plr.dead) continue;

            if (proj.Hitbox.Intersects(plr.Hitbox))
            {
                TSPlayer tsPlayer = TShock.Players[i];
                tsPlayer.DamagePlayer(proj.damage, PlayerDeathReason.ByProjectile(plr.whoAmI, proj.whoAmI));
                proj.active = false;
                projectileIds.Remove(proj.whoAmI);
            }
        }
    }

    protected void PlaySound(Vector2 pos, ushort id, int style, float volume, float pitch)
    {
        NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(pos, id, style, volume, pitch), -1, -1);
    }

    private void Sync(NPC npc)
    {
        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
    }
}
