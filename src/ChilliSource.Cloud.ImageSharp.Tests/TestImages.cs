using System.IO;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Xunit;

namespace ChilliSource.Cloud.ImageSharp.Tests;

internal static class TestImages
{
    // The parser ImageSharp.Web registers, so processors see the same converters as in a real request.
    public static readonly CommandParser Parser = new ServiceCollection().AddImageSharp().Services
        .BuildServiceProvider()
        .GetRequiredService<CommandParser>();

    public static readonly Rgba32 Red = new(255, 0, 0);
    public static readonly Rgba32 Blue = new(0, 0, 255);
    public static readonly Rgba32 White = new(255, 255, 255);

    /// <summary>Left half red, right half blue, so a test can tell which way an image was turned.</summary>
    public static Image<Rgba32> Split(int width, int height)
    {
        var image = new Image<Rgba32>(width, height);
        image.ProcessPixelRows(rows =>
        {
            for (var y = 0; y < rows.Height; y++)
            {
                var row = rows.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++) row[x] = x < width / 2 ? Red : Blue;
            }
        });
        return image;
    }

    /// <summary>A <see cref="Split"/> image stored on its side, as a phone camera saves a portrait photo.</summary>
    public static Image<Rgba32> Sideways(int width, int height)
    {
        var image = Split(width, height);
        image.Metadata.ExifProfile = new ExifProfile();
        image.Metadata.ExifProfile.SetValue(ExifTag.Orientation, (ushort)6); // display rotated 90° clockwise
        return image;
    }

    public static byte[] Jpeg(Image image)
    {
        using var stream = new MemoryStream();
        image.SaveAsJpeg(stream);
        return stream.ToArray();
    }

    /// <summary>Within JPEG's lossy tolerance of the expected colour.</summary>
    public static void AssertColour(Rgba32 expected, Rgba32 actual)
    {
        const int tolerance = 40;
        Assert.True(
            System.Math.Abs(expected.R - actual.R) <= tolerance
            && System.Math.Abs(expected.G - actual.G) <= tolerance
            && System.Math.Abs(expected.B - actual.B) <= tolerance,
            $"Expected about {expected} but was {actual}");
    }
}
