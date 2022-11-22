using FileEmulationFramework.Lib.IO.Struct;
using FileEmulationFramework.Lib.Utilities;

namespace FileEmulationFramework.Lib.IO.Interfaces;

/// <summary>
/// Represents a stream backed by file slices.
/// </summary>
public interface IFileSliceStream
{
    /// <summary>
    /// The backing file slice for this stream.
    /// </summary>
    FileSlice Slice { get; }
}

/// <summary>
/// Extensions related to 
/// </summary>
public static class FileSliceStreamExtensions
{
    /// <summary>
    /// Tries to merge two file slice streams into one.
    /// </summary>
    /// <param name="first">First slice.</param>
    /// <param name="second">Second slice.</param>
    /// <param name="result">Result slice.</param>
    /// <remarks>
    ///     Slices will only be merged if there is no gap between first and second slice.
    ///     Does not support overlapping slices.
    /// </remarks>
    public static bool TryMerge(IFileSliceStream first, IFileSliceStream second, out FileSliceStreamW32? result)
    {
        if (!FileSlice.TryMerge(first.Slice, second.Slice, out var mergedSlice))
        {
            result = default;
            return false;
        }

        result = new FileSliceStreamW32(mergedSlice!);
        return true;
    }

    /// <summary>
    /// Merges all streams which contain file slices.
    /// </summary>
    /// <param name="streams">
    ///     The list of streams to merge.
    ///     Can be specified in any order.
    /// </param>
    /// <returns>A merged list of streams.</returns>
    public static List<StreamOffsetPair<Stream>> MergeStreams(IList<StreamOffsetPair<Stream>> streams)
    {
        var result = new List<StreamOffsetPair<Stream>>();
        var streamCount = streams.Count;

        // Sort by ascending addresses.
        streams = streams.OrderBy(x => x.Offset.Start).ToArray();

        for (int x = 0; x < streamCount; x++)
        {
            // Ignore non-slice streams.
            var currentPair = streams[x];
            var stream = currentPair.Stream;
            if (stream is not IFileSliceStream fileSliceStream)
            {
                result.Add(currentPair);
                continue;
            }

            // Try merge future streams.

            for (int y = x + 1; y < streamCount; y++)
            {
                var nextStream = streams[y].Stream;
                if (nextStream is not IFileSliceStream nextFileSliceStream)
                    break;

                if (!OffsetRangeExtensions.TryJoin(currentPair.Offset, streams[y].Offset, out var joined) ||
                    !TryMerge(fileSliceStream, nextFileSliceStream, out var merged)) 
                    continue;
                
                // Copy merged into variable and also advance x.
                fileSliceStream = merged!;

                // Assign the merged data.
                currentPair.Stream = merged!;
                currentPair.Offset = joined;

                x = y;
            }
            
            result.Add(currentPair);
        }

        return result;
    } 
}