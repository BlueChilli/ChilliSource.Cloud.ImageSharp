using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ChilliSource.Cloud.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace ChilliSource.Cloud.ImageSharp
{
    //Now provided in ImageSharp
    //public class JpegEncoderHttpContextAware : IImageEncoder
    //{
    //    public static int DefaultQuality { get; set; } = 90;

    //    IHttpContextAccessor _httpContextAccessor;
    //    public JpegEncoderHttpContextAware(IHttpContextAccessor httpContextAccessor)
    //    {
    //        _httpContextAccessor = httpContextAccessor;
    //    }

    //    private int? GetQuality()
    //    {
    //        var httpContext = _httpContextAccessor.HttpContext;
    //        if (httpContext == null)
    //            return null;

    //        return httpContext.Items[QualityWebProcessor.ImageQualityKey] as int? ?? DefaultQuality;
    //    }

    //    public Task EncodeAsync<TPixel>(Image<TPixel> image, Stream stream, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
    //    {
    //        Encode(image, stream);

    //        return Task.CompletedTask;
    //    }

    //    public void Encode<TPixel>(Image<TPixel> image, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
    //    {
    //        var encoderWithOptions = new JpegEncoder()
    //        {
    //            Quality = GetQuality(),
    //            Subsample = JpegSubsample.Ratio444
    //        };

    //        encoderWithOptions.Encode(image, stream);
    //    }
    //}
}