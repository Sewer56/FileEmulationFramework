using System.Runtime.CompilerServices;
using AWB.Stream.Emulator.Interfaces;
using AWB.Stream.Emulator.Template;
using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Memory.Sigscan.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

[module: SkipLocalsInit]
namespace AWB.Stream.Emulator;

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

    private Logger _log;
    private AwbEmulator _awbEmulator;

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
        _log = new Logger(_logger, _configuration.LogLevel);
        _log.Info("Starting AWB.Stream.Emulator");
        _awbEmulator = new AwbEmulator(_log, _configuration.DumpAwb);

        _modLoader.GetController<IEmulationFramework>().TryGetTarget(out var framework);
        framework!.Register(_awbEmulator);
        
        // Create ACB & BDX Overwriters
        _modLoader.GetController<IScannerFactory>().TryGetTarget(out var factory);
        var acbEmulator = new AcbPatcherEmulator(_awbEmulator, _log, factory!, _configuration.CheckAcbExtension, _configuration.DumpAcb);
        framework.Register(acbEmulator);
        
        // Expose API
        _modLoader.AddOrReplaceController<IAwbEmulator>(context.Owner, new AwbEmulatorApi(framework, _awbEmulator, acbEmulator, _log));
    }
    
    private void OnModLoaderInitialized()
    {
        _modLoader.ModLoading -= OnModLoading;
        _modLoader.OnModLoaderInitialized -= OnModLoaderInitialized;
    }

    private void OnModLoading(IModV1 mod, IModConfigV1 modConfig) => _awbEmulator.OnModLoading(_modLoader.GetDirectoryForModId(modConfig.ModId));

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        _log.LogLevel = configuration.LogLevel;
        _configuration.DumpAwb = configuration.DumpAwb;
        _configuration.CheckAcbExtension = configuration.CheckAcbExtension;
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion

    /// <inheritdoc/>
    public Type[] GetTypes() => new[] { typeof(IAwbEmulator) };
}