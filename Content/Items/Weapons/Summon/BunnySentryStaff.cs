using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using SummonerExpansionMod.Content.Projectiles.Summon;
using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.Initialization;
using SummonerExpansionMod.ModUtils;

namespace SummonerExpansionMod.Content.Items.Weapons.Summon
{
    public class BunnySentryStaff : ModItem
    {
        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Items/BunnySentryStaff";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;

            ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f; // The default value is 1, but other values are supported. See the docs for more guidance. 
        }

        public override void SetDefaults()
        {
            Item.damage = 11;
            Item.knockBack = 3f;
            Item.mana = 10; // mana cost
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing; // how the player's arm moves when using the item
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item44; // What sound should play when using the item

            // These below are needed for a minion weapon
            Item.noMelee = true; // this item doesn't do any melee damage
            Item.DamageType = DamageClass.Summon; // Makes the damage register as summon. If your item does not have any damage type, it becomes true damage (which means that damage scalars will not affect it). Be sure to have a damage type
            // Item.buffType = ModContent.BuffType<BunnySentryBuff>();
            // No buffTime because otherwise the item tooltip would say something like "1 minute duration"
            Item.shoot = ModContent.ProjectileType<BunnySentry>(); // This item creates the minion projectile
            // Item.shoot = ModContent.ProjectileType<BabySlimeOverride>();
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Projectile projTemplate = ProjectileLoader.GetProjectile(type).Projectile;
            Vector2? result = MinionAIHelper.SearchSpawnPoint(Main.MouseWorld, projTemplate.width, (int)(projTemplate.height*1.2f));
            position = result ?? Main.MouseWorld;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
            // player.AddBuff(Item.buffType, 2);

            // Minions have to be spawned manually, then have originalDamage assigned to the damage of the summon item
            var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
            projectile.originalDamage = Item.damage;

            player.UpdateMaxTurrets();

            // Since we spawned the projectile manually already, we do not need the game to spawn it for ourselves anymore, so return false
            return false;
        }

        public override void AddRecipes() {
        	Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Bunny, 1);
            recipe.AddIngredient(ItemID.Boomstick, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            Recipe recipe2 = CreateRecipe();
            recipe2.AddIngredient(ItemID.Bunny, 1);
            recipe2.AddIngredient(ItemID.QuadBarrelShotgun, 1);
            recipe2.AddTile(TileID.Anvils);
            recipe2.Register();

            Recipe recipe3 = CreateRecipe();
            recipe3.AddIngredient(ItemID.Bunny, 1);
            recipe3.AddIngredient(ItemID.TheUndertaker, 1);
            recipe3.AddTile(TileID.Anvils);
            recipe3.Register();

            Recipe recipe4 = CreateRecipe();
            recipe4.AddIngredient(ItemID.Bunny, 1);
            recipe4.AddIngredient(ItemID.Revolver, 1);
            recipe4.AddTile(TileID.Anvils);
            recipe4.Register();

            Recipe recipe5 = CreateRecipe();
            recipe5.AddIngredient(ItemID.Bunny, 1);
            recipe5.AddIngredient(ItemID.Minishark, 1);
            recipe5.AddTile(TileID.Anvils);
            recipe5.Register();

            Recipe recipe6 = CreateRecipe();
            recipe6.AddIngredient(ItemID.Bunny, 1);
            recipe6.AddIngredient(ItemID.Handgun, 1);
            recipe6.AddTile(TileID.Anvils);
            recipe6.Register();

            Recipe recipe7 = CreateRecipe();
            recipe7.AddIngredient(ItemID.Bunny, 1);
            recipe7.AddIngredient(ItemID.Musket, 1);
            recipe7.AddTile(TileID.Anvils);
            recipe7.Register();

            Recipe recipe8 = CreateRecipe();
            recipe8.AddIngredient(ItemID.Bunny, 1);
            recipe8.AddIngredient(ItemID.FlintlockPistol, 1);
            recipe8.AddTile(TileID.Anvils);
            recipe8.Register();

            Recipe recipe9 = CreateRecipe();
            recipe9.AddIngredient(ItemID.Bunny, 1);
            recipe9.AddIngredient(ItemID.Shotgun, 1);
            recipe9.AddTile(TileID.Anvils);
            recipe9.Register();

            Recipe recipe10 = CreateRecipe();
            recipe10.AddIngredient(ItemID.Bunny, 1);
            recipe10.AddIngredient(ItemID.RedRyder, 1);
            recipe10.AddTile(TileID.Anvils);
            recipe10.Register();
        }
    }
}