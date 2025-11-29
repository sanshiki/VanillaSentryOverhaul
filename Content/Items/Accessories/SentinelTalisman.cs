using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using SummonerExpansionMod.Content.Projectiles.Summon;
using SummonerExpansionMod.Initialization;
using SummonerExpansionMod.ModUtils;

namespace SummonerExpansionMod.Content.Items.Accessories
{
    public class SentinelTalisman : ModItem
    {
        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Items/DummyStaff";
        public override void SetDefaults()
        {
			Item.width = 26;
			Item.height = 30;
			Item.maxStack = 1;
			Item.value = Item.sellPrice(gold: 5);
			Item.accessory = true;
			Item.rare = ItemRarityID.Red;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 判断当前哨兵数量是否 ≥ 3
            // Main.NewText("maxTurrets: " + player.maxTurrets);
            // if (player.maxTurrets >= 3)
            // {
            //     int armorDefense = 0;

            //     // 计算盔甲防御
            //     for (int i = 0; i < 3; i++)
            //     {
            //         Item armorPiece = player.armor[i];
            //         if (armorPiece != null && !armorPiece.IsAir)
            //         {
            //             armorDefense += armorPiece.defense;
            //         }
            //     }

            //     // 动态防御补偿计算
            //     int bonusDefense = MinionAIHelper.DefenseCompensate(armorDefense, 60);

            //     player.statDefense += bonusDefense;
            // }
            player.GetModPlayer<SentinelTalismanPlayer>().hasAccessory = true;
        }
    }

    public class SentinelTalismanPlayer : ModPlayer
    {
        public bool hasAccessory = false;

        public override void ResetEffects() {
            hasAccessory = false;
        }

        public override void PostUpdate()
        {
            // Main.NewText("hasAccessory: " + hasAccessory + " maxTurrets: " + Player.maxTurrets);
            if (hasAccessory && Player.maxTurrets >= 3) {
                int armorDefense = 0;

                for (int i = 0; i < 3; i++)
                {
                    Item armorPiece = Player.armor[i];
                    if (armorPiece != null && !armorPiece.IsAir)
                    {
                        armorDefense += armorPiece.defense;
                    }
                }

                int bonusDefense = MinionAIHelper.DefenseCompensate(armorDefense, 100, 0.5f, 26f, 15f);

                Player.statDefense += bonusDefense;
            }
        }
    }
}