using Microsoft.Xna.Framework;
using System.Text;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using TrollariaAddons.Addons.Helpers;
using TShockAPI;
using static Terraria.NetMessage;

namespace TrollariaAddons.Addons.Bosses
{
    public class Boss
    {
        Color bossMessageColor = new (175, 75, 255);

        Boss1? boss1;
        Boss2? boss2;

        public void Initialize()
        {
            boss1 = new();
            boss2 = new();

            On.Terraria.NPC.AI += NPC_AI;
            On.Terraria.NPC.CheckActive += NPC_CheckActive;
            On.Terraria.NPC.checkDead += NPC_checkDead;
            On.Terraria.Player.ItemCheck += Player_ItemCheck;
            On.Terraria.Projectile.NewProjectile_IEntitySource_Vector2_Vector2_int_int_float_int_float_float_float_NewProjectileModifier += Projectile_NewProjectile;
            On.Terraria.Chat.ChatHelper.BroadcastChatMessage += ChatHelper_BroadcastChatMessage;
        }

        public void Dispose()
        {
            On.Terraria.NPC.AI -= NPC_AI;
            On.Terraria.NPC.CheckActive -= NPC_CheckActive;
            On.Terraria.NPC.checkDead -= NPC_checkDead;
            On.Terraria.Player.ItemCheck -= Player_ItemCheck;
            On.Terraria.Chat.ChatHelper.BroadcastChatMessage -= ChatHelper_BroadcastChatMessage;
        }

        public enum BossType
        {
            Boss1,
            Boss2
        }

        public void NPC_AI(On.Terraria.NPC.orig_AI orig, NPC npc)
        {
            if (!npc.active)
            {
                orig(npc);
                return;
            }

            if (boss1.alive)
            {
                if (npc.type == boss1.bossNpcId)
                {   
                    boss1.BossAI(npc);
                    return;
                }
                if (npc.type == boss1.minionNpcId)
                {
                    boss1.PrimeMinionAI(npc);
                    return;
                }
            }

            if (boss2.alive)
            {
                if (npc.type == boss2.bossNpcId)
                {
                    boss2.BossAI(npc);
                    return;
                }
                if (npc.type == boss2.minionNpcId)
                {
                    boss2.MinionAI(npc);
                    return;
                }
            }

            orig(npc);
        }

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
            if (npc.type == boss1.bossNpcId && boss1.alive)
            {
                boss1.ClearMinions();
                boss1.alive = false;
            }
            if (npc.type == boss2.bossNpcId && boss2.alive)
            {
                boss2.alive = false;
            }
        }

        private void ChatHelper_BroadcastChatMessage(On.Terraria.Chat.ChatHelper.orig_BroadcastChatMessage orig, NetworkText text, Color color, int excludedPlayer)
        {
            string msg = text.ToString();

            if (!boss1.hasAnnounced)
            {
                if (msg.Contains("The Twins") || msg.Contains("Retinazer"))
                {
                    string status = GetBossStatus(msg);
                    text._text = $"{BossType.Boss1} {status}";;

                    boss1.hasAnnounced = true;
                }
                else if (msg.Contains("Skeletron Prime")) return;
            }

            if (!boss2.hasAnnounced && msg.Contains("Skeletron Prime"))
            {
                string status = GetBossStatus(msg);
                text._text = $"{BossType.Boss2} {status}";

                boss2.hasAnnounced = true;
            }

            orig(text, color, excludedPlayer);
        }

        private string GetBossStatus(string msg)
        {
            return msg.Contains("awoken") ? "has awoken!" : "has been defeated!";
        }

        private void Player_ItemCheck(On.Terraria.Player.orig_ItemCheck orig, Player self)
        {
            Item item = self.inventory[self.selectedItem];

            if (self.controlUseItem)
            {
                switch (item.type)
                {
                    case ItemID.SpicyPepper:
                        if (!boss1.alive) SpawnBoss(self.Center, BossType.Boss1, 1);
                        break;
                    case ItemID.Honeyfin:
                        if (!boss2.alive) SpawnBoss(self.Center, BossType.Boss2, 1);
                        break;
                }
            }

            orig(self);
        }

