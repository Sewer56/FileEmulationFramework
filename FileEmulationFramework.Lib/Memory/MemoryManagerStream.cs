using System.IO.MemoryMappedFiles;
using FileEmulationFramework.Lib.Utilities;
using MemoryExtensions = FileEmulationFramework.Lib.Utilities.MemoryExtensions;

namespace FileEmulationFramework.Lib.Memory;

/// <summary>
/// A stream class backed by <see cref="MemoryManager"/>.
/// Intended for writing to the memory manager for storage of type B (memory mapped) redirectors.
/// For reading, it is recommended to use the <see cref="MemoryManager"/> raw.
/// </summary>
public unsafe class MemoryManagerStream : Stream
{
    /// <inheritdoc />
    public override bool CanRead { get; } = true;

    /// <inheritdoc />
    public override bool CanSeek { get; } = true;

    /// <inheritdoc />
    public override bool CanWrite { get; } = true;

    /// <inheritdoc />
    public override long Length => _length;

    /// <inheritdoc />
    public override long Position
    {
        get => _position;
        set => Seek(value, SeekOrigin.Begin);
    }

    private long _position;
    private long _length;
    private MemoryManager _memoryManager;
    private MemoryMappedFile? _currentMappedFile;
    private bool _ownsManager;
    private int _allocationGranularity;
    private MemoryMappedRegion _mappedRegion;

    /// <summary>
    /// Pointer to the start of the current data within the current mapped file.
    /// </summary>
    private byte* _memoryBasePtr = (byte*)0x0;

    /// <summary>
    /// Pointer to the current position within the mapped file.
    /// </summary>
    private byte* _memoryPtr = (byte*)0x0;

    /// <summary>
    /// Remaining bytes in the current chunk.
    /// </summary>
    private int _bytesAvailable;

    /// <summary>
    /// Creates a stream backed by <see cref="MemoryManager"/>.
    /// </summary>
    /// <param name="memoryManager">The backing store for this stream.</param>
    /// <param name="ownsManager">True if the stream owns this manager, else false.</param>
    /// <remarks>
    ///     For streams with existing backing data, assumes the length is page count * allocation granularity.
    ///     Otherwise length is expanded during write operations to the longest the position has reached.
    /// </remarks>
    public MemoryManagerStream(MemoryManager memoryManager, bool ownsManager = false)
    {
        _memoryManager = memoryManager;
        _mappedRegion = _memoryManager.CreateMappedRegion();
        _allocationGranularity = _memoryManager.AllocationGranularity;
        _length = memoryManager.PageCount * memoryManager.AllocationGranularity;
        _ownsManager = ownsManager;
    }

    /// <inheritdoc />
    ~MemoryManagerStream() => Dispose(true);

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _mappedRegion.Dispose();
            if (_ownsManager)
                _memoryManager.Dispose();

