using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.Primitives;

namespace ChilliSource.Cloud.ImageSharp
{
    /// <summary>
    /// Allows changing the image quality. 
    /// This processor requires a JpegEncoderHttpContextAware instance needs to be registered via ImageSharpMiddlewareOptions.Configuration.ImageFormatsManager.SetEncoder(JpegFormat.Instance, ...)
    /// </summary>
    public class QualityWebProcessor : IImageWebProcessor
    {
        public const string ImageQualityKey = "_2C0F57C2463241EFB2441DA3F4BE5993_Quality";

        /// <summary>
        /// The command constant for quality.
        /// </summary>
        public const string Quality = "quality";

        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// The reusable collection of commands.
        /// </summary>
        private static readonly IEnumerable<string> processorCommands = new[] { Quality };

        /// <inheritdoc/>
        public IEnumerable<string> Commands { get; } = processorCommands;        

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatWebProcessor"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The http context accessor.</param>
        public QualityWebProcessor(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }        

        private HttpContext GetHttpContext()
        {
            return this.httpContextAccessor.HttpContext ?? throw new ApplicationException("HttpContext is null");
        }

        /// <inheritdoc/>
        public FormattedImage Process(FormattedImage image, ILogger logger, IDictionary<string, string> commands)
        {
            var parser = CommandParser.Instance;
            var quality = GetQuality(commands, parser);

            if (quality != null)
            {
                GetHttpContext().Items[ImageQualityKey] = GetQuality(commands, parser);
            }

            return image;
        }

        private static int? GetQuality(IDictionary<string, string> commands, CommandParser parser)
        {
            var value = commands.GetValueOrDefault(Quality);
            return String.IsNullOrEmpty(value) ? (int?)null : parser.ParseValue<int>(value);
        }
    }
}