using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using Item = Terraria.Item;
using Main = Terraria.Main;
using Player = Terraria.Player;
using Recipe = Terraria.Recipe;

namespace ContainerLibrary
{
	public static class Hooking
	{
		public static Func<(int reductionPercent, bool reduce)> CheckAlchemy = () => (33, false);
		public static Action ModifyAdjTiles;

		internal static void Load()
		{
			IL.Terraria.Recipe.FindRecipes += Recipe_FindRecipes;
			IL.Terraria.Recipe.Create += Recipe_Create;
		}

		private static void Recipe_Create(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);
			ILLabel label = cursor.DefineLabel();
			
			if (cursor.TryGotoNext(i => i.MatchLdfld(typeof(Player).GetField("chest")), i => i.MatchLdcI4(-1), i => i.MatchBeq(out _)))
			{
				cursor.Index += 2;
				cursor.Remove();
				cursor.Emit(OpCodes.Beq, label);
			}

			if (cursor.TryGotoNext(i => i.MatchLdloc(2), i => i.MatchLdcI4(0), i => i.MatchBle(out _)))
			{
				cursor.Index += 2;
				cursor.Remove();
				cursor.Emit(OpCodes.Ble, label);
			}

			if (cursor.TryGotoNext(i => i.MatchLdloc(0), i => i.MatchLdcI4(1), i => i.MatchAdd()))
			{
				cursor.MarkLabel(label);

				cursor.Emit(OpCodes.Ldarg, 0);
				cursor.Emit(OpCodes.Ldloc, 1);
				cursor.Emit(OpCodes.Ldloc, 2);

				cursor.EmitDelegate<Func<Recipe, Item, int, int>>((self, ingredient, amount) =>
				{
					foreach (ICraftingStorage storage in Main.LocalPlayer.inventory.Where(item => item.modItem is ICraftingStorage).Select(item => (ICraftingStorage)item.modItem))
					{
						for (int index = 0; index < storage.CraftingHandler.Items.Count; index++)
						{
							if (amount <= 0) return amount;
							Item item = storage.CraftingHandler.Items[index];

							if (item.IsTheSameAs(ingredient) || self.useWood(item.type, ingredient.type) || self.useSand(item.type, ingredient.type) || self.useIronBar(item.type, ingredient.type) || self.usePressurePlate(item.type, ingredient.type) || self.useFragment(item.type, ingredient.type) || self.AcceptedByItemGroups(item.type, ingredient.type))
							{
								int count = Math.Min(amount, item.stack);
								amount -= count;
								storage.CraftingHandler.ExtractItem(index, count);
							}
						}
					}

					return amount;
				});

				cursor.Emit(OpCodes.Stloc, 2);
			}
		}

		// todo: alchemics station doesn't recognize as alchemy station

		private static void Recipe_FindRecipes(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);
			ILLabel label = cursor.DefineLabel();

			if (cursor.TryGotoNext(i => i.MatchLdfld(typeof(Player).GetField("chest")), i => i.MatchLdcI4(-1), i => i.MatchBeq(out _)))
			{
				cursor.Index += 2;
				cursor.Remove();
				cursor.Emit(OpCodes.Beq, label);
			}

			if (cursor.TryGotoNext(i => i.MatchLdcI4(0), i => i.MatchStloc(9)))
			{
				cursor.MarkLabel(label);

				cursor.Emit(OpCodes.Ldloc, 6);

				cursor.EmitDelegate<Func<Dictionary<int, int>, Dictionary<int, int>>>(availableItems =>
				{
					foreach (ICraftingStorage storage in Main.LocalPlayer.inventory.Where(item => item.modItem is ICraftingStorage).Select(item => (ICraftingStorage)item.modItem))
					{
						foreach (Item item in storage.CraftingHandler.Items)
						{
							if (item.stack > 0)
							{
								if (availableItems.ContainsKey(item.netID)) availableItems[item.netID] += item.stack;
								else availableItems[item.netID] = item.stack;
							}
						}
					}

					return availableItems;
				});

				cursor.Emit(OpCodes.Stloc, 6);
			}
		}
	}
}