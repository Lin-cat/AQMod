﻿using AQMod.Assets;
using AQMod.Common.Config;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace AQMod.Effects.Trails
{
    public class Trailshader
    {
        private readonly Texture2D _texture;
        private readonly string _pass;

        private List<VertexPositionColorTexture> _positions;

        public const string TextureTrail = "Texture";

        internal static void Setup()
        {
            VertexDrawingContext_Projectile.setup();
        }

        public interface IVertexDrawingContext
        {
            bool ShouldDraw();
        }

        private class VertexDrawingContext_Projectile : IVertexDrawingContext
        {
            public VertexDrawingContext_Projectile(Projectile projectile)
            {
            }

            private static bool _shouldDrawTrails;

            bool IVertexDrawingContext.ShouldDraw()
            {
                return _shouldDrawTrails;
            }

            internal static void setup()
            {
                _shouldDrawTrails = ModLoader.GetMod("ShaderLib") == null;
            }
        }

        public static IVertexDrawingContext GetVertexDrawingContext_Projectile(Projectile projectile)
        {
            return new VertexDrawingContext_Projectile(projectile);
        }

        public static bool ShouldDrawVertexTrails(IVertexDrawingContext context = null)
        {
            return context == null ? true : context.ShouldDraw();
        }

        public Trailshader(Texture2D texture, string pass)
        {
            _texture = texture;
            _pass = pass;
        }

        public static void FullDraw(Texture2D texture, string pass, Vector2[] positions, Func<float, Vector2> getWidth, Func<float, Color> getColor)
        {
            var trail = new Trailshader(texture, pass);
            trail.PrepareVertices(positions, getWidth, getColor);
            trail.Draw();
        }

        public void PrepareVertices(Vector2[] positions, Func<float, Vector2> getWidth, Func<float, Color> getColor)
        {
            _positions = new List<VertexPositionColorTexture>();
            for (int i = 0; i < positions.Length - 1; i++)
            {
                float prog1 = i / (float)positions.Length;
                float prog2 = (i + 1) / (float)positions.Length;
                Vector2 width = getWidth(prog1);
                Vector2 width2 = getWidth(prog2);
                Vector2 pos1 = positions[i];
                Vector2 pos2 = positions[i + 1];
                Vector2 off1 = getRotationVector(positions, i) * width;
                Vector2 off2 = getRotationVector(positions, i + 1) * width;
                float coord1 = off1.Length() < off2.Length() ? 1 : 0;
                float coord2 = off1.Length() < off2.Length() ? 0 : 1;
                Color col1 = getColor(prog1);
                Color col2 = getColor(prog2);
                _positions.Add(new VertexPositionColorTexture(new Vector3(pos1 + off1, 0f), col1, new Vector2(prog1, coord1)));
                _positions.Add(new VertexPositionColorTexture(new Vector3(pos1 - off1, 0f), col1, new Vector2(prog1, coord2)));
                _positions.Add(new VertexPositionColorTexture(new Vector3(pos2 + off2, 0f), col2, new Vector2(prog2, coord1)));
                _positions.Add(new VertexPositionColorTexture(new Vector3(pos2 + off2, 0f), col2, new Vector2(prog2, coord1)));
                _positions.Add(new VertexPositionColorTexture(new Vector3(pos2 - off2, 0f), col2, new Vector2(prog2, coord2)));
                _positions.Add(new VertexPositionColorTexture(new Vector3(pos1 - off1, 0f), col1, new Vector2(prog1, coord2)));
            }
        }

        private Vector2 getRotationVector(Vector2[] oldPos, int index)
        {
            if (oldPos.Length == 1)
                return oldPos[0];

            if (index == 0)
                return Vector2.Normalize(oldPos[1] - oldPos[0]).RotatedBy(MathHelper.Pi / 2);

            return (index == oldPos.Length - 1
                ? Vector2.Normalize(oldPos[index] - oldPos[index - 1])
                : Vector2.Normalize(oldPos[index + 1] - oldPos[index - 1])).RotatedBy(MathHelper.Pi / 2);
        }

        public void Draw()
        {
            var vertexShader = EffectCache.Trailshader;
            vertexShader.Parameters["WVP"].SetValue(AQMod.WorldViewPoint);
            vertexShader.Parameters["imageTexture"].SetValue(_texture);
            vertexShader.Parameters["strength"].SetValue(1f);
            vertexShader.CurrentTechnique.Passes[_pass].Apply();
            Main.graphics.GraphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None
            };
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _positions.ToArray(), 0, _positions.Count / 3);
        }

        public void Clear()
        {
            _positions.Clear();
        }
    }
}