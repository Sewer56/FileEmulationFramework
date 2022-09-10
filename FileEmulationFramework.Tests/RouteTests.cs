using System.IO;
using FileEmulationFramework.Lib;
using Xunit;

namespace FileEmulationFramework.Tests;

public class RouteTests
{
    [Fact]
    public void Route_GetRoute()
    {
        string expectedRoute = $"English{Path.DirectorySeparatorChar}File.test";

        // Arrange
        var emuPath = Path.GetFullPath("Emulators/Test");
        var fullRoute = Path.GetFullPath(Path.Combine(emuPath, expectedRoute));

        // Act
        var route = Route.GetRoute(emuPath, fullRoute);
        
        // Assert
        Assert.Equal(expectedRoute, route);
    }

    [Fact]
    public void Route_Append()
    {
        string expectedRoute  = $"English{Path.DirectorySeparatorChar}File.test";
        string childRoutePath = $"Child.test";

        // Arrange
        var emuPath   = Path.GetFullPath("Emulators/Test");
        var fullRoute = Path.GetFullPath(Path.Combine(emuPath, expectedRoute));

        // Act
        var route = new Route(fullRoute);
        route = route.Append(childRoutePath);
        
        // Assert
        Assert.Equal($"{fullRoute}{Path.DirectorySeparatorChar}{childRoutePath}", route.FullPath);
    }

    [Fact]
    public void Route_Matches()
    {
        string expectedRoute  = $"English{Path.DirectorySeparatorChar}File.test";

        // Arrange
        var emuPath = Path.GetFullPath("Emulators/Test");
        var fullRoute = Path.GetFullPath(Path.Combine(emuPath, expectedRoute));

        // Act
        var route = new Route(fullRoute);
        
        // Assert
        Assert.True(route.Matches(expectedRoute));
    }
}