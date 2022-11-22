using csharp_prs_interfaces;
using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Lib.Utilities;
using Heroes.SDK;
using ONE.Heroes.Stream.Emulator.One;
using ONE.Heroes.Stream.Emulator.Template;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

namespace ONE.Heroes.Stream.Emulator;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class Mod : ModBase // <= Do not Remove.
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

    private OneEmulator _emulator;

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
        log.Info("Starting ONE.Heroes.Stream.Emulator");
        _emulator = new OneEmulator(log);

        _modLoader.GetController<IEmulationFramework>().TryGetTarget(out var framework);
        _modLoader.GetController<IPrsInstance>().TryGetTarget(out var prsCompressor);

        SDK.Init(null, prsCompressor);
        CompressedFilesCache.Init(prsCompressor!);
        framework!.Register(_emulator);
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

#pragma warning disable CS8618
    public Mod() { }
#pragma warning restore CS8618
}