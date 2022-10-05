# RemoteConfiguration

Implementation of key-value pair based configuration for Microsoft.Extensions.Configuration. Includes the remote file configuration provider.

## Sample

### A remote file without auth
```
var builder = new ConfigurationBuilder();
builder.AddRemoteJsonFile("https://mypubilc.oss-cn-shanghai.aliyuncs.com/appsettings.json");
var configuration = builder.Build();
Console.WriteLine($"{i}: {configuration["orleans:connectionString"]}");
```

### Aliyun OSS file
```
var builder = new ConfigurationBuilder();
builder.AddAliyunJsonFile(source =>
{
    source.Endpoint = "oss-cn-shanghai.aliyuncs.com";
    source.BucketName = "test";
    source.AccessKeyId = "";
    source.AccessKeySecret = "";
    source.Key = "appsettings.json";
});
var configuration = builder.Build();
Console.WriteLine($"{i}: {configuration["orleans:connectionString"]}");
```