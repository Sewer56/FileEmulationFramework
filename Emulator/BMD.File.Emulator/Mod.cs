using AtlusScriptLibrary.Common.Libraries;
using AtlusScriptLibrary.Common.Text.Encodings;
using BMD.File.Emulator.Configuration;
using BMD.File.Emulator.Interfaces;
using BMD.File.Emulator.Template;
using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using System.Diagnostics;
using System.Text;

namespace BMD.File.Emulator;

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
    private BmdEmulator _bmdEmulator;
    private Game _game;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        var mainModule = Process.GetCurrentProcess().MainModule;
        var fileName = Path.GetFileName(mainModule.FileName);
        if (fileName.StartsWith("p5r", StringComparison.OrdinalIgnoreCase))
            _game = Game.P5R;
        else if (fileName.StartsWith("p4g", StringComparison.OrdinalIgnoreCase))
            _game = Game.P4G;
        else if (fileName.StartsWith("p3p", StringComparison.OrdinalIgnoreCase))
            _game = Game.P3P;

        // Setup script compiler stuff
        LibraryLookup.SetLibraryPath($"{_modLoader.GetDirectoryForModId(_modConfig.ModId)}\\Libraries");
        AtlusEncoding.SetCharsetDirectory($"{_modLoader.GetDirectoryForModId(_modConfig.ModId)}\\Charsets");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Needed for shift_jis encoding to be available

        _modLoader.ModLoading += OnModLoading;
        _modLoader.OnModLoaderInitialized += OnModLoaderInitialized;
        _log = new Logger(_logger, _configuration.LogLevel);
        _log.Info("Starting BMD.File.Emulator");
        _bmdEmulator = new BmdEmulator(_log, _configuration.DumpBmd, _game);

        _modLoader.GetController<IEmulationFramework>().TryGetTarget(out var framework);
        framework!.Register(_bmdEmulator);

        // Expose API
        _modLoader.AddOrReplaceController<IBmdEmulator>(context.Owner, new BmdEmulatorApi(framework, _bmdEmulator, _log));

    }

    private void OnModLoaderInitialized()
    {
        _modLoader.ModLoading -= OnModLoading;
        _modLoader.OnModLoaderInitialized -= OnModLoaderInitialized;
    }

    private void OnModLoading(IModV1 mod, IModConfigV1 modConfig) => _bmdEmulator.OnModLoading(_modLoader.GetDirectoryForModId(modConfig.ModId));


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
    public Type[] GetTypes() => new[] { typeof(IBmdEmulator) };

    public enum Game
    {
        P4G,
        P5R,
        P3P
    }
}