﻿using AQMod.Assets;
using AQMod.Common.Configuration;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Utilities;

namespace AQMod.Effects
{
    public sealed class GameEffects
    {
        private readonly int MySeed;
        private readonly UnifiedRandom Randomizer;
        private readonly byte[] RNG;
        private byte rngIndex;

        private Vector2 flashLocation;
        private float flashBrightness;
        private float flashBrightnessDecrement;

        private float shake;
        private float shakeDecrement;

        public GameEffects()
        {
            MySeed = "Split".GetHashCode();
            Randomizer = new UnifiedRandom(MySeed);
            RNG = new byte[byte.MaxValue + 1];
            Randomizer.NextBytes(RNG);
            Initialize();
        }

        internal void Initialize()
        {
            rngIndex = 0;
            shake = 0f;
            shakeDecrement = 0f;
            flashLocation = Vector2.Zero;
            flashBrightness = 0f;
            flashBrightnessDecrement = 0f;
        }

        public byte Rand()
        {
            byte value = RNG[rngIndex];
            rngIndex++;
            return value;
        }
        public bool RandChance(int chance)
        {
            return AQUtils.FromByte(Rand(), chance) < 1f;
        }
        public float Rand(float max)
        {
            return AQUtils.FromByte(Rand(), max);
        }
        public float Rand(float min, float max)
        {
            return AQUtils.FromByte(Rand(), min, max);
        }
        public void IncRand(int amount)
        {
            int newIndex = rngIndex + amount;
            rngIndex = (byte)(newIndex % byte.MaxValue);
        }
        public void SetRand(int set)
        {
            SetRand((byte)(set % byte.MaxValue));
        }
        public void SetRand(byte set)
        {
            rngIndex = set;
        }
        public void TempSetRand(int set, out int reset)
        {
            reset = rngIndex;
            SetRand((byte)(set % byte.MaxValue));
        }

        public void SetShake(float intensity)
        {
            SetShake(intensity, 20f);
        }
        public void SetShake(float intensity, float time)
        {
            if (shake < intensity)
            {
                shake = intensity;
                shakeDecrement = shake / time;
            }
        }

        public void SetFlash(Vector2 location, float brightness)
        {
            SetFlash(location, brightness, 20f);
        }
        public void SetFlash(Vector2 location, float brightness, float time)
        {
            flashLocation = location;
            flashBrightness = brightness;
            flashBrightnessDecrement = flashBrightness * (1f / time);
        }

        internal void UpdateVisuals()
        {
            if (flashLocation != Vector2.Zero)
            {
                LegacyEffectCache.f_Flash.GetShader()
                .UseIntensity(Math.Max(flashBrightness * AQConfigClient.Instance.EffectIntensity, 1f / 18f));
                if (!LegacyEffectCache.f_Flash.IsActive())
                {
                    Filters.Scene.Activate(LegacyEffectCache.fn_Flash, flashLocation, null).GetShader()
                    .UseOpacity(1f)
                    .UseTargetPosition(flashLocation);
                }
                flashBrightness -= flashBrightnessDecrement;
                if (flashBrightness <= 0f)
                {
                    flashLocation = Vector2.Zero;
                    flashBrightness = 0f;
                    flashBrightnessDecrement = 0.05f;
                }
            }
            else
            {
                if (LegacyEffectCache.f_Flash.IsActive())
                {
                    LegacyEffectCache.f_Flash.GetShader()
                        .UseIntensity(0f)
                        .UseProgress(0f)
                        .UseOpacity(0f);
                    Filters.Scene.Deactivate(LegacyEffectCache.fn_Flash, null);
                }
            }
        }
        internal void UpdateScreen()
        {
            if (shake > 0f)
            {
                Main.screenPosition += new Vector2(Rand(-shake, shake), Rand(-shake, shake));
                shake -= shakeDecrement;
                if (shake < 0.5f)
                {
                    shake = 0f;
                }
            }
        }

        public static Vector2 GetY(Vector2 position)
        {
            return Main.player[Main.myPlayer].gravDir == -1 ? new Vector2(position.X, -position.Y + Main.screenHeight) : new Vector2(position.X, position.Y);
        }
    }
}