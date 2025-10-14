using Google.Protobuf.WellKnownTypes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TrollariaAddons.Addons.Helpers;
using TShockAPI;
using static Mysqlx.Crud.Order.Types;

namespace TrollariaAddons.Addons.Bosses
{
    public class Boss2 : Boss
    {
        Player? player;
        public int damageMultiplier = 1000;

        public void BossAI(NPC boss)
        {
            boss.aiStyle = -1;
            boss.defense = 300;

            int target = Player.FindClosest(boss.Center, boss.width, boss.height);
            player = Main.player[target];

            SetVelocity(boss, player, 2.5f, 13, 200, 12, 0.25f);

            Vector2 projDir = player.Center - boss.Center;
            if (projDir.LengthSquared() > 1f) 
                projDir.Normalize();

            Vector2 projSpeed = projDir * 9;

            boss.localAI[0]++;

            if (boss.life >= (boss.lifeMax / 2))
            {
                if (boss.localAI[0] % 100 == 0)
                {
                    Vector2 spawnPos = VectorUtils.GetRandomVectorWithinRange(player.Center, 1000);

                    int minionId = NPC.NewNPC(new EntitySource_Sync(), (int)spawnPos.X, (int)spawnPos.Y, NPCID.LunarTowerSolar);
                    NPC minion = Main.npc[minionId];
                    minion.life = 100;
                }
            }

            boss.netUpdate = true;
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, boss.whoAmI);
        }

        public void MinionAI(NPC minion)
        {
            if (player == null) return;

            minion.aiStyle = -1;

            Vector2 direction = Vector2.Normalize(player.Center - minion.Center);
            minion.SimpleFlyMovement(direction, 20);

            minion.netUpdate = true;
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, minion.whoAmI);
        }
    }
}
