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
	ItemStorage GetItemStorage();

	// bool IsVisible();
	string GetCursorTexture(Item item);
}