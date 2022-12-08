using System.Runtime.InteropServices;
using System.Text;
using Reloaded.Memory.Sources;

namespace ARC.Stream.Emulator.Arc;

public struct ArcFileEntry
{
	private unsafe fixed byte FileName[64];

	public int Offset;

	public uint Length;

	public unsafe string GetFileName()
	{
		fixed (byte* ptr = FileName)
		{
			return Marshal.PtrToStringAnsi((nint)ptr)!;
		}
	}
}
