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
    public class OneTrueFlagProjectile : FlagProjectile
    {
        protected override string FLAG_CLOTH_TEXTURE_PATH => ModGlobal.MOD_TEXTURE_PATH + "Projectiles/OneTrueFlag";
        protected override int FLAG_WIDTH => 120;
        protected override int FLAG_HEIGHT => 78;
        protected override float TAIL_OFFSET_X_1 => -33f;
        protected override float TAIL_OFFSET_Y_1 => -90f;  
        protected override float TAIL_OFFSET_X_2 => -33f;  
        protected override float TAIL_OFFSET_Y_2 => -63f;   
        protected override Color TAIL_COLOR => new Color(35, 54, 84, 100);
        protected override bool TAIL_DYNAMIC_DEBUG => false;
        // protected override bool TAIL_ENABLE_GLOBAL => false;
        protected override int FULLY_CHARGED_DUST => DustID.MushroomSpray;
        protected override int ENHANCE_BUFF_ID => ModBuffID.OneTrueFlagBuff;
        protected override int NPC_DEBUFF_ID => ModBuffID.OneTrueFlagDebuff;
        protected bool BladShotInited = false;
        protected Vector2 CursorPos;

        public override void AI()
        {
            if(!BladShotInited)
            {
                CursorPos = Main.MouseWorld;
                BladShotInited = true;
            }

            base.AI();
            if(State == WAVE_STATE && Projectile.timeLeft == TIME_LEFT_WAVE / 2)
            {
                Player player = Main.player[Projectile.owner];
                Vector2 direction = Vector2.Normalize(CursorPos - player.Center);
                Projectile bladeShot = Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromAI(),
                    player.Center + direction * PoleLength * 0.7f,
                    Vector2.Normalize(CursorPos - player.Center) * 9f,
                    ModProjectileID.OneTrueFlagBladeShot,
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }
    }
}