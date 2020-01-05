using BaseLibrary;
using BaseLibrary.Input;
using BaseLibrary.Input.Mouse;
using BaseLibrary.UI.New;
using FluidLibrary.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;

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
			Width.Pixels = Height.Pixels = 40;

			this.slot = slot;
			fluidHandler = handler;
		}

		public override int CompareTo(BaseElement other) => slot.CompareTo(((UITank)other).slot);

		protected override void MouseClick(MouseButtonEventArgs args)
		{
			if (args.Button != MouseButton.Left) return;

			args.Handled = true;

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

		protected override void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Main.magicPixel, Dimensions, Utility.ColorPanel);
			spriteBatch.DrawOutline(Dimensions.Position(), Dimensions.Position() + Dimensions.Size(), Utility.ColorOutline);

			if (Fluid != null)
			{
				float progress = Fluid.volume / (float)Handler.GetSlotLimit(0);
				spriteBatch.Draw(FluidLoader.GetTexture(Fluid), new Rectangle(Dimensions.X + 2, (int)(Dimensions.Y + 2 + (Dimensions.Height - 4) * (1f - progress)), Dimensions.Width - 4, (int)((Dimensions.Height - 4) * progress)));
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