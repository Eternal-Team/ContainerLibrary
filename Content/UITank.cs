using BaseLibrary.UI.Elements;
using FluidLibrary.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Localization;
using Terraria.UI;
using static BaseLibrary.Utility.Utility;

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
			CalculatedStyle dimensions = GetInnerDimensions();

			spriteBatch.DrawSlot(dimensions);

			if (Fluid != null)
			{
				Texture2D texture = FluidLoader.textureCache[Fluid.Texture];
				float scale = (float)Fluid.volume / Handler.GetSlotLimit(slot);

				spriteBatch.Draw(texture, new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)(dimensions.Height * scale)), Color.White);
			}

			if (IsMouseHovering)
			{
				Main.LocalPlayer.showItemIcon = false;
				Main.ItemIconCacheUpdate(0);

				if (Fluid != null) DrawMouseText($"Fluid: {Fluid.DisplayName.GetTranslation(Language.ActiveCulture)}\n{Fluid.volume / 255f:N2}/{Handler.GetSlotLimit(slot) / 255f:N2} B");
				else DrawMouseText($"Fluid: None\n{0:N2}/{Handler.GetSlotLimit(slot) / 255f:N2} B");
			}
		}
	}
}