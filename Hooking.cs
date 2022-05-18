using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace ContainerLibrary;

internal static class Hooking
{
	internal static void Load()
	{
		IL.Terraria.Main.DrawInterface_36_Cursor += DrawCursor;
		IL.Terraria.UI.ItemSlot.OverrideHover_ItemArray_int_int += ItemSlot_OverrideHover;
		// Recipe.FindRecipes += Hooking.Recipe_FindRecipes;
		// Recipe.Create += Hooking.Recipe_Create;
	}

	#region Cursor
	private const int CustomCursorOverride = 1001;
	private static Asset<Texture2D> CursorTexture;
	private static Vector2 CursorOffset;
	private static bool Pulse;

	public static void SetCursor(string texture, Vector2? offset = null, bool pulse = true)
	{
		Main.cursorOverride = CustomCursorOverride;
		CursorTexture = ModContent.Request<Texture2D>(texture);
		CursorOffset = offset ?? Vector2.Zero;
		Pulse = pulse;
	}

	internal static void DrawCursor(ILContext il)
	{
		ILCursor cursor = new ILCursor(il);
		ILLabel label = cursor.DefineLabel();

		if (cursor.TryGotoNext(i => i.MatchLdsfld(typeof(Main).GetField("cursorOverride", BindingFlags.Public | BindingFlags.Static))))
		{
			cursor.Emit(OpCodes.Ldsfld, typeof(Main).GetField("cursorOverride", BindingFlags.Public | BindingFlags.Static));
			cursor.Emit(OpCodes.Ldc_I4, CustomCursorOverride);
			cursor.Emit(OpCodes.Ceq);
			cursor.Emit(OpCodes.Brfalse, label);

			cursor.Emit(OpCodes.Ldloc, 5);
			cursor.Emit(OpCodes.Ldloc, 7);

			cursor.EmitDelegate<Action<float, float>>((rotation, scale) =>
			{
				if (CursorTexture == null) return;

				float texScale = Math.Min(20f / CursorTexture.Value.Width, 20f / CursorTexture.Value.Height);
				float s = Pulse ? Main.cursorScale * texScale : texScale;
				Main.spriteBatch.Draw(CursorTexture.Value, new Vector2(Main.mouseX, Main.mouseY), null, Color.White, rotation, CursorOffset, s, SpriteEffects.None, 0f);
			});
			cursor.Emit(OpCodes.Ret);

			cursor.MarkLabel(label);
		}
	}
	#endregion

