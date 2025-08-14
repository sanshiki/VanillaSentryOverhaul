using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;


using SummonerExpansionMod.Content.Projectiles.Summon;
using SummonerExpansionMod.Content.Buffs.Summon;

namespace SummonerExpansionMod.Content.Items.Weapons.Summon
{
    public class MyGlobalItem : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            // 判断是否为木剑
            if (item.type == ItemID.SlimeStaff)
            {
                // 修改木剑的属性
                item.damage = 50; // 修改伤害
                item.useTime = 10; // 修改使用速度
                item.useAnimation = 10; // 修改动画时间
                item.knockBack = 6f; // 修改击退
                item.value = 10000; // 修改物品价值
                item.rare = ItemRarityID.Green; // 修改物品稀有度
                // item.shoot = ModContent.ProjectileType<ExampleSimpleMinion>();
                item.shoot = ModContent.ProjectileType<BabySlimeOverride>();
            }

            if (item.type == ItemID.CopperAxe)
            {
                item.damage = 5000; // 修改伤害
                item.useTime = 1; // 修改使用速度
            }
        }

        public override bool AltFunctionUse(Item item, Player player)
        {
            if (item.type == ItemID.SlimeStaff)
            {
                return true;
            }
            return base.AltFunctionUse(item, player);
        }

        public override bool? UseItem(Item item, Player player)
        {
            if (item.type == ItemID.SlimeStaff && player.altFunctionUse == 2) // 如果是右键
            {
                SlimeOverridePlayer modPlayer = player.GetModPlayer<SlimeOverridePlayer>();
                modPlayer.useOverrideMinion = !modPlayer.useOverrideMinion;
                // 在这里实现右键的功能，例如释放特殊攻击
                // Main.NewText("You used the SlimeStaff with right-click!", 255, 255, 0);
                string msg = modPlayer.useOverrideMinion
                    ? "切换到自定义史莱姆召唤物"
                    : "切换回原版史莱姆宝宝";
                Main.NewText(msg, 100, 200, 255);
            }
            return base.UseItem(item, player);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (item.type == ItemID.SlimeStaff)
            {
                var modPlayer = player.GetModPlayer<SlimeOverridePlayer>();
                int projType = modPlayer.useOverrideMinion
                    ? ModContent.ProjectileType<BabySlimeOverride>()
                    : ProjectileID.BabySlime;

                // int buffType = modPlayer.useOverrideMinion
                //     ? ModContent.BuffType<BabySlimeOverrideBuff>()
                //     : BuffID.BabySlime;
                int buffType = BuffID.BabySlime;

                player.AddBuff(buffType, 2);
                Projectile.NewProjectile(source, position, velocity, projType, damage, knockback, player.whoAmI);
                return false; // 阻止默认发射
            }
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }

    }

    public class SlimeOverridePlayer : ModPlayer
    {
        public bool useOverrideMinion = false;

        public override void ResetEffects()
        {
            // 保持召唤物存在不会影响此变量
        }
    }

}