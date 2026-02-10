// using Microsoft.Xna.Framework;
// using System;
// using System.Collections.Generic;
// using Terraria;
// using Terraria.DataStructures;
// using Terraria.ID;
// using Terraria.ModLoader;
// using Terraria.Audio;
// using Terraria.GameContent;
// using Microsoft.Xna.Framework.Graphics;
// using SummonerExpansionMod.Content.Items.Weapons.Summon;
// using SummonerExpansionMod.Content.Buffs.Summon;
// using SummonerExpansionMod.Initialization;
// using SummonerExpansionMod.ModUtils;

// namespace SummonerExpansionMod.Content.Projectiles.Summon
// {
//     public class OneTrueFlagBladeShot : ModProjectile
//     {
//         public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Projectiles/OneTrueFlagShadow";

//         private const float SHOOT_DIST = 23*16f;
//         private const float ROT_SPEED = 0.6f;

//         private Vector2 ShootDirection;
//         private float ShootSpeed;
//         private Vector2 RelativeDisplacement;
//         private Vector2 RelativeVelocity;
//         private int TimeLeft;

//         private Vector2 relativeOffset;

//         public override void SetDefaults()
//         {
//             Projectile.width = 130;
//             Projectile.height = 290;
//             Projectile.friendly = true;
//             Projectile.tileCollide = false;
//             Projectile.timeLeft = 2;
//             Projectile.penetrate = -1;
//         }
//         public override void OnSpawn(IEntitySource source)
//         {
//             Player player = Main.player[Projectile.owner];
//             relativeOffset = Main.MouseWorld - player.Center;
//         }
//         public override void AI()
//         {
//             Player player = Main.player[Projectile.owner];

//             Projectile FlagProjectile = Main.projectile[(int)Projectile.ai[1]];

//             if (!Main.mouseLeft || player.dead || FlagProjectile.type != ModProjectileID.OneTrueFlagProjectile || FlagProjectile.owner != Projectile.owner)
//             {
//                 Projectile.Kill();
//                 return;
//             }

//             Projectile.timeLeft = 2;

//             if (Vector2.Distance(Main.MouseWorld, player.Center + relativeOffset) > 10f)
//             {
//                 relativeOffset = Main.MouseWorld - player.Center;
//             }

//             Vector2 desiredPosition = player.Center + relativeOffset.SafeNormalize(Vector2.UnitX) * (relativeOffset.Length() < SHOOT_DIST ? relativeOffset.Length() : SHOOT_DIST);

//             float followSpeed = 0.3f;
//             Projectile.Center = Vector2.Lerp(Projectile.Center, desiredPosition, followSpeed);

//             Projectile.rotation += ROT_SPEED;
//         }
//         public override bool PreDraw(ref Color lightColor)
//         {
//             Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
//             float height = texture.Height / Main.projFrames[Projectile.type];
//             float width = texture.Width;

//             Main.EntitySpriteDraw(texture,
//                 Projectile.Center - Main.screenPosition,
//                 new Rectangle(0, (int)(0 * height), (int)width, (int)height),
//                 // Projectile.GetAlpha(new Color(212, 205, 189, 200)),
//                 lightColor,
//                 Projectile.rotation,
//                 new Vector2(123, 146),
//                 Projectile.scale,
//                 SpriteEffects.None,
//                 0
//             );
//             return false;
//         }
//     }
// }