using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.ImageSharp
{
    public class CloudStorageImageProvider : IImageProvider
    {
        CloudStorageImageProviderOptions _options;
        PathString _pathPrefix;
        IRemoteStorage _remoteStorage;

        public CloudStorageImageProvider(IOptions<CloudStorageImageProviderOptions> optionsAcessor, IRemoteStorage remoteStorage)
        {
            _options = optionsAcessor.Value;
            _remoteStorage = remoteStorage;

            if (_options.UrlPrefix == null || !_options.UrlPrefix.StartsWith("~"))
            {
                throw new ArgumentException("UrlPrefix is null or is not a relative path (~).");
            }

            if (_options.UrlPrefix.StartsWith("~"))
            {
                _pathPrefix = new PathString(_options.UrlPrefix.TrimStart('~'));
            }
            else
            {
                _pathPrefix = new PathString(_options.UrlPrefix);
            }

            this.Match = this.IsAProviderMatch;
        }

        public Func<HttpContext, bool> Match { get; set; }

        public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        protected bool IsAProviderMatch(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments(_pathPrefix);
        }

        private string GetRelativeFileName(HttpContext context)
        {
            return context.Request.Path.Value.Substring(_pathPrefix.Value.Length).TrimStart('/');
        }

        public bool IsValidRequest(HttpContext context)
        {
            return true;
        }

        public async Task<IImageResolver> GetAsync(HttpContext context)
        {
            var fileName = GetRelativeFileName(context);
            if (String.IsNullOrEmpty(fileName))
                return null;

            IFileStorageMetadataResponse metadata = null;
            try
            {
                metadata = await _remoteStorage.GetMetadataAsync(fileName, context.RequestAborted)
                                     .IgnoreContext();
            }
            catch { }

            if (metadata != null)
            {
                return new CloudStorageImageResolver(_remoteStorage, fileName, metadata);
            }

            return null;
        }
    }
}