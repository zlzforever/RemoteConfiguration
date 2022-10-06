using System;
using Microsoft.Extensions.Configuration;
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
}