using System;
using Terraria;
using Terraria.ModLoader;

namespace ContainerLibrary;

public partial class ItemStorage
{
	private readonly Item[] Items;
	private StackOverride? _stackOverride;
	private CanInteract? _canInteract; // NOTE: should this be a list?

	public ItemStorage(int slots)
	{
		if (slots < 1) throw new ArgumentException("Can't create ItemStorage with less than 1 slot");

		Items = new Item[slots];
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

	public Result InsertItem(object? user, int slot, ref Item insert)
	{
		ValidateSlotIndex(slot);

		if (insert.IsAir) return Result.SourceIsAir;

		if (_canInteract?.Invoke(user, slot, Action.Insert) == false)
			return Result.CantInteract;

		Item item = Items[slot];

		if (!EvaluateFilters(slot, insert) || (!item.IsAir && (insert.type != item.type || !ItemLoader.CanStack(item, insert))))
			return Result.NotValid;

		int remainingSpace = Math.Min(EvaluateMaxStack(slot, insert) - item.stack, insert.stack);
		if (remainingSpace <= 0)
			return Result.DestinationFull;

		bool cantFit = insert.stack > remainingSpace;

		if (item.IsAir) Items[slot] = cantFit ? Utility.CloneItemWithSize(insert, remainingSpace) : insert;
		else item.stack += remainingSpace;

		insert = cantFit ? Utility.CloneItemWithSize(insert, insert.stack - remainingSpace) : new Item();

		// OnContentsChanged(user, Operation.Insert, slot);

		return Result.Success;
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

	public override string ToString() =>
		$"ItemStorage with {Items.Length} slots and {Filters.Count} filters";
}