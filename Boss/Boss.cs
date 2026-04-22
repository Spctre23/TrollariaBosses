using Microsoft.Xna.Framework;
using System.Text;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using TrollariaBosses.Helper;
using TShockAPI;
using static Terraria.NetMessage;

namespace TrollariaBosses.Boss;

public class Boss(string name, int bossNpcId, int minionNpcId, int damageMultiplier, int summonItem, string[] origBossNames)
{
    private readonly string name = name;
    private readonly int bossNpcId = bossNpcId;
    private int bossWhoAmI = -1;
    private readonly HashSet<int> minionWhoAmIs = [];
    private readonly string[] origBossNames = origBossNames;
    private readonly Color bossMessageColor = new(175, 75, 255);
    private bool alive;
    private bool hasAnnounced;

    public event Action<Boss>? OnDeath;
    protected Player? player;

    public void Initialize()
    {
        On.Terraria.NPC.AI += BossAI;
        On.Terraria.NPC.CheckActive += NPC_CheckActive;
        On.Terraria.NPC.checkDead += NPC_checkDead;
        On.Terraria.Projectile.NewProjectile_IEntitySource_Vector2_Vector2_int_int_float_int_float_float_float_NewProjectileModifier += Projectile_NewProjectile;
        On.Terraria.Chat.ChatHelper.BroadcastChatMessage += ChatHelper_BroadcastChatMessage;
    }

    public void Dispose()
    {
        On.Terraria.NPC.AI -= BossAI;
        On.Terraria.NPC.CheckActive -= NPC_CheckActive;
        On.Terraria.NPC.checkDead -= NPC_checkDead;
        On.Terraria.Projectile.NewProjectile_IEntitySource_Vector2_Vector2_int_int_float_int_float_float_float_NewProjectileModifier -= Projectile_NewProjectile;
        On.Terraria.Chat.ChatHelper.BroadcastChatMessage -= ChatHelper_BroadcastChatMessage;
    }

    private void BossAI(On.Terraria.NPC.orig_AI orig, NPC npc)
    {
        if (!npc.active || !alive)
        {
            orig(npc);
            return;
        }

        if (npc.whoAmI == bossWhoAmI)
        {
            int target = Player.FindClosest(npc.Center, npc.width, npc.height);
            player = Main.player[target];
            NPC_AI(npc);
        }
        else if (minionWhoAmIs.Contains(npc.whoAmI))
        {
            if (player == null) return;
            MinionAI(npc);
        }
        else orig(npc);
    }

    protected virtual void NPC_AI(NPC boss) { }

    protected virtual void MinionAI(NPC minion) { }

    private void NPC_CheckActive(On.Terraria.NPC.orig_CheckActive orig, NPC npc)
    {
        orig(npc);

        if (!npc.active) HandleBossRemoval(npc);
    }

    private void NPC_checkDead(On.Terraria.NPC.orig_checkDead orig, NPC npc)
    {
        if (npc.life <= 0) HandleBossRemoval(npc);

        orig(npc);
    }

    private void HandleBossRemoval(NPC npc)
    {
        if (npc.whoAmI == bossWhoAmI && alive)
        {
            ClearMinions();
            alive = false;
            bossWhoAmI = -1;
            OnDeath?.Invoke(this);

            TShock.Utils.Broadcast($"{name} has been defeated!", bossMessageColor);
        }
    }

    protected virtual void ClearMinions() { }

    private void ChatHelper_BroadcastChatMessage(On.Terraria.Chat.ChatHelper.orig_BroadcastChatMessage orig, NetworkText text, Color color, int excludedPlayer)
    {
        string msg = text.ToString();

        foreach (string origName in origBossNames)
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
        int id = NPC.NewNPC(new EntitySource_Sync(), (int)pos.X + 1000, (int)pos.Y, bossNpcId);
        bossWhoAmI = id;

        TShock.Utils.Broadcast($"{name} has awoken!", bossMessageColor);
    }


    protected void SpawnProjectile(NPC npc, Vector2 pos, Vector2 velocity, int type, int damage, float knockback, int count = 1, int spread = 0, int repeatSpacingX = 0, int repeatSpacingY = 0)
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

            Projectile.NewProjectile(new EntitySource_BossSpawn(npc),
                pos - new Vector2(repeatSpacingX * centeredOffset, repeatSpacingY * centeredOffset),
                newVelocity,
                type,
                damage,
                knockback);
        }
    }

    private int Projectile_NewProjectile(On.Terraria.Projectile.orig_NewProjectile_IEntitySource_Vector2_Vector2_int_int_float_int_float_float_float_NewProjectileModifier orig, IEntitySource spawnSource, Vector2 position, Vector2 velocity, int Type, int Damage, float KnockBack, int Owner, float ai0, float ai1, float ai2, NewProjectileModifier modifier)
    {
        int index = orig(spawnSource, position, velocity, Type, Damage, KnockBack, Owner, ai0, ai1, ai2, modifier);

        if (index >= 0 && index < 1000 && Main.projectile[index].active)
        {
            Main.projectile[index].scale = ai0;
        }

        return index;
    }

    protected async void ProjectileBurst(NPC npc, Vector2 velocity, int projectile, int damage, float knockback, int repeatCount, int delay, int range)
    {
        for (int i = 0; i < repeatCount; i++)
        {
            int target = Player.FindClosest(npc.Center, npc.width, npc.height);
            Player player = Main.player[target];
            Vector2 pos = VectorUtils.GetRandomVectorWithinRange(player.Center, range);

            Main.QueueMainThreadAction(() =>
            {
                if (npc.active)
                    SpawnProjectile(npc, pos, velocity, projectile, damage, knockback);
            });

            await Task.Delay(delay);
        }
    }

    protected void SetVelocity(NPC npc, Player player, float speed, float decelRange)
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

    protected void PlaySound(Vector2 pos, ushort id, int style, float volume, float pitch)
    {
        NetMessage.PlayNetSound(new NetSoundInfo(pos, id, style, volume, pitch), -1, -1);
    }
}
