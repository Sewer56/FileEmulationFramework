using System.Diagnostics;
using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Lib.Utilities;
using FileEmulationFramework.Template;
using FileEmulationFramework.Utilities;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

namespace FileEmulationFramework;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class Mod : ModBase, IExports // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

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
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        // For more information about this template, please see
        // https://reloaded-project.github.io/Reloaded-II/ModTemplate/
        _log = new Logger(_logger, _configuration.LogLevel);
        _log.Info("Starting FileEmulationFramework");
        var framework = new EmulationFramework();
        _modLoader.AddOrReplaceController<IEmulationFramework>(context.Owner, framework);
        FileAccessServer.Init(_log, NativeFunctions.GetInstance(_hooks!), _hooks, framework);
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