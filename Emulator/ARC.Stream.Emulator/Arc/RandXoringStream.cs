// CODE TAKEN WITH PERMISSION FROM THE JSR ARC TOOL: https://steamcommunity.com/app/205950/discussions/0/535150948594821347/
// CODE WRITTEN BY https://twitter.com/GMMan_BZFlag

namespace ARC.Stream.Emulator.Arc;

internal class RandXoringStream : BaseXoringStream
{
	private readonly JRand _rand;

	public override bool CanSeek => false;

	public RandXoringStream(System.IO.Stream baseStream, int seed) : base(baseStream)
	{
		_rand = new JRand(seed);
	}

	protected override byte GetNextKeyByte() => (byte)(_rand.Next() % 255);
	public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
	public override void SetLength(long value) => throw new NotImplementedException();
}
