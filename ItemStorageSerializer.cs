// using System;
// using System.Linq;
// using System.Reflection;
// using MonoMod.RuntimeDetour.HookGen;
// using Terraria;
// using Terraria.ModLoader;
// using Terraria.ModLoader.IO;
//
// namespace ContainerLibrary;
//
// [Autoload(false)]
// public class ItemStorageSerializer : TagSerializer<ItemStorage, TagCompound>
// {
// 	private static ItemStorageSerializer Instance = new();
//
// 	private delegate bool orig_TryGetSerializer(Type type, out TagSerializer serializer);
//
// 	private delegate bool hook_TryGetSerializer(orig_TryGetSerializer orig, Type type, out TagSerializer serializer);
//
// 	internal new static void Load()
// 	{
// 		Instance = new ItemStorageSerializer();
//
// 		HookEndpointManager.Add<hook_TryGetSerializer>(typeof(TagSerializer).GetMethod("TryGetSerializer", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), (hook_TryGetSerializer)((orig_TryGetSerializer orig, Type type, out TagSerializer serializer) =>
// 		{
// 			if (type == typeof(ItemStorage) || type.IsSubclassOf(typeof(ItemStorage)))
// 			{
// 				serializer = Instance;
// 				return true;
// 			}
//
// 			return orig(type, out serializer);
// 		}));
//
// 	}
//
// 	public override TagCompound Serialize(ItemStorage value) => new()
// 	{
// 		["Value"] = value.Items.ToList()
// 	};
//
// 	public override ItemStorage Deserialize(TagCompound tag)
// 	{
// 		return new ItemStorage(tag.GetList<Item>("Value"));
// 	}
// }