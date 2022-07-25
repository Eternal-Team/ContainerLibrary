using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;

namespace ContainerLibrary;

internal static class Hooking
{
	internal static void Load()
	{
		IL.Terraria.UI.ItemSlot.OverrideHover_ItemArray_int_int += ItemSlot_OverrideHover;
		IL.Terraria.Recipe.FindRecipes += Recipe_FindRecipes;
		IL.Terraria.Recipe.Create += Recipe_Create;
	}

	internal static void ItemSlot_OverrideHover(ILContext il)
	{
		ILCursor cursor = new ILCursor(il);

		if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdarg(1), i => i.MatchBrtrue(out _), i => i.MatchLdsfld<Main>("InReforgeMenu")))
		{
			ILLabel? label = cursor.Instrs[cursor.Index + 1].Operand as ILLabel;

			cursor.Emit(OpCodes.Ldloc, 0);
			cursor.EmitDelegate<Func<Item, bool>>(item =>
			{
				foreach (IItemStorageUI ui in ContainerLibrary.OpenedStorageUIs)
				{
					if (ui.GetItemStorage().CanInsertItem(Main.LocalPlayer, item))
					{
						string texture = ui.GetCursorTexture(item);

						if (!string.IsNullOrWhiteSpace(texture))
						{
							CustomCursor.CustomCursor.SetCursor(texture);
							return true;
						}
					}
				}

				return false;
			});

			cursor.Emit(OpCodes.Brtrue, label);
		}

		if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchCall<Main>("get_npcShop")))
		{
			ILLabel label = cursor.Instrs[cursor.Index - 1].Operand as ILLabel;

			cursor.Emit(OpCodes.Ldloc, 0);
			cursor.EmitDelegate<Func<Item, bool>>(item =>
			{
				foreach (IItemStorageUI ui in ContainerLibrary.OpenedStorageUIs)
				{
					if (ui.GetItemStorage().CanInsertItem(Main.LocalPlayer, item))
					{
						string texture = ui.GetCursorTexture(item);

						if (!string.IsNullOrWhiteSpace(texture))
						{
							CustomCursor.CustomCursor.SetCursor(texture);
							return true;
						}
					}
				}

				return false;
			});

			cursor.Emit(OpCodes.Brtrue, label);
		}
	}

	#region Crafting
	private static IEnumerable<ICraftingStorage> GetCraftingStorages(Player player)
	{
		foreach (Item item in player.inventory)
		{
			if (!item.IsAir && item.ModItem is ICraftingStorage storage) yield return storage;
		}
	}

	private static void Recipe_FindRecipes(ILContext il)
	{
		ILCursor cursor = new ILCursor(il);

		if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdcI4(0), i => i.MatchStloc(31)))
		{
			cursor.Emit(OpCodes.Ldloc, 12);

			cursor.EmitDelegate<Func<Dictionary<int, int>, Dictionary<int, int>>>(availableItems =>
			{
				foreach (ICraftingStorage craftingStorage in GetCraftingStorages(Main.LocalPlayer))
				{
					ItemStorage storage = craftingStorage.GetItemStorage();

					foreach (int slot in craftingStorage.GetSlotsForCrafting())
					{
						Item item = storage[slot];
						if (item.stack > 0)
						{
							if (availableItems.ContainsKey(item.netID)) availableItems[item.netID] += item.stack;
							else availableItems[item.netID] = item.stack;
						}
					}
				}

				return availableItems;
			});

			cursor.Emit(OpCodes.Stloc, 12);
		}
	}

	private static void Recipe_Create(ILContext il)
	{
		ILCursor cursor = new ILCursor(il);

		if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdsfld<Main>("player"), i => i.MatchLdsfld<Main>("myPlayer"), i => i.MatchLdelemRef(), i => i.MatchLdfld<Player>("chest")))
		{
			cursor.Emit(OpCodes.Ldarg, 0);
			cursor.Emit(OpCodes.Ldloc, 3);
			cursor.Emit(OpCodes.Ldloc, 4);

			cursor.EmitDelegate<Func<Recipe, Item, int, int>>((self, ingredient, amount) =>
			{
				foreach (ICraftingStorage craftingStorage in GetCraftingStorages(Main.LocalPlayer))
				{
					ItemStorage storage = craftingStorage.GetItemStorage();

					foreach (int slot in craftingStorage.GetSlotsForCrafting())
					{
						if (amount <= 0) return amount;
						Item item = storage[slot];

						if (item.type != ingredient.type && !self.AcceptedByItemGroups(item.type, ingredient.type)) continue;

						int count = Math.Min(amount, item.stack);
						amount -= count;
						storage.ModifyStackSize(Main.LocalPlayer, slot, -count);
					}
				}

				return amount;
			});

			cursor.Emit(OpCodes.Stloc, 4);
		}
	}
	#endregion
}