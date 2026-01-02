using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;

using SummonerExpansionMod.Content.Projectiles.Summon;
using SummonerExpansionMod.Initialization;
using SummonerExpansionMod.ModUtils;

namespace SummonerExpansionMod.Content.Items.Accessories
{
    public class SentinelTalismanLv3 : ModItem
    {
        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Items/SentinelTalismanLv3";
        public override void SetDefaults()
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Item.width = texture.Width;
			Item.height = texture.Height;
			Item.maxStack = 1;
			Item.value = Item.sellPrice(gold: 5);
			Item.accessory = true;
			Item.rare = ItemRarityID.Red;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<SentinelTalismanLv3Player>().hasAccessory = true;
        }
    }

    public class SentinelTalismanLv3Player : ModPlayer
    {
        public bool hasAccessory = false;

        public override void ResetEffects() {
            hasAccessory = false;
        }

        public override void PostUpdate()
        {
            // Main.NewText("hasAccessory: " + hasAccessory + " maxTurrets: " + Player.maxTurrets);
            if (hasAccessory && Player.maxTurrets >= 5) {
                int armorDefense = 0;

                for (int i = 0; i < 3; i++)
                {
                    Item armorPiece = Player.armor[i];
                    if (armorPiece != null && !armorPiece.IsAir)
                    {
                        armorDefense += armorPiece.defense;
                    }
                }

                int bonusDefense = MinionAIHelper.DefenseCompensate(armorDefense, 60, 0.5f, 25f, 25f);

                Player.statDefense += bonusDefense;
                Player.maxTurrets += 1;
            }
        }
    }
}