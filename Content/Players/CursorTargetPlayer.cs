using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

using Microsoft.Xna.Framework.Graphics;
using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.Content.NPCs;
using SummonerExpansionMod.Utils;
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.Players
{
    public class CursorTargetPlayer : ModPlayer
    {

        private int cursorDummyIndex = -1;

        private const int DUMMY_EXIST_TIME = 40;

        private int dummyExistTimer = 0;

        private Vector2 LastPosition = Vector2.Zero;

        private bool IsUsingCursor = false;

        public override void ResetEffects()
        {

        }

        public override void PostUpdate()
        {
            if(IsUsingCursor)
            {
                // 检查是否正在使用鞭子
                if (Player.HeldItem.DamageType == DamageClass.SummonMeleeSpeed && Player.itemAnimation > 0)
                {
                    // 找到已有的 Dummy
                    // Main.NewText("CursorDummyIndex: " + cursorDummyIndex);
                    // Main.NewText("CursorDummyActive: " + (cursorDummyIndex != -1 && Main.npc[cursorDummyIndex].active));
                    // Main.NewText("CursorDummyModNPC: " + (cursorDummyIndex != -1 && Main.npc[cursorDummyIndex].ModNPC is CursorTargetDummy));
                    if (cursorDummyIndex == -1 || !Main.npc[cursorDummyIndex].active || !(Main.npc[cursorDummyIndex].ModNPC is CursorTargetDummy))
                    {
                        // 新建一个
                        cursorDummyIndex = NPC.NewNPC(
                            Player.GetSource_FromThis(),
                            (int)Main.MouseWorld.X,
                            (int)Main.MouseWorld.Y,
                            ModContent.NPCType<CursorTargetDummy>()
                        );

                        // Main.NewText("CursorDummyIndex: " + cursorDummyIndex);
                    }

                    // 更新位置
                    Main.npc[cursorDummyIndex].Center = Main.MouseWorld;
                    LastPosition = Main.npc[cursorDummyIndex].Center;
                    Main.npc[cursorDummyIndex].velocity = Main.npc[cursorDummyIndex].Center - LastPosition;

                    // 强制让召唤物以它为目标
                    Player.MinionAttackTargetNPC = cursorDummyIndex;

                    // 重置timer
                    dummyExistTimer = 0;
                }
                else
                {
                    dummyExistTimer++;
                    if (dummyExistTimer >= DUMMY_EXIST_TIME)
                    {
                        // 不使用鞭子时，清理 Dummy
                        if (cursorDummyIndex != -1 && Main.npc[cursorDummyIndex].active && Main.npc[cursorDummyIndex].ModNPC is CursorTargetDummy)
                        {
                            Main.npc[cursorDummyIndex].active = false;
                        }
                        cursorDummyIndex = -1;
                    }

                }
            }
        }
    }
}