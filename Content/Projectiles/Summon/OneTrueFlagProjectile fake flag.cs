// using Microsoft.Xna.Framework;
// using System;
// using System.Collections.Generic;
// using System.Collections;
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
//     public class OneTrueFlagShadow
//     {
//         public Vector2 center;
//         public float rotation;
//         public int dir;
//     }
//     public class OneTrueFlagProjectile : FlagProjectile
//     {
//         protected override string FLAG_CLOTH_TEXTURE_PATH => ModGlobal.MOD_TEXTURE_PATH + "Projectiles/OneTrueFlag";
//         protected override int FLAG_WIDTH => 120;
//         protected override int FLAG_HEIGHT => 78;
//         protected override float TAIL_OFFSET_X_1 => -33f;
//         protected override float TAIL_OFFSET_Y_1 => -90f;  
//         protected override float TAIL_OFFSET_X_2 => -33f;  
//         protected override float TAIL_OFFSET_Y_2 => -63f;   
//         protected override Color TAIL_COLOR => new Color(35, 54, 84, 100);
//         protected override bool TAIL_DYNAMIC_DEBUG => false;
//         // protected override bool TAIL_ENABLE_GLOBAL => false;
//         protected override bool USE_CUSTOM_SENTRY_RECALL => true;
//         protected override float GRAVITY => 1.5f;
//         protected override float RECALL_SPEED => 50f;
//         protected override int FULLY_CHARGED_DUST => DustID.MushroomSpray;
//         protected override int ENHANCE_BUFF_ID => ModBuffID.OneTrueFlagBuff;
//         protected override int NPC_DEBUFF_ID => ModBuffID.OneTrueFlagDebuff;
//         protected bool BladShotInited = false;
//         protected bool SoundPlayed = false;
//         protected Vector2 CursorPos;
//         protected List<OneTrueFlagShadow> ShadowList = new List<OneTrueFlagShadow>();
//         protected List<float> OneTrueFlagStickOffsetList = new List<float>();
//         protected const int SHADOW_INTERVAL = 2;
//         protected const int SHADOW_COUNT = 4;

//         /* Fake Flag Variables */
//         protected float TotalFakeFlagOffset;
//         protected int FakeFlagNum = 3;
//         protected int FakeFlagInterval = 4;
//         protected float StickOffsetComp = 0f;
//         protected float StickOffsetCoeff = 0f;
        
//         protected int SoundPlayInterval = 10;
//         protected int SoundPlayCount = 0;
//         protected int TotalSoundPlayCount = 0;
//         protected Vector2 MouseWorldPos;
        

//         protected override void CustomSentryRecall(SentryRecallInfo info)
//         {
//             var sentry = Main.projectile[info.ID];
//             if (!info.AnchorInited)
//             {
//                 // Main.NewText("Sentry Recall Inited:"+info.ID);
//                 if(info.TileCollide) info.TargetPos = MinionAIHelper.SearchForGround(info.TargetPos+new Vector2(0, 100f), 10, 16, (int)(sentry.height * 0.5f));
//                 info.AnchorInited = true;
//                 info.Anchor_ID = Projectile.NewProjectile(
//                     Projectile.GetSource_FromAI(),
//                     info.TargetPos,
//                     Vector2.Zero,
//                     ModProjectileID.OneTrueFlagAnchor,
//                     Projectile.damage,
//                     Projectile.knockBack,
//                     Projectile.owner
//                 );
//                 Projectile proj = Main.projectile[info.Anchor_ID];
//                 if (proj.ModProjectile is OneTrueFlagAnchor anchor_)
//                 {
//                     anchor_.sentryInfo = info;
//                 }
//                 if(!SoundPlayed)
//                 {
//                     SoundEngine.PlaySound(ModSounds.HellpodSignal_2_1, Projectile.Center);
//                     SoundPlayed = true;
//                 }
//             }
//             // Projectile anchor = Main.projectile[info.Anchor_ID];
//             // if (!anchor.active && !info.IsRecalled)
//             // {
//             //     sentry.Center = info.TargetPos + new Vector2(0, -sentry.height*0.55f);
//             //     sentry.velocity = info.TileCollide ? new Vector2(0, 20f) : Vector2.Zero;
//             //     info.IsRecalled = true;
//             // }
//         }

