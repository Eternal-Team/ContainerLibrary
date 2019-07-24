using FluidLibrary.Content;
using System;
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
		public ModFluid[] Fluids;
		public int Slots => Fluids.Length;

		public Action<int> OnContentsChanged = slot => { };
		public Func<int, int> GetSlotLimit = slot => -1;
		public Func<FluidHandler, int, ModFluid, bool> IsFluidValid = (handler, slot, item) => true;

		public FluidHandler() : this(1)
		{
		}

		public FluidHandler(int size)
		{
			Fluids = new ModFluid[size];
			for (int i = 0; i < size; i++) Fluids[i] = null;
		}

		public FluidHandler(ModFluid[] fluids)
		{
			Fluids = fluids;
		}

		public FluidHandler Clone() => new FluidHandler(Fluids.Select(x => x?.Clone()).ToArray())
		{
			IsFluidValid = (Func<FluidHandler, int, ModFluid, bool>)IsFluidValid.Clone(),
			GetSlotLimit = (Func<int, int>)GetSlotLimit.Clone(),
			OnContentsChanged = (Action<int>)OnContentsChanged.Clone()
		};

		public void SetSize(int size)
		{
			Fluids = new ModFluid[size];
			for (int i = 0; i < size; i++) Fluids[i] = null;
		}

		public void SetFluidInSlot(int slot, ModFluid stack)
		{
			ValidateSlotIndex(slot);
			Fluids[slot] = stack;
			OnContentsChanged(slot);
		}

		public ModFluid GetFluidInSlot(int slot)
		{
			ValidateSlotIndex(slot);
			return Fluids[slot];
		}

		public ref ModFluid GetFluidInSlotByRef(int slot)
		{
			ValidateSlotIndex(slot);
			return ref Fluids[slot];
		}

		public static ModFluid CopyFluidWithSize(ModFluid fluid, int size)
		{
			if (size == 0) return null;
			ModFluid copy = fluid.Clone();
			copy.volume = fluid.volume;
			return copy;
		}

		public ModFluid InsertFluid(int slot, ModFluid fluid, bool simulate = false)
		{
			if (fluid == null) return null;

			ValidateSlotIndex(slot);

			if (!IsFluidValid(this, slot, fluid)) return fluid;

			ModFluid existing = Fluids[slot];

			int limit = GetVolumeLimit(slot);

			if (existing != null)
			{
				if (!fluid.Equals(existing)) return fluid;

				limit -= existing.volume;
			}

			if (limit <= 0) return fluid;

			bool reachedLimit = fluid.volume > limit;

			if (!simulate)
			{
				if (existing == null) Fluids[slot] = reachedLimit ? CopyFluidWithSize(fluid, limit) : fluid;
				else this.Grow(slot, reachedLimit ? limit : fluid.volume);

				OnContentsChanged(slot);
			}

			return reachedLimit ? CopyFluidWithSize(fluid, fluid.volume - limit) : null;
		}

		public ModFluid ExtractFluid(int slot, int volume, bool simulate = false)
		{
			if (volume == 0) return null;

			ValidateSlotIndex(slot);

			ModFluid existing = Fluids[slot];

			if (existing == null) return null;

			int toExtract = Math.Min(volume, GetVolumeLimit(slot));

			if (existing.volume <= toExtract)
			{
				if (!simulate)
				{
					Fluids[slot] = null;
					OnContentsChanged(slot);
				}

				return existing;
			}

			if (!simulate)
			{
				Fluids[slot] = CopyFluidWithSize(existing, existing.volume - toExtract);
				OnContentsChanged(slot);
			}

			return CopyFluidWithSize(existing, toExtract);
		}

		protected int GetVolumeLimit(int slot) => GetSlotLimit(slot);

		public TagCompound Save() => new TagCompound
		{
			["Fluids"] = Fluids.Select((fluid, slot) => new TagCompound
			{
				["Slot"] = slot,
				["Fluid"] = fluid
			}).ToList(),
			["Count"] = Slots
		};

		public FluidHandler Load(TagCompound tag)
		{
			SetSize(tag.ContainsKey("Count") ? tag.GetInt("Count") : Slots);
			foreach (TagCompound compound in tag.GetList<TagCompound>("Fluids"))
			{
				ModFluid fluid = compound.Get<ModFluid>("Fluid");
				int slot = compound.GetInt("Slot");

				if (slot >= 0 && slot < Slots) Fluids[slot] = fluid;
			}

			return this;
		}

		protected void ValidateSlotIndex(int slot)
		{
			if (slot < 0 || slot >= Slots) throw new Exception($"Slot {slot} not in valid range - [0,{Slots - 1})");
		}
	}
}