using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.B2C2.Client.Exceptions;
using MyJetWallet.B2C2.Client.Models.WebSocket;
using MyJetWallet.B2C2.Client.Settings;
using MyJetWallet.Sdk.Service.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyJetWallet.B2C2.Client
{
    public class B2C2WebSocketClient : IB2C2WebSocketClient
    {
        private readonly TimeSpan _timeOut = new TimeSpan(0, 0, 0, 5);
        private readonly string _baseUri;
        private readonly string _authorizationToken;
        private readonly ILogger<B2C2WebSocketClient> _log;
        private ClientWebSocket _clientWebSocket;
        private readonly SemaphoreSlim _sync = new(1);
        private readonly ConcurrentDictionary<string, Subscription> _awaitingSubscriptions;
        private readonly ConcurrentDictionary<string, Func<PriceMessage, Task>> _instrumentHandlers;
        private readonly ConcurrentDictionary<string, decimal[]> _instrumentLevels;
        private readonly ConcurrentDictionary<string, Subscription> _awaitingUnsubscriptions;
        private readonly HashSet<string> _tradableInstruments;

        private CancellationTokenSource _cancellationTokenSource;
        private DateTime _lastPriceUpdate = DateTime.UtcNow;
        public Task SubscriptionThread { get; private set; }
        private MyTaskTimer _timer;

        public B2C2WebSocketClient(
            B2C2ClientSettings settings,
            ILogger<B2C2WebSocketClient> logger,
            TimeSpan? timeOut = null)
        {
            if (settings == null) throw new NullReferenceException(nameof(settings));
            var url = settings.Url;
            var authorizationToken = settings.AuthorizationToken;
            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out _))
                throw new ArgumentOutOfRangeException(nameof(url));
            if (string.IsNullOrWhiteSpace(authorizationToken)) throw new ArgumentOutOfRangeException(nameof(authorizationToken));
            if (logger == null) throw new NullReferenceException(nameof(logger));
            if (timeOut.HasValue) _timeOut = timeOut.Value;

            _baseUri = url[^1] == '/' ? url.Substring(0, url.Length - 1) : url;
            _authorizationToken = authorizationToken;
            _log = logger;
            _awaitingSubscriptions = new ConcurrentDictionary<string, Subscription>();
            _instrumentHandlers = new ConcurrentDictionary<string, Func<PriceMessage, Task>>();
            _instrumentLevels = new ConcurrentDictionary<string, decimal[]>();
            _awaitingUnsubscriptions = new ConcurrentDictionary<string, Subscription>();
            _tradableInstruments = new();
            _cancellationTokenSource = new CancellationTokenSource();
            _timer = new MyTaskTimer("B2C2WebSocketClient", TimeSpan.FromMinutes(1), _log, DoTime);
        }

        public async Task SubscribeAsync(
            string instrument,
            decimal[] levels,
            Func<PriceMessage, Task> handler,
            CancellationToken ct = default(CancellationToken))
        {
            ThrowIfSubscriptionIsAlreadyExist(instrument);

            if (_clientWebSocket.State != WebSocketState.Open)
                throw new B2c2WebSocketException($"Subscribing to {instrument} - state is not 'Open': {_clientWebSocket.State}");

            var tag = Guid.NewGuid().ToString();

            _log.LogDebug(message: $"Subscribing to '{instrument}'. {tag}");

            var subscribeRequest = new SubscribeRequest { Instrument = instrument, Levels = levels, Tag = tag };
            await SendMessageToWebSocket(subscribeRequest, ct);

            // Save subscription state
            var taskCompletionSource = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                await _sync.WaitAsync(ct);
                _instrumentLevels[instrument] = levels;
                _awaitingSubscriptions[instrument] = new Subscription(tag, taskCompletionSource, handler, levels);
            }
            finally
            {
                _sync.Release();
            }

            var successTask = await Task.WhenAny(taskCompletionSource.Task, Task.Delay(_timeOut, ct));

            if (successTask != taskCompletionSource.Task)
            {
                throw new TimeoutException($"Subscription timeout for {instrument}.");
            }

            await taskCompletionSource.Task;
        }

        public async Task UnsubscribeAsync(string instrument, CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(instrument)) throw new NullReferenceException(nameof(instrument));

            var tag = Guid.NewGuid().ToString();

            _log.LogDebug(message: $"Attempt to subscribe to order book updates, instrument: '{instrument}'.", tag);

            await ThrowIfUnsubscriptionAlreadyExists(instrument);

            var unsubscribeRequest = new UnsubscribeRequest { Instrument = instrument, Tag = tag };
            await SendMessageToWebSocket(unsubscribeRequest, ct);

            // Save unsubscription state
            var taskCompletionSource = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            try
            {
                await _sync.WaitAsync(ct);
                _awaitingUnsubscriptions[instrument] = new Subscription(tag, taskCompletionSource, null);
            }
            finally
            {
                _sync.Release();
            }

            var successTask = await Task.WhenAny(taskCompletionSource.Task, Task.Delay(_timeOut, ct));

            if (successTask != taskCompletionSource.Task)
            {
                throw new TimeoutException($"Unsubscription timeout for {instrument}.");
            }

            await taskCompletionSource.Task;
        }

        private async Task Connect()
        {
            _log.LogDebug("Attempt to connect and start handle messages cycle...");

            _clientWebSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();
            _clientWebSocket.Options.SetRequestHeader("Authorization", $"{_authorizationToken}");

            var connected = await TryConnect(_cancellationTokenSource.Token);

            _log.LogDebug($"Connected? {connected}.");

            if (!connected)
                return;

            // Listen for messages in separate io thread
            SubscriptionThread = Task.Run(HandleMessagesCycleAsync, _cancellationTokenSource.Token);
        }

        private async Task<bool> TryConnect(CancellationToken ct)
        {
            _log.LogDebug("Try to connect...");

            try
            {
                await _clientWebSocket.ConnectAsync(new Uri($"{_baseUri}/quotes"), ct);
            }
            catch (WebSocketException e)
            {
                _log.LogError(e, $"Exception occurred while connecting to {_baseUri}: {e}.");
            }

            return _clientWebSocket.State == WebSocketState.Open;
        }

        private async Task HandleMessagesCycleAsync()
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
                return;

            var ct = _cancellationTokenSource.Token;

            try
            {
                while (_clientWebSocket?.State == WebSocketState.Open)
                {
                    using var stream = new MemoryStream(8192);
                    var receiveBuffer = new ArraySegment<byte>(new byte[1024]);
                    try
                    {
                        WebSocketReceiveResult receiveResult;
                        do
                        {
                            receiveResult = await _clientWebSocket.ReceiveAsync(receiveBuffer, ct);
                            await stream.WriteAsync(receiveBuffer.Array, receiveBuffer.Offset, receiveResult.Count, ct);
                        } while (!receiveResult.EndOfMessage);

                        var messageBytes = stream.ToArray();
                        var jsonMessage = Encoding.UTF8.GetString(messageBytes, 0, messageBytes.Length);
                        _log.LogInformation("Websocket message: {json}", jsonMessage);

                        if (!string.IsNullOrWhiteSpace(jsonMessage))
                            await HandleWebSocketMessageAsync(jsonMessage);

                        _lastPriceUpdate = DateTime.UtcNow;
                    }
                    catch (Exception)
                    {
                        // Ignore connection errors and errors during reconnection
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Something went wrong in subscription thread.");
            }

            _log.LogInformation("SubscriptionThread has completed.");
        }

        private async Task HandleWebSocketMessageAsync(string jsonMessage)
        {
            JToken jToken = null;
            var type = "";
            try
            {
                jToken = JToken.Parse(jsonMessage);
                type = jToken["event"]?.Value<string>();

                switch (type)
                {
                    case "tradable_instruments":
                        HandleTradableInstrumentMessage(jToken);
                        break;
                    case "subscribe":
                        await HandleSubscribeMessage(jToken);
                        break;
                    case "price":
                        await HandlePriceMessage(jToken);
                        break;
                    case "unsubscribe":
                        await HandleUnsubscribeMessage(jToken);
                        break;
                    default:
                        _log.LogError($"Strange type of message: {type}.");
                        break;
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, $"Type: {type}, message: {jToken}.");
            }
        }

        private void HandleTradableInstrumentMessage(JToken jToken)
        {
            if (jToken["success"]?.Value<bool>() == false)
            {
                _log.LogError($"{nameof(ConnectResponse)}.{nameof(ConnectResponse.Success)} == false. {jToken}");
                return;
            }

            var result = jToken.ToObject<ConnectResponse>();
            foreach (var instrument in result.Instruments)
                _tradableInstruments.Add(instrument);
        }

        private async Task HandleSubscribeMessage(JToken jToken)
        {
            var tag = jToken["tag"].Value<string>();
            if (jToken["success"]?.Value<bool>() == false)
            {
                var errorResponse = jToken.ToObject<ErrorResponse>();

                Subscription subscription;
                string instrument;

                try
                {
                    await _sync.WaitAsync();
                    instrument = _awaitingSubscriptions.Where(x => x.Value.Tag == tag).Select(x => x.Key).Single();
                    _awaitingSubscriptions.TryRemove(instrument, out subscription);
                }
                finally
                {
                    _sync.Release();
                }

                subscription?.TaskCompletionSource.TrySetException(new B2c2WebSocketException($"{nameof(SubscribeMessage)}.{nameof(SubscribeMessage.Success)} == false. {jToken}", errorResponse));

                _log.LogDebug($"Failed to subscribe to {instrument}.");

                return;
            }

            var result = jToken.ToObject<SubscribeMessage>();
            try
            {
                await _sync.WaitAsync();
                var instrument = result.Instrument;
                if (!_awaitingSubscriptions.ContainsKey(instrument))
                    _log.LogError($"Subscriptions doesn't have element with '{instrument}. {tag}");

                _awaitingSubscriptions.TryRemove(instrument, out var subscription);

                if (_instrumentHandlers.ContainsKey(instrument))
                    subscription.TaskCompletionSource.TrySetException(new B2c2WebSocketException($"Attempt to second subscription to {instrument}."));

                _instrumentHandlers[instrument] = subscription.Function;

                subscription.TaskCompletionSource.TrySetResult(0);

                _log.LogDebug($"Subscribed to {instrument}.");
            }
            finally
            {
                _sync.Release();
            }

        }

        private async Task HandlePriceMessage(JToken jToken)
        {
            if (jToken["success"]?.Value<bool>() == false)
            {
                var errorResponse = jToken.ToObject<ErrorResponse>();

                var message = $"{nameof(PriceMessage)}.{nameof(PriceMessage.Success)} == false.";
                if (errorResponse.Code == ErrorCode.NotAbleToQuoteAtTheMoment)
                    _log.LogDebug(message, jToken);
                else
                    _log.LogError(message, jToken);

                return;
            }

            var result = jToken.ToObject<PriceMessage>();

            Func<PriceMessage, Task> handler;

            if (!_instrumentHandlers.ContainsKey(result.Instrument))
            {
                _log.LogDebug("Received a price that we were not subscribed to.", new { result.Instrument });

                return;
            }

            if (!_instrumentHandlers.TryGetValue(result.Instrument, out handler))
            {
                _log.LogWarning("{instrument} has no handler but still subscribed!", result.Instrument);
                return;
            }

            try
            {
                await handler(result);
            }
            catch (Exception exception)
            {
                _log.LogError("WebSocket price message handler failed.", exception);
            }
        }

        private async Task HandleUnsubscribeMessage(JToken jToken)
        {
            var tag = jToken["tag"].Value<string>();
            if (jToken["success"]?.Value<bool>() == false)
            {
                try
                {
                    await _sync.WaitAsync();
                    var instrument = _awaitingUnsubscriptions.Where(x => x.Value.Tag == tag).Select(x => x.Key).Single();
                    _awaitingUnsubscriptions.TryRemove(instrument, out var value);
                    _instrumentLevels.TryRemove(instrument, out _);
                    value.TaskCompletionSource.TrySetException(
                        new B2c2WebSocketException($"{nameof(UnsubscribeMessage)}.{nameof(UnsubscribeMessage.Success)} == false. {jToken}"));

                    _log.LogError($"Failed to unsubscribe from {instrument}.");
                }
                finally
                {
                    _sync.Release();
                }

                return;
            }

            var result = jToken.ToObject<UnsubscribeMessage>();

            try
            {
                await _sync.WaitAsync();
                var instrument = jToken["instrument"].Value<string>();
                if (!_awaitingUnsubscriptions.ContainsKey(instrument))
                    _log.LogError($"Can't unsubscribe from '{instrument}', subscription does not exist. {jToken}; {tag}");

                _awaitingUnsubscriptions.TryRemove(instrument, out var subscription);

                if (_instrumentHandlers.ContainsKey(result.Instrument))
                    subscription.TaskCompletionSource.TrySetException(
                        new B2c2WebSocketException($"Attempt to second subscription to {result.Instrument}."));

                _instrumentHandlers.TryRemove(instrument, out _);

                subscription.TaskCompletionSource.TrySetResult(0);

                _log.LogDebug($"Unsubscribed from {instrument}.");
            }
            finally
            {
                _sync.Release();
            }
        }

        private static ArraySegment<byte> StringToArraySegment(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var messageArraySegment = new ArraySegment<byte>(messageBytes);
            return messageArraySegment;
        }

        private bool IsSubscriptionInProgress(string instrument)
        {
            try
            {
                _sync.Wait();
                return _awaitingSubscriptions.ContainsKey(instrument)
                       || _instrumentHandlers.ContainsKey(instrument);
            }
            finally
            {
                _sync.Release();
            }
        }

        private void ThrowIfSubscriptionIsAlreadyExist(string instrument)
        {
            if (IsSubscriptionInProgress(instrument))
                throw new B2c2WebSocketException($"Subscription to '{instrument}' is already existed.");
        }

        private async Task ThrowIfUnsubscriptionAlreadyExists(string instrument)
        {
            try
            {
                await _sync.WaitAsync();
                if (_awaitingUnsubscriptions.ContainsKey(instrument))
                    throw new B2c2WebSocketException($"Unsubscription to '{instrument}' is already exist.");
            }
            finally
            {
                _sync.Release();
            }
        }

        private async Task SendMessageToWebSocket(IRequest request, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var requestSegment = StringToArraySegment(JsonConvert.SerializeObject(request));
                await _clientWebSocket.SendAsync(
                    requestSegment,
                    WebSocketMessageType.Text,
                    true,
                    ct);
            }
            catch (Exception e)
            {
                throw new B2c2WebSocketException(
                    "Something went wrong while sending a message to the web socket, see InternalException.", e);
            }
        }

        private async Task ConnectIfNeeded()
        {
            var needToConnect = _clientWebSocket == null || _clientWebSocket.State == WebSocketState.Aborted ;

            _log.LogDebug($"WebSocket connection status: {_clientWebSocket?.State}.");

            if (needToConnect)
                await Connect();
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~B2C2WebSocketClient()
        {
            Dispose(false);
        }

        public void Start()
        {
            _lastPriceUpdate = DateTime.UtcNow;

            ConnectIfNeeded()
                .GetAwaiter()
                .GetResult();

            _timer.Start();
        }

        private async Task DoTime()
        {
            if (!_cancellationTokenSource.IsCancellationRequested &&
                _instrumentLevels.Any() &&
                DateTime.UtcNow - _lastPriceUpdate >= TimeSpan.FromMinutes(2))
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                _lastPriceUpdate = DateTime.UtcNow;

                List<(string, Func<PriceMessage, Task>)> list = new List<(string, Func<PriceMessage, Task>)>();
                var levels = _instrumentLevels.ToDictionary(x => x.Key, y => y.Value);

                foreach (var instrument in _instrumentHandlers)
                {
                    list.Add((instrument.Key, instrument.Value));
                    //_instrumentHandlers.TryRemove(, out _);
                }

                //_instrumentLevels.Clear();

                _clientWebSocket = null;
                await ConnectIfNeeded();

                foreach (var item in list)
                {
                    var level = levels[item.Item1];
                    try
                    {
                        _instrumentHandlers.TryRemove(item.Item1, out _);
                        await SubscribeAsync(item.Item1, level, item.Item2, _cancellationTokenSource.Token);
                    }
                    catch (Exception e)
                    {
                        _instrumentHandlers[item.Item1] = item.Item2;
                        Console.WriteLine("Can't resubscribe after disconnect");
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            _clientWebSocket?.Abort();
            _clientWebSocket?.Dispose();
            _clientWebSocket = null;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        #endregion

        private class Subscription
        {
            public string Tag { get; }

            public TaskCompletionSource<int> TaskCompletionSource { get; }

            public Func<PriceMessage, Task> Function { get; }

            public decimal[] Levels { get; set; }

            public Subscription(string tag, TaskCompletionSource<int> taskCompletionSource, Func<PriceMessage, Task> function, decimal[] levels)
            {
                Tag = tag;
                TaskCompletionSource = taskCompletionSource;
                Function = function;
                Levels = levels;
            }

            public Subscription(string tag, TaskCompletionSource<int> taskCompletionSource, decimal[] levels)
            {
                Tag = tag;
                TaskCompletionSource = taskCompletionSource;
                Levels = levels;
            }
        }
    }
}
