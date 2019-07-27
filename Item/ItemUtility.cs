using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;

namespace ContainerLibrary
{
	public static class ItemUtility
	{
		public static void SetCount(this ItemHandler handler, int slot, int size)
		{
			ref Item item = ref handler.GetItemInSlotByRef(slot);
			item.stack = size;
			if (item.stack <= 0) item.TurnToAir();
			handler.OnContentsChanged?.Invoke(slot);
		}

		public static bool Grow(this ItemHandler handler, int slot, int quantity)
		{
			ref Item item = ref handler.GetItemInSlotByRef(slot);
			int limit = handler.GetItemLimit(slot, item);
			if (item.IsAir || quantity <= 0 || item.stack + quantity > limit) return false;

			item.stack += quantity;
			if (item.stack <= 0) item.TurnToAir();
			handler.OnContentsChanged?.Invoke(slot);

			return true;
		}

		public static bool Shrink(this ItemHandler handler, int slot, int quantity)
		{
			ref Item item = ref handler.GetItemInSlotByRef(slot);

			if (item.IsAir || quantity <= 0 || item.stack - quantity < 0) return false;

			item.stack -= quantity;
			if (item.stack <= 0) item.TurnToAir();
			handler.OnContentsChanged?.Invoke(slot);

			return true;
		}

		//public static void MoveCoins(List<Item> from, ItemHandler handler)
		//{
		//	Item[] containerInv = handler.Items;
		//	int[] coins = new int[4];

		//	List<int> coinSlotsPlayer = new List<int>();
		//	List<int> coinSlotsContainer = new List<int>();

		//	bool anyCoins = false;
		//	int[] coinValueArr = new int[containerInv.Length];

		//	for (int i = 0; i < containerInv.Length; i++)
		//	{
		//		coinValueArr[i] = -1;
		//		if (containerInv[i].stack < 1 || containerInv[i].type < 1)
		//		{
		//			coinSlotsContainer.Add(i);
		//			containerInv[i] = new Item();
		//		}

		//		if (containerInv[i] != null && containerInv[i].stack > 0)
		//		{
		//			int num = 0;
		//			switch (containerInv[i].type)
		//			{
		//				case 71:
		//					num = 1;
		//					break;
		//				case 72:
		//					num = 2;
		//					break;
		//				case 73:
		//					num = 3;
		//					break;
		//				case 74:
		//					num = 4;
		//					break;
		//			}

		//			coinValueArr[i] = num - 1;
		//			if (num > 0)
		//			{
		//				coins[num - 1] += containerInv[i].stack;
		//				coinSlotsContainer.Add(i);
		//				containerInv[i] = new Item();
		//				anyCoins = true;
		//			}
		//		}

		//		handler.OnContentsChanged(i);
		//	}

		//	if (!anyCoins) return;
		//	Main.PlaySound(7);

		//	for (int slot = 0; slot < from.Count; slot++)
		//	{
		//		if (from[slot] != null && from[slot].stack > 0)
		//		{
		//			int index = 0;
		//			switch (from[slot].type)
		//			{
		//				case 71:
		//					index = 1;
		//					break;
		//				case 72:
		//					index = 2;
		//					break;
		//				case 73:
		//					index = 3;
		//					break;
		//				case 74:
		//					index = 4;
		//					break;
		//			}

		//			if (index > 0)
		//			{
		//				coins[index - 1] += from[slot].stack;
		//				coinSlotsPlayer.Add(slot);
		//				from[slot] = new Item();
		//			}
		//		}
		//	}

		//	for (int k = 0; k < 3; k++)
		//	{
		//		while (coins[k] >= 100)
		//		{
		//			coins[k] -= 100;
		//			coins[k + 1]++;
		//		}
		//	}

