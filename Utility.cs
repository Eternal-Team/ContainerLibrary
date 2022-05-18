namespace ContainerLibrary;

public static class Utility
{
	public static int Min(int a, int b, int c)
	{
		return a < b && a < c ? a : b < c ? b : c;
	}
}