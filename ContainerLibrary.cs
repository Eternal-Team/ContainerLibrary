using System.Collections.Generic;
using Terraria.ModLoader;

namespace ContainerLibrary;

public class ContainerLibrary : Mod
{
	public static List<IItemStorageUI> OpenedStorageUIs = new();

	public override void Load()
	{
		ItemStorageSerializer.Load();
	}
}