using System;
using System.Collections.Generic;
using System.Linq;
using BaseLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using On.Terraria.UI;
using Terraria;
using Terraria.ModLoader;
using ChestUI = Terraria.UI.ChestUI;
using Main = On.Terraria.Main;

namespace ContainerLibrary
{
	public partial class ContainerLibrary
	{
		public static List<IItemHandlerUI> elements = new List<IItemHandlerUI>();
		public static IItemHandlerUI currentUI;

		private static void ItemSlot_OverrideHover(ItemSlot.orig_OverrideHover orig, Item[] inv, int context, int slot)
		{
			if (canFavoriteAt == null) canFavoriteAt = typeof(Terraria.UI.ItemSlot).GetValue<bool[]>("canFavoriteAt");

			Item item = inv[slot];
			if (Terraria.UI.ItemSlot.ShiftInUse && item.type > 0 && item.stack > 0 && !inv[slot].favorited)
			{
				switch (context)
				{
					case Terraria.UI.ItemSlot.Context.InventoryItem:
					case Terraria.UI.ItemSlot.Context.InventoryCoin:
					case Terraria.UI.ItemSlot.Context.InventoryAmmo:
						if ((currentUI = elements.FirstOrDefault(ui => ui.Handler.HasSpace(item))) != null) Terraria.Main.cursorOverride = 1000;
						else if (Terraria.Main.npcShop > 0 && !item.favorited) Terraria.Main.cursorOverride = 10;
						else if (Terraria.Main.player[Terraria.Main.myPlayer].chest != -1)
						{
							if (ChestUI.TryPlacingInChest(item, true)) Terraria.Main.cursorOverride = 9;
						}
						else Terraria.Main.cursorOverride = 6;

						break;
					case Terraria.UI.ItemSlot.Context.ChestItem:
					case Terraria.UI.ItemSlot.Context.BankItem:
						if (Terraria.Main.player[Terraria.Main.myPlayer].ItemSpace(item)) Terraria.Main.cursorOverride = 8;
						break;
					case Terraria.UI.ItemSlot.Context.PrefixItem:
					case Terraria.UI.ItemSlot.Context.EquipArmor:
					case Terraria.UI.ItemSlot.Context.EquipArmorVanity:
					case Terraria.UI.ItemSlot.Context.EquipAccessory:
					case Terraria.UI.ItemSlot.Context.EquipAccessoryVanity:
					case Terraria.UI.ItemSlot.Context.EquipDye:
					case Terraria.UI.ItemSlot.Context.EquipGrapple:
					case Terraria.UI.ItemSlot.Context.EquipMount:
					case Terraria.UI.ItemSlot.Context.EquipMinecart:
					case Terraria.UI.ItemSlot.Context.EquipPet:
					case Terraria.UI.ItemSlot.Context.EquipLight:
						if (Terraria.Main.player[Terraria.Main.myPlayer].ItemSpace(inv[slot])) Terraria.Main.cursorOverride = 7;
						break;
				}
			}

			if (Terraria.Main.keyState.IsKeyDown(Terraria.Main.FavoriteKey) && canFavoriteAt[context])
			{
				if (item.type > 0 && item.stack > 0 && Terraria.Main.drawingPlayerChat)
				{
					Terraria.Main.cursorOverride = 2;
					return;
				}

				if (item.type > 0 && item.stack > 0) Terraria.Main.cursorOverride = 3;
			}
		}

		private static void Main_DrawInterface_36_Cursor(Main.orig_DrawInterface_36_Cursor orig)
		{
			if (placeholderTexture == null) placeholderTexture = ModContent.GetTexture("PortableStorage/Textures/MouseCursor");

			if (Terraria.Main.cursorOverride != -1)
			{
				Color colorOutline = new Color((int)(Terraria.Main.cursorColor.R * 0.2f), (int)(Terraria.Main.cursorColor.G * 0.2f), (int)(Terraria.Main.cursorColor.B * 0.2f), (int)(Terraria.Main.cursorColor.A * 0.5f));
				Color color = Terraria.Main.cursorColor;

				bool drawOutline = true;
				Vector2 value = default(Vector2);
				float scale = 1f;

				switch (Terraria.Main.cursorOverride)
				{
					case 2:
						drawOutline = false;
						color = Color.White;
						scale = 0.7f;
						value = new Vector2(0.1f);
						break;
					case 3:
					case 6:
					case 7:
					case 8:
					case 9:
					case 10:
					case BagCursorOverride:
						drawOutline = false;
						color = Color.White;
						break;
				}

				if (Terraria.Main.cursorOverride == 1000)
				{
					float texScale = Math.Min(14f / currentUI.ShiftClickIcon.Width, 14f / currentUI.ShiftClickIcon.Height);
					Terraria.Main.spriteBatch.Draw(currentUI.ShiftClickIcon, new Vector2(Terraria.Main.mouseX, Terraria.Main.mouseY), null, color, 0f, Vector2.Zero, Terraria.Main.cursorScale * scale * texScale, SpriteEffects.None, 0f);
					return;
				}

				if (drawOutline) Terraria.Main.spriteBatch.Draw(Terraria.Main.cursorTextures[Terraria.Main.cursorOverride], new Vector2(Terraria.Main.mouseX + 1, Terraria.Main.mouseY + 1), null, colorOutline, 0f, value * Terraria.Main.cursorTextures[Terraria.Main.cursorOverride].Size(), Terraria.Main.cursorScale * 1.1f * scale, SpriteEffects.None, 0f);

				Terraria.Main.spriteBatch.Draw(Terraria.Main.cursorTextures[Terraria.Main.cursorOverride], new Vector2(Terraria.Main.mouseX, Terraria.Main.mouseY), null, color, 0f, value * Terraria.Main.cursorTextures[Terraria.Main.cursorOverride].Size(), Terraria.Main.cursorScale * scale, SpriteEffects.None, 0f);
			}
			else
			{
				if (Terraria.Main.SmartCursorEnabled)
				{
					Vector2 bonus = Terraria.Main.DrawThickCursor(true);
					Terraria.Main.DrawCursor(bonus, true);
					return;
				}

				Vector2 bonus2 = Terraria.Main.DrawThickCursor();
				Terraria.Main.DrawCursor(bonus2);
			}
		}

		private static bool[] canFavoriteAt;
		private static Texture2D placeholderTexture;
		private const int BagCursorOverride = 1000;
	}
}