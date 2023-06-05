using System.Diagnostics.CodeAnalysis;
using Reloaded.Mod.Interfaces;

namespace FileEmulationFramework.Lib.Utilities;

/// <summary>
/// Provides high performance logging services for the underlying Reloaded logger.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Better stuff to do than test copy-pasta code.")]
public class Logger
{
    /// <summary>
    /// The underlying logger.
    /// </summary>
    public ILogger Log { get; private set; }
    
    /// <summary>
    /// The log level currently being used by the logger.
    /// </summary>
    public LogSeverity LogLevel { get; set; }

    /// <summary>
    /// Creates a reusable logger instance that allows for logging of events happening throughout.
    /// </summary>
    /// <param name="log">The underlying Reloaded logger.</param>
    /// <param name="logLevel">Severity to use with this logger instance.</param>
    public Logger(ILogger log, LogSeverity logLevel)
    {
        Log = log;
        LogLevel = logLevel;
    }

    /// <summary>
    /// Returns true if a given log level is enabled, else false.
    /// </summary>
    /// <param name="severity">The severity to check if enabled.</param>
    public bool IsEnabled(LogSeverity severity) => LogLevel <= severity;

    #region Fatal
    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Fatal"/> severity.
    /// </summary>
    /// <param name="message">The message to log</param>
    public void Fatal(string message)
    {
        Log.WriteLineAsync(message, Log.ColorRed);
    }

    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Fatal"/> severity.
    /// </summary>
    /// <param name="format">The format to use.</param>
    /// <param name="item1">The first generic item.</param>
    public void Fatal<T1>(string format, T1 item1)
    {
        Log.WriteLineAsync(string.Format(format, item1!.ToString()), Log.ColorRed);
    }

    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Fatal"/> severity.
    /// </summary>
    /// <param name="format">The format to use.</param>
    /// <param name="item1">The first generic item.</param>
    /// <param name="item2">The second generic item.</param>
    public void Fatal<T1, T2>(string format, T1 item1, T2 item2)
    {
        Log.WriteLineAsync(string.Format(format, item1!.ToString(), item2!.ToString()), Log.ColorRed);
    }

    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Fatal"/> severity.
    /// </summary>
    /// <param name="format">The format to use.</param>
    /// <param name="item1">The first generic item.</param>
    /// <param name="item2">The second generic item.</param>
    /// <param name="item3">The third generic item.</param>
    public void Fatal<T1, T2, T3>(string format, T1 item1, T2 item2, T3 item3)
    {
        Log.WriteLineAsync(string.Format(format, item1!.ToString(), item2!.ToString(), item3!.ToString()), Log.ColorRed);
    }
    #endregion

    #region Error
    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Error"/> severity.
    /// </summary>
    /// <param name="message">The message to log</param>
    public void Error(string message)
    {
        if (IsEnabled(LogSeverity.Error))
            Log.WriteLineAsync(message, Log.ColorRed);
    }

    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Error"/> severity.
    /// </summary>
    /// <param name="format">The format to use.</param>
    /// <param name="item1">The first generic item.</param>
    public void Error<T1>(string format, T1 item1)
    {
        if (IsEnabled(LogSeverity.Error))
            Log.WriteLineAsync(string.Format(format, item1!.ToString()), Log.ColorRed);
    }

    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Error"/> severity.
    /// </summary>
    /// <param name="format">The format to use.</param>
    /// <param name="item1">The first generic item.</param>
    /// <param name="item2">The second generic item.</param>
    public void Error<T1, T2>(string format, T1 item1, T2 item2)
    {
        if (IsEnabled(LogSeverity.Error))
            Log.WriteLineAsync(string.Format(format, item1!.ToString(), item2!.ToString()), Log.ColorRed);
    }

    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Error"/> severity.
    /// </summary>
    /// <param name="format">The format to use.</param>
    /// <param name="item1">The first generic item.</param>
    /// <param name="item2">The second generic item.</param>
    /// <param name="item3">The third generic item.</param>
    public void Error<T1, T2, T3>(string format, T1 item1, T2 item2, T3 item3)
    {
        if (IsEnabled(LogSeverity.Error))
            Log.WriteLineAsync(string.Format(format, item1!.ToString(), item2!.ToString(), item3!.ToString()), Log.ColorRed);
    }
    #endregion