		//	for (int l = 0; l < containerInv.Length; l++)
		//	{
		//		if (coinValueArr[l] >= 0 && containerInv[l].type == 0)
		//		{
		//			int num3 = l;
		//			int num4 = coinValueArr[l];
		//			if (coins[num4] > 0)
		//			{
		//				containerInv[num3].SetDefaults(71 + num4);
		//				containerInv[num3].stack = coins[num4];
		//				if (containerInv[num3].stack > containerInv[num3].maxStack) containerInv[num3].stack = containerInv[num3].maxStack;
		//				coins[num4] -= containerInv[num3].stack;
		//				coinValueArr[l] = -1;
		//				handler.OnContentsChanged(l);
		//			}

		//			coinSlotsContainer.Remove(num3);
		//		}
		//	}

		//	for (int m = 0; m < containerInv.Length; m++)
		//	{
		//		if (coinValueArr[m] >= 0 && containerInv[m].type == 0)
		//		{
		//			int num5 = m;
		//			int n = 3;
		//			while (n >= 0)
		//			{
		//				if (coins[n] > 0)
		//				{
		//					containerInv[num5].SetDefaults(71 + n);
		//					containerInv[num5].stack = coins[n];
		//					if (containerInv[num5].stack > containerInv[num5].maxStack) containerInv[num5].stack = containerInv[num5].maxStack;
		//					coins[n] -= containerInv[num5].stack;
		//					coinValueArr[m] = -1;
		//					handler.OnContentsChanged(num5);
		//					break;
		//				}

		//				if (coins[n] == 0) n--;
		//			}

		//			coinSlotsContainer.Remove(num5);
		//		}
		//	}

		//	while (coinSlotsContainer.Count > 0)
		//	{
		//		int num6 = coinSlotsContainer[0];
		//		int num7 = 3;
		//		while (num7 >= 0)
		//		{
		//			if (coins[num7] > 0)
		//			{
		//				containerInv[num6].SetDefaults(71 + num7);
		//				containerInv[num6].stack = coins[num7];
		//				if (containerInv[num6].stack > containerInv[num6].maxStack) containerInv[num6].stack = containerInv[num6].maxStack;
		//				coins[num7] -= containerInv[num6].stack;
		//				handler.OnContentsChanged(num7);
		//				break;
		//			}

		//			if (coins[num7] == 0) num7--;
		//		}

		//		coinSlotsContainer.RemoveAt(0);
		//	}

		//	int num8 = 3;
		//	while (num8 >= 0 && coinSlotsPlayer.Count > 0)
		//	{
		//		int num9 = coinSlotsPlayer[0];
		//		if (coins[num8] > 0)
		//		{
		//			from[num9].SetDefaults(71 + num8);
		//			from[num9].stack = coins[num8];
		//			if (from[num9].stack > from[num9].maxStack) from[num9].stack = from[num9].maxStack;
		//			coins[num8] -= from[num9].stack;
		//		}

		//		if (coins[num8] == 0) num8--;
		//		coinSlotsPlayer.RemoveAt(0);
		//	}
		//}

		//public static void Restock(ItemHandler handler)
		//{
		//	Player player = Main.LocalPlayer;
		//	Item[] inventory = player.inventory;
		//	Item[] item = handler.Items;

		//	HashSet<int> restackableItems = new HashSet<int>();
		//	List<int> canRestackIndex = new List<int>();
		//	List<int> list2 = new List<int>();

		//	for (int i = 57; i >= 0; i--)
		//	{
		//		if ((i < 50 || i >= 54) && (inventory[i].type < 71 || inventory[i].type > 74))
		//		{
		//			if (inventory[i].stack > 0 && inventory[i].maxStack > 1 && inventory[i].prefix == 0)
		//			{
		//				restackableItems.Add(inventory[i].netID);
		//				if (inventory[i].stack < inventory[i].maxStack) canRestackIndex.Add(i);
		//			}
		//			else if (inventory[i].stack == 0 || inventory[i].netID == 0 || inventory[i].type == 0) list2.Add(i);
		//		}
		//	}

		//	bool restocked = false;
		//	for (int j = 0; j < handler.Slots; j++)
		//	{
		//		if (item[j].stack >= 1 && item[j].prefix == 0 && restackableItems.Contains(item[j].netID))
		//		{
		//			bool flag2 = false;
		//			for (int k = 0; k < canRestackIndex.Count; k++)
		//			{
		//				int num = canRestackIndex[k];
		//				int context = 0;
		//				if (num >= 50)
		//				{
		//					context = 2;
		//				}

