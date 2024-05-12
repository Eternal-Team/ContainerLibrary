using Terraria;

namespace ContainerLibrary;

internal static class Utility
{
	internal static Item CloneItemWithSize(Item item, int size)
	{
		if (size <= 0) return new Item();

		Item copy = item.Clone();
		copy.stack = size;
		return copy;
	}
}