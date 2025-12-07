using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;
using SummonerExpansionMod.Content.Projectiles.Summon;
using SummonerExpansionMod.Initialization;
using SummonerExpansionMod.ModUtils;
namespace SummonerExpansionMod.Content.Items.Weapons.Summon
{
    
    public abstract class FlagWeapon<T> : ModItem where T : FlagProjectile
    {
        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Items/FlagWeapon";
        protected int RaiseTimer = 0;
        protected int Direction = 1;
        protected uint LastShootTime = 0;
        protected bool IsRightPressed = false;
        protected Projectile FlagProjectile = null;

        // state constants
        public const int IDLE_STATE = -1; // idle
        public const int WAVE_STATE = 0; // left-click: wave
        public const int RAISE_STATE = 1; // right-short-press: raise
        public const int PLANT_STATE = 2; // right-long-press: plant
        public const int RECALL_STATE = 3; // right-click after plant: recall

        protected int State = IDLE_STATE;

        public const int LEFT_KEY = 0;
        public const int RIGHT_KEY = 2;
        protected virtual float DIR_RESET_INTERVAL_FACTOR => 1.3f;
        protected virtual int WAVE_USE_TIME => 25+2;
        protected virtual int RAISE_USE_TIME => 60+2;
        protected virtual int MAX_RAISE_TIME => 55;
        protected virtual int ONGROUND_CNT_THRESHOLD => 45;
        protected virtual int MOD_PROJECTILE_ID => ModProjectileID.FlagProjectile;
        protected virtual int POLE_LENGTH => 280;
        protected virtual bool STATE_DEBUG => false;
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = WAVE_USE_TIME;
            Item.damage = 40;
            Item.knockBack = 4f;
            Item.DamageType = DamageClass.SummonMeleeSpeed;
            Item.useAnimation = WAVE_USE_TIME;
            Item.noMelee = true;
            Item.noUseGraphic = true; // 不绘制物品本体
            Item.autoReuse = true;
            Item.shoot = MOD_PROJECTILE_ID;
            Item.shootSpeed = 0f;

            DynamicParamManager.Register("State Debug", 0f, 0f, 1f);
        }

        public override bool MeleePrefix() {
			return true;
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
                if(CurTime - LastShootTime < DIR_RESET_INTERVAL_FACTOR * Item.useAnimation)
                {
                    Direction = -Direction;
                }
                else
                {
                    Direction = 1;
                }
                LastShootTime = CurTime;
                FlagProjectile = GenerateFlagProjectile(player, source, position, velocity, type, damage, knockback);
                if (TryGet(FlagProjectile, out T flagPole))
                {
                    flagPole.WaveDirection = (float)Direction;
                    flagPole.State = WAVE_STATE;
                    flagPole.PoleLength = POLE_LENGTH;
                }
            }

            return false;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true; // 表示该物品支持右键使用
        }

        public static bool TryGet(Projectile proj, out T result)
        {
            if (proj.ModProjectile is T t && proj.active)
            {
                result = t;                
                return true;
            }
            result = null;
            return false;
        }

        protected Projectile GenerateFlagProjectile(Player player, IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            return projectile;
        }

        public override void HoldItem(Player player)
        {
            bool state_debug = DynamicParamManager.Get("State Debug").value > 0.5f;
            if(state_debug)
            {
                string StateStr = "";
                switch(State)
                {
                    case IDLE_STATE:
                        StateStr = "IDLE";
                        break;
                    case WAVE_STATE:
                        StateStr = "WAVE";
                        break;
                    case RAISE_STATE:
                        StateStr = "RAISE";
                        break;
                    case PLANT_STATE:
                        StateStr = "PLANT";
                        break;
                    case RECALL_STATE:
                        StateStr = "RECALL";
                        break;
                }
                Main.NewText("State: " + StateStr);
            }
            if (player.controlUseTile) // right-click
            {
                switch(State)
                {
                    case IDLE_STATE:    // switch from idle state to raise state when right key pressed
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
                            // if (FlagProjectile.ModProjectile is FlagProjectile flagPole)
                            if(TryGet(FlagProjectile, out T flagPole))
                            {
                                flagPole.State = RAISE_STATE;
                                State = RAISE_STATE;
                                flagPole.PoleLength = POLE_LENGTH;
                                flagPole.TimeLeftRaise = (int)(RAISE_USE_TIME);
                            }
                            IsRightPressed = true;
                        }
                    } break;
                    case WAVE_STATE:    // kill flag projectile in waving state when right key pressed
                    {
                        if (FlagProjectile != null && FlagProjectile.active)
                        {
                            FlagProjectile.Kill();
                            FlagProjectile = null;
                        }

                        goto case IDLE_STATE;
                    }
                    case RAISE_STATE:   // raise state maintains to MAX_RAISE_TIME
                    {
                        RaiseTimer++;
                        if(RaiseTimer > RAISE_USE_TIME * 0.88f)
                        {
                            RaiseTimer = 0;
                            State = PLANT_STATE;
                            // if(FlagProjectile.ModProjectile is FlagProjectile flagPole)
                            if(TryGet(FlagProjectile, out T flagPole))
                            {
                                flagPole.SwitchFlag = true;
                            }
                        }
                    } break;
                    case PLANT_STATE:   // switch from plant state to recall state when right key pressed
                    {
                        // if(FlagProjectile.ModProjectile is FlagProjectile flagPole)
                        if (TryGet(FlagProjectile, out T flagPole))
                        {
                            if (flagPole.OnGroundCnt > ONGROUND_CNT_THRESHOLD)
                            {
                                flagPole.SwitchFlag = true;
                                State = RECALL_STATE;
                            }
                        }
                        else
                        {
                            State = IDLE_STATE;
                        }
                    } break;
                    case RECALL_STATE:  // reset params to idle state
                    {
                        Item.useStyle = ItemUseStyleID.Swing;
                        Item.useTime = Item.useAnimation;
                        Item.autoReuse = true;
                        IsRightPressed = false;
                        RaiseTimer = 0;
                        if (FlagProjectile == null || (FlagProjectile != null && !FlagProjectile.active))
                        {
                            FlagProjectile = null;
                            State = IDLE_STATE;
                        }
                    } break;
                    default:
                    {

                    } break;
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
                        // if(FlagProjectile.ModProjectile is FlagProjectile flagPole)
                        if (TryGet(FlagProjectile, out T flagPole))
                        {
                            if (flagPole.OnGroundCnt > ONGROUND_CNT_THRESHOLD)
                            {
                                flagPole.SwitchFlag = true;
                                State = RECALL_STATE;
                            }
                        }
                        else
                        {
                            State = IDLE_STATE;
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
                Item.useTime = Item.useAnimation;
                Item.autoReuse = true;
                IsRightPressed = false;
                RaiseTimer = 0;
                if (FlagProjectile == null || (FlagProjectile != null && !FlagProjectile.active))
                {
                    FlagProjectile = null;
                    State = IDLE_STATE;
                }
            }
        }
    }
}
