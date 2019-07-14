using BaseLibrary;
using BaseLibrary.UI.Elements;
using FluidLibrary.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
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
			Main.instance.GraphicsDevice.PresentationParameters.DepthStencilFormat = DepthFormat.Depth24Stencil8;

			var s1 = new DepthStencilState
			{
				StencilEnable = true,
				StencilFunction = CompareFunction.Always,
				StencilPass = StencilOperation.Replace,
				ReferenceStencil = 1,
				DepthBufferEnable = false
			};

			var s2 = new DepthStencilState
			{
				StencilEnable = true,
				StencilFunction = CompareFunction.GreaterEqual,
				StencilPass = StencilOperation.Keep,
				ReferenceStencil = 1,
				DepthBufferEnable = false
			};

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, null, null, s1, null, null);
			spriteBatch.DrawSlot(Dimensions, Color.White, Main.inventoryBackTexture);
			spriteBatch.End();

			Fluid = new Water {volume = 255};
			if (Fluid != null)
			{
				spriteBatch.Begin(SpriteSortMode.Immediate, null, null, s2, null, null);
				spriteBatch.Draw(ModContent.GetTexture(Fluid.Texture), new Rectangle((int)Dimensions.X, (int)Dimensions.Y, (int)Dimensions.Width, (int)(Dimensions.Height * (Fluid.volume / (float)Handler.GetSlotLimit(0)))));
				spriteBatch.End();
			}

			//Main.instance.GraphicsDevice.PresentationParameters.DepthStencilFormat = DepthFormat.Depth24;
			spriteBatch.Begin();

			if (IsMouseHovering)
			{
				Main.LocalPlayer.showItemIcon = false;
				Main.ItemIconCacheUpdate(0);

				Utility.DrawMouseText(Fluid != null ? $"{Fluid.DisplayName}\n{Fluid.VolumeBuckets:N2}/{Handler.GetSlotLimit(slot) / 255f:N2} B" : $"Empty\n{0:N2}/{Handler.GetSlotLimit(slot) / 255f:N2} B");
			}
		}
	}
}