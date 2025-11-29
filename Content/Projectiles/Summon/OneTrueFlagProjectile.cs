using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections;
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
    public class OneTrueFlagShadow
    {
        public Vector2 center;
        public float rotation;
        public int dir;
    }
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
        protected override bool USE_CUSTOM_SENTRY_RECALL => true;
        protected override float GRAVITY => 1.5f;
        protected override float RECALL_SPEED => 50f;
        protected override int FULLY_CHARGED_DUST => DustID.MushroomSpray;
        protected override int ENHANCE_BUFF_ID => ModBuffID.OneTrueFlagBuff;
        protected override int NPC_DEBUFF_ID => ModBuffID.OneTrueFlagDebuff;
        protected bool BladShotInited = false;
        protected bool SoundPlayed = false;
        protected Vector2 CursorPos;
        protected List<OneTrueFlagShadow> ShadowList = new List<OneTrueFlagShadow>();
        protected List<float> OneTrueFlagStickOffsetList = new List<float>();
        protected const int SHADOW_INTERVAL = 2;
        protected const int SHADOW_COUNT = 4;
        

        protected override void CustomSentryRecall(SentryRecallInfo info)
        {
            var sentry = Main.projectile[info.ID];
            if (!info.AnchorInited)
            {
                // Main.NewText("Sentry Recall Inited:"+info.ID);
                if(info.TileCollide) info.TargetPos = MinionAIHelper.SearchForGround(info.TargetPos+new Vector2(0, 100f), 10, 16, (int)(sentry.height * 0.5f));
                info.AnchorInited = true;
                info.Anchor_ID = Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    info.TargetPos,
                    Vector2.Zero,
                    ModProjectileID.OneTrueFlagAnchor,
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
                Projectile proj = Main.projectile[info.Anchor_ID];
                if (proj.ModProjectile is OneTrueFlagAnchor anchor_)
                {
                    anchor_.sentryInfo = info;
                }
                if(!SoundPlayed)
                {
                    SoundEngine.PlaySound(ModSounds.HellpodSignal_2_1, Projectile.Center);
                    SoundPlayed = true;
                }
            }
            // Projectile anchor = Main.projectile[info.Anchor_ID];
            // if (!anchor.active && !info.IsRecalled)
            // {
            //     sentry.Center = info.TargetPos + new Vector2(0, -sentry.height*0.55f);
            //     sentry.velocity = info.TileCollide ? new Vector2(0, 20f) : Vector2.Zero;
            //     info.IsRecalled = true;
            // }
        }

        // public override void AI()
        // {
        //     if(!BladShotInited)
        //     {
        //         CursorPos = Main.MouseWorld;
        //         BladShotInited = true;
        //     }

        //     base.AI();
        //     // add blade shot
        //     if(State == WAVE_STATE && Projectile.timeLeft == TIME_LEFT_WAVE / 2)
        //     {
        //         Player player = Main.player[Projectile.owner];
        //         Vector2 direction = Vector2.Normalize(CursorPos - player.Center);
        //         Projectile bladeShot = Projectile.NewProjectileDirect(
        //             Projectile.GetSource_FromAI(),
        //             player.Center + direction * PoleLength * 0.7f,
        //             Vector2.Normalize(CursorPos - player.Center) * 9f,
        //             ModProjectileID.OneTrueFlagBladeShot,
        //             Projectile.damage,
        //             Projectile.knockBack,
        //             Projectile.owner
        //         );
        //     }
        // }

        // public override void AI()
        // {
        //     base.AI();
        //     Player player = Main.player[Projectile.owner];
        //     float dir = WaveDirection;

        //     float RotRate = (ItemRot + ROT_DISPLACEMENT/2f) / ROT_DISPLACEMENT; // 0 to 1
        //     float WaveUseTime = TIME_LEFT_WAVE / AttackSpeed;

        //     if(RotRate < 0.333f)
        //     {
        //         RotAcc = 2*ROT_DISPLACEMENT / (float)WaveUseTime / (float)(WaveUseTime * 0.333f);
        //     }
        //     else
        //     {
        //         RotAcc = -2*ROT_DISPLACEMENT / (float)WaveUseTime / (float)(WaveUseTime * (1f-0.333f));
        //     }
        //     RotSpd += RotAcc;

        //     float offset_delta = 0f;
        //     if (RotRate < 0.5f)
        //     {
        //         offset_delta = MathHelper.Lerp(0.5f, -0.4f, RotRate * 2f) * PoleLength;
        //     }
        //     else
        //     {
        //         offset_delta = MathHelper.Lerp(-0.4f, 0.5f, (RotRate - 0.5f) * 2f) * PoleLength;
        //     }
        //     STICK_OFFSET = new Vector2(0f, -PoleLength / 2f + offset_delta);
        //     Vector2 StickOffset = new Vector2(STICK_OFFSET.X * dir, STICK_OFFSET.Y);
        //     OneTrueFlagStickOffsetList.Add(-PoleLength / 2f + offset_delta);
            
        //     ItemRot += RotSpd;

        //     // Main.NewText("RotRate: "+RotRate + " RotAcc: "+RotAcc + " RotSpd: "+RotSpd + " ItemRot: "+ItemRot);

        //     float Rot = AimAngle + (ROT_ANGLE * RotRate - ROT_ANGLE / 2f) * dir;

        //     OneTrueFlagShadow shadow = new OneTrueFlagShadow();
        //     shadow.center = CenterMapping(player.Center, StickOffset, Rot);
        //     shadow.rotation = Rot;
        //     shadow.dir = (int)dir;
        //     ShadowList.Add(shadow);

        //     // if(ShadowQueue.Count > SHADOW_COUNT)
        //     // {
        //     //     ShadowQueue.Dequeue();
        //     // }
        // }

        // public override bool PreDraw(ref Color lightColor)
        // {
        //     // foreach (var shadow in ShadowQueue)
        //     int CurrentTime = (int)(TIME_LEFT_WAVE / AttackSpeed) - Projectile.timeLeft;
        //     int OldPosSize = (int)Math.Min(CurrentTime, TAIL_LENGTH);
        //     for(int i = 0; i < OldPosSize;i++)
        //     {
        //         if (i % SHADOW_INTERVAL != 0) continue;
        //         Vector2 OldCenter = Projectile.oldPos[i] + new Vector2(0, PoleLength / 2f);
        //         float OldRot = Projectile.oldRot[i];
        //         Player player = Main.player[Projectile.owner];
        //         Texture2D flagTexture = ModContent.Request<Texture2D>(FLAGPOLE_TEXTURE_PATH).Value;
        //         int width = flagTexture.Width;
        //         int height = Projectile.height;
        //         int TextureHeight = flagTexture.Height;
        //         Vector2 origin = new Vector2(width / 2, height / 2);

        //         // draw tip part
        //         Rectangle tipRect = new Rectangle(0, 0, width, TIP_HEIGHT);
        //         Vector2 tipLocalPos = new Vector2(0, 0);
        //         Vector2 tipWorldPos = MinionAIHelper.ConvertToWorldPos(OldCenter, OldRot, tipLocalPos);
        //         DrawPart(flagTexture, tipWorldPos, tipRect, lightColor, origin);

        //         // draw repeat part
        //         for (int j = 0; j < (PoleLength - TIP_HEIGHT - BASE_HEIGHT) / REPEAT_SLICE_HEIGHT; j++)
        //         {
        //             int repeatY = j * REPEAT_SLICE_HEIGHT + TIP_HEIGHT;
        //             Rectangle repeatRect = new Rectangle(0, TIP_HEIGHT, width, REPEAT_SLICE_HEIGHT);
        //             Vector2 repeatLocalPos = new Vector2(0, repeatY);
        //             Vector2 repeatWorldPos = MinionAIHelper.ConvertToWorldPos(OldCenter, OldRot, repeatLocalPos);
        //             DrawPart(flagTexture, repeatWorldPos, repeatRect, lightColor, origin);
        //         }

        //         // draw base part
        //         Rectangle baseRect = new Rectangle(0, TextureHeight - BASE_HEIGHT, width, BASE_HEIGHT);
        //         Vector2 baseLocalPos = new Vector2(0, PoleLength - BASE_HEIGHT);
        //         Vector2 baseWorldPos = MinionAIHelper.ConvertToWorldPos(OldCenter, OldRot, baseLocalPos);
        //         DrawPart(flagTexture, baseWorldPos, baseRect, lightColor, origin);

        //         // draw flag cloth
        //         Vector2 FlagOffset = new Vector2(-FLAG_WIDTH / 2f * Projectile.spriteDirection, (FLAG_HEIGHT - Projectile.height) / 2f);
        //         Vector2 FlagOffsetEx = new Vector2(-2f + 1f * Projectile.spriteDirection, 0f);
        //         FlagOffset += FlagOffsetEx;
        //         Vector2 ClothCenter = OldCenter + FlagOffset.RotatedBy(OldRot);
        //         PreDrawFlagCloth(ref lightColor, ClothCenter);
        //     }

        //     return base.PreDraw(ref lightColor);
        // }
    }
}