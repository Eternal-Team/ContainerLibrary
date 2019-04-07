using Microsoft.Xna.Framework.Graphics;
using On.Terraria;
using On.Terraria.UI;
using Terraria.ModLoader;

namespace ContainerLibrary
{
	public partial class ContainerLibrary : Mod
	{
		public static Effect barShader;

		public override void Load()
		{
			ItemSlot.OverrideHover += ItemSlot_OverrideHover;
			Main.DrawInterface_36_Cursor += Main_DrawInterface_36_Cursor;

			Recipe.FindRecipes += Recipe_FindRecipes;
			Recipe.Create += Recipe_Create;

			if (!Terraria.Main.dedServ) barShader = GetEffect("Effects/BarShader");
		}
	}
}