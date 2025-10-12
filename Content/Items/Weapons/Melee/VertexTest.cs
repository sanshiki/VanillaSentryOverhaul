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

namespace SummonerExpansionMod.Content.Items.Weapons.Melee
{
    public class VertexTest : ModItem
    {
        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Projectiles/VertexTest";
        public override void SetDefaults()
        {
            Item.damage = 50;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HiddenAnimation;
            Item.knockBack = 6;
            Item.value = Item.buyPrice(silver: 1);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<VertexTestProj>();
            Item.noUseGraphic = true;
        }
    }
    /// <summary>
    /// 请注意 这只是一个用于演示 如何画出刀光拖尾 的教程，不包含其他内容 比如弹幕的碰撞箱等等是错误的
    /// </summary>
    public class VertexTestProj : ModProjectile
    {
        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Projectiles/VertexTest";
        private const string SWORD_TAIL_TEXTURE_PATH = ModGlobal.MOD_TEXTURE_PATH + "Vertexes/SwordTail3";


        private const int TAIL_LENGTH = 20;
        public override void SetDefaults()
        {
		Projectile.width = Projectile.height = 40;
            Projectile.friendly = true;//友方弹幕
            Projectile.tileCollide = false;//穿墙
            Projectile.aiStyle = -1;//不使用原版AI
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;//无限穿透
            Projectile.ignoreWater = true;//无视液体

            // DynamicParamManager.Register("TailLength", 20, 5, 20);
            // DynamicParamManager.Register("VertexColor.R", 255, 0, 255);
            // DynamicParamManager.Register("VertexColor.G", 255, 0, 255);
            // DynamicParamManager.Register("VertexColor.B", 255, 0, 255);
            // DynamicParamManager.Register("VertexColor.A", 255, 0, 255);

		//或者让它不死 一直转(
		//Projectile.timeLeft = 20;//弹幕 趋势 的时间

            base.SetDefaults();
        }
        public override void SetStaticDefaults()//以下照抄
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;//这一项赋值2可以记录运动轨迹和方向（用于制作拖尾）
            ProjectileID.Sets.TrailCacheLength[Type] = TAIL_LENGTH;//这一项代表记录的轨迹最多能追溯到多少帧以前
            base.SetStaticDefaults();
        }

        Player player => Main.player[Projectile.owner];//获取玩家
        public override void AI()//模拟"刀"的挥舞逻辑
        {
		player.itemTime = player.itemAnimation = 3;//防止弹幕没有 趋势 玩家就又可以使用武器了
		Projectile.Center = player.Center;//绑定玩家和弹幕的位置
		Projectile.velocity = new Vector2(0, -10).RotatedBy(Projectile.rotation);//给弹幕一个速度 仅仅用于击退方向
		Projectile.rotation += 0.32f * player.direction;//弹幕旋转角度
		player.heldProj = Projectile.whoAmI;//使弹幕的贴图画出来后 夹 在角色的身体和手之间

		//以下为升级内容
		/*if (Projectile.rotation > MathHelper.Pi)
			Projectile.rotation = -MathHelper.Pi;
            if (Projectile.rotation < -MathHelper.Pi)
                Projectile.rotation = MathHelper.Pi;*/
		if (player.controlUseItem)
			Projectile.timeLeft = 2;//让弹幕一直转圈圈的方法之一
            base.AI();
        }
        public override bool ShouldUpdatePosition()
        {
		return false;//让弹幕位置不受速度影响
        }
        public override bool PreDraw(ref Color lightColor)
        {
		//缩写这俩 我懒得在后面打长长的东西
		SpriteBatch sb = Main.spriteBatch;
		GraphicsDevice gd = Main.graphics.GraphicsDevice;

		//end 和 begin里和顶点的东西建议照抄 然后慢慢理解

		sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		//开始顶点绘制

		List<Vertex> ve = new List<Vertex>();
        // int tailLength = (int)DynamicParamManager.Get("TailLength").value;
        // Color vertexColor = new Color((int)DynamicParamManager.Get("VertexColor.R").value, (int)DynamicParamManager.Get("VertexColor.G").value, (int)DynamicParamManager.Get("VertexColor.B").value, (int)DynamicParamManager.Get("VertexColor.A").value);
        int tailLength = TAIL_LENGTH;
        Color vertexColor = Color.White;

		for(int i = 0; i < tailLength;i++)
		{
			// Color b = Color.Lerp(Color.Red, Color.Blue, i / (float)TAIL_LENGTH);
            // Color b = Color.White;
            float ratio = i / (float)tailLength;
            float color_rate = MathHelper.Clamp(ratio*3, 0, 1);
            // Color b = new Color(255, (int)(255*color_rate), (int)(255*color_rate), (int)(255*color_rate));
            Color b = vertexColor;

			//存顶点																										从这一—————————————到这里都是乱弄的 你可以随便改改数据看看能发生什么
			ve.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -80).RotatedBy(Projectile.oldRot[i])/*  * (1 + (float)Math.Cos(Projectile.oldRot[i] - MathHelper.PiOver2) * player.direction) */,
                      new Vector3(ratio, 1, 1),
                      b));
			ve.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -1).RotatedBy(Projectile.oldRot[i])/*  * (1 + (float)Math.Cos(Projectile.oldRot[i] - MathHelper.PiOver2) * player.direction) */,
                      new Vector3(ratio, 0, 1),
                      b));
		}

		if(ve.Count >= 3)//因为顶点需要围成一个三角形才能画出来 所以需要判顶点数>=3 否则报错
		{
			gd.Textures[0] = ModContent.Request<Texture2D>(SWORD_TAIL_TEXTURE_PATH).Value;//获取刀光的拖尾贴图
			gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, ve.ToArray(), 0, ve.Count - 2);//画
            }

		//结束顶点绘制
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);


		//画出这把 剑 的样子
		Main.spriteBatch.Draw(TextureAssets.Projectile[Type].Value,
                         Projectile.Center - Main.screenPosition,
                         null,
                         lightColor,
                         Projectile.rotation - MathHelper.PiOver4,
                         new Vector2(0, 40),
                         1.5f,
                         SpriteEffects.None,
                         0);

		return false;//让弹幕不画原来的样子
        }
    }
}