using Microsoft.Xna.Framework;
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
        bool boss1_alive = false;
        bool boss2_alive = false;

        public void Initialize()
        {
            boss1 = new();
            boss2 = new();

            On.Terraria.NPC.AI += NPC_AI;
            On.Terraria.NPC.checkDead += NPC_checkDead;
            On.Terraria.Chat.ChatHelper.BroadcastChatMessage += ChatHelper_BroadcastChatMessage;
        }

        public void Dispose()
        {
            On.Terraria.NPC.AI -= NPC_AI;
            On.Terraria.NPC.checkDead -= NPC_checkDead;
            On.Terraria.Chat.ChatHelper.BroadcastChatMessage -= ChatHelper_BroadcastChatMessage;
        }

        public enum BossType
        {
            Boss1,
            Boss2
        }

        public void NPC_AI(On.Terraria.NPC.orig_AI original_AI, NPC npc)
        {
            if (!npc.active)
            {
                original_AI(npc);
                return;
            }

            if (boss1_alive)
            {
                if (npc.type == NPCID.Retinazer)
                {   
                    boss1.BossAI(npc);
                    return;
                }
                if (npc.type == NPCID.SkeletronPrime)
                {
                    boss1.PrimeMinionAI(npc);
                    return;
                }
            }

            if (boss2_alive)
            {
                if (npc.type == NPCID.SkeletronPrime)
                {
                    boss2.BossAI(npc);
                    return;
                }
                if (npc.type == NPCID.LunarTowerSolar)
                {
                    boss2.MinionAI(npc);
                    return;
                }
            }

            original_AI(npc);
        }

        private void NPC_checkDead(On.Terraria.NPC.orig_checkDead orig, NPC npc)
        {
            if (npc.life <= 0 && npc.type == NPCID.Retinazer && boss1_alive)
            {
                boss1.ClearMinions();
            }
            orig(npc);
        }

        public void ChatHelper_BroadcastChatMessage(On.Terraria.Chat.ChatHelper.orig_BroadcastChatMessage orig, NetworkText text, Color color, int excludedPlayer)
        {
            if (boss1_alive)
            {
                if (text.ToString().Contains("The Twins"))
                {
                    text._text = $"{BossType.Boss1} has been defeated!";
                    boss1_alive = false;
                }
                else if (text.ToString().Contains("Skeletron Prime")) return;
            }

            if (text.ToString().Contains("Skeletron Prime") && boss2_alive)
            {
                text._text = $"{BossType.Boss2} has been defeated!";
                boss2_alive = false;
            }

            orig(text, color, excludedPlayer);
        }

        public void SpawnBoss(Vector2 pos, BossType type, int amount)
        {
            int npcID = 0;
            string spawnMessage = "";

            switch (type)
            {
                case BossType.Boss1:
                    npcID = NPCID.Retinazer;
                    spawnMessage = $"{BossType.Boss1} has awoken!";
                    boss1_alive = true;
                    break;
                case BossType.Boss2:
                    npcID = NPCID.SkeletronPrime;                  
                    spawnMessage = $"{BossType.Boss2} has awoken!";
                    boss2_alive = true;
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
                Vector2 newVelocity = velocity.Length() > 0 ? velocity.RotatedBy(start + step * i) * speed : velocity;
                Projectile.NewProjectile(new EntitySource_BossSpawn(npc), pos - new Vector2(repeatSpacingX * i, repeatSpacingY * i), newVelocity, type, damage, knockback);
            }
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

        public void PlaySound(Vector2 pos, ushort id, int style, float volume, float pitch)
        {
            NetMessage.PlayNetSound(new NetSoundInfo(pos, id, style, volume, pitch), -1, -1);
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
    }
}
