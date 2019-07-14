using FluidLibrary.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace ContainerLibrary
{
	public interface IFluidHandler
	{
		FluidHandler Handler { get; }
	}

	public class FluidHandler
	{
		public List<ModFluid> tanks;

		public Action<int> OnContentsChanged = slot => { };
		public Func<int, int> GetSlotLimit = slot => -1;
		public Func<FluidHandler, int, ModFluid, bool> IsFluidValid = (handler, slot, item) => true;

		public FluidHandler() : this(1)
		{
		}

		public FluidHandler(int size)
		{
			tanks = new List<ModFluid>(size);
			for (int i = 0; i < size; i++) tanks.Add(null);
		}

		public FluidHandler(List<ModFluid> tanks)
		{
			this.tanks = tanks;
		}

		public FluidHandler Clone() => new FluidHandler(tanks.Select(x => x?.Clone()).ToList())
		{
			IsFluidValid = (Func<FluidHandler, int, ModFluid, bool>)IsFluidValid.Clone(),
			GetSlotLimit = (Func<int, int>)GetSlotLimit.Clone(),
			OnContentsChanged = (Action<int>)OnContentsChanged.Clone()
		};

		public void SetSize(int size)
		{
			tanks = new List<ModFluid>(size);
			for (int i = 0; i < size; i++) tanks.Add(null);
		}

		public void SetFluidInSlot(int slot, ModFluid stack)
		{
			ValidateSlotIndex(slot);
			tanks[slot] = stack;
			OnContentsChanged(slot);
		}

		public int GetSlots() => tanks.Count;

		public ModFluid GetFluidInSlot(int slot)
		{
			ValidateSlotIndex(slot);
			return tanks[slot];
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
			FluidUtility.SetCount(ref copy, size);
			return copy;
		}

		public ModFluid InsertFluid(int slot, ModFluid stack, bool simulate = false)
		{
			if (stack == null) return null;

			ValidateSlotIndex(slot);

			if (!IsFluidValid(this, slot, stack)) return stack;

			ModFluid existing = tanks[slot];

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
				if (existing == null) tanks[slot] = reachedLimit ? CopyFluidWithSize(stack, limit) : stack;
				else existing.Grow(reachedLimit ? limit : stack.volume);

				OnContentsChanged(slot);
			}

			return reachedLimit ? CopyFluidWithSize(stack, stack.volume - limit) : null;
		}

		public ModFluid ExtractFluid(int slot, int amount, bool simulate = false)
		{
			if (amount == 0) return null;

			ValidateSlotIndex(slot);

			ModFluid existing = tanks[slot];

			if (existing == null) return null;

			int toExtract = Math.Min(amount, GetStackLimit(slot, existing));

			if (existing.volume <= toExtract)
			{
				if (!simulate)
				{
					tanks[slot] = null;
					OnContentsChanged(slot);
				}

				return existing;
			}

			if (!simulate)
			{
				tanks[slot] = CopyFluidWithSize(existing, existing.volume - toExtract);
				OnContentsChanged(slot);
			}

			return CopyFluidWithSize(existing, toExtract);
		}

		protected int GetStackLimit(int slot, ModFluid stack)
		{
			return GetSlotLimit(slot) /*== -1 ? stack.maxVolume : Math.Min(GetSlotLimit(slot), stack.maxVolume)*/;
		}

		public TagCompound Save() => new TagCompound
		{
			["Fluids"] = tanks.Select((fluid, slot) => new TagCompound
			{
				["Slot"] = slot,
				["Fluid"] = fluid
			}).ToList(),
			["Count"] = tanks.Count
		};

		public FluidHandler Load(TagCompound tag)
		{
			SetSize(tag.ContainsKey("Count") ? tag.GetInt("Count") : tanks.Count);
			foreach (TagCompound compound in tag.GetList<TagCompound>("Fluids"))
			{
				ModFluid fluid = compound.Get<ModFluid>("Fluid");
				int slot = compound.GetInt("Slot");

				if (slot >= 0 && slot < tanks.Count) tanks[slot] = fluid;
			}

			return this;
		}

		protected void ValidateSlotIndex(int slot)
		{
			if (slot < 0 || slot >= tanks.Count) throw new Exception($"Slot {slot} not in valid range - [0,{tanks.Count - 1})");
		}
	}
}