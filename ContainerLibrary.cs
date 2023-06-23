using System.Collections.Generic;
using Terraria.ModLoader;

namespace ContainerLibrary;

public class ContainerLibrary : Mod
{
	public static readonly List<IItemStorageUI> OpenedStorageUIs = new();
}