            GC.SuppressFinalize(this);
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc />
    public override void Flush() { /* no-op */ }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpanFast(offset, count));

    /// <inheritdoc />
    public override int Read(Span<byte> buffer)
    {
        EnsureAccessorNoAlloc();

        var count = buffer.Length;
        
        // Check if we can fit within boundaries.
        if (count <= _bytesAvailable)
        {
            MemoryExtensions.ToSpanFast(_memoryPtr, count).CopyTo(buffer);
            _memoryPtr += count;
            _bytesAvailable -= count;
            _position += count;
        }
        else
        {
            // Else we might need to do something more complex across boundaries.
            // This could probably be optimised to shave a few more instructions, but is the cold path ultimately.
            if (Length - Position < count)
                count = (int)(Length - Position);

            int bytesRemaining = count;
            var bufferOffset = 0;
            while (bytesRemaining > 0)
            {
                var numToCopy = Math.Min(bytesRemaining, _bytesAvailable);
                MemoryExtensions.ToSpanFast(_memoryPtr, numToCopy).CopyTo(buffer.Slice(bufferOffset, numToCopy));

                bytesRemaining -= numToCopy;
                bufferOffset += numToCopy;

                // We could do some of this once after the loop, but we're taking an assumption a read operation 
                // isn't hopefully going to span over too many boundaries.
                _position += numToCopy;
                _memoryPtr += numToCopy;
                _bytesAvailable -= numToCopy;

                GetNextAccessorIfNeededNoAlloc();
            }
        }

        return count;
    }

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpanFast(offset, count));
    
    /// <summary>
    /// Writes the contents of the specified buffer to the output.
    /// </summary>
    /// <param name="buffer">The buffer to write contents from.</param>
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        EnsureAccessorAlloc();
        var count = buffer.Length;

        // Check if we can fit within boundaries.
        if (count <= _bytesAvailable)
        {
            buffer.CopyTo(MemoryExtensions.ToSpanFast(_memoryPtr, count));
            _memoryPtr += count;
            _bytesAvailable -= count;
            _position += count;
        }
        else
        {
            // Else we need to perform a cross boundary operation.
            // Like read, could probably be optimised for a few more instructions, but differences would probably be negligible.
            var bytesRemaining = count;
            var bufferOffset = 0;
            while (bytesRemaining > 0)
            {
                var numToCopy = Math.Min(bytesRemaining, _bytesAvailable);
                buffer.Slice(bufferOffset, numToCopy).CopyTo(MemoryExtensions.ToSpanFast(_memoryPtr, numToCopy));

                bytesRemaining -= numToCopy;
                bufferOffset += numToCopy;

                // We could do this once after the loop, but we're taking an assumption a read operation 
                // isn't hopefully going to span over too many boundaries.
                _position += numToCopy;
                _memoryPtr += numToCopy;
                _bytesAvailable -= numToCopy;

                GetNextAccessorIfNeededAlloc();
            }
        }

        // Update length
        if (Position > _length)
            _length = Position;
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin)
    {
        if (origin == SeekOrigin.Begin)
        {
            if (TryFastSeek(offset))
                return _position;

            _position = offset;
            GetNextAccessorAlloc();
        }
        else if (origin == SeekOrigin.Current)
        {
            _position += offset;
            _memoryPtr += offset;
            _bytesAvailable -= (int)offset;
            GetNextAccessorIfNeededAlloc();
        }
        else if (origin == SeekOrigin.End)
        {
            var newPosition = _length + offset;
            if (TryFastSeek(newPosition))
                return _position;

            _position = newPosition;
            GetNextAccessorAlloc();
        }

        return _position;
    }

    private bool TryFastSeek(long target)
    {
        var offset = target - Position;
        var maxBytesFromStart = _memoryBasePtr - _memoryPtr;
        if (offset <= _bytesAvailable && offset >= 0 || offset >= maxBytesFromStart && offset < 0)
        {
            _position += offset;
            _memoryPtr += offset;
            _bytesAvailable -= (int)offset;
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override void SetLength(long value)
    {
        _length = value;

        // Ensure we have allocated sufficient space.
        _memoryManager.GetMappedFileWithAlloc(_length, out _, out _);
    }

    private void EnsureAccessorNoAlloc()
    {
        if (!_mappedRegion.IsMapped)
        {
            var currentMappedFile = _memoryManager.GetMappedFile(Position, out var byteOffset, out _bytesAvailable);
            CreateAccessor(currentMappedFile, byteOffset);
        }
        else
        {
            GetNextAccessorIfNeededNoAlloc();
        }
    }

    private void GetNextAccessorIfNeededNoAlloc()
    {
        if (IsNewAccessorNeeded())
        {
            var currentMappedFile = _memoryManager.GetMappedFile(Position, out var byteOffset, out _bytesAvailable);
            CreateAccessor(currentMappedFile, byteOffset);
        }
    }

    private bool IsNewAccessorNeeded()
    {
        // Second condition is necessary in case of seek backwards across page boundary.
        return _bytesAvailable <= 0 || _bytesAvailable > _allocationGranularity;
    }

    private void EnsureAccessorAlloc()
    {
        if (!_mappedRegion.IsMapped)
        {
            var currentMappedFile = _memoryManager.GetMappedFileWithAlloc(Position, out var byteOffset, out _bytesAvailable);
            CreateAccessor(currentMappedFile, byteOffset);
        }
        else
        {
            GetNextAccessorIfNeededAlloc();
        }
    }

    private void GetNextAccessorIfNeededAlloc()
    {
        if (IsNewAccessorNeeded())
            GetNextAccessorAlloc();
    }

    private void GetNextAccessorAlloc()
    {
        _currentMappedFile = _memoryManager.GetMappedFileWithAlloc(Position, out var byteOffset, out _bytesAvailable);
        CreateAccessor(_currentMappedFile, byteOffset);
    }

    private void CreateAccessor(MemoryMappedFile mappedFile, int byteOffset)
    {
        _mappedRegion.Update(mappedFile);
        _memoryBasePtr = _mappedRegion.MappedRegion;
        _memoryPtr = _memoryBasePtr + byteOffset;
    }
}