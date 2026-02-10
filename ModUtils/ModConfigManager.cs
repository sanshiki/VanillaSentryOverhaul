using Terraria.ModLoader.Config;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;
using Newtonsoft.Json;
using System.IO;

namespace SummonerExpansionMod.ModUtils
{
    public class ProjectileConfigData
    {
        [Label("宽度")]
        [DefaultValue(10)]
        public int Width = 10;

        [Label("高度")]
        [DefaultValue(10)]
        public int Height = 10;

        [Label("AI 样式")]
        [DefaultValue(0)]
        public int AIStyle = 0;

        [Label("友好")]
        [DefaultValue(true)]
        public bool Friendly = true;

        [Label("敌对")]
        [DefaultValue(false)]
        public bool Hostile = false;

        [Label("穿透次数")]
        [DefaultValue(1)]
        public int Penetrate = 1;

        [Label("存活时间")]
        [DefaultValue(600)]
        public int TimeLeft = 600;

        [Label("是否碰撞地形")]
        [DefaultValue(true)]
        public bool TileCollide = true;

        [Label("伤害类型")]
        [OptionStrings(new string[] { "Melee", "Ranged", "Magic", "Summon", "Default" })]
        [DefaultValue("Melee")]
        public string DamageType = "Melee";
    }

    [Label("Projectile 参数配置")]
    public class ProjectileConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;
        // 如果想每个玩家自己调，用 ConfigScope.ClientSide

        [Label("弹幕参数表")]
        [Tooltip("为每个 ModProjectile 定义配置参数")]
        public Dictionary<string, ProjectileConfigData> ProjectileSettings = new();

        // 当用户修改配置时自动调用
        public override void OnChanged()
        {
            // 热更新：重新应用配置
            ProjectileConfigSystem.ApplyAllConfigs();
        }
    }


    public class ProjectileConfigSystem : ModSystem
    {
        private static Dictionary<string, ProjectileConfigData> DefaultValues = new();

        public override void OnModLoad()
        {
            LoadDefaultJson();
            InitializeProjectileConfigs();
        }

        private void LoadDefaultJson()
        {
            // 在开发环境中，配置文件位于mod源目录的Config文件夹
            string configPath = Path.Combine("Config", "ModConfig.json");
            
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                var config = JsonConvert.DeserializeObject<ProjectileConfig>(json);
                if (config?.ProjectileSettings != null)
                {
                    DefaultValues = config.ProjectileSettings;
                    Mod.Logger.Info($"[ProjectileConfigSystem] Loaded {DefaultValues.Count} default projectile entries.");
                }
                else
                {
                    DefaultValues = new Dictionary<string, ProjectileConfigData>();
                }
            }
            else
            {
                Mod.Logger.Warn($"ModConfig.json not found at {configPath}");
                DefaultValues = new Dictionary<string, ProjectileConfigData>();
            }
        }

        public static void InitializeProjectileConfigs()
        {
            var cfg = ModContent.GetInstance<ProjectileConfig>();
            var mod = ModContent.GetInstance<SummonerExpansionMod>();

            foreach (var proj in mod.GetContent<ModProjectile>())
            {
                if (!cfg.ProjectileSettings.ContainsKey(proj.Name))
                {
                    if (DefaultValues.TryGetValue(proj.Name, out var data))
                        cfg.ProjectileSettings[proj.Name] = data;
                    else
                        cfg.ProjectileSettings[proj.Name] = new ProjectileConfigData(); // fallback defaults
                }
            }
        }

        public static void ApplyAllConfigs()
        {
            // 重新初始化配置，确保所有弹幕都有配置条目
            InitializeProjectileConfigs();
        }
    }
}