//         public override void AI()
//         {
//             base.AI();
//             if(State == WAVE_STATE)
//             {
//                 Player player = Main.player[Projectile.owner];
//                 MouseWorldPos = Main.MouseWorld;
//                 StickOffsetComp = DynamicParamManager.QuickGet("FakeFlagOffset", -33f, -300f, 300f).value;
//                 StickOffsetCoeff = DynamicParamManager.QuickGet("FakeFlagOffsetCoeff", 2f, 0f, 10f).value;
//                 // TotalFakeFlagOffset = (STICK_OFFSET.Length() + StickOffsetComp) * StickOffsetCoeff;
//                 if(State == WAVE_STATE)
//                 {
//                     SoundPlayCount++;
//                     if(SoundPlayCount >= SoundPlayInterval && TotalSoundPlayCount < FakeFlagNum)
//                     {
//                         SoundEngine.PlaySound(SoundID.Item102, Projectile.Center);
//                         SoundPlayCount = 0;
//                         TotalSoundPlayCount++;
//                     }
//                 }
//             }
//         }

//         public override bool PreDraw(ref Color lightColor)
//         {
            
//             if(State == WAVE_STATE)
//             {
//                 SpriteBatch sb = Main.spriteBatch;
//                 sb.End();
//                 sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
//                 float WaveUseTime = TIME_LEFT_WAVE / AttackSpeed;
//                 Player player = Main.player[Projectile.owner];
//                 for(int fakeFlagIndex = FakeFlagNum; fakeFlagIndex > 0; fakeFlagIndex--)
//                 {
//                     if(WaveUseTime - Projectile.timeLeft < FakeFlagInterval * fakeFlagIndex) continue;
//                     float FakeFlagRatio = (float)fakeFlagIndex / (float)FakeFlagNum;
//                     // Vector2 FakeFlagOffset = Vector2.Zero;
//                     float FakeFlagAlpha = MathHelper.Lerp(160, 30, FakeFlagRatio);
//                     float alphaRatio = FakeFlagAlpha / 255f;
//                     Color FakeFlagColor = lightColor * alphaRatio;
//                     Texture2D flagTexture = ModContent.Request<Texture2D>(FLAGPOLE_TEXTURE_PATH).Value;
//                     int width = flagTexture.Width;
//                     int height = Projectile.height;
//                     int TextureHeight = flagTexture.Height;
//                     Vector2 origin = new Vector2(width / 2, height / 2);

//                     Vector2 OldCenter = Projectile.oldPos[FakeFlagInterval * fakeFlagIndex] + new Vector2(0, PoleLength / 2f);
//                     float OldRotation = Projectile.oldRot[FakeFlagInterval * fakeFlagIndex];
//                     // Vector2 OldCenter = player.Center + new Vector2(0, MinionAIHelper.TaianglePeekFunc(PoleLength, WaveUseTime, WaveUseTime/2f, Projectile.timeLeft)).RotatedBy(OldRotation);
//                     List<float> StickOffsetListInvert = new List<float>(StickOffsetList);
//                     // StickOffsetListInvert.Reverse();
//                     float StickOffset = (StickOffsetListInvert[FakeFlagInterval * fakeFlagIndex - 1] + StickOffsetComp) * StickOffsetCoeff;
//                     Main.NewText("index * interval: " + FakeFlagInterval * fakeFlagIndex + " count: " + StickOffsetList.Count + " offset: " + StickOffset);
//                     Vector2 FakeFlagOffset = (OldCenter - player.Center).SafeNormalize(Vector2.UnitX) * StickOffset * FakeFlagRatio;
//                     // Vector2 OldCenter = Projectile.Center + new Vector2(0f, PoleLength/2f).RotatedBy(OldRotation+ModGlobal.PI_FLOAT);

