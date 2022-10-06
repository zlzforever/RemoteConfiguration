using System.Threading;
using Microsoft.Extensions.Primitives;

namespace RemoteConfiguration.Abstractions;

internal interface IPollingChangeToken : IChangeToken
{
    CancellationTokenSource CancellationTokenSource { get; }
}