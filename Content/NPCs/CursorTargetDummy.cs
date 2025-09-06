using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.Localization;

using Microsoft.Xna.Framework.Graphics;
using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.Utils;
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.NPCs
{
    public class CursorTargetDummy : ModNPC
    {

        public override LocalizedText DisplayName => Language.GetText("Mods.SummonerExpansionMod.EmptyName");

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.width = 32;
            NPC.height = 32;
            // NPC.dontTakeDamage = true;
            // NPC.dontCountMe = true;
            NPC.lifeMax = 999999;
            // NPC.friendly = true; // 避免被敌人攻击
            // NPC.immortal = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.GivenName = " ";
            // NPC.FullName = "";
            // NPC.DisplayName = "";
            // NPC.DisplayNameOverride = "";

        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            // 返回 false，彻底屏蔽名字和血条
            return false;
        }

        public override bool? CanBeHitByProjectile(Projectile projectile) => false;
        public override bool? CanBeHitByItem(Player player, Item item) => false;
        public override bool CanBeHitByNPC(NPC attacker) => false;
    }
}
