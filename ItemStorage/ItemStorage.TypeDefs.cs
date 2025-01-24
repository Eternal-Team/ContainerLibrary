using System;

namespace ContainerLibrary;

public partial class ItemStorage
{
	public enum Result
	{
		Success,
		PartialSuccess,
		ItemIsAir,
		CantInteract,
		NotValid,
		DestinationFull,
		SourceEmpty
	}

	[Flags]
	public enum Action
	{
		Insert = 1,
		Remove = 2,
		Both = 3
	}

	public delegate int StackOverride(int slot);

	public delegate bool CanInteract(object? user, int slot, Action action);
}