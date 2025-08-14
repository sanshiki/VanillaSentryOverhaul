using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;
using SummonerExpansionMod.Content.Projectiles.Summon;

namespace SummonerExpansionMod.Content.Items.Weapons.Summon
{
    public class FlagWeapon : ModItem
    {

        private int RaiseTimer = 0;
        private int Direction = 1;
        private uint LastShootTime = 0;
        private bool IsRightPressed = false;
        private Projectile FlagProjectile = null;

        // state constants
        public const int IDLE_STATE = -1; // idle
        public const int WAVE_STATE = 0; // left-click: wave
        public const int RAISE_STATE = 1; // right-short-press: raise
        public const int PLANT_STATE = 2; // right-long-press: plant
        public const int RECALL_STATE = 3; // right-click after plant: recall

        private int State = IDLE_STATE;

        public const int LEFT_KEY = 0;
        public const int RIGHT_KEY = 2;
        private const int DIR_RESET_INTERVAL = 40;
        private const int WAVE_USE_TIME = 25+2;
        private const int RAISE_USE_TIME = 60+2;
        private const int MAX_RAISE_TIME = 55;
        private const int ONGROUND_CNT_THRESHOLD = 45;
        private int MOD_PROJECTILE_ID = -1;

        public override void SetDefaults()
        {
            MOD_PROJECTILE_ID = ModProjectileID.FlagPole;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = WAVE_USE_TIME;
            Item.damage = 42;
            Item.knockBack = 4f;
            Item.DamageType = DamageClass.Summon;
            Item.useAnimation = WAVE_USE_TIME;
            Item.noMelee = true;
            Item.noUseGraphic = true; // 不绘制物品本体
            Item.autoReuse = true;
            Item.shoot = MOD_PROJECTILE_ID;
            Item.shootSpeed = 0f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == LEFT_KEY)
            {
                if(FlagProjectile != null && FlagProjectile.active)
                {
                    FlagProjectile.Kill();
                    FlagProjectile = null;
                }
                uint CurTime = Main.GameUpdateCount;
                if(CurTime - LastShootTime < DIR_RESET_INTERVAL)
                {
                    Direction = -Direction;
                }
                else
                {
                    Direction = 1;
                }
                LastShootTime = CurTime;
                FlagProjectile = GenerateFlagProjectile(player, source, position, velocity, type, damage, knockback);
                // Main.NewText(type);
                if (FlagProjectile.ModProjectile is FlagPole flagPole)
                {
                    flagPole.WaveDirection = (float)Direction;
                    flagPole.State = WAVE_STATE;
                    flagPole.PoleLength = 280;
                }
            }

            return false;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true; // 表示该物品支持右键使用
        }


        private Projectile GenerateFlagProjectile(Player player, IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            return projectile;
        }

        public override void HoldItem(Player player)
        {
            // Main.NewText("HoldItem:"+State);
            if (player.controlUseTile) // right-click
            {
                if(State == IDLE_STATE)
                {
                    Item.useStyle = ItemUseStyleID.Thrust;
                    Item.useTime = RAISE_USE_TIME;
                    Item.autoReuse = false;
                    Item.shoot = ProjectileID.None;
                    RaiseTimer = 0;
                    if(!IsRightPressed)
                    {
                        // Main.NewText("Raising");
                        if(FlagProjectile != null && FlagProjectile.active)
                        {
                            FlagProjectile.Kill();
                            FlagProjectile = null;
                        }
                        
                        FlagProjectile = GenerateFlagProjectile(player, player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, MOD_PROJECTILE_ID, Item.damage, Item.knockBack);
                        if (FlagProjectile.ModProjectile is FlagPole flagPole)
                        {
                            flagPole.State = RAISE_STATE;
                            State = RAISE_STATE;
                            flagPole.PoleLength = 280;
                        }
                        IsRightPressed = true;
                    }
                }
                else if(State == RAISE_STATE)
                {
                    RaiseTimer++;
                    if(RaiseTimer > MAX_RAISE_TIME)
                    {
                        RaiseTimer = 0;
                        State = PLANT_STATE;
                        if(FlagProjectile.ModProjectile is FlagPole flagPole)
                        {
                            flagPole.SwitchFlag = true;
                        }
                    }
                }
                else if(State == PLANT_STATE)
                {
                    if(FlagProjectile.ModProjectile is FlagPole flagPole)
                    {
                        if(flagPole.OnGroundCnt > ONGROUND_CNT_THRESHOLD)
                        {
                            flagPole.SwitchFlag = true;
                            State = RECALL_STATE;
                        }
                    }
                }
            }
            else
            {
                // left-click
                if(player.controlUseItem)
                {
                    if(State == IDLE_STATE)
                    {
                        Item.shoot = MOD_PROJECTILE_ID;
                        State = WAVE_STATE;
                    }
                    else if(State == PLANT_STATE)
                    {
                        if(FlagProjectile.ModProjectile is FlagPole flagPole)
                        {
                            if(flagPole.OnGroundCnt > ONGROUND_CNT_THRESHOLD)
                            {
                            flagPole.SwitchFlag = true;
                                State = RECALL_STATE;
                            }
                        }
                    }
                }
                // no key pressed
                else
                {
                    if (State != PLANT_STATE && State != RECALL_STATE && State != WAVE_STATE)
                    {
                        State = IDLE_STATE;
                        if(FlagProjectile != null && FlagProjectile.active)
                        {
                            FlagProjectile.Kill();
                            FlagProjectile = null;
                        }
                    }
                }
                
                Item.useStyle = ItemUseStyleID.Swing;
                Item.useTime = WAVE_USE_TIME;
                Item.autoReuse = true;
                IsRightPressed = false;
                RaiseTimer = 0;
                if(FlagProjectile != null && !FlagProjectile.active)
                {
                    FlagProjectile = null;
                    State = IDLE_STATE;
                }
            }
        }
    }
}