		//				if (inventory[num].netID == item[j].netID && ItemSlot.PickItemMovementAction(inventory, context, num, item[j]) != -1)
		//				{
		//					int num2 = item[j].stack;
		//					if (inventory[num].maxStack - inventory[num].stack < num2)
		//					{
		//						num2 = inventory[num].maxStack - inventory[num].stack;
		//					}

		//					inventory[num].stack += num2;
		//					item[j].stack -= num2;
		//					handler.OnContentsChanged(j);
		//					restocked = true;
		//					if (inventory[num].stack == inventory[num].maxStack)
		//					{
		//						canRestackIndex.RemoveAt(k);
		//						k--;
		//					}

		//					if (item[j].stack == 0)
		//					{
		//						item[j] = new Item();
		//						flag2 = true;
		//						handler.OnContentsChanged(j);
		//						break;
		//					}
		//				}
		//			}

		//			if (!flag2 && list2.Count > 0 && item[j].ammo != 0)
		//			{
		//				for (int l = 0; l < list2.Count; l++)
		//				{
		//					int context2 = 0;
		//					if (list2[l] >= 50)
		//					{
		//						context2 = 2;
		//					}

		//					if (ItemSlot.PickItemMovementAction(inventory, context2, list2[l], item[j]) != -1)
		//					{
		//						Item temp = inventory[list2[l]];
		//						inventory[list2[l]] = item[j];
		//						item[j] = temp;

		//						handler.OnContentsChanged(j);

		//						canRestackIndex.Add(list2[l]);
		//						list2.RemoveAt(l);
		//						restocked = true;
		//						break;
		//					}
		//				}
		//			}
		//		}
		//	}

		//	if (restocked) Main.PlaySound(7);
		//}

		//public static void QuickStack(ItemHandler handler, Func<Item, bool> selector = null)
		//{
		//	if (Main.LocalPlayer.IsStackingItems()) return;

		//	Item[] Items = handler.Items;

		//	bool stacked = false;
		//	for (int i = 0; i < handler.Slots; i++)
		//	{
		//		if (Items[i].type > 0 && Items[i].stack > 0 && !Items[i].favorited && (selector?.Invoke(Items[i]) ?? true))
		//		{
		//			int type = Items[i].type;
		//			int stack = Items[i].stack;
		//			Items[i] = Chest.PutItemInNearbyChest(Items[i], Main.LocalPlayer.Center);
		//			if (Items[i].type != type || Items[i].stack != stack) stacked = true;

		//			handler.OnContentsChanged(i);
		//		}
		//	}

		//	if (stacked) Main.PlaySound(7);
		//}

		//public static void QuickRestack(ItemHandler handler)
		//{
		//	if (Main.LocalPlayer.IsStackingItems()) return;
		//	Item[] Items = handler.Items;

		//	bool stacked = false;
		//	for (int i = 0; i < handler.Slots; i++)
		//	{
		//		if (Items[i].type > 0 && Items[i].stack > 0)
		//		{
		//			int type = Items[i].type;
		//			int stack = Items[i].stack;
		//			Items[i] = TakeItemFromNearbyChest(Items[i], Main.LocalPlayer.Center);
		//			handler.OnContentsChanged(i);
		//			if (Items[i].type != type || Items[i].stack != stack) stacked = true;
		//		}
		//	}

		//	if (stacked) Main.PlaySound(7);
		//}

