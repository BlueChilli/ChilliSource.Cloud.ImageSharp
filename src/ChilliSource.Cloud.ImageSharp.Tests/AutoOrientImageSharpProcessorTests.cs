using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using Xunit;

namespace ChilliSource.Cloud.ImageSharp.Tests;

public class AutoOrientImageSharpProcessorTests
{
    [Fact]
    public void Process_AutoOrient_StandsSidewaysImageUpright()
    {
        using var image = TestImages.Sideways(40, 20);
        var commands = new CommandCollection { { AutoOrientImageSharpProcessor.AutoOrient, "true" } };

        var result = new AutoOrientImageSharpProcessor().Process(new FormattedImage(image, JpegFormat.Instance), NullLogger.Instance, commands, TestImages.Parser, CultureInfo.InvariantCulture);

        var upright = (SixLabors.ImageSharp.Image<Rgba32>)result.Image;
        Assert.Equal(20, upright.Width);
        Assert.Equal(40, upright.Height);
        Assert.Equal(TestImages.Red, upright[10, 5]);
    }

    [Fact]
    public void Process_NoAutoOrientCommand_LeavesSidewaysImageAlone()
    {
        using var image = TestImages.Sideways(40, 20);

        var result = new AutoOrientImageSharpProcessor().Process(new FormattedImage(image, JpegFormat.Instance), NullLogger.Instance, new CommandCollection(), TestImages.Parser, CultureInfo.InvariantCulture);

        Assert.Equal(40, result.Image.Width);
        Assert.Equal(20, result.Image.Height);
    }
}