    #region Warning
    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Warning"/> severity.
    /// </summary>
    /// <param name="message">The message to log</param>
    public void Warning(string message)
    {
        if (IsEnabled(LogSeverity.Warning))
            Log.WriteLineAsync(message, Log.ColorYellow);
    }

    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Warning"/> severity.
    /// </summary>
    /// <param name="format">The format to use.</param>
    /// <param name="item1">The first generic item.</param>
    public void Warning<T1>(string format, T1 item1)
    {
        if (IsEnabled(LogSeverity.Warning))
            Log.WriteLineAsync(string.Format(format, item1!.ToString()), Log.ColorYellow);
    }

    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Warning"/> severity.
    /// </summary>
    /// <param name="format">The format to use.</param>
    /// <param name="item1">The first generic item.</param>
    /// <param name="item2">The second generic item.</param>
    public void Warning<T1, T2>(string format, T1 item1, T2 item2)
    {
        if (IsEnabled(LogSeverity.Warning))
            Log.WriteLineAsync(string.Format(format, item1!.ToString(), item2!.ToString()), Log.ColorYellow);
    }

    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Warning"/> severity.
    /// </summary>
    /// <param name="format">The format to use.</param>
    /// <param name="item1">The first generic item.</param>
    /// <param name="item2">The second generic item.</param>
    /// <param name="item3">The third generic item.</param>
    public void Warning<T1, T2, T3>(string format, T1 item1, T2 item2, T3 item3)
    {
        if (IsEnabled(LogSeverity.Warning))
            Log.WriteLineAsync(string.Format(format, item1!.ToString(), item2!.ToString(), item3!.ToString()), Log.ColorYellow);
    }
    #endregion

    #region Info
    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Information"/> severity.
    /// </summary>
    /// <param name="message">The message to log</param>
    public void Info(string message)
    {
        if (IsEnabled(LogSeverity.Information))
            Log.WriteLineAsync(message, Log.ColorLightBlue);
    }

    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Information"/> severity.
    /// </summary>
    /// <param name="format">The format to use.</param>
    /// <param name="item1">The first generic item.</param>
    public void Info<T1>(string format, T1 item1)
    {
        if (IsEnabled(LogSeverity.Information))
            Log.WriteLineAsync(string.Format(format, item1!.ToString()), Log.ColorLightBlue);
    }

    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Information"/> severity.
    /// </summary>
    /// <param name="format">The format to use.</param>
    /// <param name="item1">The first generic item.</param>
    /// <param name="item2">The second generic item.</param>
    public void Info<T1, T2>(string format, T1 item1, T2 item2)
    {
        if (IsEnabled(LogSeverity.Information))
            Log.WriteLineAsync(string.Format(format, item1!.ToString(), item2!.ToString()), Log.ColorLightBlue);
    }

    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Information"/> severity.
    /// </summary>
    /// <param name="format">The format to use.</param>
    /// <param name="item1">The first generic item.</param>
    /// <param name="item2">The second generic item.</param>
    /// <param name="item3">The third generic item.</param>
    public void Info<T1, T2, T3>(string format, T1 item1, T2 item2, T3 item3)
    {
        if (IsEnabled(LogSeverity.Information))
            Log.WriteLineAsync(string.Format(format, item1!.ToString(), item2!.ToString(), item3!.ToString()), Log.ColorLightBlue);
    }
    #endregion

    #region Debug
    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Debug"/> severity.
    /// </summary>
    /// <param name="message">The message to log</param>
    public void Debug(string message)
    {
        if (IsEnabled(LogSeverity.Debug))
            Log.WriteLineAsync(message);
    }

    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Debug"/> severity.
    /// </summary>
    /// <param name="format">The format to use.</param>
    /// <param name="item1">The first generic item.</param>
    public void Debug<T1>(string format, T1 item1)
    {
        if (IsEnabled(LogSeverity.Debug))
            Log.WriteLineAsync(string.Format(format, item1!.ToString()));
    }

    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Debug"/> severity.
    /// </summary>
    /// <param name="format">The format to use.</param>
    /// <param name="item1">The first generic item.</param>
    /// <param name="item2">The second generic item.</param>
    public void Debug<T1, T2>(string format, T1 item1, T2 item2)
    {
        if (IsEnabled(LogSeverity.Debug))
            Log.WriteLineAsync(string.Format(format, item1!.ToString(), item2!.ToString()));
    }

    /// <summary>
    /// Logs a message using the <see cref="LogSeverity.Debug"/> severity.
    /// </summary>
    /// <param name="format">The format to use.</param>
    /// <param name="item1">The first generic item.</param>
    /// <param name="item2">The second generic item.</param>
    /// <param name="item3">The third generic item.</param>
    public void Debug<T1, T2, T3>(string format, T1 item1, T2 item2, T3 item3)
    {
        if (IsEnabled(LogSeverity.Debug))
            Log.WriteLineAsync(string.Format(format, item1!.ToString(), item2!.ToString(), item3!.ToString()));
    }
    #endregion
}

/// <summary>
/// Different logger levels.
/// </summary>
public enum LogSeverity
{
    /// <summary>
    /// Contains information generally only useful to people working on emulators.
    /// </summary>
    Debug,

    /// <summary>
    /// Anything that may be noteworthy to the user.
    /// </summary>
    Information,

    /// <summary>
    /// Something interesting happened, pay attention.
    /// </summary>
    Warning,

    /// <summary>
    /// Something went wrong, hopefully we can recover.
    /// </summary>
    Error,

    /// <summary>
    /// Application will probably crash.
    /// </summary>
    Fatal
}