// CODE TAKEN WITH PERMISSION FROM THE JSR ARC TOOL: https://steamcommunity.com/app/205950/discussions/0/535150948594821347/
// CODE WRITTEN BY https://twitter.com/GMMan_BZFlag
namespace ARC.Stream.Emulator.Arc;

internal class JRand
{
	private int _rSeed;

	public JRand(int seed)
	{
		_rSeed = seed;
	}

	public int Next()
	{
		int num = _rSeed / 44488;
		int num2 = _rSeed - 44488 * num;
		int num3 = 48271 * num2 - 3399 * num;
		if (num3 <= 0)
		{
			num3 += int.MaxValue;
		}
		_rSeed = num3;
		return _rSeed;
	}
}