        public void SpawnBoss(Vector2 pos, BossType type, int amount)
        {
            int npcID = 0;
            string spawnMessage = "";

            switch (type)
            {
                case BossType.Boss1:
                    npcID = boss1.bossNpcId;
                    spawnMessage = $"{BossType.Boss1} has awoken!";
                    boss1.alive = true;
                    boss1.hasAnnounced = false;
                    break;
                case BossType.Boss2:
                    npcID = boss2.bossNpcId;                  
                    spawnMessage = $"{BossType.Boss2} has awoken!";
                    boss2.alive = true;
                    boss2.hasAnnounced = false;
                    break;
            }

            for (int i = 0; i < amount; i++)
            {
                NPC.NewNPC(new EntitySource_Sync(), (int)pos.X + 1000, (int)pos.Y, npcID);

                TShock.Utils.Broadcast(spawnMessage, bossMessageColor);
            }
        }

        public void SpawnProjectile(NPC npc, Vector2 velocity, int type, int damage, float knockback)
        {
            SpawnProjectile(npc, npc.Center, velocity, type, damage, knockback, 1, 0, 0, 0);
        }

        public void SpawnProjectile(NPC npc, Vector2 pos, Vector2 velocity, int type, int damage, float knockback)
        {
            SpawnProjectile(npc, pos, velocity, type, damage, knockback, 1, 0, 0, 0);
        }

        public void SpawnProjectile(NPC npc, Vector2 velocity, int type, int damage, float knockback, int count, int spread)
        {
            SpawnProjectile(npc, npc.Center, velocity, type, damage, knockback, count, spread, 0, 0);
        }

        public void SpawnProjectile(NPC npc, Vector2 pos, Vector2 velocity, int type, int damage, float knockback, int count, int spread)
        {
            SpawnProjectile(npc, pos, velocity, type, damage, knockback, count, spread, 0, 0);
        }

        public void SpawnProjectile(NPC npc, Vector2 velocity, int type, int damage, float knockback, int count, int spread, int repeatSpacingX, int repeatSpacingY)
        {
            SpawnProjectile(npc, npc.Center, velocity, type, damage, knockback, count, spread, repeatSpacingX, repeatSpacingY);
        }

        public void SpawnProjectile(NPC npc, Vector2 pos, Vector2 velocity, int type, int damage, float knockback, int count, int spread, int repeatSpacingX, int repeatSpacingY)
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

            for (int i = -count; i <= count; i++)
            {
                float scale = 5f;
                Vector2 newVelocity = velocity.Length() > 0 ? velocity.RotatedBy(start + step * i) * speed : velocity;

                Projectile.NewProjectile(new EntitySource_BossSpawn(npc), pos - new Vector2(repeatSpacingX * i, repeatSpacingY * i), newVelocity, type, damage, knockback, -1, scale, scale, scale);
            }
        }

        private int Projectile_NewProjectile(On.Terraria.Projectile.orig_NewProjectile_IEntitySource_Vector2_Vector2_int_int_float_int_float_float_float_NewProjectileModifier orig, IEntitySource spawnSource, Vector2 position, Vector2 velocity, int Type, int Damage, float KnockBack, int Owner, float ai0, float ai1, float ai2, NewProjectileModifier modifier)
        {
            int num = 1000;
            for (int i = 0; i < 1000; i++)
            {
                if (!Main.projectile[i].active)
                {
                    num = i;
                    break;
                }
            }
            if (num == 1000)
            {
                num = Projectile.FindOldestProjectile();
            }

            Projectile proj = Main.projectile[num];
            proj.scale = ai0;

            return orig(spawnSource, position, velocity, Type, Damage, KnockBack, Owner, ai0, ai1, ai2, modifier);
        }

        public async void ProjectileBurst(NPC npc, Vector2 velocity, int projectile, int damage, float knockback, int repeatCount, int delay, int range)
        {
            for (int i = -repeatCount; i < repeatCount; i++)
            {
                int target = Player.FindClosest(npc.Center, npc.width, npc.height);
                Player player = Main.player[target];

                SpawnProjectile(npc, VectorUtils.GetRandomVectorWithinRange(player.Center, range), velocity, projectile, damage, knockback);
                await Task.Delay(delay);
            }
        }

        public void SetVelocity(NPC npc, Player player, float speed, float decelRange)
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

        public void PlaySound(Vector2 pos, ushort id, int style, float volume, float pitch)
        {
            NetMessage.PlayNetSound(new NetSoundInfo(pos, id, style, volume, pitch), -1, -1);
        }
    }
}
