using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ContainerLibrary
{
	public class CLPlayer : ModPlayer
	{
		private static List<int> ValidShiftClickSlots = new List<int> {ItemSlot.Context.InventoryItem, ItemSlot.Context.InventoryCoin, ItemSlot.Context.InventoryAmmo};

		public override bool Autoload(ref string name) => ModLoader.GetMod("BaseLibrary") != null;

		public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
		{
			ref Item item = ref inventory[slot];
			Item item1 = item;

			if (item.favorited || item.IsAir) return false;

			if (!ValidShiftClickSlots.Contains(context)) return false;

			if (!BaseLibrary.BaseLibrary.PanelGUI.UI.Elements.Any(panel => panel is IItemHandlerUI ui && ui.Handler.HasSpace(item1))) return false;

			foreach (UIElement panel in BaseLibrary.BaseLibrary.PanelGUI.UI.Elements)
			{
				ItemHandler container = (panel as IItemHandlerUI)?.Handler;

				container?.InsertItem(ref item);
			}

			Main.PlaySound(SoundID.Grab);

			return false;
		}
	}
}