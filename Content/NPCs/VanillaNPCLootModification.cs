using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
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
using System.Linq;
using SummonerExpansionMod.Content.Items.Weapons.Summon;

namespace SummonerExpansionMod.Content.NPCs
{
    public class VanillaNPCLootModification : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if(npc.type == NPCID.CursedSkull)
            {
                npcLoot.Add(
                    ItemDropRule.Common(
                        ModContent.ItemType<DarkMagicTowerStaff>(),
                        50,  // drop rate 
                        1, // min count
                        1 // max count
                    )
                );
            }
            if(npc.type == NPCID.Mothron)
            {
                npcLoot.Add(
                    ItemDropRule.Common(
                        ModContent.ItemType<MothronQueenTurretStaff>(),
                        (int)(1f / 0.33f),  // drop rate 
                        1, // min count
                        1 // max count
                    )
                );
            }
            if(npc.type == NPCID.GoblinPeon || npc.type == NPCID.GoblinThief || npc.type == NPCID.GoblinWarrior)
            {
                npcLoot.Add(
                    ItemDropRule.Common(
                        ModContent.ItemType<GoblinFlag>(),
                        (int)(1f / 0.05f),  // drop rate 
                        1, // min count
                        1 // max count
                    )
                );
            }

            if(npc.type == NPCID.PirateShip)
            {
                npcLoot.Add(
                    ItemDropRule.Common(
                        ModContent.ItemType<PirateFlag>(),
                        (int)(1f / 0.25f),  // drop rate 
                        1, // min count
                        1 // max count
                    )
                );
            }

            if(npc.type == NPCID.SantaNK1)
            {
                foreach (var rule in npcLoot.Get())
                {
                    if(rule is LeadingConditionRule leadingConditionRule)
                    {
                        foreach (var subRule in leadingConditionRule.ChainedRules)
                        {
                            if (subRule.RuleToChain is OneFromOptionsDropRule oneFromOptionsDrop && oneFromOptionsDrop.dropIds.Contains(ItemID.ElfMelter))
                            {
                                var original = oneFromOptionsDrop.dropIds.ToList();
                                original.Add(ModContent.ItemType<SantaFlag>());
                                oneFromOptionsDrop.dropIds = original.ToArray();
                            }
                        }
                    }
                }
            }

            if(npc.type == NPCID.MartianSaucerCore)
            {
                foreach (var rule in npcLoot.Get())
                {
                    if(rule is OneFromOptionsNotScaledWithLuckDropRule oneFromOptionsDrop && oneFromOptionsDrop.dropIds.Contains(ItemID.InfluxWaver))
                    {
                        var original = oneFromOptionsDrop.dropIds.ToList();
                        original.Add(ModContent.ItemType<OneTrueFlag>());
                        oneFromOptionsDrop.dropIds = original.ToArray();
                    }
                    // Console.WriteLine(rule.GetType().Name);
                }
            }
        }
    }
}