	internal static void ItemSlot_OverrideHover(ILContext il)
	{
		ILCursor cursor = new ILCursor(il);

		if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdarg(1), i => i.MatchBrtrue(out _), i => i.MatchLdsfld<Main>("InReforgeMenu")))
		{
			ILLabel label = cursor.Instrs[cursor.Index - 2].Operand as ILLabel;

			cursor.Emit(OpCodes.Ldloc, 0);
			cursor.EmitDelegate<Func<Item, bool>>(item =>
			{
				foreach (IItemStorageUI ui in ContainerLibrary.OpenedStorageUIs)
				{
					if (ui.GetItemStorage().CanInsertItem(Main.LocalPlayer, item))
					{
						string texture = ui.GetTexture(item);

						if (!string.IsNullOrWhiteSpace(texture))
						{
							SetCursor(texture);
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
						string texture = ui.GetTexture(item);

						if (!string.IsNullOrWhiteSpace(texture))
						{
							SetCursor(texture);
							return true;
						}
					}
				}

				return false;
			});

			cursor.Emit(OpCodes.Brtrue, label);
		}
	}

	private static IEnumerable<ICraftingStorage> GetCraftingStorages(Player player)
	{
		foreach (Item item in player.inventory)
		{
			if (!item.IsAir && item.ModItem is ICraftingStorage storage) yield return storage;
		}
	}

	// private static void Recipe_FindRecipes(ILContext il)
	// {
	// 	ILCursor cursor = new ILCursor(il);
	//
	// 	if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdcI4(0), i => i.MatchStloc(31)))
	// 	{
	// 		cursor.Emit(OpCodes.Ldloc, 12);
	//
	// 		cursor.EmitDelegate<Func<Dictionary<int, int>, Dictionary<int, int>>>(availableItems =>
	// 		{
	// 			foreach (ICraftingStorage craftingStorage in GetCraftingStorages(Main.LocalPlayer))
	// 			{
	// 				ItemStorage storage = craftingStorage.GetItemStorage();
	//
	// 				foreach (int slot in craftingStorage.GetSlotsForCrafting())
	// 				{
	// 					Item item = storage[slot];
	// 					if (item.stack > 0)
	// 					{
	// 						if (availableItems.ContainsKey(item.netID)) availableItems[item.netID] += item.stack;
	// 						else availableItems[item.netID] = item.stack;
	// 					}
	// 				}
	// 			}
	//
	// 			return availableItems;
	// 		});
	//
	// 		cursor.Emit(OpCodes.Stloc, 12);
	// 	}
	// }

	// private static void Recipe_Create(ILContext il)
	// {
	// 	bool AcceptedByItemGroups(Recipe recipe, int invType, int reqType)
	// 	{
	// 		return recipe.acceptedGroups.Any(num =>
	// 		{
	// 			var group = RecipeGroup.recipeGroups[num];
	// 			return group.ContainsItem(invType) && group.ContainsItem(reqType);
	// 		});
	// 	}
	//
	// 	ILCursor cursor = new ILCursor(il);
	//
	// 	if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdloc(3), i => i.MatchLdcI4(1), i => i.MatchAdd()))
	// 	{
	// 		cursor.Emit(OpCodes.Ldarg, 0);
	// 		cursor.Emit(OpCodes.Ldloc, 2);
	// 		cursor.Emit(OpCodes.Ldloc, 4);
	//
	// 		cursor.EmitDelegate<Func<Recipe, Item, int, int>>((self, ingredient, amount) =>
	// 		{
	// 			foreach (ICraftingStorage craftingStorage in GetCraftingStorages(Main.LocalPlayer))
	// 			{
	// 				ItemStorage storage = craftingStorage.GetItemStorage();
	//
	// 				foreach (int slot in craftingStorage.GetSlotsForCrafting())
	// 				{
	// 					if (amount <= 0) return amount;
	// 					Item item = storage[slot];
	//
	// 					if (!item.IsTheSameAs(ingredient) && !AcceptedByItemGroups(self, item.type, ingredient.type)) continue;
	//
	// 					int count = Math.Min(amount, item.stack);
	// 					amount -= count;
	// 					storage.ModifyStackSize(Main.LocalPlayer, slot, -count);
	// 				}
	// 			}
	//
	// 			return amount;
	// 		});
	//
	// 		cursor.Emit(OpCodes.Stloc, 4);
	// 	}
	// }

	// internal static void ItemSlot_OverrideHover(ILContext il)
	// {
	// 	ILCursor cursor = new ILCursor(il);
	// 	ILLabel label = cursor.DefineLabel();
	// 	ILLabel caseStart = cursor.DefineLabel();
	//
	// 	ILLabel[] targets = null;
	// 	if (cursor.TryGotoNext(i => i.MatchSwitch(out targets))) targets[0] = caseStart;
	//
	// 	if (cursor.TryGotoNext(i => i.MatchLdsfld(typeof(Main).GetField("npcShop", BindingFlags.Public | BindingFlags.Static))))
	// 	{
	// 		cursor.MarkLabel(caseStart);
	//
	// 		cursor.Emit(OpCodes.Ldloc, 0);
	// 		cursor.EmitDelegate<Func<Item, bool>>(item =>
	// 		{
	// 			// BaseElement BaseElement = PanelUI.Instance.Children.FirstOrDefault(element => (element as IItemHandlerUI)?.Handler?.HasSpace(item) ?? false);
	// 			// string texture = (BaseElement as IItemHandlerUI)?.GetTexture(item);
	// 			//
	// 			// if (!string.IsNullOrWhiteSpace(texture))
	// 			// {
	// 			// 	BaseLibrary.Hooking.SetCursor(texture);
	// 			// 	return true;
	// 			// }
	//
	// 			Main.NewText("overriding cursor");
	// 			
	// 			return false;
	// 		});
	// 		cursor.Emit(OpCodes.Brtrue, label);
	// 	}
	//

	// 	// if (cursor.TryGotoNext(i => i.MatchLdsflda(typeof(Main).GetField("keyState", Utility.defaultFlags)))) cursor.MarkLabel(label);


	// }
}