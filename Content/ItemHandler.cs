using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader.IO;

namespace ContainerLibrary.Content
{
	public class ItemHandler
	{
		public List<Item> stacks;
		public int Slots => stacks.Count;

		public Action<ItemHandler, int> OnContentsChanged;
		public Func<ItemHandler, int, int> GetSlotLimit = (handler, slot) => -1;
		public Func<ItemHandler, int, Item, bool> IsItemValid = (handler, slot, item) => true;

		public ItemHandler() : this(1)
		{
		}

		public ItemHandler(int size)
		{
			stacks = new List<Item>(size);
			for (int i = 0; i < size; i++) stacks.Add(new Item());
		}

		public ItemHandler(List<Item> stacks)
		{
			this.stacks = stacks;
		}

		public void SetSize(int size)
		{
			stacks = new List<Item>(size);
			for (int i = 0; i < size; i++) stacks.Add(new Item());
		}

		public void SetItemInSlot(int slot, Item stack)
		{
			ValidateSlotIndex(slot);
			stacks[slot] = stack;
			OnContentsChanged?.Invoke(this, slot);
		}

		public Item GetItemInSlot(int slot)
		{
			ValidateSlotIndex(slot);
			return stacks[slot];
		}

		public static bool CanItemsStack(Item a, Item b)
		{
			if (a.IsAir || a.type != b.type || a.HasTagCompound() != b.HasTagCompound()) return false;

			return !a.HasTagCompound() || a.GetTagCompound().Equals(b.GetTagCompound());
		}

		public static Item CopyItemWithSize(Item itemStack, int size)
		{
			if (size == 0) return new Item();
			Item copy = itemStack.Clone();
			copy.SetCount(size);
			return copy;
		}

		public Item InsertItem(int slot, Item stack, bool simulate = false)
		{
			if (stack.IsAir) return stack;

			ValidateSlotIndex(slot);

			if (!IsItemValid(this, slot, stack)) return stack;

			Item existing = stacks[slot];

			int limit = GetItemLimit(slot, stack);

			if (!existing.IsAir)
			{
				if (!CanItemsStack(stack, existing))
					return stack;

				limit -= existing.stack;
			}

			if (limit <= 0)
				return stack;

			bool reachedLimit = stack.stack > limit;

			if (!simulate)
			{
				if (existing.IsAir) stacks[slot] = reachedLimit ? CopyItemWithSize(stack, limit) : stack;
				else existing.Grow(reachedLimit ? limit : stack.stack);

				OnContentsChanged?.Invoke(this, slot);
			}

			return reachedLimit ? CopyItemWithSize(stack, stack.stack - limit) : new Item();
		}

		public Item ExtractItem(int slot, int amount, bool simulate = false)
		{
			if (amount == 0) return new Item();

			ValidateSlotIndex(slot);

			Item existing = stacks[slot];

			if (existing.IsAir) return new Item();

			int toExtract = Math.Min(amount, existing.maxStack);

			if (existing.stack <= toExtract)
			{
				if (!simulate)
				{
					stacks[slot] = new Item();
					OnContentsChanged?.Invoke(this, slot);
				}

				return existing;
			}

			if (!simulate)
			{
				stacks[slot] = CopyItemWithSize(existing, existing.stack - toExtract);
				OnContentsChanged?.Invoke(this, slot);
			}

			return CopyItemWithSize(existing, toExtract);
		}

		protected int GetItemLimit(int slot, Item stack) => GetSlotLimit(this, slot) == -1 ? stack.maxStack : Math.Min(GetSlotLimit(this, slot), stack.maxStack);

		public TagCompound Save()
		{
			List<TagCompound> items = stacks.Select((item, slot) => new TagCompound
			{
				["Slot"] = slot,
				["Item"] = ItemIO.Save(item)
			}).ToList();
			return new TagCompound
			{
				["Items"] = items,
				["Count"] = stacks.Count
			};
		}

		public ItemHandler Load(TagCompound tag)
		{
			SetSize(tag.ContainsKey("Count") ? tag.GetInt("Count") : stacks.Count);
			foreach (TagCompound compound in tag.GetList<TagCompound>("Items"))
			{
				Item item = ItemIO.Load(compound.GetCompound("Item"));
				int slot = compound.GetInt("Slot");

				if (slot >= 0 && slot < stacks.Count) stacks[slot] = item;
			}

			return this;
		}

		protected void ValidateSlotIndex(int slot)
		{
			if (slot < 0 || slot >= stacks.Count) throw new Exception($"Slot {slot} not in valid range - [0,{stacks.Count - 1})");
		}
	}

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
	}
}