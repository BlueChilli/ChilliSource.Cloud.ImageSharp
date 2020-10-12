using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace ChilliSource.Cloud.ImageSharp
{
    //Adds no function over default imagesharp implementation?
    //public class PngEncoderHttpContextAware : IImageEncoder
    //{
    //    IHttpContextAccessor _httpContextAccessor;
    //    public PngEncoderHttpContextAware(IHttpContextAccessor httpContextAccessor)
    //    {
    //        _httpContextAccessor = httpContextAccessor;
    //    }

    //    public void Encode<TPixel>(Image<TPixel> image, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
    //    {
    //        var encoderWithOptions = new PngEncoder()
    //        {
    //            CompressionLevel = PngCompressionLevel.DefaultCompression
    //        };

    //        encoderWithOptions.Encode(image, stream);
    //    }


    //    public Task EncodeAsync<TPixel>(Image<TPixel> image, Stream stream, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
    //    {
    //        Encode(image, stream);

    //        return Task.CompletedTask;
    //    }
    //}
}