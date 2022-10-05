// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using RemoteConfiguration.Json;
using RemoteConfiguration.Json.Aliyun;

var builder = new ConfigurationBuilder();
// builder.AddRemoteJsonFile("https://mypubilc.oss-cn-shanghai.aliyuncs.com/appsettings.json");
builder.AddAliyunJsonFile(source =>
{
    source.Endpoint = "oss-cn-shanghai.aliyuncs.com";
    source.BucketName = "test";
    source.AccessKeyId = "";
    source.AccessKeySecret = "";
    source.Key = "appsettings.json";
});
var configuration = builder.Build();
var key = ' ';
Parallel.For(0, 8, new ParallelOptions
{
    MaxDegreeOfParallelism = 8
}, (i) =>
{
    while (key != 'c')
    {
        Console.WriteLine($"{i}: {configuration["orleans:connectionString"]}");
        Thread.Sleep(3000);
    }
});
while (true)
{
    key = (char)Console.Read();
}


Console.WriteLine("Bye!");