using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace RemoteConfiguration.Abstractions;

public abstract class RemoteConfigurationProvider
    : ConfigurationProvider, IDisposable
{
    private Timer _timer;
    private bool _timerInitialized;
    private object _timerLock = new();
    private byte[] _currentBytes = Array.Empty<byte>();
    private readonly ConcurrentDictionary<IPollingChangeToken, IPollingChangeToken> _pollingChangeTokens;
    private static readonly HttpClient HttpClient = new();
    private readonly Lazy<IDisposable> _changeTokenRegistration;

    /// <summary>
    /// The source settings for this provider.
    /// </summary>
    public RemoteConfigurationSource Source { get; }

    protected RemoteConfigurationProvider(RemoteConfigurationSource source)
    {
        Source = source;
        if (!Source.ReloadOnChange)
        {
            return;
        }

        _pollingChangeTokens = new ConcurrentDictionary<IPollingChangeToken, IPollingChangeToken>();
        _changeTokenRegistration = new Lazy<IDisposable>(() =>
        {
            return ChangeToken.OnChange(
                () =>
                {
                    LazyInitializer.EnsureInitialized(ref _timer, ref _timerInitialized, ref _timerLock, TimerFactory);

                    var pollingChangeToken = new PollingChangeToken(_currentBytes)
                    {
                        ReadByteArray = GetByteArray,
                        ActiveChangeCallbacks = true,
                        CancellationTokenSource = new CancellationTokenSource()
                    };
                    _pollingChangeTokens.TryAdd(pollingChangeToken, pollingChangeToken);

                    return pollingChangeToken;
                },
                () =>
                {
                    Thread.Sleep(Source.ReloadDelay);
                    Load(reload: true);
                });
        });
    }

    protected virtual byte[] GetByteArray()
    {
        var url = Source.UriProducer();
        var bytes = HttpClient.GetByteArrayAsync(url).Result;
        Debug.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} sync configuration");
        return bytes;
    }

    public override void Load()
    {
        Load(false);
        // ReSharper disable once UnusedVariable
        var disposable = _changeTokenRegistration.Value;
    }

    /// <summary>Loads this provider's data from a stream.</summary>
    /// <param name="stream">The stream to read.</param>
    public abstract void Load(Stream stream);

    public void Dispose()
    {
        _changeTokenRegistration.Value.Dispose();
        _timer?.Dispose();
    }

    private static void RaiseChangeEvents(object state)
    {
        // Iterating over a concurrent bag gives us a point in time snapshot making it safe
        // to remove items from it.
        var changeTokens = (ConcurrentDictionary<IPollingChangeToken, IPollingChangeToken>)state;
        foreach (var item in changeTokens)
        {
            var token = item.Key;

            if (!token.HasChanged)
            {
                continue;
            }

            if (!changeTokens.TryRemove(token, out _))
            {
                // Move on if we couldn't remove the item.
                continue;
            }

            // We're already on a background thread, don't need to spawn a background Task to cancel the CTS
            try
            {
                token.CancellationTokenSource.Cancel();
            }
            catch
            {
                // 
            }
        }
    }

    private Timer TimerFactory()
    {
        // Don't capture the current ExecutionContext and its AsyncLocals onto the timer
        bool restoreFlow = false;
        try
        {
            if (!ExecutionContext.IsFlowSuppressed())
            {
                ExecutionContext.SuppressFlow();
                restoreFlow = true;
            }

            return new Timer(RaiseChangeEvents, _pollingChangeTokens, TimeSpan.Zero,
                period: TimeSpan.FromSeconds(4));
        }
        finally
        {
            // Restore the current ExecutionContext
            if (restoreFlow)
            {
                ExecutionContext.RestoreFlow();
            }
        }
    }

    private void Load(bool reload)
    {
        _currentBytes = GetByteArray();
        if (_currentBytes is { Length: > 0 })
        {
            try
            {
                using var stream = new MemoryStream(_currentBytes);
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
}