using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;

using SummonerExpansionMod.Content.Projectiles.Summon;
using SummonerExpansionMod.Initialization;
using SummonerExpansionMod.ModUtils;

namespace SummonerExpansionMod.Content.Items.Armors
{
    public class VanillaHallowedArmorOverridePlayer : ModPlayer
    {
        public override void PostUpdateEquips()
        {
            // 神圣盔甲
            if (Player.head == ArmorIDs.Head.HallowedHood)
            {
                Player.maxTurrets += 1;
                if (Player.body == ArmorIDs.Body.HallowedPlateMail && Player.legs == ArmorIDs.Legs.HallowedGreaves)
                {
                    Player.maxTurrets += 1;
                }
            }

            // 远古神圣盔甲
            if (Player.head == ArmorIDs.Head.AncientHallowedHood)
            {
                Player.maxTurrets += 1;
                if (Player.body == ArmorIDs.Body.AncientHallowedPlateMail && Player.legs == ArmorIDs.Legs.AncientHallowedGreaves)
                {
                    Player.maxTurrets += 1;
                }
            }

            

            // 阴森盔甲
            if (Player.head == ArmorIDs.Head.SpookyHelmet)
            {
                Player.maxTurrets += 1;
            }
            if (Player.body == ArmorIDs.Body.SpookyBreastplate)
            {
                Player.maxTurrets += 1;
            }
            if (Player.legs == ArmorIDs.Legs.SpookyLeggings)
            {
                Player.maxTurrets += 1;
            }

            // 提基盔甲
            if (Player.head == ArmorIDs.Head.TikiMask)
            {
                Player.maxTurrets += 1;
            }
            if (Player.body == ArmorIDs.Body.TikiShirt)
            {
                Player.maxTurrets += 1;
            }
            if (Player.legs == ArmorIDs.Legs.TikiPants)
            {
                Player.maxTurrets += 1;
            }
        }
    }
}