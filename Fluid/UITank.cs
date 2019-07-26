using BaseLibrary;
using BaseLibrary.UI.Elements;
using FluidLibrary.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ContainerLibrary
{
	public class UITank : BaseElement
	{
		private IFluidHandler fluidHandler;
		public FluidHandler Handler => fluidHandler.Handler;

		public int slot;
		private int MaxVolume => Handler.GetSlotLimit(slot);

		public ModFluid Fluid
		{
			get => Handler.GetFluidInSlot(slot);
			set => Handler.SetFluidInSlot(slot, value);
		}

		public UITank(IFluidHandler handler, int slot = 0)
		{
			Width = Height = (40, 0);

			this.slot = slot;
			fluidHandler = handler;
		}

		public override int CompareTo(object obj) => slot.CompareTo(((UITank)obj).slot);

		public override void Click(UIMouseEvent evt)
		{
			Item item = Main.LocalPlayer.GetHeldItem();
			if (item.type == ItemID.EmptyBucket)
			{
				if (Fluid == null || Fluid.volume < 255) return;

				switch (Fluid.Name)
				{
					case "Water":
						Main.LocalPlayer.PutItemInInventory(ItemID.WaterBucket);
						break;
					case "Lava":
						Main.LocalPlayer.PutItemInInventory(ItemID.LavaBucket);
						break;
					case "Honey":
						Main.LocalPlayer.PutItemInInventory(ItemID.HoneyBucket);
						break;
				}

				item.stack--;
				if (item.stack <= 0) item.TurnToAir();

				Handler.Shrink(slot, 255);
			}
			else if (item.type == ItemID.WaterBucket)
			{
				if (Fluid != null && (!Fluid.Equals(FluidLoader.GetFluid<Water>()) || Fluid.volume > MaxVolume - 255)) return;

				if (Fluid == null) Fluid = FluidLoader.GetFluid<Water>().Clone();

				item.stack--;
				if (item.stack <= 0) item.TurnToAir();
				Main.LocalPlayer.PutItemInInventory(ItemID.EmptyBucket);

				Handler.Grow(slot, 255);
			}
			else if (item.type == ItemID.LavaBucket)
			{
				if (Fluid != null && (!Fluid.Equals(FluidLoader.GetFluid<Lava>()) || Fluid.volume > MaxVolume - 255)) return;

				if (Fluid == null) Fluid = new Lava();

				item.stack--;
				if (item.stack <= 0) item.TurnToAir();
				Main.LocalPlayer.PutItemInInventory(ItemID.EmptyBucket);

				Handler.Grow(slot, 255);
			}
			else if (item.type == ItemID.HoneyBucket)
			{
				if (Fluid != null && (!Fluid.Equals(FluidLoader.GetFluid<Honey>()) || Fluid.volume > MaxVolume - 255)) return;

				if (Fluid == null) Fluid = new Honey();

				item.stack--;
				if (item.stack <= 0) item.TurnToAir();
				Main.LocalPlayer.PutItemInInventory(ItemID.EmptyBucket);

				Handler.Grow(slot, 255);
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Main.magicPixel, Dimensions, Utility.ColorPanel);
			spriteBatch.DrawOutline(Dimensions.Position(), Dimensions.Position() + Dimensions.Size(), Utility.ColorOutline);

			if (Fluid != null)
			{
				float progress = Fluid.volume / (float)Handler.GetSlotLimit(0);
				spriteBatch.Draw(ModContent.GetTexture(Fluid.Texture), new Rectangle((int)Dimensions.X + 2, (int)(Dimensions.Y + 2 + (Dimensions.Height - 4) * (1f - progress)), (int)Dimensions.Width - 4, (int)((Dimensions.Height - 4) * progress)));
			}

			if (IsMouseHovering)
			{
				Main.LocalPlayer.showItemIcon = false;
				Main.ItemIconCacheUpdate(0);

				Utility.DrawMouseText(Fluid != null ? $"{Fluid.DisplayName.GetTranslation(Language.ActiveCulture)}\n{Fluid.VolumeBuckets:N2}/{MaxVolume / 255f:N2} B" : $"Empty\n{0:N2}/{MaxVolume / 255f:N2} B");
			}
		}
	}
}