//                     // draw tip part
//                     Rectangle tipRect = new Rectangle(0, 0, width, TIP_HEIGHT);
//                     Vector2 tipLocalPos = new Vector2(0, 0);
//                     Vector2 tipWorldPos = MinionAIHelper.ConvertToWorldPos(OldCenter + FakeFlagOffset, OldRotation, tipLocalPos);
//                     Main.spriteBatch.Draw(
//                         flagTexture,
//                         tipWorldPos,
//                         tipRect,
//                         FakeFlagColor,
//                         OldRotation,
//                         origin,
//                         Projectile.scale,
//                         Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
//                         0f    
//                     );

//                     // draw repeat part
//                     for (int i = 0; i < (PoleLength - TIP_HEIGHT - BASE_HEIGHT) / REPEAT_SLICE_HEIGHT; i++)
//                     {
//                         int repeatY = i * REPEAT_SLICE_HEIGHT + TIP_HEIGHT;
//                         Rectangle repeatRect = new Rectangle(0, TIP_HEIGHT, width, REPEAT_SLICE_HEIGHT);
//                         Vector2 repeatLocalPos = new Vector2(0, repeatY);
//                         Vector2 repeatWorldPos = MinionAIHelper.ConvertToWorldPos(OldCenter + FakeFlagOffset, OldRotation, repeatLocalPos);
//                         Main.spriteBatch.Draw(
//                             flagTexture,
//                             repeatWorldPos,
//                             repeatRect,
//                             FakeFlagColor,
//                             OldRotation,
//                             origin,
//                             Projectile.scale,
//                             Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
//                             0f
//                         );
//                     }

//                     // draw base part
//                     Rectangle baseRect = new Rectangle(0, TextureHeight - BASE_HEIGHT, width, BASE_HEIGHT);
//                     Vector2 baseLocalPos = new Vector2(0, PoleLength - BASE_HEIGHT);
//                     Vector2 baseWorldPos = MinionAIHelper.ConvertToWorldPos(OldCenter + FakeFlagOffset, OldRotation, baseLocalPos);
//                     Main.spriteBatch.Draw(
//                         flagTexture,
//                         baseWorldPos,
//                         baseRect,
//                         FakeFlagColor,
//                         OldRotation,
//                         origin,
//                         Projectile.scale,
//                         Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
//                         0f
//                     );

//                     // draw flag cloth
//                     Vector2 FlagOffset = new Vector2(-FLAG_WIDTH / 2f * Projectile.spriteDirection, (FLAG_HEIGHT - Projectile.height) / 2f);
//                     Vector2 FlagOffsetEx = new Vector2(-2f + 1f * Projectile.spriteDirection, 0f);
//                     FlagOffset += FlagOffsetEx;
//                     Vector2 ClothCenter = OldCenter + FlagOffset.RotatedBy(OldRotation);
//                     if(ENABLE_VERTEX_FLAG)
//                     {
//                         if (State == WAVE_STATE)
//                             PredrawFlagClothDynamicVertices(ref lightColor, ClothCenter);   // draw dynamic vertices
//                         else
//                             // PreDrawFlagClothVertices(ref lightColor, ClothCenter);   // draw static vertices
//                             PreDrawFlagCloth(ref lightColor, ClothCenter);
//                     }
//                     else
//                     {
//                         PreDrawFlagCloth(ref lightColor, ClothCenter);   // legacy draw flag cloth (now abandoned)
//                     }
//                 }
//                 // sb.End();
//             }

//             base.PreDraw(ref lightColor);

//             return false;
//         }

//         protected void PredrawFlagClothDynamicVertices(ref Color lightColor, Vector2 ClothCenter)
//         {
            
