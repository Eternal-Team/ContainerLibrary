using FluidLibrary.Content;

namespace ContainerLibrary
{
	public static class FluidUtility
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