using BaseLibrary.UI;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ContainerLibrary
{
	public class CLPlayer : ModPlayer
	{
		private static List<int> ValidShiftClickSlots = new List<int> { ItemSlot.Context.InventoryItem, ItemSlot.Context.InventoryCoin, ItemSlot.Context.InventoryAmmo };

		public override bool Autoload(ref string name) => ModLoader.GetMod("BaseLibrary") != null;

		public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
		{
			ref Item item = ref inventory[slot];

			if (item.favorited || item.IsAir || !ValidShiftClickSlots.Contains(context)) return false;

			bool block = false;
			foreach (BaseElement element in PanelUI.Instance.Children)
			{
				if (element is IItemHandlerUI ui && ui.Handler.HasSpace(item))
				{
					block = true;

					ItemHandler container = ui.Handler;
					container.InsertItem(ref item);
				}
			}

			if (block)
			{
				Main.PlaySound(SoundID.Grab);
				return true;
			}

			return false;
		}
	}
}