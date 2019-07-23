using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ContainerLibrary
{
	public class ContainerLibrary : Mod
	{
		public static Effect barShader;

		public override void Load()
		{
			Hooking.Load();

			if (!Main.dedServ) barShader = GetEffect("Effects/BarShader");
		}
	}
}