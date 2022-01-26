﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace AQMod.UserInterface
{
    internal static class UserInterfaceUtilities
    {
        public static bool GameInterfaceLayersAreBeingDrawn;

        public struct InventoryItemDrawResults
        {
            public Item item;
            public Texture2D texture;
            public Vector2 drawPos;
            public Vector2 origin;
            public Rectangle frame;
            public Color drawColor;
            public Color color;
            public float scale;
            public float scale2;
        }

        public static Vector2 ItemSlotMiddle(Vector2 position, Texture2D inventoryBackTexture)
        {
            return position + inventoryBackTexture.Size() / 2f * Main.inventoryScale;
        }

        public static InventoryItemDrawResults DrawItemInv(Vector2 position, Item item, Color? color = null, int maxSize = 32)
        {
            var drawResults = new InventoryItemDrawResults
            {
                item = item,
                texture = Main.itemTexture[item.type]
            };
            drawResults.frame = Main.itemAnimations[item.type] == null ? drawResults.texture.Frame() : Main.itemAnimations[item.type].GetFrame(drawResults.texture);
            drawResults.scale2 = 1f;
            drawResults.color = Color.White;
            ItemSlot.GetItemLight(ref drawResults.color, ref drawResults.scale2, item.type);
            drawResults.drawColor = item.GetAlpha(drawResults.color);
            if (color != null)
            {
                float r = drawResults.drawColor.R / 255f;
                float g = drawResults.drawColor.G / 255f;
                float b = drawResults.drawColor.B / 255f;
                float a = drawResults.drawColor.A / 255f;

                var color2 = color.Value;
                float r2 = color2.R / 255f;
                float g2 = color2.G / 255f;
                float b2 = color2.B / 255f;
                float a2 = color2.A / 255f;

                drawResults.drawColor = new Color(r * r2, g * g2, b * b2, a * a2);
            }
            float scale = 1f;
            if (drawResults.frame.Width > maxSize || drawResults.frame.Height > maxSize)
                scale = drawResults.frame.Width <= drawResults.frame.Height ? (float)maxSize / drawResults.frame.Height : (float)maxSize / drawResults.frame.Width;
            drawResults.scale = scale * Main.inventoryScale;
            Vector2 backSize = Main.inventoryBack3Texture.Size() * Main.inventoryScale;
            drawResults.origin = drawResults.frame.Size() * (drawResults.scale2 / 2f - 0.5f);
            drawResults.drawPos = position + backSize / 2f - drawResults.frame.Size() * drawResults.scale / 2f;
            DrawItemInv(drawResults.drawPos, item, drawResults.frame, drawResults.drawColor, drawResults.color, drawResults.origin, drawResults.scale, drawResults.scale2);
            return drawResults;
        }

        public static InventoryItemDrawResults GetDrawResults(Vector2 position, Item item)
        {
            var drawResults = new InventoryItemDrawResults
            {
                item = item,
                texture = Main.itemTexture[item.type],
            };
            drawResults.frame = Main.itemAnimations[item.type] == null ? drawResults.texture.Frame() : Main.itemAnimations[item.type].GetFrame(drawResults.texture);
            drawResults.scale2 = 1f;
            drawResults.color = Color.White;
            ItemSlot.GetItemLight(ref drawResults.color, ref drawResults.scale2, item.type);
            drawResults.drawColor = item.GetAlpha(drawResults.color);
            float scale = 1f;
            if (drawResults.frame.Width > 32 || drawResults.frame.Height > 32)
                scale = drawResults.frame.Width <= drawResults.frame.Height ? 32f / drawResults.frame.Height : 32f / drawResults.frame.Width;
            drawResults.scale = scale * Main.inventoryScale;
            Vector2 backSize = Main.inventoryBack3Texture.Size() * Main.inventoryScale;
            drawResults.origin = drawResults.frame.Size() * (drawResults.scale2 / 2f - 0.5f);
            drawResults.drawPos = position + backSize / 2f - drawResults.frame.Size() * drawResults.scale / 2f;
            return drawResults;
        }

        public static void DrawItemInv(Vector2 position, Item item, Rectangle frame, Color drawColor, Color lightColor, Vector2 origin, float scale, float scale2)
        {
            Texture2D itemTexture = Main.itemTexture[item.type];
            if (ItemLoader.PreDrawInInventory(item, Main.spriteBatch, position, frame, drawColor, item.GetColor(Main.inventoryBack), origin, scale * scale2))
            {
                Main.spriteBatch.Draw(itemTexture, position, frame, drawColor, 0f, origin, scale * scale2, SpriteEffects.None, 0f);
                if (item.color != Color.Transparent)
                    Main.spriteBatch.Draw(itemTexture, position, frame, item.GetColor(Main.inventoryBack), 0f, origin, scale * scale2, SpriteEffects.None, 0f);
            }
            ItemLoader.PostDrawInInventory(item, Main.spriteBatch, position, frame, drawColor, item.GetColor(lightColor), origin, scale * scale2);
            if (ItemID.Sets.TrapSigned[item.type])
                Main.spriteBatch.Draw(Main.wireTexture, position + new Vector2(40f, 40f) * Main.inventoryScale, new Rectangle(4, 58, 8, 8), Main.inventoryBack, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);
            if (item.stack > 1)
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontItemStack, item.stack.ToString(), position + new Vector2(10f, 26f) * Main.inventoryScale, Main.inventoryBack, 0f, Vector2.Zero, new Vector2(Main.inventoryScale), -1f, Main.inventoryScale);
        }

        public static void DrawItemInvAmmo(InventoryItemDrawResults results, int amount)
        {
            if (amount != -1)
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontItemStack, amount.ToString(), results.drawPos + new Vector2(8f, 30f) * Main.inventoryScale, results.color, 0f, Vector2.Zero, new Vector2(Main.inventoryScale * 0.8f), -1f, Main.inventoryScale);
        }

        public static void DrawSlotNumber(InventoryItemDrawResults results, int slot)
        {
            string text = string.Concat(slot + 1);
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontItemStack, text, results.drawPos + new Vector2(8f, 4f) * Main.inventoryScale, results.color, 0f, Vector2.Zero, new Vector2(Main.inventoryScale), -1f, Main.inventoryScale);
        }

        public static void DrawItemCrossOut(InventoryItemDrawResults results)
        {
            Vector2 drawPos = results.drawPos + results.texture.Size() * Main.inventoryScale / 2f - Main.cdTexture.Size() * Main.inventoryScale / 2f;
            Main.spriteBatch.Draw(Main.cdTexture, drawPos, null, Color.White, 0f, default(Vector2), results.scale, SpriteEffects.None, 0f);
        }

        public static void DrawItemCrossOut(InventoryItemDrawResults results, float fade = 1f)
        {
            Vector2 drawPos = results.drawPos + results.texture.Size() * Main.inventoryScale / 2f - Main.cdTexture.Size() * Main.inventoryScale / 2f;
            Color color = results.item.GetAlpha(results.color) * fade;
            Main.spriteBatch.Draw(Main.cdTexture, drawPos, null, color, 0f, default(Vector2), results.scale, SpriteEffects.None, 0f);
        }

        public static void HoverItem(Item item)
        {
            Main.HoverItem = item.Clone();
            Main.hoverItemName = item.Name;
            if (item.stack > 1)
                Main.hoverItemName += " (" + item.stack + ")";
        }

        public static void DrawItemSlot(Vector2 position, Texture2D inventoryBackTexture)
        {
            Color color = Main.inventoryBack;
            Main.spriteBatch.Draw(inventoryBackTexture, position, null, color, 0f, default(Vector2), Main.inventoryScale, SpriteEffects.None, 0f);
        }
    }
}