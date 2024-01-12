using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using SPD.File.Emulator.Interfaces;
using SPD.File.Emulator.Template;

namespace SPD.File.Emulator;

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
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    private readonly SpdEmulator _emulator;
    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _logger = context.Logger;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        // For more information about this template, please see
        // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

        // If you want to implement e.g. unload support in your mod,
        // and some other neat features, override the methods in ModBase.
        _modLoader.ModLoading += OnModLoading;
        _modLoader.OnModLoaderInitialized += OnModLoaderInitialized;
        var log = new Logger(_logger, _configuration.LogLevel);
        log.Info("Starting SPD.File.Emulator");
        _emulator = new SpdEmulator(log, _configuration.DumpSpd);

        _modLoader.GetController<IEmulationFramework>().TryGetTarget(out var framework);
        framework!.Register(_emulator);

        // Expose API
        _modLoader.AddOrReplaceController<ISpdEmulator>(context.Owner, new SpdEmulatorApi(framework, _emulator, log));
    }

    private void OnModLoaderInitialized()
    {
        _modLoader.ModLoading -= OnModLoading;
        _modLoader.OnModLoaderInitialized -= OnModLoaderInitialized;
    }

    private void OnModLoading(IModV1 mod, IModConfigV1 modConfig) => _emulator.OnModLoading(_modLoader.GetDirectoryForModId(modConfig.ModId));

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion

    /// <inheritdoc/>
    public Type[] GetTypes() => new[] { typeof(ISpdEmulator) };
}