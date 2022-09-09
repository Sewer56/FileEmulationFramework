using System.ComponentModel;
using FileEmulationFramework.Template.Configuration;

namespace FileEmulationFramework;

public class Config : Configurable<Config>
{
    [DisplayName("Log Level")]
    [Description("Declares which elements should be logged to the console. From ")]
    [DefaultValue(LogSeverity.Warning)]
    public LogSeverity LogLevel { get; set; } = LogSeverity.Warning;

    public enum LogSeverity
    {
        Fatal, 
        Error,
        Warning,
        Information,
        Debug
    }
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}