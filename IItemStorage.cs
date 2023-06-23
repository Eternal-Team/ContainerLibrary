using System.Collections.Generic;
using Terraria;

namespace ContainerLibrary;

public interface IItemStorage
{
	ItemStorage GetItemStorage();
}

public interface ICraftingStorage : IItemStorage
{
	ItemStorage GetCraftingStorage();
}

public interface IItemStorageUI
{
	IEnumerable<ItemStorage> GetItemStorages();

	string GetCursorTexture(Item item, ItemStorage storage);
}