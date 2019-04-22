using System;
using System.Collections.Generic;
using System.Linq;
using BaseLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using On.Terraria.UI;
using Terraria;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;
using ChestUI = Terraria.UI.ChestUI;
using Main = On.Terraria.Main;
using Recipe = On.Terraria.Recipe;

namespace ContainerLibrary
{
	public partial class ContainerLibrary
	{
		public static List<IItemHandlerUI> ItemHandlerUI = new List<IItemHandlerUI>();
		public static IItemHandlerUI currentUI;

		public static Func<(int reductionPercent, bool reduce)> CheckAlchemy = () => (33, false);
		public static Action ModifyAdjTiles;

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
						if ((currentUI = ItemHandlerUI.FirstOrDefault(ui => ui.Handler.HasSpace(item))) != null) Terraria.Main.cursorOverride = 1000;
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

		private static void Recipe_Create(Recipe.orig_Create orig, Terraria.Recipe self)
		{
			for (int i = 0; i < Terraria.Recipe.maxRequirements; i++)
			{
				Item item = self.requiredItem[i];

				if (item.type == 0) break;
				int amount = item.stack;
				if (self is ModRecipe modRecipe) amount = modRecipe.ConsumeItem(item.type, item.stack);
				(int reductionPercent, bool reduce) = CheckAlchemy();
				if (self.alchemy && Terraria.Main.LocalPlayer.alchemyTable || reduce)
				{
					int num2 = 0;
					for (int j = 0; j < amount; j++)
					{
						if (Terraria.Main.rand.Next(100 / reductionPercent) == 0) num2++;
					}

					amount -= num2;
				}

				if (amount > 0)
				{
					Item[] array = Terraria.Main.LocalPlayer.inventory;
					for (int k = 0; k < array.Length; k++)
					{
						ref Item invItem = ref array[k];
						if (amount <= 0) break;

						if (invItem.IsTheSameAs(item) || self.useWood(invItem.type, item.type) || self.useSand(invItem.type, item.type) || self.useFragment(invItem.type, item.type) || self.useIronBar(invItem.type, item.type) || self.usePressurePlate(invItem.type, item.type) || self.AcceptedByItemGroups(invItem.type, item.type))
						{
							int count = Math.Min(amount, invItem.stack);
							amount -= count;
							invItem.stack -= count;
							if (invItem.stack <= 0) invItem = new Item();
						}
					}

					if (Terraria.Main.LocalPlayer.chest != -1)
					{
						if (Terraria.Main.LocalPlayer.chest > -1) array = Terraria.Main.chest[Terraria.Main.LocalPlayer.chest].item;
						else if (Terraria.Main.LocalPlayer.chest == -2) array = Terraria.Main.LocalPlayer.bank.item;
						else if (Terraria.Main.LocalPlayer.chest == -3) array = Terraria.Main.LocalPlayer.bank2.item;
						else if (Terraria.Main.LocalPlayer.chest == -4) array = Terraria.Main.LocalPlayer.bank3.item;

						for (int l = 0; l < array.Length; l++)
						{
							ref Item invItem = ref array[l];
							if (amount <= 0) break;

							if (invItem.IsTheSameAs(item) || self.useWood(invItem.type, item.type) || self.useSand(invItem.type, item.type) || self.useIronBar(invItem.type, item.type) || self.usePressurePlate(invItem.type, item.type) || self.useFragment(invItem.type, item.type) || self.AcceptedByItemGroups(invItem.type, item.type))
							{
								int count = Math.Min(amount, invItem.stack);
								amount -= count;
								invItem.stack -= count;
								if (invItem.stack <= 0) invItem = new Item();

								if (Terraria.Main.netMode == NetmodeID.MultiplayerClient && Terraria.Main.LocalPlayer.chest >= 0) NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, Terraria.Main.LocalPlayer.chest, l);
							}
						}
					}

					for (int j = 0; j < Terraria.Main.LocalPlayer.inventory.Length; j++)
					{
						if (Terraria.Main.LocalPlayer.inventory[j].modItem is ICraftingStorage storage)
						{
							for (int index = 0; index < storage.CraftingHandler.Items.Count; index++)
							{
								if (amount <= 0) break;
								Item invItem = storage.CraftingHandler.Items[index];

								if (invItem.IsTheSameAs(item) || self.useWood(invItem.type, item.type) || self.useSand(invItem.type, item.type) || self.useIronBar(invItem.type, item.type) || self.usePressurePlate(invItem.type, item.type) || self.useFragment(invItem.type, item.type) || self.AcceptedByItemGroups(invItem.type, item.type))
								{
									int count = Math.Min(amount, invItem.stack);
									amount -= count;
									storage.CraftingHandler.ExtractItem(index, count);
								}
							}
						}
					}
				}
			}