//             if(State == WAVE_STATE)
//             {
//                 float WaveUseTime = TIME_LEFT_WAVE / AttackSpeed;
//                 Player player = Main.player[Projectile.owner];
//                 for(int fakeFlagIndex = FakeFlagNum; fakeFlagIndex > 0; fakeFlagIndex--)
//                 {
//                     if(WaveUseTime - Projectile.timeLeft < FakeFlagInterval * fakeFlagIndex) continue;
//                     float FakeFlagRatio = (float)fakeFlagIndex / (float)FakeFlagNum;
//                     float alphaRatio = MathHelper.Lerp(160, 30, FakeFlagRatio) / 255f;
//                     Color FakeFlagColor = lightColor * alphaRatio;
//                     SpriteBatch sb = Main.spriteBatch;
//                     GraphicsDevice gd = Main.graphics.GraphicsDevice;

//                     Vector2 OldCenter = Projectile.oldPos[FakeFlagInterval * fakeFlagIndex] + new Vector2(0, PoleLength / 2f);
//                     float OldRotation = Projectile.oldRot[FakeFlagInterval * fakeFlagIndex];
//                     List<float> StickOffsetListInvert = new List<float>(StickOffsetList);
//                     // StickOffsetListInvert.Reverse();
//                     float OldStickOffset = (StickOffsetListInvert[FakeFlagInterval * fakeFlagIndex - 1] + StickOffsetComp) * StickOffsetCoeff;
//                     Vector2 FakeFlagOffset = (OldCenter - player.Center).SafeNormalize(Vector2.UnitX) * OldStickOffset * FakeFlagRatio;


//                     List<Vertex> FlagClothVerteces = new List<Vertex>();
//                     List<Vertex> FlagTailVerteces = new List<Vertex>();
//                     Vector2 SpinCenter = player.Center;
//                     int CurrentTime = (int)(TIME_LEFT_WAVE / AttackSpeed) - Projectile.timeLeft;
//                     int OverlapSize = TAIL_OVERLAP_SIZE;
//                     if(State == RECALL_STATE) CurrentTime = TIME_LEFT_RECALL - Projectile.timeLeft;
//                     int OldPosSize = (int)Math.Min(CurrentTime, TAIL_LENGTH+FLAG_CLOTH_LENGTH-OverlapSize);

//                     // construct polar points
//                     // List<float> StickOffsetListInvert = new List<float>(StickOffsetList);
//                     StickOffsetListInvert.Reverse();
//                     List<MinionAIHelper.PolarCurveFitter.Polar> PolarPoints = new List<MinionAIHelper.PolarCurveFitter.Polar>();
//                     for(int i = 0; i < OldPosSize;i++)
//                     {
//                         PolarPoints.Add(new MinionAIHelper.PolarCurveFitter.Polar(StickOffsetListInvert[i] + PoleLength, Projectile.oldRot[i]+(OldRotation-Projectile.rotation)));
//                     }
                    
//                     bool VertexDebug = DynamicParamManager.QuickGet("VertexDebug", 0f, 0f, 1f).value > 0.5f;
//                     if(VertexDebug)
//                     {
//                         string polarPointsString = "";
//                         foreach(var point in PolarPoints)
//                         {
//                             polarPointsString += Math.Round(point.r, 2) + " " + Math.Round(point.theta, 2) + "\n";
//                         }
//                         Main.NewText("Before fit: "+ polarPointsString);
//                     }
//                     PolarPoints = MinionAIHelper.PolarCurveFitter.FitAndInsert(PolarPoints, TAIL_FIT_INSERT_SIZE);
//                     if(VertexDebug)
//                     {
//                         string polarPointsString = "";
//                         foreach(var point in PolarPoints)
//                         {
//                             polarPointsString += Math.Round(point.r, 2) + " " + Math.Round(point.theta, 2) + "\n";
//                         }
//                         Main.NewText("After fit: "+ polarPointsString);
//                     }

//                     float FlagClothLength = FLAG_CLOTH_LENGTH * (TAIL_FIT_INSERT_SIZE+1) - 1;
//                     float TailLength = TAIL_LENGTH * (TAIL_FIT_INSERT_SIZE+1) - 1;

