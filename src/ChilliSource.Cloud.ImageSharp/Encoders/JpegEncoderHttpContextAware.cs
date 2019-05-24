using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace ChilliSource.Cloud.ImageSharp
{
    public class JpegEncoderHttpContextAware : IImageEncoder
    {
        public static int DefaultQuality { get; set; } = 90;

        IHttpContextAccessor _httpContextAccessor;
        public JpegEncoderHttpContextAware(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Encode<TPixel>(Image<TPixel> image, Stream stream) where TPixel : struct, IPixel<TPixel>
        {
            var encoderWithOptions = new JpegEncoder()
            {
                Quality = GetQuality(),
                Subsample = JpegSubsample.Ratio444
            };

            encoderWithOptions.Encode(image, stream);
        }

        private int? GetQuality()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return null;

            return httpContext.Items[QualityWebProcessor.ImageQualityKey] as int? ?? DefaultQuality;
        }
    }
}