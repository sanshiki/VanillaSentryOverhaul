using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

using SummonerExpansionMod.Content.Projectiles.Summon;
using SummonerExpansionMod.Initialization;
using SummonerExpansionMod.ModUtils;

namespace SummonerExpansionMod.Content.Items.Armors
{
    public class VanillaHallowedArmorOverridePlayer : ModPlayer
    {
        private float sentryCount = 0f;

        public override void ResetEffects()
        {
            sentryCount = 0f;
        }

        public override void PostUpdateEquips()
        {
            // 神圣盔甲
            if (Player.head == ArmorIDs.Head.HallowedHood)
            {
                Player.maxTurrets += 1;
                if (Player.body == ArmorIDs.Body.HallowedPlateMail && Player.legs == ArmorIDs.Legs.HallowedGreaves)
                {
                    Player.maxTurrets += 1;

                    sentryCount = 0f;
                    foreach (Projectile p in Main.projectile)
                    {
                        if (!p.active || p.owner != Player.whoAmI)
                            continue;

                        if (p.sentry)
                            sentryCount++;
                    }

                    Player.statDefense += (int)(sentryCount * 1f);
                }
            }

            // 远古神圣盔甲
            if (Player.head == ArmorIDs.Head.AncientHallowedHood)
            {
                Player.maxTurrets += 1;
                if (Player.body == ArmorIDs.Body.AncientHallowedPlateMail && Player.legs == ArmorIDs.Legs.AncientHallowedGreaves)
                {
                    Player.maxTurrets += 1;

                    sentryCount = 0f;
                    foreach (Projectile p in Main.projectile)
                    {
                        if (!p.active || p.owner != Player.whoAmI)
                            continue;

                        if (p.sentry)
                            sentryCount++;
                    }

                    Player.statDefense += (int)(sentryCount * 1f);
                }
            }

            

            // 阴森盔甲
            // if (Player.head == ArmorIDs.Head.SpookyHelmet)
            // {
            //     Player.maxTurrets += 1;
            // }
            if (Player.body == ArmorIDs.Body.SpookyBreastplate)
            {
                Player.maxTurrets += 1;
            }
            if (Player.legs == ArmorIDs.Legs.SpookyLeggings)
            {
                Player.maxTurrets += 1;
            }

            // 提基盔甲
            // if (Player.head == ArmorIDs.Head.TikiMask)
            // {
            //     Player.maxTurrets += 1;
            // }
            if (Player.body == ArmorIDs.Body.TikiShirt)
            {
                Player.maxTurrets += 1;
            }
            if (Player.legs == ArmorIDs.Legs.TikiPants)
            {
                Player.maxTurrets += 1;
            }

            if(Player.body == ArmorIDs.Body.StardustPlate)
            {
                Player.maxTurrets += 1;
            }
            if (Player.legs == ArmorIDs.Legs.StardustLeggings)
            {
                Player.maxTurrets += 1;
            }

            if (Player.head == ArmorIDs.Head.StardustHelmet &&
                Player.body == ArmorIDs.Body.StardustPlate &&
                Player.legs == ArmorIDs.Legs.StardustLeggings)
            {
                sentryCount = 0f;
                foreach (Projectile p in Main.projectile)
                {
                    if (!p.active || p.owner != Player.whoAmI)
                        continue;

                    if (p.sentry)
                        sentryCount++;
                }

                Player.statDefense += (int)(sentryCount * 1f);
            }
        }
    }

    public class VanillaArmorOverride : GlobalItem
    {
        public static LocalizedText HallowSetBonusText { get; private set; }
        public static LocalizedText StardustSetBonusExtraDefenseText { get; private set; }
        public static LocalizedText AddSentryToTooltipText { get; private set; }

        public static List<int> AddSentryToTooltipItemTypes = new List<int>()
        {
            ItemID.HallowedHood,
            ItemID.AncientHallowedHood,
            // ItemID.HallowedPlateMail,
            // ItemID.AncientHallowedPlateMail,
            // ItemID.HallowedGreaves,
            // ItemID.AncientHallowedGreaves,
            ItemID.TikiShirt,
            ItemID.TikiPants,
            ItemID.SpookyBreastplate,
            ItemID.SpookyLeggings,
            ItemID.StardustBreastplate,
            ItemID.StardustLeggings,
        };

        public override void SetStaticDefaults()
        {
            HallowSetBonusText = Mod.GetLocalization("HallowSetBonus");
            StardustSetBonusExtraDefenseText = Mod.GetLocalization("StardustSetBonusExtraDefense");
            AddSentryToTooltipText = Mod.GetLocalization("AddSentryToTooltip");
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.type == ItemID.HallowedHood || item.type == ItemID.AncientHallowedHood ||
                item.type == ItemID.HallowedPlateMail || item.type == ItemID.AncientHallowedPlateMail ||
                item.type == ItemID.HallowedGreaves || item.type == ItemID.AncientHallowedGreaves)
            {
                foreach (var line in tooltips)
                {
                    // Main.NewText(line.Name + ": " + line.Text);
                    // Console.WriteLine(line.Name + ": " + line.Text);
                    if(line.Mod == "Terraria" && line.Name == "SetBonus")
                    {
                        string extraSetBonus = HallowSetBonusText.Value?.Trim() ?? string.Empty;

                        // Keep vanilla set bonus text and only append mod-added lines.
                        if (extraSetBonus.StartsWith(line.Text, StringComparison.Ordinal))
                        {
                            extraSetBonus = extraSetBonus.Substring(line.Text.Length).TrimStart('\r', '\n');
                        }

                        if (!string.IsNullOrWhiteSpace(extraSetBonus))
                        {
                            line.Text = $"{line.Text}\n{extraSetBonus}";
                        }
                    }
                }
            }

            if (item.type == ItemID.StardustHelmet || item.type == ItemID.StardustBreastplate || item.type == ItemID.StardustLeggings)
            {
                foreach (var line in tooltips)
                {
                    if (line.Mod == "Terraria" && line.Name == "SetBonus")
                    {
                        string extraSetBonus = StardustSetBonusExtraDefenseText.Value?.Trim() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(extraSetBonus))
                        {
                            line.Text = $"{line.Text}\n{extraSetBonus}";
                        }
                    }
                }
            }

            if (AddSentryToTooltipItemTypes.Contains(item.type))
            {
                foreach (var line in tooltips)
                {
                    if(line.Mod == "Terraria" && line.Name == "Tooltip0")
                    {
                        line.Text = AddSentryToTooltipText.Value;
                    }
                }
            }
           
        }
    }
}