//                     // Main.NewText("FlagClothLength: "+FlagClothLength + " TailLength: "+TailLength + " Count: "+PolarPoints.Count);
//                     for(int i = 0; i < PolarPoints.Count;i++)
//                     {
//                         // float ratio = (i) / (float)(OldPosSize-1);
//                         float FlagClothRatio = (i) / (float)(FlagClothLength-1);
//                         float FlagTailRatio = (i - FlagClothLength + OverlapSize - 1) / (float)(TailLength-OverlapSize);
//                         // float color_rate = MathHelper.Clamp(ratio*3, 0, 1);
//                         // 根据ratio插值alpha值，越靠后的点越透明
//                         // byte alpha = (byte)MathHelper.Clamp(MathHelper.Lerp(500, 0, ratio), 0, 255);

//                         if(PolarPoints.Count <= 1) break;

//                         // Main.NewText("FlagClothRatio: "+FlagClothRatio + " FlagTailRatio: "+FlagTailRatio);

//                         Vector2 UpperVertexPffset, LowerVertexPffset;
//                         if (State == WAVE_STATE)
//                         {
//                             int extra = (int)DynamicParamManager.Get("StickOffsetList.extra").value;
//                             // Vector2 StickOffset = new Vector2(0, StickOffsetList[(int)MathHelper.Clamp(StickOffsetList.Count - (i + extra), 0, StickOffsetList.Count - 1)]);
//                             Vector2 StickOffset = new Vector2(0, (float)PolarPoints[i].r - PoleLength);
//                             SpinCenter = CenterMapping(OldCenter + FakeFlagOffset, StickOffset, OldRotation + ModGlobal.PI_FLOAT);
//                             UpperVertexPffset = new Vector2(-4f * Projectile.spriteDirection, -(PoleLength / 2f)) + StickOffset;
//                             LowerVertexPffset = new Vector2(-4f * Projectile.spriteDirection, (-PoleLength / 2f + FLAG_HEIGHT)) + StickOffset;
//                         }
//                         else
//                         {
//                             SpinCenter = OldCenter + FakeFlagOffset;
//                             UpperVertexPffset = new Vector2(-4f * Projectile.spriteDirection, -PoleLength / 2f);
//                             LowerVertexPffset = new Vector2(-4f * Projectile.spriteDirection, (-PoleLength / 2f + FLAG_HEIGHT));
//                         }

//                         float OldRot = (float)PolarPoints[i].theta;

//                         if (i < FlagClothLength)  // add flag cloth verteces
//                         {
//                             // Color b = new Color(255, 255, 255, 225);
//                             Color b = FakeFlagColor;
//                             FlagClothVerteces.Add(new Vertex(SpinCenter - Main.screenPosition + UpperVertexPffset.RotatedBy(OldRot),
//                                     new Vector3(1-FlagClothRatio, 0, 1),
//                                     b));
//                             FlagClothVerteces.Add(new Vertex(SpinCenter - Main.screenPosition + LowerVertexPffset.RotatedBy(OldRot),
//                                     new Vector3(1-FlagClothRatio, 1, 1),
//                                     b));
//                         }
//                         if (i >= FlagClothLength - OverlapSize && i < FlagClothLength + TailLength - OverlapSize)  // add flag tail verteces
//                         {
//                             byte alpha = (byte)(MathHelper.Clamp(FlagTailRatio * 255, 0, 255));
//                             Color tailColor = new Color(TAIL_COLOR.R, TAIL_COLOR.G, TAIL_COLOR.B, alpha);
//                             Color b = new Color(Math.Min(FakeFlagColor.R, tailColor.R), Math.Min(FakeFlagColor.G, tailColor.G), Math.Min(FakeFlagColor.B, tailColor.B), alpha);
//                             FlagTailVerteces.Add(new Vertex(SpinCenter - Main.screenPosition + UpperVertexPffset.RotatedBy(OldRot),
//                                     new Vector3(FlagTailRatio, 1, 1),
//                                     b));
//                             FlagTailVerteces.Add(new Vertex(SpinCenter - Main.screenPosition + LowerVertexPffset.RotatedBy(OldRot),
//                                     new Vector3(FlagTailRatio, 0, 1),
//                                     b));
//                         }

