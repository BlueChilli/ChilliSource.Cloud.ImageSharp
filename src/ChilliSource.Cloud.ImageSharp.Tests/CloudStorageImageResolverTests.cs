using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ChilliSource.Cloud.Core;
using Moq;
using Xunit;

namespace ChilliSource.Cloud.ImageSharp.Tests;

public class CloudStorageImageResolverTests
{
    private static readonly DateTime Modified = new(2026, 11, 23, 0, 2, 8, DateTimeKind.Utc);

    [Fact]
    public async Task GetMetaDataAsync_CopiesStorageMetadata()
    {
        var metadata = new FileStorageMetadataResponse { LastModifiedUtc = Modified, ContentLength = 1234, CacheControl = "public, max-age=3600" };

        var result = await new CloudStorageImageResolver(Mock.Of<IRemoteStorage>(), "logo.jpg", metadata).GetMetaDataAsync();

        Assert.Equal(Modified, result.LastWriteTimeUtc);
        Assert.Equal(1234, result.ContentLength);
        Assert.Equal(TimeSpan.FromHours(1), result.CacheControlMaxAge);
    }

    [Fact]
    public async Task GetMetaDataAsync_NoCacheControl_MaxAgeIsMinValue()
    {
        var metadata = new FileStorageMetadataResponse { LastModifiedUtc = Modified, ContentLength = 1234 };

        var result = await new CloudStorageImageResolver(Mock.Of<IRemoteStorage>(), "logo.jpg", metadata).GetMetaDataAsync();

        Assert.Equal(TimeSpan.MinValue, result.CacheControlMaxAge);
    }

    [Fact]
    public async Task OpenReadAsync_ReturnsStoredStream()
    {
        var stream = new MemoryStream([1, 2, 3]);
        var storage = new Mock<IRemoteStorage>();
        storage.Setup(x => x.GetContentAsync("logo.jpg", It.IsAny<CancellationToken>()))
            .ReturnsAsync(FileStorageResponse.Create("logo.jpg", 3, "image/jpeg", stream));

        var result = await new CloudStorageImageResolver(storage.Object, "logo.jpg", new FileStorageMetadataResponse()).OpenReadAsync();

        Assert.Same(stream, result);
    }

    [Fact]
    public async Task OpenReadAsync_FileGone_Throws()
    {
        var storage = new Mock<IRemoteStorage>();
        storage.Setup(x => x.GetContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileStorageResponse)null);

        await Assert.ThrowsAsync<ApplicationException>(() => new CloudStorageImageResolver(storage.Object, "logo.jpg", new FileStorageMetadataResponse()).OpenReadAsync());
    }
}
