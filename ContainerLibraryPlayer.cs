using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ContainerLibrary;

public class ContainerLibraryPlayer : ModPlayer
{
	private static List<int> ValidShiftClickSlots = new() { Terraria.UI.ItemSlot.Context.InventoryItem, Terraria.UI.ItemSlot.Context.InventoryCoin, Terraria.UI.ItemSlot.Context.InventoryAmmo };

	public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
	{
		ref Item item = ref inventory[slot];

		if (item.favorited || item.IsAir || !ValidShiftClickSlots.Contains(context)) return false;

		bool block = false;

		foreach (IItemStorageUI ui in ContainerLibrary.OpenedStorageUIs)
		{
			ItemStorage storage = ui.GetItemStorage();
			if (storage.InsertItem(Player, ref item)) block = true;
		}

		if (block)
		{
			SoundEngine.PlaySound(SoundID.Grab);
			return true;
		}

		return false;
	}
}