using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace test1.Content.Items.Weapons.Summon
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
                // 在这里实现右键的功能，例如释放特殊攻击
                Main.NewText("You used the SlimeStaff with right-click!", 255, 255, 0);
            }
            return base.UseItem(item, player);
        }
    }
}