using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Primitives;

namespace RemoteConfiguration.Abstractions;

public class PollingChangeToken : IPollingChangeToken
{
    private CancellationTokenSource _tokenSource;
    private CancellationChangeToken _changeToken;
    private bool _hasChanged;
    private DateTime _lastCheckedTimeUtc;
    private readonly byte[] _currentBytes;

    public static int PollingInterval = 30;

    public IDisposable RegisterChangeCallback(Action<object> callback, object state)
    {
        return _changeToken.RegisterChangeCallback(callback, state);
    }

    public Func<byte[]> ReadByteArray;

    public PollingChangeToken(byte[] currentBytes)
    {
        _currentBytes = currentBytes;
    }

    public bool HasChanged
    {
        get
        {
            if (_hasChanged)
            {
                return _hasChanged;
            }

            var currentTime = DateTime.UtcNow;
            var interval = (currentTime - _lastCheckedTimeUtc).TotalSeconds;
            if (interval < PollingInterval)
            {
                return _hasChanged;
            }

            var bytes = ReadByteArray();

            if (!bytes.SequenceEqual(_currentBytes))
            {
                _hasChanged = true;
            }
            else
            {
                Debug.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} configuration no changed.");
            }

            _lastCheckedTimeUtc = currentTime;
            return _hasChanged;
        }
    }

    public bool ActiveChangeCallbacks { get; internal set; } = true;

    public CancellationTokenSource CancellationTokenSource
    {
        get => _tokenSource;
        set
        {
            Debug.Assert(_tokenSource == null, "We expect CancellationTokenSource to be initialized exactly once.");

            _tokenSource = value;
            _changeToken = new CancellationChangeToken(_tokenSource.Token);
        }
    }
}