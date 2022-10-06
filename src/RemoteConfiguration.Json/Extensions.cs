using System;
using Microsoft.Extensions.Configuration;

namespace RemoteConfiguration.Json;

public static class Extensions
{
    public static IConfigurationBuilder AddRemoteJsonFile(
        this IConfigurationBuilder builder,
        string uri, bool optional = true, bool reloadOnChange = true)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (string.IsNullOrEmpty(uri))
        {
            throw new ArgumentNullException(nameof(uri));
        }

        return builder.Add(new RemoteJsonConfigurationSource
        {
            UriProducer = () => new Uri(uri),
            Optional = optional,
            ReloadOnChange = reloadOnChange
        });
    }
}