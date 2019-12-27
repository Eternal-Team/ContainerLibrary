using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace ContainerLibrary
{
	public static class ItemUtility
	{
		public static bool BlockGetItem;

		public static bool Grow(this ItemHandler handler, int slot, int quantity)
		{
			ref Item item = ref handler.GetItemInSlotByRef(slot);
			int limit = handler.GetItemLimit(slot);
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

		public static bool Contains(this ItemHandler handler, int type)
		{
			for (int i = 0; i < handler.Slots; i++)
			{
				Item item = handler.GetItemInSlot(i);

				if (!item.IsAir && item.type == type) return true;
			}

			return false;
		}

		public static long CoinsValue(this ItemHandler handler)
		{
			long num = 0L;
			for (int i = 0; i < handler.Slots; i++)
			{
				Item item = handler.GetItemInSlot(i);
				switch (item.type)
				{
					case 71:
						num += item.stack;
						break;
					case 72:
						num += item.stack * 100;
						break;
					case 73:
						num += item.stack * 10000;
						break;
					case 74:
						num += item.stack * 1000000;
						break;
				}
			}

			return num;
		}

		public static int CountItems(this ItemHandler handler, Func<Item, bool> predicate)
		{
			int count = 0;

			for (int i = 0; i < handler.Slots; i++)
			{
				Item item = handler.GetItemInSlot(i);
				if (predicate(item)) count += item.stack;
			}

			return count;
		}

		/// <summary>
		///     Quick stacks items from player inventory to handler
		/// </summary>
		public static void QuickStack(ItemHandler handler, Player player)
		{
			for (int i = 49; i >= 10; i--)
			{
				ref Item inventory = ref player.inventory[i];

				if (!inventory.IsAir && handler.Contains(inventory.type)) handler.InsertItem(ref inventory);
			}

			Main.PlaySound(SoundID.Grab);
		}

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
					ret = item.Clone();
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

		/// <summary>
		///     Drops items in a handler in world with a specified hitbox
		/// </summary>
		public static void DropItems(this ItemHandler handler, Rectangle hitbox)
		{
			for (var i = 0; i < handler.Slots; i++)
			{
				Item item = handler.GetItemInSlot(i);
				if (!item.IsAir)
				{
					Item.NewItem(hitbox, item.type, item.stack, prefixGiven: item.prefix);
					item.TurnToAir();
					handler.OnContentsChanged(i);
				}
			}
		}

		/// <summary>
		///     Checks whether an handler has space for an item
		/// </summary>
		public static bool HasSpace(this ItemHandler handler, Item item)
		{
			for (int i = 0; i < handler.Slots; i++)
			{
				Item inHandler = handler.GetItemInSlot(i);

				if (inHandler.IsAir && handler.IsItemValid(i, item)) return true;
				if (inHandler.type == item.type && inHandler.stack < handler.GetItemLimit(i)) return true;
			}

			return false;
		}

		public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
			if (source == null || predicate == null) throw new ArgumentNullException();

			int index = 0;
			return source.Any(element => predicate(element, index++));
		}
	}
}