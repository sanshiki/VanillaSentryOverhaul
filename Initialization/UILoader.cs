// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;

// using Microsoft.Xna.Framework.Graphics;
// using Terraria.Graphics.Effects;
// using Terraria.Graphics.Shaders;
// using Terraria.ID;
// using Terraria.ModLoader;
// using ReLogic.Content;
// using Terraria;
// using SummonerExpansionMod.Content.Projectiles.Summon;
// using SummonerExpansionMod.Content.Buffs.Summon;
// using SummonerExpansionMod.Content.UI;
// using System.IO;
// using Terraria.UI;
// using Terraria.GameContent.UI.Elements;
// using Terraria.GameContent.UI.Chat;
// using Terraria.GameContent.UI.States;

// // For GameTime
// using Microsoft.Xna.Framework;

// namespace SummonerExpansionMod.Initialization
// {
//     public class UILoader
//     {
//         internal static ModKeybind ToggleDynamicParamManagerUI;
//         internal static UserInterface dynamicParamManagerInterface;
//         internal static DynamicParamManagerUI dynamicParamManagerUI;

//         public static void Load(Mod mod)
//         {
//             ToggleDynamicParamManagerUI = KeybindLoader.RegisterKeybind(mod, "Toggle Dynamic Param Manager UI", "P");

//             if(!Main.dedServ)
//             {
//                 dynamicParamManagerUI = new DynamicParamManagerUI();
//                 dynamicParamManagerUI.Acticate();
//                 dynamicParamManagerInterface = new UserInterface();
//             }
//         }

//         public static void Unload()
//         {
//             ToggleDynamicParamManagerUI = null;
//             dynamicParamManagerInterface = null;
//             dynamicParamManagerUI = null;
//         }

//         public static void UpdateUI(GameTime gameTime)
//         {
//             if(dynamicParamManagerUI.Visible)
//             {
//                 dynamicParamManagerInterface?.Update(gameTime);
//             }
//         }

//         public static void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
//         {
//             int inventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
//             if (inventoryLayerIndex != -1) {
//                 layers.Insert(inventoryLayerIndex, new LegacyGameInterfaceLayer(
//                     "SummonerExpansionMod: Dynamic Param Manager UI",
//                     delegate {
//                         if (dynamicParamManagerUI?.Visible == true) {
//                             dynamicParamManagerInterface?.Draw(Main.spriteBatch, new GameTime());
//                         }
//                         return true;
//                     },
//                     InterfaceScaleType.UI)
//                 );
//             }
//         }

//         public static void HandlePacket(BinaryReader reader, int whoAmI) { }

//         public static void PostUpdateInput() {
//             if (ToggleDynamicParamManagerUI.JustPressed) {
//                 dynamicParamManagerUI.Visible = !dynamicParamManagerUI.Visible;
//                 if (dynamicParamManagerUI.Visible) {
//                     dynamicParamManagerInterface?.SetState(dynamicParamManagerUI);
//                 } else {
//                     dynamicParamManagerInterface?.SetState(null);
//                 }
//             }
//         }
//     }
// }