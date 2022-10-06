// See https://aka.ms/new-console-template for more information

using ConsoleApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RemoteConfiguration.Json.Aliyun;

var builder = new ConfigurationBuilder();
builder.AddAliyunJsonFile(source =>
{
    source.Endpoint = "oss-cn-shanghai.aliyuncs.com";
    source.BucketName = "forestrycloud-test";
    source.AccessKeyId = Environment.GetEnvironmentVariable("Aliyun_AccessKeyId");
    source.AccessKeySecret = Environment.GetEnvironmentVariable("Aliyun_AccessKeySecret");
    source.Key = "appsettings_monitor.json";
});
var configuration = builder.Build();

var service = new ServiceCollection();
service.Configure<MyOptions>(configuration);

var provider = service.BuildServiceProvider();
var myOptions1 = provider.GetRequiredService<IOptionsMonitor<MyOptions>>();

Console.Read();
Console.WriteLine("Bye!");