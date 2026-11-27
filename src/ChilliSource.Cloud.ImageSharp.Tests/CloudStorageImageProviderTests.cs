using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ChilliSource.Cloud.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ChilliSource.Cloud.ImageSharp.Tests;

public class CloudStorageImageProviderTests
{
    private static CloudStorageImageProvider Provider(IRemoteStorage storage = null, string urlPrefix = "~/S3")
    {
        return new CloudStorageImageProvider(Options.Create(new CloudStorageImageProviderOptions { UrlPrefix = urlPrefix }), storage ?? Mock.Of<IRemoteStorage>());
    }

    private static HttpContext Request(string path)
    {
        return new DefaultHttpContext { Request = { Path = path } };
    }

    [Theory]
    [InlineData(null)]
    [InlineData("/S3")]
    public void Constructor_PrefixNotAppRelative_Throws(string urlPrefix)
    {
        Assert.Throws<ArgumentException>(() => Provider(urlPrefix: urlPrefix));
    }

    [Theory]
    [InlineData("/S3/Company/logo.jpg", true)]
    [InlineData("/s3/Company/logo.jpg", true)]
    [InlineData("/S3x/logo.jpg", false)]
    [InlineData("/Images/logo.jpg", false)]
    public void Match_PathUnderPrefix(string path, bool expected)
    {
        Assert.Equal(expected, Provider().Match(Request(path)));
    }

    [Fact]
    public async Task GetAsync_StoredFile_ReturnsResolver()
    {
        var storage = new Mock<IRemoteStorage>();
        storage.Setup(x => x.GetMetadataAsync("Company/logo.jpg", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileStorageMetadataResponse { FileName = "Company/logo.jpg", ContentLength = 10 });

        var resolver = await Provider(storage.Object).GetAsync(Request("/S3/Company/logo.jpg"));

        Assert.NotNull(resolver);
    }

    [Fact]
    public async Task GetAsync_StorageThrows_ReturnsNull()
    {
        var storage = new Mock<IRemoteStorage>();
        storage.Setup(x => x.GetMetadataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException());

        var resolver = await Provider(storage.Object).GetAsync(Request("/S3/Company/missing.jpg"));

        Assert.Null(resolver);
    }

    [Fact]
    public async Task GetAsync_NoFileName_ReturnsNull()
    {
        var resolver = await Provider().GetAsync(Request("/S3/"));

        Assert.Null(resolver);
    }
}
