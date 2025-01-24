using System.Numerics;
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
	
	internal static T Min<T>(T a, T b, T c) where T : IComparisonOperators<T, T, bool>
	{
		T m = a;
		if (m > b) m = b;
		if (m > c) m = c;
		return m;
	}
}