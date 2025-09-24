using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using SummonerExpansionMod.Content.Projectiles.Summon;
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.Items.Accessories
{
    [AutoloadEquip(EquipType.Back)]
    public class SuperEarthCloak : ModItem
    {
        public override void SetDefaults()
        {
			Item.width = 26;
			Item.height = 30;
			Item.maxStack = 1;
			Item.value = Item.sellPrice(gold: 5);
			Item.accessory = true;
			Item.rare = ItemRarityID.Red;
        }

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetModPlayer<SuperEarthCloakPlayer>().hasAccessory = true;
        }
    }

    public class SuperEarthCloakPlayer : ModPlayer
    {
        public bool hasAccessory = false;

        public override void ResetEffects() {
            hasAccessory = false;
        }


        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers) {
            if (hasAccessory) {
                if(proj.owner == Player.whoAmI) {
                    // decrease dealt damage and completely disable knockback
                    float t = 200f / 19f;
                    float dynamicDecayCoeff = (float)MathHelper.Clamp(t / (t + proj.damage), 0.005f, 0.8f);
                    modifiers.FinalDamage *= dynamicDecayCoeff;
                    modifiers.Knockback *= 0f;
                    Main.NewText("dynamicDecayCoeff: " + dynamicDecayCoeff  + " SourceDamage: " + proj.damage);
                }
                // Main.NewText("hasAccessory: " + hasAccessory + " proj.owner: " + proj.owner + " Player.whoAmI: " + Player.whoAmI);
            }
        }
    }
}