using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ContainerLibrary;

public partial class ItemStorage
{
	private Item[] Items;
	private StackOverride? _stackOverride;
	private CanInteract? _canInteract; // NOTE: should this be a list?

	public int Count => Items.Length;

	public ItemStorage(int slots)
	{
		if (slots < 1) throw new ArgumentException("Can't create ItemStorage with less than 1 slot");

		Items = new Item[slots];
		for (int i = 0; i < slots; i++)
		{
			Items[i] = new Item();
		}
	}

	public Item this[int index]
	{
		get
		{
			ValidateSlotIndex(index);
			return Items[index];
		}
	}

	public ItemStorage Clone()
	{
		ItemStorage storage = (ItemStorage)MemberwiseClone();
		storage.Items = Items.Select(item => item.Clone()).ToArray();
		return storage;
	}

	public ItemStorage SetStackOverride(StackOverride @override)
	{
		_stackOverride = @override;
		return this;
	}

	public ItemStorage SetPermission(CanInteract canInteract)
	{
		_canInteract = canInteract;
		return this;
	}

	/// <summary>
	/// Inserts an item into the ItemStorage.
	/// </summary>
	/// <param name="user">User that performed this action</param>
	/// <param name="slot">Slot to which the item is to be inserted</param>
	/// <param name="source">Item to insert</param>
	/// <returns>Success or reason why the action could not be performed.</returns>
	public Result InsertItem(object? user, int slot, ref Item source, int amount = -1)
	{
		ValidateSlotIndex(slot);

		if (source.IsAir)
			return Result.ItemIsAir;

		if (_canInteract?.Invoke(user, slot, Action.Insert) == false)
			return Result.CantInteract;

		Item destination = Items[slot];

		if (!EvaluateFilters(slot, source) || (!destination.IsAir && (source.type != destination.type || !ItemLoader.CanStack(destination, source))))
			return Result.NotValid;

		int toInsert = StorageUtility.Min(amount < 0 ? int.MaxValue : amount, EvaluateMaxStack(slot, source) - destination.stack, source.stack);
		if (toInsert <= 0)
			return Result.DestinationFull;

		bool cantFit = source.stack > toInsert;

		if (destination.IsAir)
		{
			if (cantFit)
			{
				Items[slot] = source.Clone();
				ItemLoader.SplitStack(Items[slot], source, toInsert);
			}
			else
			{
				Items[slot] = source;
				source = new Item();
			}
		}
		else
		{
			if (cantFit)
			{
				ItemLoader.OnStack(destination, source, toInsert);
				destination.stack += toInsert;
				source.stack -= toInsert;
			}
			else
			{
				ItemLoader.OnStack(destination, source, toInsert);
				destination.stack += toInsert;
				source = new Item();
			}
		}

		// OnContentsChanged(user, Operation.Insert, slot);

		return cantFit ? Result.PartialSuccess : Result.Success;
	}

	/// <summary>
	/// Checks whether an item fits inside a slot
	/// </summary>
	/// <param name="user">User that performed this action</param>
	/// <param name="slot">Slot from which the item is to be fitted</param>
	/// <param name="item">Item checked for fitness</param>
	/// <returns>Returns true if item fits into the slot</returns>
	public bool CheckFit(object? user, int slot, Item item)
	{
		ValidateSlotIndex(slot);

		if (item.IsAir)
			return true;

		if (_canInteract?.Invoke(user, slot, Action.Insert) == false)
			return false;

		if (!EvaluateFilters(slot, item))
			return false;

		return item.stack <= EvaluateMaxStack(slot, item);
	}

	/// <summary>
	/// Removes an item from the ItemStorage.
	/// </summary>
	/// <param name="user">User that performed this action</param>
	/// <param name="slot">Slot from which the item is to be removed</param>
	/// <param name="item">Item which was removed</param>
	/// <param name="amount">Amount to remove, negative values result in up to int.MaxValue items being removed</param>
	/// <returns>Success or reason why the action could not be performed.</returns>
	public Result RemoveItem(object? user, int slot, out Item item, int amount = -1)
	{
		ValidateSlotIndex(slot);

		item = new Item();

		if (amount == 0)
			return Result.NotValid;

		if (_canInteract?.Invoke(user, slot, Action.Remove) == false)
			return Result.CantInteract;

		if (Items[slot].IsAir)
			return Result.SourceEmpty;

		int toExtract = StorageUtility.Min(amount < 0 ? int.MaxValue : amount, Items[slot].maxStack, Items[slot].stack);

		// TODO: should partial removal be a success?
		if (Items[slot].stack == toExtract)
		{
			item = Items[slot];
			Items[slot] = new Item();

			return Result.Success;
		}

		item = Items[slot].Clone();
		ItemLoader.SplitStack(item, Items[slot], toExtract);

		return Result.PartialSuccess;

		// OnContentsChanged(user, Operation.Remove, slot);
	}

	public Result SimulateRemoveItem(object? user, int slot, out Item item, int amount = -1)
	{
		ValidateSlotIndex(slot);

		item = new Item();

		if (amount == 0)
			return Result.NotValid;

		if (_canInteract?.Invoke(user, slot, Action.Remove) == false)
			return Result.CantInteract;

		if (Items[slot].IsAir)
			return Result.SourceEmpty;

		int toExtract = StorageUtility.Min(amount < 0 ? int.MaxValue : amount, Items[slot].maxStack, Items[slot].stack);

		// TODO: should partial removal be a success?
		if (Items[slot].stack == toExtract)
		{
			item = Items[slot].Clone();

			return Result.Success;
		}

		item = StorageUtility.CloneItemWithSize(Items[slot], toExtract);

		return Result.PartialSuccess;
	}

	private void ValidateSlotIndex(int slot)
	{
		if (slot < 0 || slot >= Items.Length)
			throw new ArgumentException($"Slot is outside of the valid range 0 to {Items.Length - 1}");
	}

	private int EvaluateMaxStack(int slot, Item item)
	{
		return _stackOverride?.Invoke(slot) ?? item.maxStack;
	}

	public TagCompound Save() => new TagCompound {
		["Value"] = Items.ToList()
	};

	public void Load(TagCompound tag)
	{
		Items = tag.GetList<Item>("Value").ToArray();
	}

	public override string ToString() =>
		$"ItemStorage with {Items.Length} slots and {Filters.Count} filters";
}