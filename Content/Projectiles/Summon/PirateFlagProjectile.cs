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
        protected override float TAIL_OFFSET_Y_1 => -150f;  
        protected override float TAIL_OFFSET_X_2 => -75f;  
        protected override float TAIL_OFFSET_Y_2 => -96f;   
        protected override Color TAIL_COLOR => new Color(35, 45, 65, 100);
        protected override bool TAIL_DYNAMIC_DEBUG => true;
        protected override int FULLY_CHARGED_DUST => DustID.MushroomSpray;
    }
}