using BaseLibrary;
using BaseLibrary.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Terraria;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace ContainerLibrary
{
	public class UIContainerSlot : BaseElement
	{
		public Texture2D backgroundTexture = Main.inventoryBackTexture;

		public bool ShortStackSize = false;

		public Item PreviewItem;

		private readonly Func<ItemHandler> FuncHandler;
		public ItemHandler Handler => FuncHandler();

		public int slot;

		public Item Item
		{
			get => Handler.GetItemInSlot(slot);
			set => Handler.SetItemInSlot(slot, value);
		}

		public UIContainerSlot(Func<ItemHandler> itemHandler, int slot = 0)
		{
			Width = Height = (40, 0);

			this.slot = slot;
			FuncHandler = itemHandler;
		}

		public override void ScrollWheel(UIScrollWheelEvent evt)
		{
			if (!Main.keyState.IsKeyDown(Keys.LeftAlt)) return;

			if (evt.ScrollWheelValue > 0)
			{
				if (Main.mouseItem.type == Item.type && Main.mouseItem.stack < Main.mouseItem.maxStack)
				{
					Main.mouseItem.stack++;
					Handler.Shrink(slot, 1);
				}
				else if (Main.mouseItem.IsAir)
				{
					Main.mouseItem = Item.Clone();
					Main.mouseItem.stack = 1;
					Handler.Shrink(slot, 1);
				}
			}
			else if (evt.ScrollWheelValue < 0)
			{
				if (Item.type == Main.mouseItem.type && Handler.Grow(slot, 1))
				{
					if (--Main.mouseItem.stack <= 0) Main.mouseItem.TurnToAir();
				}
				else if (Item.IsAir)
				{
					Item = Main.mouseItem.Clone();
					Item.stack = 1;
					if (--Main.mouseItem.stack <= 0) Main.mouseItem.TurnToAir();
				}
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			if (Handler.IsItemValid(slot, Main.mouseItem) || Main.mouseItem.IsAir)
			{
				Item.newAndShiny = false;
				Player player = Main.LocalPlayer;

				if (ItemSlot.ShiftInUse)
				{
					ItemUtility.Loot(Handler, slot, Main.LocalPlayer);

					base.Click(evt);

					return;
				}

				if (Main.mouseItem.IsAir) Main.mouseItem = Handler.ExtractItem(slot, Item.maxStack);
				else
				{
					if (Item.IsTheSameAs(Main.mouseItem)) Main.mouseItem = Handler.InsertItem(slot, Main.mouseItem);
					else
					{
						if (Item.stack <= Item.maxStack)
						{
							Item temp = Item;
							Utils.Swap(ref temp, ref Main.mouseItem);
							Item = temp;
						}
					}
				}

				if (Item.stack > 0) AchievementsHelper.NotifyItemPickup(player, Item);

				if (Main.mouseItem.type > 0 || Item.type > 0)
				{
					Recipe.FindRecipes();
					Main.PlaySound(SoundID.Grab);
				}
			}

			base.Click(evt);
		}

		public override void RightClickContinuous(UIMouseEvent evt)
		{
			if (Handler.IsItemValid(slot, Main.mouseItem) || Main.mouseItem.IsAir)
			{
				Player player = Main.LocalPlayer;
				Item.newAndShiny = false;

				if (player.itemAnimation > 0) return;

				if (Main.stackSplit <= 1 && Main.mouseRight)
				{
					if ((Main.mouseItem.IsTheSameAs(Item) || Main.mouseItem.type == 0) && (Main.mouseItem.stack < Main.mouseItem.maxStack || Main.mouseItem.type == 0))
					{
						if (Main.mouseItem.type == 0)
						{
							Main.mouseItem = Item.Clone();
							Main.mouseItem.stack = 0;
							if (Item.favorited && Item.maxStack == 1) Main.mouseItem.favorited = true;
							Main.mouseItem.favorited = false;
						}

						Main.mouseItem.stack++;
						Handler.Shrink(slot, 1);

						Recipe.FindRecipes();

						Main.soundInstanceMenuTick.Stop();
						Main.soundInstanceMenuTick = Main.soundMenuTick.CreateInstance();
						Main.PlaySound(12);

						Main.stackSplit = Main.stackSplit == 0 ? 15 : Main.stackDelay;
					}
				}
			}
		}

		public override int CompareTo(object obj) => slot.CompareTo(((UIContainerSlot)obj).slot);

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (IsMouseHovering && Main.keyState.IsKeyDown(Keys.LeftAlt)) BaseLibrary.Hooking.BlockScrolling = true;
		}

		private void DrawItem(SpriteBatch spriteBatch, Item item, float scale)
		{
			Texture2D itemTexture = Main.itemTexture[item.type];
			Rectangle rect = Main.itemAnimations[item.type] != null ? Main.itemAnimations[item.type].GetFrame(itemTexture) : itemTexture.Frame();
			Color newColor = Color.White;
			float pulseScale = 1f;
			ItemSlot.GetItemLight(ref newColor, ref pulseScale, item);
			int height = rect.Height;
			int width = rect.Width;
			float drawScale = 1f;

			float availableWidth = InnerDimensions.Width;
			if (width > availableWidth || height > availableWidth)
			{
				if (width > height) drawScale = availableWidth / width;
				else drawScale = availableWidth / height;
			}

			drawScale *= scale;
			Vector2 position = Dimensions.Position() + Dimensions.Size() * 0.5f;
			Vector2 origin = rect.Size() * 0.5f;

			if (ItemLoader.PreDrawInInventory(item, spriteBatch, position - rect.Size() * 0.5f * drawScale, rect, item.GetAlpha(newColor), item.GetColor(Color.White), origin, drawScale * pulseScale))
			{
				spriteBatch.Draw(itemTexture, position, rect, item.GetAlpha(newColor), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
				if (item.color != Color.Transparent) spriteBatch.Draw(itemTexture, position, rect, item.GetColor(Color.White), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
			}

			ItemLoader.PostDrawInInventory(item, spriteBatch, position - rect.Size() * 0.5f * drawScale, rect, item.GetAlpha(newColor), item.GetColor(Color.White), origin, drawScale * pulseScale);
			if (ItemID.Sets.TrapSigned[item.type]) spriteBatch.Draw(Main.wireTexture, position + new Vector2(40f, 40f) * scale, new Rectangle(4, 58, 8, 8), Color.White, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);
			if (item.stack > 1)
			{
				string text = !ShortStackSize || item.stack < 1000 ? item.stack.ToString() : item.stack.ToSI("N1");
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, text, InnerDimensions.Position() + new Vector2(8, InnerDimensions.Height - Main.fontMouseText.MeasureString(text).Y * scale), Color.White, 0f, Vector2.Zero, new Vector2(0.85f), -1f, scale);
			}

			if (IsMouseHovering)
			{
				Main.LocalPlayer.showItemIcon = false;
				Main.ItemIconCacheUpdate(0);
				Main.HoverItem = item.Clone();
				Main.hoverItemName = Main.HoverItem.Name;

				if (ItemSlot.ShiftInUse) BaseLibrary.Hooking.SetCursor("Terraria/UI/Cursor_7");
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			spriteBatch.DrawSlot(Dimensions, Color.White, !Item.IsAir && Item.favorited ? Main.inventoryBack10Texture : backgroundTexture);

			float scale = Math.Min(InnerDimensions.Width / backgroundTexture.Width, InnerDimensions.Height / backgroundTexture.Height);

			if (!Item.IsAir) DrawItem(spriteBatch, Item, scale);
			else if (PreviewItem != null && !PreviewItem.IsAir) spriteBatch.DrawWithEffect(BaseLibrary.BaseLibrary.DesaturateShader, () => DrawItem(spriteBatch, PreviewItem, scale));
		}
	}
}