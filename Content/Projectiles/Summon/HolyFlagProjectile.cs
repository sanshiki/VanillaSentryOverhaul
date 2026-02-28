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
    public class HolyFlagProjectile : FlagProjectile
    {
        protected override string FLAG_CLOTH_TEXTURE_PATH => ModGlobal.MOD_TEXTURE_PATH + "Projectiles/HolyFlag";
        protected override int FLAG_WIDTH => 120;
        protected override int FLAG_HEIGHT => 78;
        protected override float TAIL_OFFSET_X_1 => -33f;
        protected override float TAIL_OFFSET_Y_1 => -90f;  
        protected override float TAIL_OFFSET_X_2 => -33f;  
        protected override float TAIL_OFFSET_Y_2 => -63f;   
        protected override Color TAIL_COLOR => new Color(145, 145, 145, 47);
        protected override bool TAIL_DYNAMIC_DEBUG => false;
        protected override float GRAVITY => 1.5f;
        protected override float MAX_FALL_SPEED => 25f;
        protected override bool USE_CURSOR_ASSISTED_PLANT => true;
        // protected override bool TAIL_ENABLE_GLOBAL => false;
        protected override int FULLY_CHARGED_DUST => 263;
        protected override int ENHANCE_BUFF_ID => ModBuffID.HolyFlagBuff;
        protected override int NPC_DEBUFF_ID => BuffID.SwordWhipNPCDebuff;
        protected override bool AUTO_READD_BUFF_ON_PLANT => true;
        protected override float SENTRY_RECALL_SPEED => 40f;
        protected override float SENTRY_RECALL_THRESHOLD => 40f;
        protected override float SENTRY_RECALL_DECAY_DIST => 800f;
        protected override float SENTRY_RECALL_MAX_DIST => 3500f;
        protected override int ONGROUND_CNT_THRESHOLD => 20;
        // protected override int TAIL_BLEND_STATE => TAIL_BLEND_STATE_NONPREMULTIPLIED;
    }
}