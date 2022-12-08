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

	public unsafe static ArcHeader GetDefault()
	{
		ArcHeader result = default(ArcHeader);
		for (int i = 0; i < 4; i++)
		{
			result._tag[i] = 0;
		}
		Memory.CurrentProcess.WriteRaw((nuint)result._tag, Encoding.ASCII.GetBytes("ARCL"));
		return result;
	}

	public static ArcHeader FromNumberOfFiles(ushort numberOfFiles)
	{
		ArcHeader @default = GetDefault();
		@default.NumberOfFiles = numberOfFiles;
		return @default;
	}

	public ushort GetNumberOfFiles()
	{
		return NumberOfFiles;
	}
}
