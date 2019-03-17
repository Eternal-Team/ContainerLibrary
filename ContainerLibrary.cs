using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ContainerLibrary
{
	public class ContainerLibrary : Mod
	{
		public static Effect barShader;

		public override void Load()
		{
			if (!Main.dedServ) barShader = GetEffect("Effects/BarShader");
		}
	}
}