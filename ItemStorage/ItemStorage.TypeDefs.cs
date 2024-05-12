using System;

namespace ContainerLibrary;

public partial class ItemStorage
{
	public enum Result
	{
		Success,
		SourceIsAir,
		CantInteract,
		NotValid,
		DestinationFull
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