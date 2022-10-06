using System;
using Aliyun.OSS;
using Microsoft.Extensions.Configuration;

namespace RemoteConfiguration.Json.Aliyun;

public static class Extensions
{
    public static IConfigurationBuilder AddAliyunJsonFile(
        this IConfigurationBuilder builder,
        Action<AliyunRemoteJsonConfigurationSource> configure, bool optional = true, bool reloadOnChange = true)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var source = new AliyunRemoteJsonConfigurationSource();
        configure?.Invoke(source);

        source.Optional = optional;
        source.ReloadOnChange = reloadOnChange;

        if (string.IsNullOrWhiteSpace(source.Endpoint)
            || string.IsNullOrWhiteSpace(source.Key)
            || string.IsNullOrWhiteSpace(source.BucketName)
            || string.IsNullOrWhiteSpace(source.AccessKeyId)
            || string.IsNullOrWhiteSpace(source.AccessKeySecret))
        {
            throw new ArgumentException("Endpoint, AccessKeyId, AccessKeySecret, BucketName, Key is required.");
        }

        var ossClient = new OssClient(source.Endpoint, source.AccessKeyId, source.AccessKeySecret);
        source.UriProducer = () => ossClient.GeneratePresignedUri(source.BucketName, source.Key,
            DateTime.Now.AddMinutes(2),
            SignHttpMethod.Get);

        return builder.Add(source);
    }
}