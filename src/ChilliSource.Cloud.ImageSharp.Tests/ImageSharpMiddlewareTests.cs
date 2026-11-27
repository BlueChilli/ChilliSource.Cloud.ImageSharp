using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ChilliSource.Cloud.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Xunit;

namespace ChilliSource.Cloud.ImageSharp.Tests;

/// <summary>
/// The real ImageSharp.Web middleware over fake cloud storage, set up the way the apps using this package do, so a
/// package upgrade that changes what a resize URL returns fails here.
/// </summary>
public class ImageSharpMiddlewareTests : IClassFixture<ImageSharpMiddlewareTests.Server>
{
    private readonly HttpClient _client;
    private readonly Server _server;

    public ImageSharpMiddlewareTests(Server server)
    {
        _server = server;
        _client = server.Client;
    }

    [Fact]
    public async Task Get_PadToEmailHeaderSize_FillsWithColour()
    {
        using var image = await GetImage("/S3/Company/logo.jpg?width=1300&height=168&rmode=pad&rcolor=FFF");

        Assert.Equal(1300, image.Width);
        Assert.Equal(168, image.Height);
        TestImages.AssertColour(TestImages.White, image[5, 84]);
        TestImages.AssertColour(TestImages.Red, image[600, 84]);
        TestImages.AssertColour(TestImages.Blue, image[700, 84]);
    }

    [Fact]
    public async Task Get_WidthOnly_KeepsAspectRatio()
    {
        using var image = await GetImage("/S3/Company/logo.jpg?width=48");

        Assert.Equal(48, image.Width);
        Assert.Equal(24, image.Height);
    }

    [Fact]
    public async Task Get_Rotate90_TurnsImageClockwise()
    {
        using var image = await GetImage("/S3/Company/logo.jpg?rotate=90");

        Assert.Equal(200, image.Width);
        Assert.Equal(400, image.Height);
        TestImages.AssertColour(TestImages.Red, image[100, 50]);
        TestImages.AssertColour(TestImages.Blue, image[100, 350]);
    }

    [Fact]
    public async Task Get_AutoOrient_StandsSidewaysPhotoUpright()
    {
        using var image = await GetImage("/S3/Company/sideways.jpg?autoorient=true");

        Assert.Equal(200, image.Width);
        Assert.Equal(400, image.Height);
        TestImages.AssertColour(TestImages.Red, image[100, 50]);
    }

    [Fact]
    public async Task Get_Resize_ReturnsJpegWithCacheHeaders()
    {
        var response = await _client.GetAsync("/S3/Company/logo.jpg?width=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/jpeg", response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(response.Headers.ETag);
    }

    // Requests without commands fall through, so the app's own middleware can stream the original file.
    [Fact]
    public async Task Get_NoCommands_PassesToNextMiddleware()
    {
        var response = await _client.GetAsync("/S3/Company/logo.jpg");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_FileNotInStorage_PassesToNextMiddleware()
    {
        var response = await _client.GetAsync("/S3/Company/missing.jpg?width=48");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<Image<Rgba32>> GetImage(string url)
    {
        var response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return Image.Load<Rgba32>(await response.Content.ReadAsByteArrayAsync());
    }

    public sealed class Server : IAsyncLifetime
    {
        private readonly string _cacheRoot = Path.Combine(Path.GetTempPath(), "cs-imagesharp-tests", Guid.NewGuid().ToString("N"));
        private readonly Dictionary<string, byte[]> _files = new()
        {
            ["Company/logo.jpg"] = TestImages.Jpeg(TestImages.Split(400, 200)),
            ["Company/sideways.jpg"] = TestImages.Jpeg(TestImages.Sideways(400, 200)),
        };
        private IHost _host;

        public HttpClient Client { get; private set; }

        public async Task InitializeAsync()
        {
            _host = await new HostBuilder()
                .ConfigureWebHost(web => web
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton(Storage());
                        services.Configure<CloudStorageImageProviderOptions>(o => o.UrlPrefix = "~/S3");
                        services.AddImageSharp()
                            .Configure<PhysicalFileSystemCacheOptions>(o => o.CacheRootPath = _cacheRoot)
                            .ClearProviders()
                            .AddProvider<CloudStorageImageProvider>()
                            .AddProcessor<AutoOrientImageSharpProcessor>()
                            .AddProcessor<RotateImageSharpProcessor>();
                    })
                    .Configure(app => app.UseImageSharp()))
                .StartAsync();

            Client = _host.GetTestClient();
        }

        public async Task DisposeAsync()
        {
            await _host.StopAsync();
            _host.Dispose();
            if (Directory.Exists(_cacheRoot)) Directory.Delete(_cacheRoot, recursive: true);
        }

        // Throws for a missing file, as the S3 storage does.
        private IRemoteStorage Storage()
        {
            var storage = new Mock<IRemoteStorage>();
            storage.Setup(x => x.GetMetadataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string name, CancellationToken _) => _files.TryGetValue(name, out var data)
                    ? new FileStorageMetadataResponse { FileName = name, ContentLength = data.Length, ContentType = "image/jpeg", LastModifiedUtc = DateTime.UtcNow }
                    : throw new FileNotFoundException(name));
            storage.Setup(x => x.GetContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string name, CancellationToken _) => FileStorageResponse.Create(name, _files[name].Length, "image/jpeg", new MemoryStream(_files[name])));
            return storage.Object;
        }
    }
}
