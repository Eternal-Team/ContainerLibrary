using System;
using System.Collections.Generic;
using System.Linq;
using FluidLibrary.Content;
using Terraria.ModLoader.IO;

namespace ContainerLibrary.Content
{
	public class FluidHandler
	{
		public List<ModFluid> stacks;

		public FluidHandler() : this(1)
		{
		}

		public FluidHandler(int size)
		{
			stacks = new List<ModFluid>(size);
			for (int i = 0; i < size; i++) stacks.Add(null);
		}

		public FluidHandler(List<ModFluid> stacks)
		{
			this.stacks = stacks;
		}

		public void SetSize(int size)
		{
			stacks = new List<ModFluid>(size);
			for (int i = 0; i < size; i++) stacks.Add(null);
		}

		public void SetFluidInSlot(int slot, ModFluid stack)
		{
			ValidateSlotIndex(slot);
			stacks[slot] = stack;
			OnContentsChanged(slot);
		}

		public int GetSlots() => stacks.Count;

		public ModFluid GetFluidInSlot(int slot)
		{
			ValidateSlotIndex(slot);
			return stacks[slot];
		}

		public static bool CanFluidsStack(ModFluid a, ModFluid b)
		{
			return a.Name == b.Name;

			//if (a.IsAir || a.type != b.type || a.HasTagCompound() != b.HasTagCompound()) return false;

			//return !a.HasTagCompound() || a.GetTagCompound().Equals(b.GetTagCompound());
		}

		public static ModFluid CopyFluidWithSize(ModFluid itemStack, int size)
		{
			if (size == 0) return null;
			ModFluid copy = itemStack.Clone();
			Utility.SetCount(ref copy, size);
			return copy;
		}

		public ModFluid InsertFluid(int slot, ModFluid stack, bool simulate = false)
		{
			if (stack == null) return null;

			ValidateSlotIndex(slot);

			ModFluid existing = stacks[slot];

			int limit = GetStackLimit(slot, stack);

			if (existing != null)
			{
				if (!CanFluidsStack(stack, existing))
					return stack;

				limit -= existing.volume;
			}

			if (limit <= 0)
				return stack;

			bool reachedLimit = stack.volume > limit;

			if (!simulate)
			{
				if (existing == null) stacks[slot] = reachedLimit ? CopyFluidWithSize(stack, limit) : stack;
				else existing.Grow(reachedLimit ? limit : stack.volume);

				OnContentsChanged(slot);
			}

			return reachedLimit ? CopyFluidWithSize(stack, stack.volume - limit) : null;
		}

		public ModFluid ExtractFluid(int slot, int amount, bool simulate = false)
		{
			if (amount == 0) return null;

			ValidateSlotIndex(slot);

			ModFluid existing = stacks[slot];

			if (existing == null) return null;

			int toExtract = Math.Min(amount, existing.maxVolume);

			if (existing.volume <= toExtract)
			{
				if (!simulate)
				{
					stacks[slot] = null;
					OnContentsChanged(slot);
				}

				return existing;
			}

			if (!simulate)
			{
				stacks[slot] = CopyFluidWithSize(existing, existing.volume - toExtract);
				OnContentsChanged(slot);
			}

			return CopyFluidWithSize(existing, toExtract);
		}

		public int GetSlotLimit(int slot) => -1;

		protected int GetStackLimit(int slot, ModFluid stack)
		{
			return GetSlotLimit(slot) == -1 ? stack.maxVolume : Math.Min(GetSlotLimit(slot), stack.maxVolume);
		}

		public bool IsItemValid(int slot, ModFluid stack)
		{
			return true;
		}

		public TagCompound Save() => new TagCompound
		{
			["Fluids"] = stacks.Select((fluid, slot) => new TagCompound
			{
				["Slot"] = slot,
				["Fluid"] = fluid
			}).ToList(),
			["Count"] = stacks.Count
		};

		public FluidHandler Load(TagCompound tag)
		{
			SetSize(tag.ContainsKey("Count") ? tag.GetInt("Count") : stacks.Count);
			foreach (TagCompound compound in tag.GetList<TagCompound>("Fluids"))
			{
				ModFluid fluid = tag.Get<ModFluid>("Fluid");
				int slot = compound.GetInt("Slot");

				if (slot >= 0 && slot < stacks.Count) stacks[slot] = fluid;
			}

			return this;
		}

		protected void ValidateSlotIndex(int slot)
		{
			if (slot < 0 || slot >= stacks.Count) throw new Exception($"Slot {slot} not in valid range - [0,{stacks.Count - 1})");
		}

		protected void OnContentsChanged(int slot)
		{
		}
	}

	public static partial class Utility
	{
		public static void Grow(this ModFluid item, int quantity) => SetCount(ref item, item.volume + quantity);

		public static void Shrink(this ModFluid item, int quantity) => item.Grow(-quantity);

		public static void SetCount(ref ModFluid item, int size)
		{
			item.volume = size;
			if (item.volume <= 0) item = null;
		}
	}
}