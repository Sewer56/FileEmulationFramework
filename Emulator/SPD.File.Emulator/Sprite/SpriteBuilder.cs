using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;

namespace SPD.File.Emulator.Sprite;

public abstract class SpriteBuilder
{
    protected Dictionary<string, FileSlice> CustomSprFiles = new();
    protected Dictionary<string, FileSlice> CustomTextureFiles = new();

    protected Logger? _log = null;

    public SpriteBuilder() { }
    public SpriteBuilder(Logger log)
    {
        _log = log;
    }

    /// <summary>
    /// Adds a file to the Virtual SpriteFile builder.
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    public abstract void AddOrReplaceFile(string filePath);

    /// <summary>
    /// Builds a Sprite file.
    /// </summary>
    public abstract unsafe MultiStream Build(IntPtr handle, string filepath, Logger? logger = null, string folder = "", long baseOffset = 0);

    internal static T GetHeaderFromSpr<T>(Stream stream, long pos) where T : unmanaged
    {
        stream.Seek(pos, SeekOrigin.Begin);

        return stream.Read<T>();
    }

    internal static HashSet<int> GetSpriteIdsFromFilename(string fileName)
    {
        HashSet<int> ids = new();

        // Remove 'spr_' in the filename and Separate Ids by '_'
        var spriteIds = fileName.Split('_', StringSplitOptions.TrimEntries);

        foreach (var spriteIdStr in spriteIds)
        {
            // Check for sprite ranges
            if (spriteIdStr.Contains('-'))
            {
                // Parse sprite range
                var spriteIdRangeStr = spriteIdStr.Split("-");
                if (!int.TryParse(spriteIdRangeStr[0], out int spriteIdRangeLower)) break;
                if (!int.TryParse(spriteIdRangeStr[1], out int spriteIdRangeUpper)) break;

                for (int i = spriteIdRangeLower; i <= spriteIdRangeUpper; i++)
                {
                    ids.Add(i);
                }
            }
            else if (int.TryParse(spriteIdStr, out int spriteId))
            {
                ids.Add(spriteId);
            }
        }

        return ids;
    }
}
