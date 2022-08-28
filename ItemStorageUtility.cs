using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;

namespace ContainerLibrary;

public static partial class ItemStorageUtility
{
	/// <summary>
	/// If you need to check if a storage contains an item, use <see cref="Contains(ItemStorage, int)" />. It is much faster.
	/// </summary>
	public static int Count(this ItemStorage storage, int type) => storage.Count(item => !item.IsAir && item.type == type);

	/// <summary>
	/// If you need to check if a storage contains an item, use <see cref="Contains(ItemStorage, Item)" />. It is much faster.
	/// </summary>
	public static int Count(this ItemStorage storage, Item item) => storage.Count(item.type);

	/// <summary>
	/// Transfers an item from one item storage to another.
	/// </summary>
	/// <param name="from">The item storage to take from.</param>
	/// <param name="user">The object doing this.</param>
	/// <param name="to">The item storage to send into.</param>
	/// <param name="fromSlot">The slot to take from.</param>
	/// <param name="amount">The amount of items to take from the slot.</param>
	public static void Transfer(this ItemStorage from, object? user, ItemStorage to, int fromSlot, int amount)
	{
		if (from.RemoveItem(user, fromSlot, out var item, amount))
		{
			to.InsertItem(user, ref item);
			from.InsertItem(user, ref item);
		}
	}

	/// <summary>
	/// Drops items from the storage into the rectangle specified.
	/// </summary>
	public static void DropItems(this ItemStorage storage, object? user, Rectangle hitbox)
	{
		IEntitySource? source = null;

		if (user is Entity entity)
		{
			source = new EntitySource_Parent(entity);
		}

		for (int i = 0; i < storage.Count; i++)
		{
			Item item = storage[i];
			if (!item.IsAir)
			{
				Item.NewItem(source, hitbox, item.type, item.stack, prefixGiven: item.prefix);
				storage.RemoveItem(user, i);
			}
		}
	}

	/// <summary>
	/// Quick stacks player's items into the storage.
	/// </summary>
	public static void QuickStack(this Player player, ItemStorage storage)
	{
		for (int i = 49; i >= 10; i--)
		{
			ref Item inventory = ref player.inventory[i];

			if (!inventory.IsAir && storage.Contains(inventory.type))
				storage.InsertItem(player, ref inventory);
		}
	}

	/// <summary>
	/// Loots storage's items into a player's inventory
	/// </summary>
	public static void LootAll(this Player player, ItemStorage storage)
	{
		for (int i = 0; i < storage.Count; i++)
		{
			player.Loot(storage, i);
		}
	}

	/// <summary>
	/// Loots storage's items into the player's inventory.
	/// <param name="slot">Slot to loot items from</param>
	/// </summary>
	public static void Loot(this Player player, ItemStorage storage, int slot)
	{
		Item item = storage[slot].Clone();
		if (!item.IsAir)
		{
			int count = 0;
			foreach (var split in item.Split())
			{
				split.position = player.Center;
				split.noGrabDelay = 0;
				Item fail = player.GetItem(player.whoAmI, split, GetItemSettings.LootAllSettings);
				if (fail.IsAir) count += split.stack;
				else
				{
					count += split.stack - fail.stack;
					break;
				}
			}

			storage.ModifyStackSize(player, slot, -count);
		}
	}

	/// <summary>
	/// Deposits a player's items into storage.
	/// </summary>
	public static void DepositAll(this Player player, ItemStorage storage)
	{
		for (int i = 49; i >= 10; i--)
		{
			ref Item item = ref player.inventory[i];
			if (item.IsAir || item.favorited) continue;
			storage.InsertItem(player, ref item);
		}
	}

	/// <summary>
	/// Combines several stacks of items into one stack, disregarding max stack.
	/// </summary>
	public static Item Combine(IEnumerable<Item> items)
	{
		Item ret = new Item();

		foreach (Item item in items)
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
	/// Splits an item into separate stacks that respect max stack.
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
}