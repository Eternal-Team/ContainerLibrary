using System.Collections.Generic;
using Terraria;

namespace ContainerLibrary;

public interface IItemStorage
{
	ItemStorage GetItemStorage();
}

public interface ICraftingStorage : IItemStorage
{
	IEnumerable<int> GetSlotsForCrafting();
}

public interface IItemStorageUI
{
	ItemStorage GetItemStorage();

	bool IsVisible();
	string GetTexture(Item item);
}