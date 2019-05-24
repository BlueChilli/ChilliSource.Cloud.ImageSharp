using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.Primitives;

namespace ChilliSource.Cloud.ImageSharp
{
    /// <summary>
    /// Allows auto orienting and rotating images.
    /// </summary>
    public class RotateImageSharpProcessor : IImageWebProcessor
    {
        /// <summary>
        /// The command constant for auto orienting.
        /// </summary>
        public const string Rotate = "rotate";

        private static readonly IEnumerable<string> processorCommands = new[] { Rotate };
        
        /// <inheritdoc/>
        public IEnumerable<string> Commands { get; } = processorCommands;

        /// <inheritdoc/>
        public FormattedImage Process(FormattedImage image, ILogger logger, IDictionary<string, string> commands)
        {
            var parser = CommandParser.Instance;
            var degrees = GetRotateDegrees(commands, parser);

            if (degrees != null)
            {
                image.Image.Mutate(context => context.Rotate(degrees.Value));
            }

            return image;
        }

        private static float? GetRotateDegrees(IDictionary<string, string> commands, CommandParser parser)
        {
            var value = commands.GetValueOrDefault(Rotate);
            return string.IsNullOrEmpty(value) ? (float?)null : parser.ParseValue<float>(value);
        }
    }
}