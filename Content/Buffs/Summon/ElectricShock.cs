using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using SummonerExpansionMod.Content.Projectiles.Summon;
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.Buffs.Summon
{
    public class ElectricShock : ModBuff
    {
        public override string Texture => ModGlobal.VANILLA_BUFF_TEXTURE_PATH + BuffID.Electrified;
        private int buffTimer = 0;
        private const int APPLY_INTERVAL = 30;
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true; // This buff won't save when you exit the world
            Main.buffNoTimeDisplay[Type] = false; // The time remaining won't display on this buff
            Main.debuff[Type] = true;    // 是负面效果
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if(buffTimer++ >= APPLY_INTERVAL)
            {
                npc.velocity = Vector2.Zero;
                buffTimer = 0;
            }
        }
    }
}