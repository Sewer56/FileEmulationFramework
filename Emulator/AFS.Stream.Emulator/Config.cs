using System.ComponentModel;
using AFS.Stream.Emulator.Template.Configuration;
using FileEmulationFramework.Lib.Utilities;

namespace AFS.Stream.Emulator;

public class Config : Configurable<Config>
{
    [DisplayName("Log Level")]
    [Description("Declares which elements should be logged to the console.\nMessages less important than this level will not be logged.")]
    [DefaultValue(LogSeverity.Warning)]
    public LogSeverity LogLevel { get; set; } = LogSeverity.Information;

    [DisplayName("Dump Emulated AFS Files")]
    [Description("Creates a dump of emulated AFS files as they are written.")]
    [DefaultValue(LogSeverity.Information)]
    public bool DumpAfs { get; set; } = false;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}