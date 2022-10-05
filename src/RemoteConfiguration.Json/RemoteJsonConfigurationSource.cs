using Microsoft.Extensions.Configuration;
using RemoteConfiguration.Abstractions;

namespace RemoteConfiguration.Json;

public class RemoteJsonConfigurationSource : RemoteConfigurationSource
{
    public string Url { get; set; }
    
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new RemoteJsonConfigurationProvider(new RemoteJsonConfigurationSource
        {
            Url = Url,
            Optional = Optional,
            ReloadDelay = ReloadDelay,
            ReloadOnChange = ReloadOnChange
        });
    }
}