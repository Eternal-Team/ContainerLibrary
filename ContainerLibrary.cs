using Terraria.ModLoader;

namespace ContainerLibrary;

public class ContainerLibrary : Mod
{
	public override void Load()
	{
		ItemStorage storage = new ItemStorage(9)
			.AddFilter(ItemStorage.Filter.NoCoin)
			.SetStackOverride(_ => int.MaxValue)
			.SetPermission((user, slot, action) => action != ItemStorage.Action.Remove);
	}
}