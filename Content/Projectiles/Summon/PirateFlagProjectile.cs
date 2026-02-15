using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using SummonerExpansionMod.Content.Items.Weapons.Summon;
using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.Initialization;
using SummonerExpansionMod.ModUtils;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class PirateFlagProjectile : FlagProjectile
    {
        protected override string FLAG_CLOTH_TEXTURE_PATH => ModGlobal.MOD_TEXTURE_PATH + "Projectiles/PirateFlag";
        protected override int FLAG_WIDTH => 100;
        protected override int FLAG_HEIGHT => 70;
        protected override float TAIL_OFFSET_X_1 => -70f;
        protected override float TAIL_OFFSET_Y_1 => -138f;  
        protected override float TAIL_OFFSET_X_2 => -75f;  
        protected override float TAIL_OFFSET_Y_2 => -78f;   
        protected override Color TAIL_COLOR => new Color(35, 45, 65, 100);
        protected override bool TAIL_DYNAMIC_DEBUG => false;
        protected override float GRAVITY => 1.25f;
        protected override float MAX_FALL_SPEED => 22.5f;
        protected override bool USE_CURSOR_ASSISTED_PLANT => true;
        protected override int FULLY_CHARGED_DUST => DustID.MushroomSpray;
        protected override int ENHANCE_BUFF_ID => ModBuffID.PirateFlagBuff;
        protected override int NPC_DEBUFF_ID => ModBuffID.PirateFlagDebuff;protected override float SENTRY_RECALL_SPEED => 37f;
        protected override float SENTRY_RECALL_THRESHOLD => 40f;
        protected override float SENTRY_RECALL_DECAY_DIST => 800f;
        protected override float SENTRY_RECALL_MAX_DIST => 3500f;
        protected override int ONGROUND_CNT_THRESHOLD => 25;
    }
}