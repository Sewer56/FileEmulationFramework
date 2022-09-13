using System.IO;
using FileEmulationFramework.Lib;
using Xunit;

namespace FileEmulationFramework.Tests;

public class RouteTests
{
    [Fact]
    public void Route_GetRoute()
    {
        string expectedRoute = NormalizeSlashes("English/File.test");

        // Arrange
        var emuPath = Path.GetFullPath("Emulators/Test");
        var fullRoute = Path.GetFullPath(Path.Combine(emuPath, expectedRoute));

        // Act
        var route = Route.GetRoute(emuPath, fullRoute);
        
        // Assert
        Assert.Equal(expectedRoute, route);
    }

    [Fact]
    public void Route_Merge()
    {
        string currentRoute = NormalizeSlashes("Data/BGM/VOICE.AFS");
        string newFilePath = NormalizeSlashes("FEmulator/AFS/VOICE.AFS/00000.adx");
        string expected = NormalizeSlashes("Data/BGM/VOICE.AFS/00000.adx");

        // Arrange
        var route = new Route(currentRoute);

        // Act
        route = route.Merge(newFilePath);

        // Assert
        Assert.Equal(expected, route.FullPath);
    }

    [Fact]
    public void Route_Merge_WithMultipleFolders()
    {
        string currentRoute = NormalizeSlashes("Data/BGM/VOICE.AFS");
        string newFilePath = NormalizeSlashes("FEmulator/AFS/VOICE.AFS/NestedFolderA/NestedFolderB/00000.adx");
        string expected = NormalizeSlashes("Data/BGM/VOICE.AFS/NestedFolderA/NestedFolderB/00000.adx");

        // Arrange
        var route = new Route(currentRoute);

        // Act
        route = route.Merge(newFilePath);

        // Assert
        Assert.Equal(expected, route.FullPath);
    }

    [Fact]
    public void Route_Matches()
    {
        string expectedRoute  = NormalizeSlashes($"English/File.test");

        // Arrange
        var emuPath = Path.GetFullPath("Emulators/Test");
        var fullRoute = Path.GetFullPath(Path.Combine(emuPath, expectedRoute));

        // Act
        var route = new Route(fullRoute);
        
        // Assert
        Assert.True(route.Matches(expectedRoute));
    }

    private string NormalizeSlashes(string path)
    {
        return path.Replace('/', Path.DirectorySeparatorChar);
    }
}