using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using MyJetWallet.B2C2.Client.Models.WebSocket;

namespace MyJetWallet.B2C2.Client
{
    public interface IB2C2WebSocketClient : IStartable, IDisposable
    {
        Task SubscribeAsync(string instrument, decimal[] levels, Func<PriceMessage, Task> handler,
            CancellationToken ct = default(CancellationToken));

        Task UnsubscribeAsync(string instrument, CancellationToken ct = default(CancellationToken));
    }
}
