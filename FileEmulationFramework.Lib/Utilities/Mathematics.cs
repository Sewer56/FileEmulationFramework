namespace FileEmulationFramework.Lib.Utilities;

/// <summary>
/// Number related utility functions.
/// </summary>
public class Mathematics
{
    /// <summary>
    /// Rounds up the integer to the next multiple.
    /// If the integer is already a multiple, returns same value.
    /// </summary>
    /// <param name="number">The number to be rounded up.</param>
    /// <param name="multiple">The multiple to round to.</param>
    public static int RoundUp(int number, int multiple)
    {
        if (multiple == 0)
            return number;

        int remainder = number % multiple;
        if (remainder == 0)
            return number;

        return number + multiple - remainder;
    }

    /// <summary>
    /// Rounds up the integer to the next multiple.
    /// If the integer is already a multiple, returns same value.
    /// </summary>
    /// <param name="number">The number to be rounded up.</param>
    /// <param name="multiple">The multiple to round to.</param>
    public static long RoundUp(long number, long multiple)
    {
        if (multiple == 0)
            return number;

        long remainder = number % multiple;
        if (remainder == 0)
            return number;

        return number + multiple - remainder;
    }
}