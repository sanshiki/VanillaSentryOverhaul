using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SummonerExpansionMod.ModUtils;
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.Items.Weapons.Melee
{
	public class ExampleSword : ModItem
    {
        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Projectiles/VertexTest";
		public override void SetDefaults() {
			Item.width = 40; // The item texture's width.
			Item.height = 40; // The item texture's height.

			Item.useStyle = ItemUseStyleID.Swing; // The useStyle of the Item.
			Item.useTime = 20; // The time span of using the weapon. Remember in terraria, 60 frames is a second.
			Item.useAnimation = 20; // The time span of the using animation of the weapon, suggest setting it the same as useTime.
			Item.autoReuse = true; // Whether the weapon can be used more than once automatically by holding the use button.

			Item.DamageType = DamageClass.Melee; // Whether your item is part of the melee class.
			Item.damage = 50; // The damage your item deals.
			Item.knockBack = 6; // The force of knockback of the weapon. Maximum is 20
			Item.crit = 6; // The critical strike chance the weapon has. The player, by default, has a 4% critical strike chance.

			Item.value = Item.buyPrice(gold: 1); // The value of the weapon in copper coins.
            Item.UseSound = SoundID.Item1; // The sound when the weapon is being used.

            DynamicParamManager.Register("SearchWidth", 32, 1, 100);
            DynamicParamManager.Register("SearchHeight", 32, 1, 100);
		}

		public override void MeleeEffects(Player player, Rectangle hitbox) {
            int width = (int)DynamicParamManager.Get("SearchWidth").value;
            int height = (int)DynamicParamManager.Get("SearchHeight").value;
            Vector2 Pos = MinionAIHelper.SearchForGround(player.Center+new Vector2(0, -50f), 10, width, height);
            Rectangle PosRect = new Rectangle((int)Pos.X - width / 2, (int)Pos.Y - height, width, height);
            Dust.QuickDustLine(PosRect.BottomLeft(), PosRect.TopLeft(), 10, Color.Yellow);
            Dust.QuickDustLine(PosRect.BottomRight(), PosRect.TopRight(), 10, Color.Yellow);
            Dust.QuickDustLine(PosRect.TopLeft(), PosRect.TopRight(), 10, Color.Yellow);
            Dust.QuickDustLine(PosRect.BottomLeft(), PosRect.BottomRight(), 10, Color.Yellow);
		}
	}
}