using System;
using Microsoft.Extensions.Configuration;

namespace RemoteConfiguration.Json.Aliyun;

public static class Extensions
{
    public static IConfigurationBuilder AddAliyunJsonFile(
        this IConfigurationBuilder builder,
        Action<RemoteJsonConfigurationSource> configure, bool optional = true, bool reloadOnChange = true)
    {
        if (configure == null)
        {
            throw new ApplicationException("Configure can't be null");
        }

        var source = new RemoteJsonConfigurationSource();
        configure(source);
        source.Optional = optional;
        source.ReloadOnChange = reloadOnChange;
        if (string.IsNullOrWhiteSpace(source.Endpoint)
            || string.IsNullOrWhiteSpace(source.Key)
            || string.IsNullOrWhiteSpace(source.BucketName)
            || string.IsNullOrWhiteSpace(source.AccessKeyId)
            || string.IsNullOrWhiteSpace(source.AccessKeySecret))
        {
            throw new ArgumentNullException("");
        }

        return builder.Add(source);
    }
}