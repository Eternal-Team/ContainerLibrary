using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ContainerLibrary;

public class ContainerLibraryPlayer : ModPlayer
{
	private static List<int> ValidShiftClickSlots = new() { ItemSlot.Context.InventoryItem, ItemSlot.Context.InventoryCoin, ItemSlot.Context.InventoryAmmo };

	private static IEnumerable<ICraftingStorage> GetCraftingStorages(Player player)
	{
		foreach (Item item in player.inventory)
		{
			if (!item.IsAir && item.ModItem is ICraftingStorage storage) yield return storage;
		}
	}

	public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback)
	{
		itemConsumedCallback = (item, index) =>
		{
			// bug: problems with syncing
		};

		List<Item> items = new();
		foreach (ICraftingStorage craftingStorage in GetCraftingStorages(Main.LocalPlayer))
		{
			ItemStorage storage = craftingStorage.GetCraftingStorage();

			for (int slot = 0; slot < storage.Count; slot++)
			{
				var item = storage[slot];
				if (item.stack > 0 && storage.CanInteract(slot, ItemStorage.Operation.Remove, Main.LocalPlayer))
				{
					items.Add(item);
				}
			}
		}

		return items;
	}

	public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
	{
		ref Item item = ref inventory[slot];

		if (item.favorited || item.IsAir || !ValidShiftClickSlots.Contains(context)) return false;

		bool block = false;

		foreach (ItemStorage storage in ContainerLibrary.OpenedStorageUIs.SelectMany(ui => ui.GetItemStorages()))
		{
			if (storage.InsertItem(Player, ref item)) block = true;
		}

		if (block)
		{
			SoundEngine.PlaySound(SoundID.Grab);
			return true;
		}

		return false;
	}

	public override bool HoverSlot(Item[] inventory, int context, int slot)
	{
		if (ValidShiftClickSlots.Contains(context) && ItemSlot.ShiftInUse)
		{
			foreach (IItemStorageUI ui in ContainerLibrary.OpenedStorageUIs)
			{
				foreach (ItemStorage storage in ui.GetItemStorages())
				{
					if (storage.CanInsertItem(Main.LocalPlayer, inventory[slot]))
					{
						string texture = ui.GetCursorTexture(inventory[slot], storage);

						if (!string.IsNullOrWhiteSpace(texture))
						{
							CustomCursor.CustomCursor.SetCursor(texture);
							return true;
						}
					}
				}
			}
		}

		return base.HoverSlot(inventory, context, slot);
	}
}