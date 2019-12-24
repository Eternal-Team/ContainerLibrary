using FluidLibrary.Content;

namespace ContainerLibrary
{
	public static class FluidUtility
	{
		public static void Grow(this FluidHandler handler, int slot, int quantity)
		{
			if (quantity == 0) return;

			ref ModFluid fluid = ref handler.GetFluidInSlotByRef(slot);

			fluid.volume += quantity;
			if (fluid.volume <= 0) fluid = null;
		}

		public static void Shrink(this FluidHandler handler, int slot, int quantity)
		{
			if (quantity == 0) return;

			ref ModFluid fluid = ref handler.GetFluidInSlotByRef(slot);

			fluid.volume -= quantity;
			if (fluid.volume <= 0) fluid = null;
		}

		public static void SetCount(this FluidHandler handler, int slot, int quantity)
		{
			ref ModFluid fluid = ref handler.GetFluidInSlotByRef(slot);
			fluid.volume = quantity;
		}
	}
}