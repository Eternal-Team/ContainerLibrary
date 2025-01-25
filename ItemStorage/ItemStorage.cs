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
	/// <param name="insert">Item to insert</param>
	/// <returns>Success or reason why the action could not be performed.</returns>
	public Result InsertItem(object? user, int slot, ref Item insert)
	{
		ValidateSlotIndex(slot);

		if (insert.IsAir)
			return Result.ItemIsAir;

		if (_canInteract?.Invoke(user, slot, Action.Insert) == false)
			return Result.CantInteract;

		Item item = Items[slot];

		if (!EvaluateFilters(slot, insert) || (!item.IsAir && (insert.type != item.type || !ItemLoader.CanStack(item, insert))))
			return Result.NotValid;

		int canInsert = Math.Min(EvaluateMaxStack(slot, insert) - item.stack, insert.stack);
		if (canInsert <= 0)
			return Result.DestinationFull;

		bool cantFit = insert.stack > canInsert;

		// I have to differentiate between inserting into an
		// - empty slot
		// - partially full slot
		// Also whether the source item will fit into either
		// Completely full slots require no additional logic
		
		// ItemLoader.OnStack();
		
		if (item.IsAir) Items[slot] = cantFit ? Utility.CloneItemWithSize(insert, canInsert) : insert;
		else item.stack += canInsert;

		insert = cantFit ? Utility.CloneItemWithSize(insert, insert.stack - canInsert) : new Item();

		// OnContentsChanged(user, Operation.Insert, slot);

		// NOTE: this should be inspected
		// inv[slot] = ItemLoader.TransferWithLimit(Main.mouseItem, 1);
		// ItemLoader.TryStackItems

		return Result.Success;
	}

	/// <summary>
	/// Removes an item from the ItemStorage.
	/// </summary>
	/// <param name="user">User that performed this action</param>
	/// <param name="slot">Slot from which the item is to be removed</param>
	/// <param name="item">Item which was removed</param>
	/// <param name="amount">Amount to remove, negative values result in up to int.MaxValue items being removed</param>
	/// <returns>Success or reason why the action could not be performed.</returns>
	public Result RemoveItem(object? user, int slot, out Item? item, int amount = -1)
	{
		ValidateSlotIndex(slot);

		item = new Item();

		if (amount == 0)
			return Result.NotValid;

		if (_canInteract?.Invoke(user, slot, Action.Remove) == false)
			return Result.CantInteract;

		if (Items[slot].IsAir)
			return Result.SourceEmpty;

		// item = Items[slot];

		int toExtract = Utility.Min(amount < 0 ? int.MaxValue : amount, Items[slot].maxStack, Items[slot].stack);
		Result result = Items[slot].stack < toExtract ? Result.PartialSuccess : Result.Success; // TODO: should partial removal be a success?

		item = Utility.CloneItemWithSize(Items[slot], toExtract);
		Items[slot] = Utility.CloneItemWithSize(Items[slot], Items[slot].stack - toExtract);

		// OnContentsChanged(user, Operation.Remove, slot);

		return result;
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