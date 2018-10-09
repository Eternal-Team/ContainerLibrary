using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace ContainerLibrary
{
	public static partial class Utility
	{
		public static Item SplitStack(this Item item, int amount)
		{
			int i = Math.Min(amount, item.stack);
			Item itemstack = item.Clone();
			itemstack.SetCount(i);
			item.Shrink(i);
			return itemstack;
		}

		public static void SetCount(this Item item, int size)
		{
			item.stack = size;
			if (item.stack <= 0) item.TurnToAir();
		}

		public static void Grow(this Item item, int quantity) => item.SetCount(item.stack + quantity);

		public static void Shrink(this Item item, int quantity) => item.Grow(-quantity);

		public static bool AreItemStackTagsEqual(Item stackA, Item stackB)
		{
			if (stackA.IsAir && stackB.IsAir) return true;
			if (!stackA.IsAir && !stackB.IsAir)
			{
				TagCompound tagA = stackA.modItem.Save();
				TagCompound tagB = stackB.modItem.Save();

				if (tagA == null && tagB != null) return false;
				return tagA == null || tagA.Equals(tagB);
			}

			return false;
		}

		public static bool AreItemStacksEqual(Item stackA, Item stackB)
		{
			if (stackA.IsAir && stackB.IsAir) return true;

			return !stackA.IsAir && !stackB.IsAir && stackA.IsItemStackEqual(stackB);
		}

		private static bool IsItemStackEqual(this Item item, Item other)
		{
			if (item.stack != other.stack) return false;

			if (item != other) return false;

			TagCompound tagA = item.modItem.Save();
			TagCompound tagB = other.modItem.Save();

			if (tagA == default(TagCompound) && tagB != default(TagCompound)) return false;

			return tagA == default(TagCompound) || tagA.Equals(tagB);
		}

		public static bool IsItemEqual(this Item item, Item other) => !other.IsAir && item == other;

		public static bool HasTagCompound(this Item item) => item.modItem?.Save() != null;

		public static TagCompound GetTagCompound(this Item item) => item.modItem?.Save();

		public static void MoveCoins(List<Item> from, ItemHandler handler)
		{
			List<Item> containerInv = handler.stacks;
			int[] coins = new int[4];

			List<int> coinSlotsPlayer = new List<int>();
			List<int> coinSlotsContainer = new List<int>();

			bool anyCoins = false;
			int[] coinValueArr = new int[containerInv.Count];

			for (int i = 0; i < containerInv.Count; i++)
			{
				coinValueArr[i] = -1;
				if (containerInv[i].stack < 1 || containerInv[i].type < 1)
				{
					coinSlotsContainer.Add(i);
					containerInv[i] = new Item();
				}

				if (containerInv[i] != null && containerInv[i].stack > 0)
				{
					int num = 0;
					switch (containerInv[i].type)
					{
						case 71:
							num = 1;
							break;
						case 72:
							num = 2;
							break;
						case 73:
							num = 3;
							break;
						case 74:
							num = 4;
							break;
					}

					coinValueArr[i] = num - 1;
					if (num > 0)
					{
						coins[num - 1] += containerInv[i].stack;
						coinSlotsContainer.Add(i);
						containerInv[i] = new Item();
						anyCoins = true;
					}
				}

				handler.OnContentsChanged(i);
			}

			if (!anyCoins) return;
			Main.PlaySound(7);

			for (int slot = 0; slot < from.Count; slot++)
			{
				if (from[slot] != null && from[slot].stack > 0)
				{
					int index = 0;
					switch (from[slot].type)
					{
						case 71:
							index = 1;
							break;
						case 72:
							index = 2;
							break;
						case 73:
							index = 3;
							break;
						case 74:
							index = 4;
							break;
					}

					if (index > 0)
					{
						coins[index - 1] += from[slot].stack;
						coinSlotsPlayer.Add(slot);
						from[slot] = new Item();
					}
				}
			}

			for (int k = 0; k < 3; k++)
			{
				while (coins[k] >= 100)
				{
					coins[k] -= 100;
					coins[k + 1]++;
				}
			}

			for (int l = 0; l < containerInv.Count; l++)
			{
				if (coinValueArr[l] >= 0 && containerInv[l].type == 0)
				{
					int num3 = l;
					int num4 = coinValueArr[l];
					if (coins[num4] > 0)
					{
						containerInv[num3].SetDefaults(71 + num4);
						containerInv[num3].stack = coins[num4];
						if (containerInv[num3].stack > containerInv[num3].maxStack) containerInv[num3].stack = containerInv[num3].maxStack;
						coins[num4] -= containerInv[num3].stack;
						coinValueArr[l] = -1;
						handler.OnContentsChanged(l);
					}

					coinSlotsContainer.Remove(num3);
				}
			}

			for (int m = 0; m < containerInv.Count; m++)
			{
				if (coinValueArr[m] >= 0 && containerInv[m].type == 0)
				{
					int num5 = m;
					int n = 3;
					while (n >= 0)
					{
						if (coins[n] > 0)
						{
							containerInv[num5].SetDefaults(71 + n);
							containerInv[num5].stack = coins[n];
							if (containerInv[num5].stack > containerInv[num5].maxStack) containerInv[num5].stack = containerInv[num5].maxStack;
							coins[n] -= containerInv[num5].stack;
							coinValueArr[m] = -1;
							handler.OnContentsChanged(num5);
							break;
						}

						if (coins[n] == 0) n--;
					}

					coinSlotsContainer.Remove(num5);
				}
			}

			while (coinSlotsContainer.Count > 0)
			{
				int num6 = coinSlotsContainer[0];
				int num7 = 3;
				while (num7 >= 0)
				{
					if (coins[num7] > 0)
					{
						containerInv[num6].SetDefaults(71 + num7);
						containerInv[num6].stack = coins[num7];
						if (containerInv[num6].stack > containerInv[num6].maxStack) containerInv[num6].stack = containerInv[num6].maxStack;
						coins[num7] -= containerInv[num6].stack;
						handler.OnContentsChanged(num7);
						break;
					}

					if (coins[num7] == 0) num7--;
				}

				coinSlotsContainer.RemoveAt(0);
			}

			int num8 = 3;
			while (num8 >= 0 && coinSlotsPlayer.Count > 0)
			{
				int num9 = coinSlotsPlayer[0];
				if (coins[num8] > 0)
				{
					from[num9].SetDefaults(71 + num8);
					from[num9].stack = coins[num8];
					if (from[num9].stack > from[num9].maxStack) from[num9].stack = from[num9].maxStack;
					coins[num8] -= from[num9].stack;
				}

				if (coins[num8] == 0) num8--;
				coinSlotsPlayer.RemoveAt(0);
			}
		}

		public static void Restock(ItemHandler handler)
		{
			Player player = Main.LocalPlayer;
			Item[] inventory = player.inventory;
			List<Item> item = handler.stacks;

			HashSet<int> restackableItems = new HashSet<int>();
			List<int> canRestackIndex = new List<int>();
			List<int> list2 = new List<int>();

			for (int i = 57; i >= 0; i--)
			{
				if ((i < 50 || i >= 54) && (inventory[i].type < 71 || inventory[i].type > 74))
				{
					if (inventory[i].stack > 0 && inventory[i].maxStack > 1 && inventory[i].prefix == 0)
					{
						restackableItems.Add(inventory[i].netID);
						if (inventory[i].stack < inventory[i].maxStack) canRestackIndex.Add(i);
					}
					else if (inventory[i].stack == 0 || inventory[i].netID == 0 || inventory[i].type == 0) list2.Add(i);
				}
			}

			bool restocked = false;
			for (int j = 0; j < item.Count; j++)
			{
				if (item[j].stack >= 1 && item[j].prefix == 0 && restackableItems.Contains(item[j].netID))
				{
					bool flag2 = false;
					for (int k = 0; k < canRestackIndex.Count; k++)
					{
						int num = canRestackIndex[k];
						int context = 0;
						if (num >= 50)
						{
							context = 2;
						}

						if (inventory[num].netID == item[j].netID && ItemSlot.PickItemMovementAction(inventory, context, num, item[j]) != -1)
						{
							int num2 = item[j].stack;
							if (inventory[num].maxStack - inventory[num].stack < num2)
							{
								num2 = inventory[num].maxStack - inventory[num].stack;
							}

							inventory[num].stack += num2;
							item[j].stack -= num2;
							handler.OnContentsChanged(j);
							restocked = true;
							if (inventory[num].stack == inventory[num].maxStack)
							{
								canRestackIndex.RemoveAt(k);
								k--;
							}

							if (item[j].stack == 0)
							{
								item[j] = new Item();
								flag2 = true;
								handler.OnContentsChanged(j);
								break;
							}
						}
					}

					if (!flag2 && list2.Count > 0 && item[j].ammo != 0)
					{
						for (int l = 0; l < list2.Count; l++)
						{
							int context2 = 0;
							if (list2[l] >= 50)
							{
								context2 = 2;
							}

							if (ItemSlot.PickItemMovementAction(inventory, context2, list2[l], item[j]) != -1)
							{
								Item temp = inventory[list2[l]];
								inventory[list2[l]] = item[j];
								item[j] = temp;

								handler.OnContentsChanged(j);

								canRestackIndex.Add(list2[l]);
								list2.RemoveAt(l);
								restocked = true;
								break;
							}
						}
					}
				}
			}

			if (restocked) Main.PlaySound(7);
		}

		public static void QuickStack(ItemHandler handler, Func<Item, bool> selector = null)
		{
			if (Main.LocalPlayer.IsStackingItems()) return;

			List<Item> Items = handler.stacks;

			bool stacked = false;
			for (int i = 0; i < Items.Count; i++)
			{
				if (Items[i].type > 0 && Items[i].stack > 0 && !Items[i].favorited && (selector?.Invoke(Items[i]) ?? true))
				{
					int type = Items[i].type;
					int stack = Items[i].stack;
					Items[i] = Chest.PutItemInNearbyChest(Items[i], Main.LocalPlayer.Center);
					if (Items[i].type != type || Items[i].stack != stack) stacked = true;

					handler.OnContentsChanged(i);
				}
			}

			if (stacked) Main.PlaySound(7);
		}

		public static void QuickRestack(ItemHandler handler)
		{
			if (Main.LocalPlayer.IsStackingItems()) return;
			List<Item> Items = handler.stacks;

			bool stacked = false;
			for (int i = 0; i < Items.Count; i++)
			{
				if (Items[i].type > 0 && Items[i].stack > 0)
				{
					int type = Items[i].type;
					int stack = Items[i].stack;
					Items[i] = TakeItemFromNearbyChest(Items[i], Main.LocalPlayer.Center);
					handler.OnContentsChanged(i);
					if (Items[i].type != type || Items[i].stack != stack) stacked = true;
				}
			}

			if (stacked) Main.PlaySound(7);
		}

		private static bool IsPlayerInChest(int i) => Main.player.Any(x => x.chest == i);

		public static Item TakeItemFromNearbyChest(Item item, Vector2 position)
		{
			if (Main.netMode == 1) return item;
			for (int i = 0; i < Main.chest.Length; i++)
			{
				//bool hasItem = false;
				//bool emptySlot = false;
				if (Main.chest[i] != null && !IsPlayerInChest(i) && !Chest.isLocked(Main.chest[i].x, Main.chest[i].y))
				{
					Vector2 value = new Vector2(Main.chest[i].x * 16 + 16, Main.chest[i].y * 16 + 16);
					if ((value - position).Length() < 200f)
					{
						for (int j = 0; j < Main.chest[i].item.Length; j++)
						{
							if (Main.chest[i].item[j].type > 0 && Main.chest[i].item[j].stack > 0)
							{
								if (item.IsTheSameAs(Main.chest[i].item[j]))
								{
									//hasItem = true;
									int num = item.maxStack - item.stack;
									if (num > 0)
									{
										if (num > Main.chest[i].item[j].stack) num = Main.chest[i].item[j].stack;
										item.stack += num;
										Main.chest[i].item[j].stack -= num;
										if (Main.chest[i].item[j].stack <= 0) Main.chest[i].item[j].SetDefaults();
									}
								}
							}

							//else emptySlot = true;
						}

						//if (hasItem && emptySlot && item.stack > 0)
						//{
						//	for (int k = 0; k < Main.chest[i].item.Length; k++)
						//	{
						//		if (Main.chest[i].item[k].type == 0 || Main.chest[i].item[k].stack == 0)
						//		{
						//			Main.chest[i].item[k] = item.Clone();
						//			item.SetDefaults();
						//			return item;
						//		}
						//	}
						//}
					}
				}
			}

			return item;
		}

		public static void LootAll(ItemHandler handler, Func<Item, int, bool> selector)
		{
			Player player = Main.LocalPlayer;

			List<Item> Items = handler.stacks;

			for (int i = 0; i < Items.Count; i++)
			{
				Item Item = Items[i];
				if (Item.type > 0 && (selector?.Invoke(Item, i) ?? true))
				{
					Item.position = player.Center;
					Item = player.GetItem(Main.myPlayer, Item);
				}

				Items[i] = Item;
				handler.OnContentsChanged?.Invoke(i);
			}
		}

		public static void LootAll(ItemHandler handler, Func<Item, bool> selector = null) => LootAll(handler, (item, index) => selector?.Invoke(item) ?? true);

		public static void DepositAll(ItemHandler handler, Func<Item, bool> selector = null)
		{
			Player player = Main.LocalPlayer;
			List<Item> Items = handler.stacks;

			MoveCoins(player.inventory.Take(player.inventory.Length - 1).ToList(), handler);

			for (int pIndex = 49; pIndex >= 10; pIndex--)
			{
				if (player.inventory[pIndex].stack > 0 && player.inventory[pIndex].type > 0 && !player.inventory[pIndex].favorited && (selector?.Invoke(player.inventory[pIndex]) ?? true))
				{
					if (player.inventory[pIndex].maxStack > 1)
					{
						for (int bIndex = 0; bIndex < Items.Count; bIndex++)
						{
							if (Items[bIndex].stack < Items[bIndex].maxStack && player.inventory[pIndex].IsTheSameAs(Items[bIndex]))
							{
								int stack = player.inventory[pIndex].stack;
								if (player.inventory[pIndex].stack + Items[bIndex].stack > Items[bIndex].maxStack) stack = Items[bIndex].maxStack - Items[bIndex].stack;

								player.inventory[pIndex].stack -= stack;
								Items[bIndex].stack += stack;
								Main.PlaySound(7);

								if (player.inventory[pIndex].stack <= 0)
								{
									player.inventory[pIndex].SetDefaults();
									handler.OnContentsChanged(bIndex);
									break;
								}

								if (Items[bIndex].type == 0)
								{
									Items[bIndex] = player.inventory[pIndex].Clone();
									player.inventory[pIndex].SetDefaults();
								}

								handler.OnContentsChanged(bIndex);
							}
						}
					}

					if (player.inventory[pIndex].stack > 0)
					{
						for (int bIndex = 0; bIndex < Items.Count; bIndex++)
						{
							if (Items[bIndex].stack == 0)
							{
								Main.PlaySound(7);
								Items[bIndex] = player.inventory[pIndex].Clone();
								player.inventory[pIndex].SetDefaults();
								handler.OnContentsChanged(bIndex);
								break;
							}
						}
					}
				}
			}
		}

		public static void DropItems(this ItemHandler handler, Rectangle hitbox)
		{
			List<Item> list = handler.stacks;
			for (var i = 0; i < list.Count; i++)
			{
				Item item = list[i];
				if (!item.IsAir)
				{
					Item.NewItem(hitbox, item.type, item.stack, prefixGiven: item.prefix);
					item.TurnToAir();
					handler.OnContentsChanged(i);
				}
			}
		}
	}
}