		//public static Item TakeItemFromNearbyChest(Item item, Vector2 position)
		//{
		//	if (Main.netMode == 1)
		//		return item;
		//	for (int i = 0; i < Main.chest.Length; i++)
		//	{
		//		//bool hasItem = false;
		//		//bool emptySlot = false;
		//		if (Main.chest[i] != null && !IsPlayerInChest(i) && !Chest.isLocked(Main.chest[i].x, Main.chest[i].y))
		//		{
		//			Vector2 value = new Vector2(Main.chest[i].x * 16 + 16, Main.chest[i].y * 16 + 16);
		//			if ((value - position).Length() < 200f)
		//			{
		//				for (int j = 0; j < Main.chest[i].item.Length; j++)
		//				{
		//					if (Main.chest[i].item[j].type > 0 && Main.chest[i].item[j].stack > 0)
		//					{
		//						if (item.IsTheSameAs(Main.chest[i].item[j]))
		//						{
		//							//hasItem = true;
		//							int num = item.maxStack - item.stack;
		//							if (num > 0)
		//							{
		//								if (num > Main.chest[i].item[j].stack)
		//									num = Main.chest[i].item[j].stack;
		//								item.stack += num;
		//								Main.chest[i].item[j].stack -= num;
		//								if (Main.chest[i].item[j].stack <= 0)
		//									Main.chest[i].item[j].SetDefaults();
		//							}
		//						}
		//					}

		//					//else emptySlot = true;
		//				}

		//				//if (hasItem && emptySlot && item.stack > 0)
		//				//{
		//				//	for (int k = 0; k < Main.chest[i].item.Length; k++)
		//				//	{
		//				//		if (Main.chest[i].item[k].type == 0 || Main.chest[i].item[k].stack == 0)
		//				//		{
		//				//			Main.chest[i].item[k] = item.Clone();
		//				//			item.SetDefaults();
		//				//			return item;
		//				//		}
		//				//	}
		//				//}
		//			}
		//		}
		//	}

		//	return item;
		//}

		//private static bool IsPlayerInChest(int i) => Main.player.Any(player => player.chest == i);

		/// <summary>
		///     Deposits items from the handler to player inventory
		/// </summary>
		public static void LootAll(ItemHandler handler, Player player)
		{
			for (int i = 0; i < handler.Slots; i++)
			{
				ref Item item = ref handler.GetItemInSlotByRef(i);
				if (!item.IsAir)
				{
					item.position = player.Center;
					foreach (Item item1 in item.Split()) player.GetItem(player.whoAmI, item1);
					handler.OnContentsChanged?.Invoke(i);
				}
			}
		}

		/// <summary>
		///     Deposits item in a slot from the handler to player inventory
		/// </summary>
		public static void Loot(ItemHandler handler, int slot, Player player)
		{
			ref Item item = ref handler.GetItemInSlotByRef(slot);
			if (!item.IsAir)
			{
				item.position = player.Center;
				foreach (Item item1 in item.Split()) player.GetItem(player.whoAmI, item1);
				handler.OnContentsChanged?.Invoke(slot);
			}
		}

		public static IEnumerable<Item> Split(this Item item)
		{
			while (item.stack > 0)
			{
				Item clone = item.Clone();
				int count = Math.Min(item.stack, item.maxStack);
				clone.stack = count;
				yield return clone;

				item.stack -= count;
				if (item.stack <= 0)
				{
					item.TurnToAir();
					yield break;
				}
			}
		}

		/// <summary>
		///     Deposits items from the player inventory to the handler
		/// </summary>
		public static void DepositAll(ItemHandler handler, Player player)
		{
			for (int i = 53; i >= 10; i--)
			{
				ref Item item = ref player.inventory[i];
				if (item.IsAir || item.favorited) continue;
				handler.InsertItem(ref item);
				Main.PlaySound(SoundID.Grab);
			}
		}

		public static void DropItems(this ItemHandler handler, Rectangle hitbox)
		{
			Item[] list = handler.Items;
			for (var i = 0; i < handler.Slots; i++)
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

		public static bool HasSpace(this ItemHandler handler, Item item)
		{
			return handler.Items.Any((item1, i) => (item1.IsAir || item1.type == item.type && item1.stack < item1.maxStack) && handler.IsItemValid(i, item));
		}

		public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
			if (source == null || predicate == null) throw new ArgumentNullException();

			int index = 0;
			return source.Any(element => predicate(element, index++));
		}
	}
}