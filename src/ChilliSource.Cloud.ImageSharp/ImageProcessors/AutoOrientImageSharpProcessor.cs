using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;

namespace ChilliSource.Cloud.ImageSharp
{
    /// <summary>
    /// Allows auto orienting images.
    /// </summary>
    public class AutoOrientImageSharpProcessor : IImageWebProcessor
    {
        /// <summary>
        /// The command constant for auto orienting.
        /// </summary>
        public const string AutoOrient = "autoorient";

        private static readonly IEnumerable<string> processorCommands = new[] { AutoOrient };

        /// <inheritdoc/>
        public IEnumerable<string> Commands { get; } = processorCommands;

        /// <inheritdoc/>
        public FormattedImage Process(FormattedImage image, ILogger logger, CommandCollection commands, CommandParser parser, CultureInfo culture)
        {
            var autoOrient = GetAutoOrient(commands, parser, culture);

            if (autoOrient)
            {
                image.Image.Mutate(context => context.AutoOrient());
            }

            return image;
        }

        private static bool GetAutoOrient(CommandCollection commands, CommandParser parser, CultureInfo culture)
        {
            return parser.ParseValue<bool>(commands.GetValueOrDefault(AutoOrient), culture);
        }

        public bool RequiresTrueColorPixelFormat(CommandCollection commands, CommandParser parser, CultureInfo culture)
        {
            return false; //TODO check
        }
    }
}