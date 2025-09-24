using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace SummonerExpansionMod.Initialization
{
    public class ModGlobal
    {
        public const float SENTRY_GRAVITY = 0.2f;
        public const float SENTRY_MAX_FALL_SPEED = 20f;
        public const float PI_FLOAT = (float)Math.PI;
        public const float TWO_PI_FLOAT = (float)(Math.PI * 2);
        public const float DEG_TO_RAD_FLOAT = (float)(Math.PI / 180);
        public const float RAD_TO_DEG_FLOAT = (float)(180 / Math.PI);
        
        public const string SUMMON_PRJECTILE_PATH = "SummonerExpansionMod/Content/Projectiles/Summon/";

        public const string MOD_TEXTURE_PATH = "SummonerExpansionMod/Assets/Textures/";

        public const string VANILLA_PROJECTILE_TEXTURE_PATH = "Terraria/Images/Projectile_";

        public const string VANILLA_NPC_TEXTURE_PATH = "Terraria/Images/NPC_";

        public const string VANILLA_ITEM_TEXTURE_PATH = "Terraria/Images/Item_";

        public const string VANILLA_BUFF_TEXTURE_PATH = "Terraria/Images/Buff_";

        public static Dictionary<int, int> WhipAddDamageDict = new Dictionary<int, int>()
        {
            {BuffID.BlandWhipEnemyDebuff, 4},
            {BuffID.ThornWhipNPCDebuff, 6},
            {BuffID.BoneWhipNPCDebuff, 7},
            {BuffID.FlameWhipEnemyDebuff, 0},
            {BuffID.CoolWhipNPCDebuff, 6},
            {BuffID.SwordWhipNPCDebuff, 9},
            {BuffID.ScytheWhipEnemyDebuff, 10},
            {BuffID.MaceWhipNPCDebuff, 8},
            {BuffID.RainbowWhipNPCDebuff, 20}
        };
    }

    public class ModSounds
    {
        public static readonly SoundStyle HellpodSignal_1 = new SoundStyle("SummonerExpansionMod/Assets/Sounds/HellpodSignal_1") {
            Volume = 500f,
            PitchVariance = 0.0f,
            MaxInstances = 5,
        };
        public static readonly SoundStyle HellpodSignal_2_1 = new SoundStyle("SummonerExpansionMod/Assets/Sounds/HellpodSignal_2_1") {
            Volume = 500f,
            PitchVariance = 0.0f,
            MaxInstances = 5,
        };
        public static readonly SoundStyle HellpodSignal_3_2 = new SoundStyle("SummonerExpansionMod/Assets/Sounds/HellpodSignal_3_2") {
            Volume = 500f,
            PitchVariance = 0.0f,
            MaxInstances = 5,
        };
    }
}