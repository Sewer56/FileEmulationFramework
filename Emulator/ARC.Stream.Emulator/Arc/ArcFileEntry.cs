using System.Runtime.InteropServices;

namespace ARC.Stream.Emulator.Arc;

public struct ArcFileEntry
{
#pragma warning disable CS0649
	private unsafe fixed byte _fileName[64];
#pragma warning restore CS0649

	public int Offset;
	public uint Length;

	public unsafe string GetFileName()
	{
		fixed (byte* ptr = _fileName)
		{
			return Marshal.PtrToStringAnsi((nint)ptr)!;
		}
	}
}
