using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Microsoft.Xna.Framework;
using Terraria;
using SummonerExpansionMod.ModUtils;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.GameContent.UI.Chat;
using Terraria.GameContent.UI.States;

// texture assets
using Terraria.GameContent;

namespace SummonerExpansionMod.Content.UI
{
    public class DynamicParamManagerUI : UIState
    {
        public UIPanel panel;
        public bool Visible;

        public void BuildPanel() {
            panel.RemoveAllChildren();
            int y = 20;
            int cnt = 0;
            foreach (var param in DynamicParamManager.GetAll()) {
                if (param is DynamicParam dynamic_param) {
                    var slider = new UISliderFloat(dynamic_param.name, dynamic_param.value, dynamic_param.lower_limit, dynamic_param.upper_limit);
                    slider.Top.Set(y, 0f);
                    slider.OnValueChanged += (val) => dynamic_param.value = val;
                    panel.Append(slider);
                    y += 40;
                    cnt++;
                    Main.NewText("param: " + dynamic_param.name + " value: " + dynamic_param.value + " lower_limit: " + dynamic_param.lower_limit + " upper_limit: " + dynamic_param.upper_limit);
                }
            }
            Main.NewText("sliders in panel: " + cnt);
        }

        public override void OnInitialize() {
            panel = new UIPanel();
            panel.SetPadding(10);
            panel.Left.Set(400f, 0f);
            panel.Top.Set(100f, 0f);
            panel.Width.Set(300f, 0f);
            panel.Height.Set(150f, 0f);
            Append(panel);

            BuildPanel();
        }

        

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) {
            if (Visible) base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime) {
            if (Visible) base.Update(gameTime);
        }
    }

    public class UISliderFloat : UISliderBase
    {
        private UIText label;
        private string name;
        private float value;
        private float min;
        private float max;
        private bool dragging = false;
        public delegate void ValueChangedHandler(float newValue);
        public event ValueChangedHandler OnValueChanged;

        public UISliderFloat(string name, float defaultValue, float min, float max) {
            this.name = name;
            this.value = defaultValue;
            this.min = min;
            this.max = max;

            label = new UIText($"{name}: {value:F2}");
            label.Top.Set(-20f, 0f);
            Append(label);

            Width.Set(200, 0f);
            Height.Set(20, 0f);
        }

        public override void LeftClick(UIMouseEvent evt) {
            base.LeftClick(evt);
            float percent = (evt.MousePosition.X - GetDimensions().X) / GetDimensions().Width;
            value = min + (max - min) * percent;
            OnValueChanged?.Invoke(value);
            label.SetText($"{name}: {value:F2}");
        }

        public override void Draw(SpriteBatch spriteBatch) {
            // 背景条
            var dimensions = GetDimensions().ToRectangle();
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, dimensions, Color.Gray * 0.5f);

            // 滑块位置
            float percent = (value - min) / (max - min);
            int knobX = (int)(dimensions.X + percent * dimensions.Width);

            // 绘制滑块
            var knobRect = new Rectangle(knobX - 5, dimensions.Y, 10, dimensions.Height);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, knobRect, Color.White);

            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if (dragging) {
                float percent = Utils.GetLerpValue(
                    GetDimensions().X, 
                    GetDimensions().X + 200, // 滑条长度 = 200
                    Main.mouseX, 
                    true
                );

                float step = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) ? 0.01f : 0.1f;
                float newValue = MathHelper.Lerp(min, max, percent);
                newValue = (float)Math.Round(newValue / step) * step;

                Main.NewText("percent: " + percent + " newValue: " + newValue + "min: " + min + " max: " + max + " value: " + value);

                if (newValue != value) {
                    value = newValue;
                    label.SetText($"{name}: {value:F2}");
                    OnValueChanged?.Invoke(value);
                }
            }
        }

        public override void LeftMouseDown(UIMouseEvent evt) {
            // base.LeftMouseDown(evt);
            // 只在点击滑条区域时开启拖动
            if (evt.MousePosition.X < GetDimensions().X + 200) {
                dragging = true;
            }
        }

        public override void LeftMouseUp(UIMouseEvent evt) {
            // base.LeftMouseUp(evt);
            dragging = false;
        }

        public override void ScrollWheel(UIScrollWheelEvent evt) {
            // base.ScrollWheel(evt);
            float step = 0.1f;
            float newVal = MathHelper.Clamp(value + (evt.ScrollWheelValue > 0 ? step : -step), min, max);
            if (newVal != value) {
                value = newVal;
                label.SetText($"{name}: {value:F2}");
                OnValueChanged?.Invoke(value);
            }
        }
    }
}