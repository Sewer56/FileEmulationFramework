using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Lib.Utilities;
using FileEmulationFramework.Template;
using FileEmulationFramework.Utilities;
using Reloaded.Mod.Interfaces;

namespace FileEmulationFramework;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class Mod : ModBase, IExports // <= Do not Remove.
{
    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    private Logger _log;

    public Mod(ModContext context)
    {
        var modLoader = context.ModLoader;
        var hooks = context.Hooks;
        var logger = context.Logger;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        // For more information about this template, please see
        // https://reloaded-project.github.io/Reloaded-II/ModTemplate/
        _log = new Logger(logger, _configuration.LogLevel);
        _log.Info("Starting FileEmulationFramework");
        var framework = new EmulationFramework();
        modLoader.AddOrReplaceController<IEmulationFramework>(context.Owner, framework);
        FileAccessServer.Init(_log, NativeFunctions.GetInstance(hooks!), hooks, modLoader.GetDirectoryForModId(_modConfig.ModId));
    }

    #region Standard Overrides

    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _log.Info($"[{_modConfig.ModId}] Config Updated: Applying");
        _log.LogLevel = configuration.LogLevel;
    }
    #endregion

    /// <summary>
    /// All types exported to other mods.
    /// </summary>
    public Type[] GetTypes() => new[] { typeof(IEmulator) };

    /// <summary>
    /// For IExports.
    /// </summary>
#pragma warning disable CS8618
    public Mod() { }
#pragma warning restore CS8618
}