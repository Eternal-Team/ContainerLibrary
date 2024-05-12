using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace ContainerLibrary;

public partial class ItemStorage
{
	public delegate bool StorageFilter(int slot, Item item);

	public static class Filter
	{
		// TODO: Add more filters
		public static readonly StorageFilter NoCoin = (slot, item) => !item.IsACoin;
	}

	private readonly List<StorageFilter> Filters = [];

	public ItemStorage AddFilter(StorageFilter filter)
	{
		Filters.Add(filter);
		return this;
	}

	private bool EvaluateFilters(int slot, Item item) => Filters.All(filter => filter.Invoke(slot, item));
}