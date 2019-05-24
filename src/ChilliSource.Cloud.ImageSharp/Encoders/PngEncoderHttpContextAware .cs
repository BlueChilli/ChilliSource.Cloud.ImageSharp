using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace ChilliSource.Cloud.ImageSharp
{
    public class PngEncoderHttpContextAware : IImageEncoder
    {
        IHttpContextAccessor _httpContextAccessor;
        public PngEncoderHttpContextAware(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Encode<TPixel>(Image<TPixel> image, Stream stream) where TPixel : struct, IPixel<TPixel>
        {
            var encoderWithOptions = new PngEncoder()
            {
                CompressionLevel = 6
            };

            encoderWithOptions.Encode(image, stream);
        }
    }
}