using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.GameContent.ItemDropRules;

using Microsoft.Xna.Framework.Graphics;
using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.ModUtils;
using SummonerExpansionMod.Initialization;

using SummonerExpansionMod.Content.Items.Weapons.Summon;

namespace SummonerExpansionMod.Content.Items
{
    public class VanillaBossBagLootModification : GlobalItem
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot) {
			// In addition to this code, we also do similar code in Common/GlobalNPCs/ExampleNPCLoot.cs to edit the boss loot for non-expert drops. Remember to do both if your edits should affect non-expert drops as well.
			if (item.type == ItemID.PlanteraBossBag) {
				// The following code is attempting to retrieve the ItemDropRule found in the ItemDropDatabase.RegisterBossBags method:
				// RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.BeeGun, ItemID.BeeKeeper, ItemID.BeesKnees));
				foreach (var rule in itemLoot.Get()) {
					if (rule is OneFromRulesRule oneFromOptionsDrop) {

						bool found = false;
						foreach (var subRule in oneFromOptionsDrop.options) {
							if (subRule is CommonDrop commonDrop && commonDrop.itemId == ItemID.GrenadeLauncher) {
								// Console.WriteLine(commonDrop.GetType().Name + " " + commonDrop.itemId + " " + (commonDrop == ItemDropRule.Common(ItemID.Seedler)));
								found = true;
								break;
							}
						}

						if (found) {
							var original = oneFromOptionsDrop.options.ToList();
							original.Add(ItemDropRule.Common(ModContent.ItemType<GiantLeavesOfPlantera>()));
							oneFromOptionsDrop.options = original.ToArray();
						}

					}
					// Console.WriteLine(rule.GetType().Name);
				}
			}
		}
    }
}
