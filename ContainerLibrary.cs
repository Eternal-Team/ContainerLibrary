using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ContainerLibrary
{
	public class ContainerLibrary : Mod
	{
		internal static Effect BarShader;

		public override void Load()
		{
			Hooking.Load();

			if (!Main.dedServ) BarShader = GetEffect("Effects/BarShader");
		}
	}
}