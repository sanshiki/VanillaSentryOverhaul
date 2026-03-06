using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SummonerExpansionMod.Content.Items.Weapons.Summon;
public class CustomShimmerSystem : ModSystem
{
    public override void PostSetupContent()
    {
        ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<GiantLeavesOfPlantera>()] =
            ItemID.PygmyStaff;
        ItemID.Sets.ShimmerTransformToItem[ItemID.PygmyStaff] =
            ModContent.ItemType<GiantLeavesOfPlantera>();
    }
}