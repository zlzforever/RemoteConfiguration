using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using RemoteConfiguration.Abstractions;

namespace RemoteConfiguration.Json;

public class RemoteJsonConfigurationProvider : RemoteConfigurationProvider
{
    private static readonly MethodInfo ParseMethod;

    static RemoteJsonConfigurationProvider()
    {
        var type = typeof(JsonConfigurationExtensions).Assembly.GetTypes()
            .FirstOrDefault(
                x => x.FullName == "Microsoft.Extensions.Configuration.Json.JsonConfigurationFileParser");
        if (type == null)
        {
            throw new ApplicationException(
                "Type Microsoft.Extensions.Configuration.Json.JsonConfigurationFileParser not found.");
        }

        var method = type.GetMethod("Parse");
        if (method == null)
        {
            throw new ApplicationException(
                "Method Microsoft.Extensions.Configuration.Json.JsonConfigurationFileParser.Parse not found.");
        }

        ParseMethod = method;
    }

    public RemoteJsonConfigurationProvider(RemoteJsonConfigurationSource source) : base(source)
    {
    }

    public override void Load(Stream input)
    {
        Data = (IDictionary<string, string>)ParseMethod.Invoke(null, new object[] { input });
    }
}