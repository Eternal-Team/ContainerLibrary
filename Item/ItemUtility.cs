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
		public static bool BlockGetItem;

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

					BlockGetItem = true;
					item = Combine(item.Split().Select(split => player.GetItem(player.whoAmI, split)));
					BlockGetItem = false;

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

				BlockGetItem = true;
				item = Combine(item.Split().Select(split => player.GetItem(player.whoAmI, split)));
				BlockGetItem = false;

				handler.OnContentsChanged?.Invoke(slot);
			}
		}

		/// <summary>
		///     Combines a list of items with the same type into one item
		/// </summary>
		public static Item Combine(IEnumerable<Item> items)
		{
			List<Item> list = items.ToList();

			Item ret = new Item();

			foreach (Item item in list)
			{
				if (ret.IsAir && !item.IsAir)
				{
					ret.SetDefaults(item.type);
					ret.stack = 0;
				}

				if (ret.type == item.type) ret.stack += item.stack;
			}

			return ret;
		}

		/// <summary>
		///     Splits an item into multiple items with stack clamped to max stack
		/// </summary>
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
		///     Deposits Items from the player inventory to the handler
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

		/// <summary>
		///     Drops items in a ItemHandler in world with a specified hitbox
		/// </summary>
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

		/// <summary>
		///     Checks whether an ItemHandler has space for an item
		/// </summary>
		public static bool HasSpace(this ItemHandler handler, Item item)
		{
			return handler.Items.Any((item1, i) => (item1.IsAir || item1.type == item.type && item1.stack < handler.GetItemLimit(i, item1)) && handler.IsItemValid(i, item));
		}

		public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
			if (source == null || predicate == null) throw new ArgumentNullException();

			int index = 0;
			return source.Any(element => predicate(element, index++));
		}
	}
}