//                     }


//                     // draw flag cloth
//                     sb.End();
//                     sb.Begin(SpriteSortMode.Immediate, 
//                                             BlendState.AlphaBlend, //NonPremultiplied 
//                                             SamplerState.AnisotropicClamp, 
//                                             DepthStencilState.None, 
//                                             RasterizerState.CullNone, 
//                                             null, 
//                                             Main.GameViewMatrix.
//                                             TransformationMatrix);
//                     if(FlagClothVerteces.Count >= 3) // verteces should be at least 3 to form a triangle
//                     {
//                         gd.Textures[0] = ModContent.Request<Texture2D>(FLAG_CLOTH_TEXTURE_PATH).Value;
//                         gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, FlagClothVerteces.ToArray(), 0, FlagClothVerteces.Count - 2);
//                     }

//                     sb.End();
//                     sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

//                     if(FlagTailVerteces.Count >= 3) // verteces should be at least 3 to form a triangle
//                     {
//                         gd.Textures[0] = ModContent.Request<Texture2D>(FLAG_TAIL_TEXTURE_PATH).Value;
//                         // gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, FlagTailVerteces.ToArray(), 0, FlagTailVerteces.Count - 2);
//                     }

//                     sb.End();
//                     sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
//                 }
//             }

//             base.PredrawFlagClothDynamicVertices(ref lightColor, ClothCenter);
//         }

//         public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
//         {
//             if(State == RAISE_STATE || State == PLANT_STATE) return false;
//             Vector2 PoleStart = Projectile.Center + new Vector2(0, PoleLength/2f).RotatedBy(Projectile.rotation);
//             Vector2 PoleEnd = Projectile.Center + new Vector2(0, PoleLength/2f).RotatedBy(Projectile.rotation+Math.PI);
//             int CurrentTime = (int)(TIME_LEFT_WAVE / AttackSpeed) - Projectile.timeLeft;
//             if(State == RECALL_STATE) CurrentTime = TIME_LEFT_RECALL - Projectile.timeLeft;
//             Vector2 PoleOldStart = PoleStart;
//             Vector2 PoleOldEnd = PoleEnd;
//             int buffer = 0;
//             if(CurrentTime > buffer)
//             {
//                 PoleOldStart = Projectile.oldPos[buffer] + new Vector2(0, PoleLength/2f).RotatedBy(Projectile.oldRot[buffer]) + new Vector2(0, PoleLength / 2f);
//                 PoleOldEnd = Projectile.oldPos[buffer] + new Vector2(0, PoleLength/2f).RotatedBy(Projectile.oldRot[buffer]+Math.PI) + new Vector2(0, PoleLength / 2f);
//             }
//             float collisionPoint = 0f;
//             if(Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), PoleStart, PoleEnd, 10f, ref collisionPoint))
//             {
//                 return true;
//             }
//             float factor1 = 0.75f;
//             Vector2 PoleMidStart1 = factor1 * PoleStart + (1-factor1) * PoleOldStart;
//             Vector2 PoleMidEnd1 = factor1 * PoleEnd + (1-factor1) * PoleOldEnd;
//             if(Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), PoleMidStart1, PoleMidEnd1, 10f, ref collisionPoint))
//             {
//                 return true;
//             }
//             float factor2 = 0.25f;
//             Vector2 PoleMidStart2 = factor2 * PoleStart + (1-factor2) * PoleOldStart;
//             Vector2 PoleMidEnd2 = factor2 * PoleEnd + (1-factor2) * PoleOldEnd;
//             if(Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), PoleMidStart2, PoleMidEnd2, 10f, ref collisionPoint))
//             {
//                 return true;
//             }
//             //  Dust.QuickDustLine(PoleStart, PoleEnd, 10f, Color.Red);
//             //  Dust.QuickDustLine(PoleMidStart1, PoleMidEnd1, 10f, Color.Blue);
//             //  Dust.QuickDustLine(PoleMidStart2, PoleMidEnd2, 10f, Color.Green);
//             return false;
//         }
//     }
// }