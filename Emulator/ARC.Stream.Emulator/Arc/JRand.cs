namespace ARC.Stream.Emulator.Arc;

internal class JRand
{
	private const int m = int.MaxValue;

	private const int a = 48271;

	private const int q = 44488;

	private const int r = 3399;

	private int r_seed;

	public JRand(int seed)
	{
		r_seed = seed;
	}

	public int Next()
	{
		int num = r_seed / 44488;
		int num2 = r_seed - 44488 * num;
		int num3 = 48271 * num2 - 3399 * num;
		if (num3 <= 0)
		{
			num3 += int.MaxValue;
		}
		r_seed = num3;
		return r_seed;
	}
}
