using System.Runtime.InteropServices;
using System.Text;
using Reloaded.Memory.Sources;

namespace ARC.Stream.Emulator.Arc;

public struct ArcFileEntry
{
	private unsafe fixed byte FileName[64];

	public int Offset;

	public uint Length;

	public unsafe static ArcFileEntry NewFileEntry(string filename, int offset, uint length)
	{
		ArcFileEntry result = default(ArcFileEntry);
		for (int i = 0; i < 64; i++)
		{
			result.FileName[i] = 0;
		}
		Memory.CurrentProcess.WriteRaw((nuint)result.FileName, Encoding.ASCII.GetBytes(filename));
		result.Offset = offset;
		result.Length = length;
		return result;
	}

	public unsafe string GetFileName()
	{
		fixed (byte* ptr = FileName)
		{
			return Marshal.PtrToStringAnsi((nint)ptr)!;
		}
	}
}
