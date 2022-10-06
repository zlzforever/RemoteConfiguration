using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace RemoteConfiguration.Abstractions;

public abstract class RemoteConfigurationProvider
    : ConfigurationProvider, IDisposable
{
    private string _lastHash;
    private DateTime _lastCheckedTimeUtc;
    private readonly Timer _timer;

    protected static readonly HttpClient HttpClient = new();
    public static TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The source settings for this provider.
    /// </summary>
    public RemoteConfigurationSource Source { get; }

    public RemoteConfigurationProvider(RemoteConfigurationSource source)
    {
        Source = source;
        if (!Source.ReloadOnChange)
        {
            return;
        }

        var md5 = MD5.Create();

        _timer = new Timer(_ =>
        {
            var currentTime = DateTime.UtcNow;
            if (currentTime - _lastCheckedTimeUtc < PollingInterval)
            {
                return;
            }

            using var stream = GetStream();
            var hash = md5.ComputeHash(stream).ToHex();
            stream.Seek(0, SeekOrigin.Begin);
            if (_lastHash != hash)
            {
                _lastHash = hash;
                Thread.Sleep(Source.ReloadDelay);
                try
                {
                    Load(stream);
                }
                catch
                {
                    Debug.Print("Load configuration failed.");
                }
            }
            else
            {
                Debug.Print("Remote configuration is no changed.");
            }

            _lastCheckedTimeUtc = currentTime;
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(4));
    }

    protected virtual Stream GetStream()
    {
        var url = Source.UriProducer();
        var bytes = HttpClient.GetByteArrayAsync(url).Result;
        return new MemoryStream(bytes);
    }

    /// <summary>
    /// Loads the contents of the file at <see cref="T:System.IO.Path" />.
    /// </summary>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">Optional is <c>false</c> on the source and a
    /// directory cannot be found at the specified Path.</exception>
    /// <exception cref="T:System.IO.FileNotFoundException">Optional is <c>false</c> on the source and a
    /// file does not exist at specified Path.</exception>
    /// <exception cref="T:System.IO.InvalidDataException">An exception was thrown by the concrete implementation of the
    /// <see cref="M:Microsoft.Extensions.Configuration.FileConfigurationProvider.Load" /> method. Use the source <see cref="P:Microsoft.Extensions.Configuration.FileConfigurationSource.OnLoadException" /> callback
    /// if you need more control over the exception.</exception>
    public override void Load() => Load(false);

    /// <summary>Loads this provider's data from a stream.</summary>
    /// <param name="stream">The stream to read.</param>
    public abstract void Load(Stream stream);

    private void Load(bool reload)
    {
        using var stream = GetStream();
        if (stream != null)
        {
            try
            {
                Load(stream);
            }
            catch
            {
                if (reload)
                {
                    Data = new Dictionary<string, string>(
                        StringComparer.OrdinalIgnoreCase);
                }

                throw;
            }
        }
        else
        {
            if (Source.Optional | reload)
            {
                Data = new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                throw new ApplicationException("Read configuration stream failed");
            }
        }

        OnReload();
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}