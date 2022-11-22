namespace AWB.Stream.Emulator.Awb.Utilities;

/// <summary>
/// Utility classes for reading numbers.
/// </summary>
public static unsafe class ValueReaders
{
    /// <summary>
    /// Reads the number into a long, using the specified bit count [multiple of byte].
    /// </summary>
    /// <param name="data">The address to read from.</param>
    /// <param name="size">Size of the data in bits. Accepts 1/2/4/8.</param>
    public static long ReadNumber(byte* data, int size)
    {
        switch (size)
        {
            case 1:
                return *data;
            case 2:
                return *(short*)data;
            case 4:
                return *(int*)data;
            case 8:
                return *(long*)data;
            default:
                ThrowHelpers.ThrowBadFieldSizeException();
                return 0;
        }
    }
    
    /// <summary>
    /// Reads the number into a long, using the specified bit count [multiple of byte].
    /// </summary>
    /// <param name="data">The address to read from.</param>
    /// <param name="size">Size of the data in bits. Accepts 1/2/4/8.</param>
    public static long ReadNumberAndIncrementPtr(ref byte* data, int size)
    {
        switch (size)
        {
            case 1:
                var resultByte = *data;
                data += 1;
                return resultByte;
            case 2:
                var resultShort = *(short*)data;
                data += 2;
                return resultShort;
            case 4:                
                var resultInt = *(int*)data;
                data += 4;
                return resultInt;
            case 8:             
                var resultLong = *(long*)data;
                data += 8;
                return resultLong;
            default:
                ThrowHelpers.ThrowBadFieldSizeException();
                return 0;
        }
    }
}