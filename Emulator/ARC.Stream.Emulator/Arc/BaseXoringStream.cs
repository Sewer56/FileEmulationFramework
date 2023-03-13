// CODE TAKEN WITH PERMISSION FROM THE JSR ARC TOOL: https://steamcommunity.com/app/205950/discussions/0/535150948594821347/
// CODE WRITTEN BY https://twitter.com/GMMan_BZFlag

namespace ARC.Stream.Emulator.Arc;

public abstract class BaseXoringStream : System.IO.Stream
{
	protected System.IO.Stream BaseStream { get; set; }

	public override bool CanRead => BaseStream.CanRead;

	public override bool CanWrite => BaseStream.CanWrite;

	public override long Length => BaseStream.Length;

	public override long Position
	{
		get
		{
			return BaseStream.Position;
		}
		set
		{
			Seek(value, SeekOrigin.Begin);
		}
	}

	public bool CloseBase { get; set; }

	protected BaseXoringStream(System.IO.Stream baseStream)
	{
		BaseStream = baseStream;
	}

	public override void Flush()
	{
		BaseStream.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Offset in buffer cannot be negative.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Cannot read negative number of bytes.");
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException("Not enough space in array to store requested number of bytes.");
		}
		if (BaseStream.CanSeek)
		{
			long num = BaseStream.Length - BaseStream.Position;
			if (num <= 0)
			{
				return 0;
			}
			if (num < count)
			{
				count = (int)num;
			}
		}
		int num2 = BaseStream.Read(buffer, offset, count);
		for (int i = offset; i < num2; i++)
		{
			buffer[i] ^= GetNextKeyByte();
		}
		return num2;
	}

	public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

	protected abstract byte GetNextKeyByte();

	protected override void Dispose(bool disposing)
	{
		if (disposing && CloseBase)
		{
			BaseStream.Close();
		}
		base.Dispose(disposing);
	}
    public override void Write(ReadOnlySpan<byte> buffer)
	{
        var array = buffer.ToArray();
        for (int i = 0; i < array.Length; i++)
        {
            array[i] ^= GetNextKeyByte();
        }
        BaseStream.Write(array, 0, array.Length);
    }
}
