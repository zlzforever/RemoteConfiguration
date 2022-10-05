using Microsoft.Extensions.Configuration;

namespace RemoteConfiguration.Json.Aliyun;

public class RemoteJsonConfigurationSource : RemoteConfigurationSource
{
    public string Endpoint { get; set; }
    public string AccessKeyId { get; set; }
    public string AccessKeySecret { get; set; }
    public string BucketName { get; set; }
    public string Key { get; set; }

    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new RemoteJsonConfigurationProvider(new RemoteJsonConfigurationSource
        {
            Optional = Optional,
            ReloadDelay = ReloadDelay,
            ReloadOnChange = ReloadOnChange,
            Endpoint = Endpoint,
            Key = Key,
            AccessKeyId = AccessKeyId,
            AccessKeySecret = AccessKeySecret,
            BucketName = BucketName
        });
    }
}