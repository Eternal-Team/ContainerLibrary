using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader.IO;

namespace ContainerLibrary
{
	public interface IItemHandler
	{
		ItemHandler Handler { get; }
	}

	public class ItemHandler
	{
		public List<Item> stacks;
		public int Slots => stacks.Count;

		public Action<int> OnContentsChanged = slot => { };
		public Func<int, int> GetSlotLimit = slot => -1;
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

		public ItemHandler Clone() => new ItemHandler(stacks.Select(x => x.Clone()).ToList())
		{
			IsItemValid = (Func<ItemHandler, int, Item, bool>)IsItemValid.Clone(),
			GetSlotLimit = (Func<int, int>)GetSlotLimit.Clone(),
			OnContentsChanged = (Action<int>)OnContentsChanged.Clone()
		};

		public void SetSize(int size)
		{
			stacks = new List<Item>(size);
			for (int i = 0; i < size; i++) stacks.Add(new Item());
		}

		public void SetItemInSlot(int slot, Item stack)
		{
			ValidateSlotIndex(slot);
			stacks[slot] = stack;
			OnContentsChanged?.Invoke(slot);
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
				if (existing.IsAir) stacks[slot] = reachedLimit ? CopyItemWithSize(stack, limit) : stack.Clone();
				else existing.Grow(reachedLimit ? limit : stack.stack);

				OnContentsChanged?.Invoke(slot);
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
					OnContentsChanged?.Invoke(slot);
				}

				return existing;
			}

			if (!simulate)
			{
				stacks[slot] = CopyItemWithSize(existing, existing.stack - toExtract);
				OnContentsChanged?.Invoke(slot);
			}

			return CopyItemWithSize(existing, toExtract);
		}

		protected int GetItemLimit(int slot, Item stack) => GetSlotLimit(slot) == -1 ? stack.maxStack : GetSlotLimit(slot);

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

		public void Serialize(BinaryWriter writer)
		{
			writer.Write(stacks.Count);
			for (int i = 0; i < stacks.Count; i++) writer.WriteItem(stacks[i], true, true);
		}

		public void Deserialize(BinaryReader reader)
		{
			int size = reader.ReadInt32();
			SetSize(size);

			for (int i = 0; i < stacks.Count; i++) stacks[i] = reader.ReadItem(true, true);
		}

		protected void ValidateSlotIndex(int slot)
		{
			if (slot < 0 || slot >= stacks.Count) throw new Exception($"Slot {slot} not in valid range - [0,{stacks.Count - 1})");
		}
	}
}