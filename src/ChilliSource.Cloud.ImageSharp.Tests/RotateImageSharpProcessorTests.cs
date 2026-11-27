using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using Xunit;

namespace ChilliSource.Cloud.ImageSharp.Tests;

public class RotateImageSharpProcessorTests
{
    [Fact]
    public void Process_Rotate90_TurnsImageClockwise()
    {
        using var image = TestImages.Split(40, 20);
        var commands = new CommandCollection { { RotateImageSharpProcessor.Rotate, "90" } };

        var result = new RotateImageSharpProcessor().Process(new FormattedImage(image, JpegFormat.Instance), NullLogger.Instance, commands, TestImages.Parser, CultureInfo.InvariantCulture);

        var rotated = (SixLabors.ImageSharp.Image<Rgba32>)result.Image;
        Assert.Equal(20, rotated.Width);
        Assert.Equal(40, rotated.Height);
        Assert.Equal(TestImages.Red, rotated[10, 5]);
        Assert.Equal(TestImages.Blue, rotated[10, 35]);
    }

    [Fact]
    public void Process_NoRotateCommand_LeavesImageAlone()
    {
        using var image = TestImages.Split(40, 20);

        var result = new RotateImageSharpProcessor().Process(new FormattedImage(image, JpegFormat.Instance), NullLogger.Instance, new CommandCollection(), TestImages.Parser, CultureInfo.InvariantCulture);

        Assert.Equal(40, result.Image.Width);
        Assert.Equal(20, result.Image.Height);
    }
}
