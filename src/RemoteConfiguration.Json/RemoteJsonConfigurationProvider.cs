using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace RemoteConfiguration.Json;

public class RemoteJsonConfigurationProvider : RemoteConfigurationProvider
{
    private static readonly HttpClient HttpClient = new();
    private readonly MethodInfo _method;

    public RemoteJsonConfigurationProvider(RemoteJsonConfigurationSource source) : base(source)
    {
        var type = typeof(JsonConfigurationExtensions).Assembly.GetTypes()
            .FirstOrDefault(
                x => x.FullName == "Microsoft.Extensions.Configuration.Json.JsonConfigurationFileParser");
        if (type == null)
        {
            throw new ApplicationException("JsonConfigurationFileParser is not found");
        }

        var method = type.GetMethod("Parse");
        if (method == null)
        {
            throw new ApplicationException("Parse method is not found");
        }

        _method = method;
    }

    protected override Stream GetStream()
    {
        var source = (RemoteJsonConfigurationSource)Source;
        var bytes = HttpClient.GetByteArrayAsync(source.Url).Result;
        return new MemoryStream(bytes);
    }

    public override void Load(Stream input)
    {
        Data = (IDictionary<string, string>)_method.Invoke(null, new object[] { input });
    }
}