using System.ComponentModel;
using AWB.Stream.Emulator.Template.Configuration;
using FileEmulationFramework.Lib.Utilities;

namespace AWB.Stream.Emulator;

public class Config : Configurable<Config>
{
    [DisplayName("Log Level")]
    [Description("Declares which elements should be logged to the console.\nMessages less important than this level will not be logged.")]
    [DefaultValue(LogSeverity.Warning)]
    public LogSeverity LogLevel { get; set; } = LogSeverity.Information;

    [DisplayName("Dump Emulated AWB Files")]
    [Description("Creates a dump of emulated AWB files as they are written.")]
    [DefaultValue(LogSeverity.Information)]
    public bool DumpAwb { get; set; } = false;
    
    [DisplayName("Dump Emulated ACB Files")]
    [Description("Creates a dump of emulated ACB files as they are written.")]
    [DefaultValue(LogSeverity.Information)]
    public bool DumpAcb { get; set; } = false;
    
    [DisplayName("Check Extensions for ACB files.")]
    [Description("Discards files based on known extensions to help with load times.")]
    [DefaultValue(true)]
    public bool CheckAcbExtension { get; set; } = true;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}