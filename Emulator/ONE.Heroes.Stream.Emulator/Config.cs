using System.ComponentModel;
using FileEmulationFramework.Lib.Utilities;
using ONE.Heroes.Stream.Emulator.Template.Configuration;

namespace ONE.Heroes.Stream.Emulator;

public class Config : Configurable<Config>
{
    [DisplayName("Log Level")]
    [Description("Declares which elements should be logged to the console.\nMessages less important than this level will not be logged.")]
    [DefaultValue(LogSeverity.Warning)]
    public LogSeverity LogLevel { get; set; } = LogSeverity.Information;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}