using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Resolvers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.ImageSharp
{
    internal class CloudStorageImageResolver : IImageResolver
    {
        IRemoteStorage _storage;
        string _fileName;
        IFileStorageMetadataResponse _metadata;

        public CloudStorageImageResolver(IRemoteStorage storage, string fileName, IFileStorageMetadataResponse metadata)
        {
            _storage = storage;
            _fileName = fileName;
            _metadata = metadata;
        }

        public Task<ImageMetaData> GetMetaDataAsync()
        {
            CacheControlHeaderValue cacheControl = null;
            CacheControlHeaderValue.TryParse(_metadata.CacheControl, out cacheControl);

            var metadata = new ImageMetaData(
                _metadata.LastModifiedUtc,
                _metadata.ContentType,
                cacheControl?.MaxAge ?? TimeSpan.MinValue
            );

            return Task.FromResult<ImageMetaData>(metadata);
        }

        public async Task<Stream> OpenReadAsync()
        {
            var response = await _storage.GetContentAsync(_fileName, CancellationToken.None)
                                .IgnoreContext();
            if (response == null)
                throw new ApplicationException("CloudStorageImageResolver.OpenReadAsync failed to find file.");

            return response.Stream;
        }
    }
}