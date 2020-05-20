using BaseLibrary;
using BaseLibrary.UI;
using IL.Terraria.UI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace ContainerLibrary
{
	public static class Hooking
	{
		public static Func<int> AlchemyConsumeChance = () => 3;
		public static Func<bool> AlchemyApplyChance = () => Main.LocalPlayer.alchemyTable;
		public static Action<Player> ModifyAdjTiles = player => { };

		internal static void Load()
		{
			IL.Terraria.Recipe.FindRecipes += Recipe_FindRecipes;
			IL.Terraria.Recipe.Create += Recipe_Create;
			IL.Terraria.Player.AdjTiles += Player_AdjTiles;

			if (ModLoader.GetMod("BaseLibrary") != null) ItemSlot.OverrideHover += ItemSlot_OverrideHover;
		}

		private static void Player_AdjTiles(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			if (cursor.TryGotoNext(i => i.MatchLdarg(0), i => i.MatchLdarg(0), i => i.MatchLdfld<Player>("adjWater")))
			{
				cursor.Emit(OpCodes.Ldarg, 0);
				cursor.EmitDelegate<Action<Player>>(player => ModifyAdjTiles(player));
			}
		}

		private static void Recipe_Create(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);
			ILLabel label = cursor.DefineLabel();
			ILLabel labelAlchemy = cursor.DefineLabel();
			ILLabel labelCheckAmount = cursor.DefineLabel();

			if (cursor.TryGotoNext(i => i.MatchLdloc(5), i => i.MatchBrfalse(out _)))
			{
				cursor.Index++;
				cursor.Remove();
				cursor.Emit(OpCodes.Brfalse, labelAlchemy);
			}

			if (cursor.TryGotoNext( i => i.MatchLdarg(0), i => i.MatchLdfld<Recipe>("alchemy"), i => i.MatchBrfalse(out _)))
			{
				cursor.MarkLabel(labelAlchemy);
			
				cursor.Emit(OpCodes.Ldarg, 0);
				cursor.Emit<Recipe>(OpCodes.Ldfld, "alchemy");
				cursor.Emit(OpCodes.Ldloc, 4);
			
				cursor.EmitDelegate<Func<bool, int, int>>((alchemy, amount) =>
				{
					if (alchemy && AlchemyApplyChance())
					{
						int reduction = 0;
						for (int j = 0; j < amount; j++)
						{
							if (Main.rand.Next(AlchemyConsumeChance()) == 0) reduction++;
						}
			
						amount -= reduction;
					}
			
					return amount;
				});
			
				cursor.Emit(OpCodes.Stloc, 4);
				cursor.Emit(OpCodes.Br, labelCheckAmount);
			}
			
			if (cursor.TryGotoNext(i => i.MatchLdloc(4), i => i.MatchLdcI4(1), i => i.MatchBle(out _))) cursor.MarkLabel(labelCheckAmount);

			if (cursor.TryGotoNext(i => i.MatchLdfld<Player>("chest"), i => i.MatchLdcI4(-1), i => i.MatchBeq(out _)))
			{
				cursor.Index += 2;
				cursor.Remove();
				cursor.Emit(OpCodes.Beq, label);
			}
			
			// if (cursor.TryGotoNext(i => i.MatchLdloc(2), i => i.MatchLdcI4(0), i => i.MatchBle(out _)))
			// {
			// 	cursor.Index += 2;
			// 	cursor.Remove();
			// 	cursor.Emit(OpCodes.Ble, label);
			// }
			//
			if (cursor.TryGotoNext(i => i.MatchLdloc(3), i => i.MatchLdcI4(1), i => i.MatchAdd()))
			{
				cursor.MarkLabel(label);
			
				cursor.Emit(OpCodes.Ldarg, 0);
				cursor.Emit(OpCodes.Ldloc, 2);
				cursor.Emit(OpCodes.Ldloc, 4);
			
				cursor.EmitDelegate<Func<Recipe, Item, int, int>>((self, ingredient, amount) =>
				{
					foreach (ICraftingStorage storage in Main.LocalPlayer.inventory.Where(item => item.modItem is ICraftingStorage).Select(item => (ICraftingStorage)item.modItem))
					{
						for (int i = 0; i < storage.CraftingHandler.Slots; i++)
						{
							if (amount <= 0) return amount;
							Item item = storage.CraftingHandler.GetItemInSlot(i);
			
							if (item.IsTheSameAs(ingredient) || self.useWood(item.type, ingredient.type) || self.useSand(item.type, ingredient.type) || self.useIronBar(item.type, ingredient.type) || self.usePressurePlate(item.type, ingredient.type) || self.useFragment(item.type, ingredient.type) || self.AcceptedByItemGroups(item.type, ingredient.type))
							{
								int count = Math.Min(amount, item.stack);
								amount -= count;
								storage.CraftingHandler.ExtractItem(i, count);
							}
						}
					}
			
					return amount;
				});
			
				cursor.Emit(OpCodes.Stloc, 4);
			}
		}

		private static void Recipe_FindRecipes(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdcI4(0), i => i.MatchStloc(9)))
			{
				cursor.Emit(OpCodes.Ldloc, 6);

				cursor.EmitDelegate<Func<Dictionary<int, int>, Dictionary<int, int>>>(availableItems =>
				{
					foreach (ICraftingStorage storage in Main.LocalPlayer.inventory.Where(item => item.modItem is ICraftingStorage).Select(item => (ICraftingStorage)item.modItem))
					{
						for (int i = 0; i < storage.CraftingHandler.Slots; i++)
						{
							Item item = storage.CraftingHandler.GetItemInSlot(i);
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

		private static void ItemSlot_OverrideHover(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);
			ILLabel label = cursor.DefineLabel();
			ILLabel caseStart = cursor.DefineLabel();

			ILLabel[] targets = null;
			if (cursor.TryGotoNext(i => i.MatchSwitch(out targets))) targets[0] = caseStart;

			if (cursor.TryGotoNext(i => i.MatchLdsfld(typeof(Main).GetField("npcShop", Utility.defaultFlags))))
			{
				cursor.MarkLabel(caseStart);

				cursor.Emit(OpCodes.Ldloc, 0);
				cursor.EmitDelegate<Func<Item, bool>>(item =>
				{
					BaseElement BaseElement = PanelUI.Instance.Children.FirstOrDefault(element => (element as IItemHandlerUI)?.Handler?.HasSpace(item) ?? false);
					string texture = (BaseElement as IItemHandlerUI)?.GetTexture(item);

					if (!string.IsNullOrWhiteSpace(texture))
					{
						BaseLibrary.Hooking.SetCursor(texture);
						return true;
					}

					return false;
				});
				cursor.Emit(OpCodes.Brtrue, label);
			}

			if (cursor.TryGotoNext(i => i.MatchLdsflda(typeof(Main).GetField("keyState", Utility.defaultFlags)))) cursor.MarkLabel(label);
		}
	}
}