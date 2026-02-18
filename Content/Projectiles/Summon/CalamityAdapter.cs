using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

using Microsoft.Xna.Framework.Graphics;
using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.ModUtils;
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class CalamityGlobalAdapter : GlobalProjectile
    {
        public static List<string> CalamitySentriesNeedToBeStopped = new List<string>()
        {
            "EXPLODINGFROG",
            "HarvestStaffSentry",
            "SquirrelSquireMinion",
            "PolypLauncherSentry",
            "Hive",
            "Spikecrag",
            "OldDukeHeadCorpse",
            "PulseTurret",
            "AtlasMunitionsDropPod"
        };
        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            if (!ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            return true;

            if (projectile.ModProjectile?.Mod.Name == "CalamityMod")
            {
                if(CalamitySentriesNeedToBeStopped.Contains(projectile.ModProjectile.GetType().Name))
                {
                    projectile.velocity.X = 0f;
                }
            }
            return true;
        }
    }

    public class CalamityInstanceAdapter : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        private Vector2 lastVelocity = new Vector2(0, 0);
        private int SpawnCnt = 0;
        private const float DEACCELERATION = 0.5f;

        public static List<string> CalamitySentriesNeedToBeMoved = new List<string>()
        {
            "RustyDrone",
            "IceSentry",
            "DreadmineTurret",
            "LanternSoul",
            "ProfanedEnergy"
        };

        public static List<string> CalamitySentriesNeedToBeStopped = new List<string>()
        {
            "FlyingOrthocera",
            "AquasScepterCloud",
        };

        public override bool PreAI(Projectile projectile)
        {
            if (!ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            return true;

            if (projectile.ModProjectile?.Mod.Name == "CalamityMod")
            {
                if(CalamitySentriesNeedToBeMoved.Contains(projectile.ModProjectile.GetType().Name))
                {
                    SpawnCnt++;
                    if(SpawnCnt >= 5)
                    {
                        Vector2 vel = lastVelocity;
                        Vector2 vel_dir = vel.SafeNormalize(Vector2.Zero);
                        if(vel.Length() > DEACCELERATION)
                        {
                            lastVelocity -= vel_dir * DEACCELERATION;
                        }
                        else
                        {
                            lastVelocity = Vector2.Zero;
                        }
                        // apply velocity
                        projectile.Center += lastVelocity;
                        if(!(projectile.velocity == Vector2.Zero && lastVelocity != Vector2.Zero))
                            lastVelocity = projectile.velocity;
                        SpawnCnt = 5;

                        // Main.NewText("lastVelocity: "+lastVelocity+", velocity: "+projectile.velocity);
                    }
                }
                else if(CalamitySentriesNeedToBeStopped.Contains(projectile.ModProjectile.GetType().Name))
                {
                    Vector2 vel = projectile.velocity;
                    Vector2 vel_dir = vel.SafeNormalize(Vector2.Zero);
                    if(vel.Length() > DEACCELERATION)
                    {
                        projectile.velocity -= vel_dir * DEACCELERATION;
                    }
                    else
                    {
                        projectile.velocity = Vector2.Zero;
                    }
                }
            }

            return true;
        }
    }
}