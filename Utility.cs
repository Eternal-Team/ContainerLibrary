using Terraria;
using Terraria.ID;

namespace ContainerLibrary;

public static class Utility
{
	public static int Min(int a, int b, int c)
	{
		return a < b && a < c ? a : b < c ? b : c;
	}

	public static long GetCoinValue(Item item)
	{
		return item.type switch
		{
			ItemID.CopperCoin => item.stack,
			ItemID.SilverCoin => item.stack * 100,
			ItemID.GoldCoin => item.stack * 10000,
			ItemID.PlatinumCoin => item.stack * 1000000,
			_ => 0
		};
	}
}