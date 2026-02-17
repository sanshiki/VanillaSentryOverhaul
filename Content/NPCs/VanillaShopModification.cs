using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.Localization;

using Microsoft.Xna.Framework.Graphics;
using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.ModUtils;
using SummonerExpansionMod.Initialization;

using SummonerExpansionMod.Content.Items.Weapons.Summon;
using SummonerExpansionMod.Content.Items.Armors;

namespace SummonerExpansionMod.Content.NPCs
{
    public class VanillaShopModification : GlobalNPC
    {
        public override void ModifyShop(NPCShop shop)
        {
            if(shop.NpcType == NPCID.Dryad)
            {
                shop.Add(
                    ModContent.ItemType<TowerOfDryadsBlessingStaff>()
                );
            }
            if(shop.NpcType == NPCID.WitchDoctor)
            {
                shop.Add(
                    ModContent.ItemType<TikiVisage>(),
                    Condition.DownedPlantera
                );
            }
        }
    }
}
