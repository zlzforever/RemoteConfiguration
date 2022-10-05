using System;
using Microsoft.Extensions.Configuration;

namespace RemoteConfiguration.Json;

public static class Extensions
{
    public static IConfigurationBuilder AddRemoteJsonFile(
        this IConfigurationBuilder builder,
        string url, bool optional = true, bool reloadOnChange = true)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException(nameof(url));
        }

        return builder.Add(new RemoteJsonConfigurationSource
        {
            Url = url,
            Optional = optional,
            ReloadOnChange = reloadOnChange
        });
    }
}