			AchievementsHelper.NotifyItemCraft(self);
			AchievementsHelper.NotifyItemPickup(Terraria.Main.LocalPlayer, self.createItem);
			Terraria.Recipe.FindRecipes();
		}

		private static void Recipe_FindRecipes(Recipe.orig_FindRecipes orig)
		{
			int focusIndex = Terraria.Main.availableRecipe[Terraria.Main.focusRecipe];
			float focusY = Terraria.Main.availableRecipeY[Terraria.Main.focusRecipe];
			for (int i = 0; i < Terraria.Recipe.maxRecipes; i++) Terraria.Main.availableRecipe[i] = 0;
			Terraria.Main.numAvailableRecipes = 0;
			bool guideMenu = Terraria.Main.guideItem.type > 0 && Terraria.Main.guideItem.stack > 0 && Terraria.Main.guideItem.Name != "";
			if (guideMenu)
			{
				for (int i = 0; i < Terraria.Recipe.maxRecipes; i++)
				{
					if (Terraria.Main.recipe[i].createItem.type == 0) break;
					int index = 0;
					while (index < Terraria.Recipe.maxRequirements && Terraria.Main.recipe[i].requiredItem[index].type != 0)
					{
						if (Terraria.Main.guideItem.IsTheSameAs(Terraria.Main.recipe[i].requiredItem[index]) || Terraria.Main.recipe[i].useWood(Terraria.Main.guideItem.type, Terraria.Main.recipe[i].requiredItem[index].type) || Terraria.Main.recipe[i].useSand(Terraria.Main.guideItem.type, Terraria.Main.recipe[i].requiredItem[index].type) || Terraria.Main.recipe[i].useIronBar(Terraria.Main.guideItem.type, Terraria.Main.recipe[i].requiredItem[index].type) || Terraria.Main.recipe[i].useFragment(Terraria.Main.guideItem.type, Terraria.Main.recipe[i].requiredItem[index].type) || Terraria.Main.recipe[i].AcceptedByItemGroups(Terraria.Main.guideItem.type, Terraria.Main.recipe[i].requiredItem[index].type) || Terraria.Main.recipe[i].usePressurePlate(Terraria.Main.guideItem.type, Terraria.Main.recipe[i].requiredItem[index].type))
						{
							Terraria.Main.availableRecipe[Terraria.Main.numAvailableRecipes] = i;
							Terraria.Main.numAvailableRecipes++;
							break;
						}

						index++;
					}
				}
			}
			else
			{
				Dictionary<int, int> availableItems = new Dictionary<int, int>();
				Item item;
				Item[] array = Terraria.Main.LocalPlayer.inventory;
				for (int i = 0; i < array.Length; i++)
				{
					item = array[i];
					if (item.stack > 0)
					{
						if (availableItems.ContainsKey(item.netID)) availableItems[item.netID] += item.stack;
						else availableItems[item.netID] = item.stack;
					}
				}

				if (Terraria.Main.player[Terraria.Main.myPlayer].chest != -1)
				{
					if (Terraria.Main.player[Terraria.Main.myPlayer].chest > -1) array = Terraria.Main.chest[Terraria.Main.player[Terraria.Main.myPlayer].chest].item;
					else if (Terraria.Main.player[Terraria.Main.myPlayer].chest == -2) array = Terraria.Main.player[Terraria.Main.myPlayer].bank.item;
					else if (Terraria.Main.player[Terraria.Main.myPlayer].chest == -3) array = Terraria.Main.player[Terraria.Main.myPlayer].bank2.item;
					else if (Terraria.Main.player[Terraria.Main.myPlayer].chest == -4) array = Terraria.Main.player[Terraria.Main.myPlayer].bank3.item;

					for (int i = 0; i < array.Length; i++)
					{
						item = array[i];
						if (item.stack > 0)
						{
							if (availableItems.ContainsKey(item.netID)) availableItems[item.netID] += item.stack;
							else availableItems[item.netID] = item.stack;
						}
					}
				}

				for (int j = 0; j < Terraria.Main.LocalPlayer.inventory.Length; j++)
				{
					if (Terraria.Main.LocalPlayer.inventory[j].modItem is ICraftingStorage storage)
					{
						for (int i = 0; i < storage.CraftingHandler.Items.Count; i++)
						{
							item = storage.CraftingHandler.Items[i];
							if (item.stack > 0)
							{
								if (availableItems.ContainsKey(item.netID)) availableItems[item.netID] += item.stack;
								else availableItems[item.netID] = item.stack;
							}
						}
					}
				}

				int index = 0;
				while (index < Terraria.Recipe.maxRecipes && Terraria.Main.recipe[index].createItem.type != 0)
				{
					bool hasTile = true;
					int tileIndex = 0;
					ModifyAdjTiles?.Invoke();
					while (tileIndex < Terraria.Recipe.maxRequirements && Terraria.Main.recipe[index].requiredTile[tileIndex] != -1)
					{
						if(Terraria.Main.recipe[index].createItem.type==ItemID.LesserHealingPotion)Terraria.Main.NewText(Terraria.Main.recipe[index].requiredTile[tileIndex]);

						if (!Terraria.Main.LocalPlayer.adjTile[Terraria.Main.recipe[index].requiredTile[tileIndex]])
						{
							hasTile = false;
							break;
						}

						tileIndex++;
					}

					if (hasTile)
					{
						for (int m = 0; m < Terraria.Recipe.maxRequirements; m++)
						{
							item = Terraria.Main.recipe[index].requiredItem[m];
							if (item.type == 0) break;

							int stack = item.stack;
							bool recGroup = false;
							foreach (int current in availableItems.Keys)
							{
								if (Terraria.Main.recipe[index].useWood(current, item.type) || Terraria.Main.recipe[index].useSand(current, item.type) || Terraria.Main.recipe[index].useIronBar(current, item.type) || Terraria.Main.recipe[index].useFragment(current, item.type) || Terraria.Main.recipe[index].AcceptedByItemGroups(current, item.type) || Terraria.Main.recipe[index].usePressurePlate(current, item.type))
								{
									stack -= availableItems[current];
									recGroup = true;
								}
							}

							if (!recGroup && availableItems.ContainsKey(item.netID))
							{
								stack -= availableItems[item.netID];
							}

							if (stack > 0)
							{
								hasTile = false;
								break;
							}
						}
					}

					if (hasTile)
					{
						bool flag4 = !Terraria.Main.recipe[index].needWater || Terraria.Main.player[Terraria.Main.myPlayer].adjWater || Terraria.Main.player[Terraria.Main.myPlayer].adjTile[172];
						bool flag5 = !Terraria.Main.recipe[index].needHoney || Terraria.Main.recipe[index].needHoney == Terraria.Main.player[Terraria.Main.myPlayer].adjHoney;
						bool flag6 = !Terraria.Main.recipe[index].needLava || Terraria.Main.recipe[index].needLava == Terraria.Main.player[Terraria.Main.myPlayer].adjLava;
						bool flag7 = !Terraria.Main.recipe[index].needSnowBiome || Terraria.Main.player[Terraria.Main.myPlayer].ZoneSnow;
						if (!flag4 || !flag5 || !flag6 || !flag7)
						{
							hasTile = false;
						}
					}

					if (hasTile && RecipeHooks.RecipeAvailable(Terraria.Main.recipe[index]))
					{
						Terraria.Main.availableRecipe[Terraria.Main.numAvailableRecipes] = index;
						Terraria.Main.numAvailableRecipes++;
					}

					index++;
				}
			}

			for (int n = 0; n < Terraria.Main.numAvailableRecipes; n++)
			{
				if (focusIndex == Terraria.Main.availableRecipe[n])
				{
					Terraria.Main.focusRecipe = n;
					break;
				}
			}

			if (Terraria.Main.focusRecipe >= Terraria.Main.numAvailableRecipes) Terraria.Main.focusRecipe = Terraria.Main.numAvailableRecipes - 1;
			if (Terraria.Main.focusRecipe < 0) Terraria.Main.focusRecipe = 0;

			float num7 = Terraria.Main.availableRecipeY[Terraria.Main.focusRecipe] - focusY;
			for (int num8 = 0; num8 < Terraria.Recipe.maxRecipes; num8++)
			{
				Terraria.Main.availableRecipeY[num8] -= num7;
			}
		}
	}
}