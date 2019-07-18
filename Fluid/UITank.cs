using BaseLibrary;
using BaseLibrary.UI.Elements;
using FluidLibrary.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ContainerLibrary
{
	public class UITank : BaseElement
	{
		private IFluidHandler fluidHandler;
		public FluidHandler Handler => fluidHandler.Handler;

		public int slot;

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

				Utility.DrawMouseText(Fluid != null ? $"{Fluid.DisplayName.GetTranslation(Language.ActiveCulture)}\n{Fluid.VolumeBuckets:N2}/{Handler.GetSlotLimit(slot) / 255f:N2} B" : $"Empty\n{0:N2}/{Handler.GetSlotLimit(slot) / 255f:N2} B");
			}
		}
	}
}