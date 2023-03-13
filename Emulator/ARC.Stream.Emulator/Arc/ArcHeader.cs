#pragma warning disable CS0169
#pragma warning disable CS0649
namespace ARC.Stream.Emulator.Arc;

public struct ArcHeader
{
	private unsafe fixed byte _tag[4];
	private ushort _numberOfFiles;

	public ushort GetNumberOfFiles() => _numberOfFiles;
}
