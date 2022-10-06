using Microsoft.Extensions.Configuration;
using RemoteConfiguration.Abstractions;

namespace RemoteConfiguration.Json;

public class RemoteJsonConfigurationSource : RemoteConfigurationSource
{
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new RemoteJsonConfigurationProvider(new RemoteJsonConfigurationSource
        {
            UriProducer = UriProducer,
            Optional = Optional,
            ReloadDelay = ReloadDelay,
            ReloadOnChange = ReloadOnChange
        });
    }
}