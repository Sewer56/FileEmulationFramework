using System;
using System.Runtime.InteropServices;
using System.Text;
using Reloaded.Memory.Sources;

namespace ARC.Stream.Emulator.Arc;

public struct ArcHeader
{
	private const int TagLength = 4;

	private unsafe fixed byte _tag[4];

	private ushort NumberOfFiles;

	public unsafe bool IsArcArchive
	{
		get
		{
			fixed (byte* ptr = _tag)
			{
				return Marshal.PtrToStringAnsi((nint)ptr)!.Equals("ARCL", StringComparison.OrdinalIgnoreCase);
			}
		}
	}
	
	public ushort GetNumberOfFiles()
	{
		return NumberOfFiles;
	}
}
