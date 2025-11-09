using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using SummonerExpansionMod.Content.Projectiles.Summon;
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.Buffs.Summon
{
	public class OneTrueFlagDebuff : ModBuff
	{
        public static readonly int TagDamage = 20;
        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Buffs/OneTrueFlagBuff";

		public override void SetStaticDefaults() {
			// This allows the debuff to be inflicted on NPCs that would otherwise be immune to all debuffs.
			// Other mods may check it for different purposes.
			BuffID.Sets.IsATagBuff[Type] = true;
		}
	}

	public class OneTrueFlagDebuffNPC : GlobalNPC
	{
		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) {
			// Only player attacks should benefit from this buff, hence the NPC and trap checks.
			if (projectile.npcProj || projectile.trap || !projectile.IsMinionOrSentryRelated)
				return;


			// SummonTagDamageMultiplier scales down tag damage for some specific minion and sentry projectiles for balance purposes.
			var projTagMultiplier = ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type];
			if (npc.HasBuff<OneTrueFlagDebuff>()) {
				// Apply a flat bonus to every hit
				modifiers.FlatBonusDamage += OneTrueFlagDebuff.TagDamage * projTagMultiplier;
			}
		}
	}
}