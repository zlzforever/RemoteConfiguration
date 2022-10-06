using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RemoteConfiguration.Abstractions;
using RemoteConfiguration.Json;
using RemoteConfiguration.Json.Aliyun;
using Xunit;

namespace RemoteConfiguration.Tests;

public class RemoteConfigurationTests
{
    [Fact]
    public void Default()
    {
        var builder = new ConfigurationBuilder();
        builder.AddRemoteJsonFile(
            "https://raw.githubusercontent.com/zlzforever/RemoteConfiguration/main/appsettings.json");
        var configuration = builder.Build();
        Assert.Equal("value1", configuration["key1"]);
        Assert.Equal("value3", configuration["key2:key3"]);
    }

    [Fact]
    public void AliyunOSS()
    {
        var builder = new ConfigurationBuilder();
        builder.AddAliyunJsonFile(source =>
        {
            source.Endpoint = "oss-cn-shanghai.aliyuncs.com";
            source.BucketName = "forestrycloud-test";
            source.AccessKeyId = Environment.GetEnvironmentVariable("Aliyun_AccessKeyId");
            source.AccessKeySecret = Environment.GetEnvironmentVariable("Aliyun_AccessKeySecret");
            source.Key = "appsettings.json";
        });
        var configuration = builder.Build();
        Assert.Equal("value1", configuration["key1"]);
        Assert.Equal("value3", configuration["key2:key3"]);
    }

    [Fact]
    public void DefaultOptions()
    {
        var builder = new ConfigurationBuilder();
        builder.AddAliyunJsonFile(source =>
        {
            source.Endpoint = "oss-cn-shanghai.aliyuncs.com";
            source.BucketName = "forestrycloud-test";
            source.AccessKeyId = Environment.GetEnvironmentVariable("Aliyun_AccessKeyId");
            source.AccessKeySecret = Environment.GetEnvironmentVariable("Aliyun_AccessKeySecret");
            source.Key = "appsettings.json";
        });
        var configuration = builder.Build();

        var service = new ServiceCollection();
        service.Configure<MyOptions>(configuration);

        var provider = service.BuildServiceProvider();
        var myOptions = provider.GetRequiredService<IOptions<MyOptions>>();
        Assert.Equal("value1", myOptions.Value.Key1);
    }

    [Fact]
    public async Task MonitorOptions()
    {
        var config = await File.ReadAllTextAsync("appsettings_monitor.json");

        PollingChangeToken.PollingInterval = 10;

        await PutAsync(Environment.GetEnvironmentVariable("Aliyun_AccessKeyId"),
            Environment.GetEnvironmentVariable("Aliyun_AccessKeySecret"),
            "appsettings_monitor.json", config);

        var builder = new ConfigurationBuilder();
        builder.AddAliyunJsonFile(source =>
        {
            source.Endpoint = "oss-cn-shanghai.aliyuncs.com";
            source.BucketName = "forestrycloud-test";
            source.AccessKeyId = Environment.GetEnvironmentVariable("Aliyun_AccessKeyId");
            source.AccessKeySecret = Environment.GetEnvironmentVariable("Aliyun_AccessKeySecret");
            source.Key = "appsettings_monitor.json";
        });
        var configuration = builder.Build();

        var service = new ServiceCollection();
        service.Configure<MyOptions>(configuration);

        var provider = service.BuildServiceProvider();
        var myOptions1 = provider.GetRequiredService<IOptionsMonitor<MyOptions>>();
        Assert.Equal("value1", myOptions1.CurrentValue.Key1);

        await PutAsync(Environment.GetEnvironmentVariable("Aliyun_AccessKeyId"),
            Environment.GetEnvironmentVariable("Aliyun_AccessKeySecret"),
            "appsettings_monitor.json", config.Replace("value1", "value4"));

        await Task.Delay(15000);
        Assert.Equal("value4", myOptions1.CurrentValue.Key1);
    }

    public class MyOptions
    {
        public string Key1 { get; set; }
    }

    private async Task PutAsync(string keyId, string secret, string key, string text)
    {
        using var request = new HttpRequestMessage();
        request.Method = HttpMethod.Put;
        request.RequestUri =
            new Uri($"http://forestrycloud-test.oss-cn-shanghai.aliyuncs.com/{key}");

        var now = DateTime.UtcNow.ToString("r");
        var stringToSign = $"PUT\n\napplication/octet-stream\n{now}\n/forestrycloud-test/{key}";
        using var algorithm = new HMACSHA1();
        algorithm.Key = Encoding.UTF8.GetBytes(secret);
        var signature = Convert.ToBase64String(
            algorithm.ComputeHash(Encoding.UTF8.GetBytes(stringToSign.ToCharArray())));
        request.Headers.Add("Host", $"forestrycloud-test.oss-cn-shanghai.aliyuncs.com");
        request.Headers.Add("Date", now);
        request.Headers.Add("Authorization", "OSS " + keyId + ":" + signature);
        using var content = new ByteArrayContent(Encoding.UTF8.GetBytes(text));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        request.Content = content;
        var result = await new HttpClient().SendAsync(request);
        result.EnsureSuccessStatusCode();
    }
}