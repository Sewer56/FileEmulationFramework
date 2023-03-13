// ReSharper disable InconsistentNaming
namespace FileEmulationFramework.Tests;

internal static class Assets
{
    internal static string StreamTestFile = "stream-read-test.bin";
    internal static string StreamTestFileReverse = "stream-read-test-reverse.bin";

    internal static string AssetsFolder = "Assets";
    internal static string AssetsSoundFolder = $"{AssetsFolder}/Sound";

    internal static string AssetMiniTV = $"{AssetsFolder}/Textures/init_free.bin/init/loading.arc/mini_tv.tmx";
    internal static string AssetNestedArgSiren = $"{AssetsSoundFolder}/Sound.pak/0_arg_siren.flac";
    internal static string AssetNestedArgHeehoo = $"{AssetsSoundFolder}/Sound.pak/1_arg_heehoo.flac";
    internal static string AssetArgSiren = $"{AssetsSoundFolder}/0_arg_siren.flac";
    internal static string AssetArgHeehoo = $"{AssetsSoundFolder}/1_arg_heehoo.flac";
    internal static string AssetArgHoohaa = $"{AssetsSoundFolder}/2_arg_hoohaa.flac";
    internal static string AssetArgMario = $"{AssetsSoundFolder}/6_arg_mario_6.flac";
    internal static string AssetArgPAKSiren = $"{AssetsSoundFolder}/arg_mario.flac";

    internal static string EmulatorsDirectory = "Emulators";
    internal static string AwbEmulatorSampleFile = "Emulators/AWB/arg.awb";
    internal static string AfsEmulatorSampleFile = "Emulators/AFS/original.afs";
    internal static string Pakinit_free = "Emulators/PAK/init_free.bin";
    internal static string PakV1EmulatorSampleFile = "Emulators/PAK/originalV1.pak";
    internal static string PakV2EmulatorSampleFile = "Emulators/PAK/originalV2.pak";
    internal static string PakV2NESTEDEmulatorSampleFile = "Emulators/PAK/originalV2NESTED.pak";
    internal static string PakV2BEEmulatorSampleFile = "Emulators/PAK/originalV2BE.pak";
    internal static string PakV3EmulatorSampleFile = "Emulators/PAK/originalV3.pak";
    internal static string PakV3BEEmulatorSampleFile = "Emulators/PAK/originalV3BE.pak";
    internal static string OneEmulatorSampleFile = "Emulators/ONE/arg_swag.one";
}
