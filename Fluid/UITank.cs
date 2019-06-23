using BaseLibrary;
using BaseLibrary.UI.Elements;
using FluidLibrary.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

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

			Main.OnRenderTargetsInitialized += (width, height) => tankTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, width, height, false, Main.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
			Main.OnRenderTargetsReleased += () => tankTarget?.Dispose();
		}

		public override int CompareTo(object obj) => slot.CompareTo(((UITank)obj).slot);

		public RenderTarget2D tankTarget;

		public void DrawTarget(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();

			Main.graphics.GraphicsDevice.SetRenderTarget(tankTarget);
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			spriteBatch.End();
			spriteBatch.Begin();

			spriteBatch.DrawSlot(new Rectangle(0, 0, (int)dimensions.Width, (int)dimensions.Height), Color.White, Main.inventoryBackTexture);

			spriteBatch.End();
			spriteBatch.Begin();

			Main.graphics.GraphicsDevice.SetRenderTarget(ModLoader.GetMod("PortableStorage").GetValue<RenderTarget2D>("uiTarget"));

			ContainerLibrary.barShader.Parameters["barColor"].SetValue(BaseLibrary.Utility.ColorSlot.ToVector4());
			ContainerLibrary.barShader.Parameters["progress"].SetValue((Fluid?.volume ?? 0) / (float)Handler.GetSlotLimit(slot));
			ContainerLibrary.barShader.Parameters["texOverlay"].SetValue((Texture2D)null);

			if (Fluid != null)
			{
				ContainerLibrary.barShader.Parameters["texOverlay"].SetValue(FluidLoader.textureCache[Fluid.Texture]);
			}

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, ContainerLibrary.barShader, Main.UIScaleMatrix);

			if (tankTarget != null) spriteBatch.Draw(tankTarget, dimensions.ToRectangle(), Color.White);

			spriteBatch.End();
			spriteBatch.Begin();

			if (IsMouseHovering)
			{
				Main.LocalPlayer.showItemIcon = false;
				Main.ItemIconCacheUpdate(0);

				//.GetTranslation(Language.ActiveCulture)
				BaseLibrary.Utility.DrawMouseText(Fluid != null ? $"Fluid: {Fluid.DisplayName}\n{Fluid.VolumeBuckets:N2}/{Handler.GetSlotLimit(slot) / 255f:N2} B" : $"Fluid: None\n{0:N2}/{Handler.GetSlotLimit(slot) / 255f:N2} B");
			}
		}
	}
}