using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using SummonerExpansionMod.Content.Projectiles.Summon;
using SummonerExpansionMod.Initialization;
using SummonerExpansionMod.Content.Players;

namespace SummonerExpansionMod.Content.Buffs.Summon
{
    public class SummonFatigueDebuff : ModBuff
    {
        public override string Texture => ModGlobal.VANILLA_BUFF_TEXTURE_PATH + BuffID.Electrified;

        public float fatigueLevel = 0f;
        public float fatigueMultiplier = 1f;

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true; // This buff won't save when you exit the world
            Main.buffNoTimeDisplay[Type] = true; // The time remaining won't display on this buff
            Main.debuff[Type] = true;    // 是负面效果
        }

        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            Player player = Main.player[Main.myPlayer];
            fatigueLevel = player.GetModPlayer<SummonFatiguePlayer>().fatigueLevel;
            fatigueMultiplier = player.GetModPlayer<SummonFatiguePlayer>().fatigueMultiplier;
            string FatigueText = fatigueLevel > 0.5f ? "筋疲力竭" : "有些疲惫";
            string tooltip = "同时召唤了哨兵与仆从使你" + FatigueText + "。\n伤害降低了" + (int)(100f * (1-fatigueMultiplier)) + "%。";
            tip = tooltip;
        }
    }
}