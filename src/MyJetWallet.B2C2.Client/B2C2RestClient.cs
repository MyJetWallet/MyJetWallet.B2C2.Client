using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Grpc.Core;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MyJetWallet.B2C2.Client.Exceptions;
using MyJetWallet.B2C2.Client.Models.Rest;
using MyJetWallet.B2C2.Client.Settings;
using MyJetWallet.Sdk.Service;
using Newtonsoft.Json;

namespace MyJetWallet.B2C2.Client
{
    public class B2C2RestClient : IB2C2RestClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<B2C2RestClient> _log;

        public B2C2RestClient(
            B2C2ClientSettings settings,
            IHttpClientFactory clientFactory,
            ILogger<B2C2RestClient> logger)
        {
            if (settings == null) throw new NullReferenceException(nameof(settings));
            var url = settings.Url;
            var authorizationToken = settings.AuthorizationToken;
            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out _))
                throw new ArgumentOutOfRangeException(nameof(url));
            if (string.IsNullOrWhiteSpace(authorizationToken))
                throw new ArgumentOutOfRangeException(nameof(authorizationToken));

            url = url[^1] == '/' ? url.Substring(0, url.Length - 1) : url;
            _httpClient = clientFactory.CreateClient(ClientNames.B2C2ClientName);
            _httpClient.BaseAddress = new Uri(url);
            _httpClient.DefaultRequestHeaders.Add("Authorization", authorizationToken);
            _log = logger;
        }

        public async Task<IReadOnlyDictionary<string, decimal>> BalanceAsync(CancellationToken ct = default(CancellationToken))
        {
            var requestId = Guid.NewGuid();

            _log.LogDebug("balance - request {context}", requestId);

            var responseStr = string.Empty;

            try
            {
                using var response = await _httpClient.GetAsync("balance/", ct).ConfigureAwait(false);
                var status = response.StatusCode;

                responseStr = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                _log.LogDebug("balance - response {requestId}; {responseStr}", requestId, responseStr);

                CheckForErrorInResponse(responseStr, status, requestId);

                var result = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(responseStr);

                return result;
            }
            catch (Exception e)
            {
                _log.LogError(e, "balance - response {requestId}; {responseStr}", requestId, responseStr);

                throw;
            }
        }

        public async Task<IReadOnlyCollection<Instrument>> InstrumentsAsync(CancellationToken ct = default(CancellationToken))
        {
            var requestId = Guid.NewGuid();

            _log.LogDebug("instruments - request", requestId);

            var responseStr = string.Empty;

            try
            {
                using var response = await _httpClient.GetAsync("instruments/", ct).ConfigureAwait(false);
                var status = response.StatusCode;

                responseStr = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                _log.LogDebug("instruments - response {requestId}; {responseStr}", requestId, responseStr);

                CheckForErrorInResponse(responseStr, status, requestId);

                var result = JsonConvert.DeserializeObject<IReadOnlyCollection<Instrument>>(responseStr);

                return result;
            }
            catch (Exception e)
            {
                _log.LogError(e, "instruments - response exception", new { RequestId = requestId, Response = responseStr });

                throw;
            }
        }

        public async Task<RequestForQuoteResponse> RequestForQuoteAsync(RequestForQuoteRequest requestForQuoteRequest, CancellationToken ct = default(CancellationToken))
        {
            if (requestForQuoteRequest == null) throw new ArgumentNullException(nameof(requestForQuoteRequest));

            var requestId = Guid.NewGuid();

            _log.LogDebug("request for quote - request", requestForQuoteRequest);

            var responseStr = string.Empty;

            try
            {
                using var response = await _httpClient.PostAsJsonAsync("request_for_quote/", requestForQuoteRequest, ct).ConfigureAwait(false);
                var status = response.StatusCode;

                responseStr = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                _log.LogDebug("request for quote - response {requestId}; {responseStr}", requestId, responseStr);
                CheckForErrorInResponse(responseStr, status, requestId);

                var result = JsonConvert.DeserializeObject<RequestForQuoteResponse>(responseStr);

                if (result.ClientRfqId != requestForQuoteRequest.ClientRfqId)
                    throw new B2c2RestException($"request.client_rfq_id '{requestForQuoteRequest.ClientRfqId}' != " +
                                                $"response.client_rfq_id '{result.ClientRfqId}'", requestId);

                return result;
            }
            catch (Exception e)
            {
                _log.LogError(e, "request for quote - response exception", new { RequestId = requestId, Response = responseStr });

                throw;
            }
        }

        public async Task<OrderResponse> OrderAsync(OrderRequest orderRequest, CancellationToken ct = default(CancellationToken))
        {
            if (orderRequest == null) throw new ArgumentNullException(nameof(orderRequest));

            var requestId = Guid.NewGuid();

            _log.LogDebug("order - request", orderRequest);

            var responseStr = string.Empty;

            try
            {
                using var response = await _httpClient.PostAsJsonAsync("order/", orderRequest, ct).ConfigureAwait(false);
                var status = response.StatusCode;

                responseStr = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                _log.LogDebug("order - response {requestId}; {responseStr}", requestId, responseStr);

                CheckForErrorInResponse(responseStr, status, requestId);

                var result = JsonConvert.DeserializeObject<OrderResponse>(responseStr);

                if (result.ClientOrderId != orderRequest.ClientOrderId)
                    throw new B2c2RestException($"request.client_order_id '{orderRequest.ClientOrderId}' != " +
                                                $"response.client_order_id '{result.ClientOrderId}'", requestId);

                return result;
            }
            catch (Exception e)
            {
                _log.LogError(e, "order - response exception", new { RequestId = requestId, Response = responseStr });

                throw;
            }
        }

        public async Task<OrderResponse> GetOrderAsync(string orderId, CancellationToken ct = default(CancellationToken))
        {
            var requestId = Guid.NewGuid();

            _log.LogDebug("order - request", orderId);

            var responseStr = string.Empty;

            try
            {
                using var response = await _httpClient.GetAsync("order/{orderId}/", ct).ConfigureAwait(false);
                var status = response.StatusCode;

                responseStr = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                _log.LogDebug("order - response {requestId}; {responseStr}", orderId, responseStr);

                CheckForErrorInResponse(responseStr, status, requestId);

                var result = JsonConvert.DeserializeObject<OrderResponse>(responseStr);

                return result;
            }
            catch (Exception e)
            {
                _log.LogError(e, "order - response exception", new { RequestId = requestId, Response = responseStr });

                throw;
            }
        }

        public async Task<Trade> TradeAsync(TradeRequest tradeRequest, CancellationToken ct = default(CancellationToken))
        {
            if (tradeRequest == null) throw new ArgumentNullException(nameof(tradeRequest));

            var requestId = Guid.NewGuid();

            _log.LogDebug("trade - request", tradeRequest);

            var responseStr = string.Empty;
            HttpStatusCode status = HttpStatusCode.OK;

            try
            {
                using var response = await _httpClient.PostAsJsonAsync("trade/", tradeRequest, ct).ConfigureAwait(false);
                status = response.StatusCode;

                responseStr = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                _log.LogDebug("trade - response {requestId}; {responseStr}", requestId, responseStr);

                CheckForErrorInResponse(responseStr, status, requestId);

                var result = JsonConvert.DeserializeObject<Trade>(responseStr);

                return result;
            }
            catch (Exception e)
            {
                _log.LogError(e, "trade - response exception: {jsonText}", new
                {
                    RequestId = requestId, 
                    StatusCode = status.ToString(),
                    Response = responseStr
                }.ToJson());

                throw;
            }
        }

        public async Task<List<TradeLog>> GetTradeHistoryAsync(int offset = 0, int limit = 50, CancellationToken ct = default(CancellationToken))
        {
            var requestId = Guid.NewGuid();

            _log.LogDebug("trade history - request", requestId);

            var responseStr = string.Empty;
            HttpStatusCode status = HttpStatusCode.OK;
                
            try
            {
                using var response = await _httpClient.GetAsync($"trade/?offset={offset}&limit={limit}", ct).ConfigureAwait(false); 
                status = response.StatusCode;

                responseStr = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                _log.LogDebug("trade history - response {requestId}; {responseStr}", requestId, responseStr);
                CheckForErrorInResponse(responseStr, status, requestId);

                var result = JsonConvert.DeserializeObject<List<TradeLog>>(responseStr);

                return result;
            }
            catch (Exception e)
            {
                _log.LogError(e, "trade history - response exception: {jsonText}", new
                {
                    RequestId = requestId,
                    StatusCode = status,
                    Response = responseStr
                }.ToJson());

                throw;
            }
        }

        public async Task<TradeLog> GetTradeAsync(string tradeId, CancellationToken ct = default(CancellationToken))
        {
            var requestId = Guid.NewGuid();

            _log.LogDebug("trade history - request", requestId);

            var responseStr = string.Empty;
            HttpStatusCode status = HttpStatusCode.OK;
            
            try
            {
                using var response = await _httpClient.GetAsync($"trade/{tradeId}/", ct).ConfigureAwait(false);
                status = response.StatusCode;

                responseStr = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                _log.LogDebug("trade history - response {requestId}; {responseStr}", requestId, responseStr);
                CheckForErrorInResponse(responseStr, status, requestId);

                var result = JsonConvert.DeserializeObject<TradeLog>(responseStr);

                return result;
            }
            catch (Exception e)
            {
                _log.LogError(e, "trade history - response exception: {jsonText}", new
                {
                    RequestId = requestId,
                    StatusCode = status.ToString(),
                    Response = responseStr
                }.ToJson());

                throw;
            }
        }

        public async Task<List<LedgerLog>> GetLedgerHistoryAsync(int offset = 0, int limit = 50, CancellationToken ct = default(CancellationToken))
        {
            var requestId = Guid.NewGuid();
            _log.LogDebug("ledger history - request", requestId);

            try
            {
                using var response = await _httpClient.GetAsync($"ledger/?offset={offset}&limit={limit}", ct).ConfigureAwait(false);
                var status = response.StatusCode;

                var responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _log.LogDebug("ledger history - response", requestId);

                CheckForErrorInResponse(responseStr, status, requestId);

                var result = JsonConvert.DeserializeObject<List<LedgerLog>>(responseStr);

                return result;
            }
            catch (Exception e)
            {
                _log.LogError(e, "ledger history - response exception");
                throw;
            }
        }

        public async Task<PaginationResponse<List<LedgerLog>>> GetLedgerHistoryAsync(LedgersRequest request, CancellationToken ct = default(CancellationToken))
        {
            var requestId = Guid.NewGuid();
            _log.LogDebug($"ledger history - request requestId: {requestId}, request: {request?.ToJson()}");

            HttpStatusCode status = HttpStatusCode.OK;
            
            try
            {
                var param = new Dictionary<string, string>();

                if (request != null)
                {
                    if (request.CreatedAfter.HasValue)
                    {
                        param.Add("created__gte", request.CreatedAfter.Value.ToString("yyyy-MM-ddThh:mm:ss"));
                    }

                    if (request.CreatedBefore.HasValue)
                    {
                        param.Add("created__lt", request.CreatedBefore.Value.ToString("yyyy-MM-ddThh:mm:ss"));
                    }

                    if (!string.IsNullOrEmpty(request.Currency))
                    {
                        param.Add("currency", request.Currency);
                    }

                    if (request.Type.HasValue)
                    {
                        param.Add("type", request.Type.ToString());
                    }

                    if (request.Since.HasValue)
                    {
                        param.Add("since", request.Since.Value.ToString("yyyy-MM-ddThh:mm:ss"));
                    }

                    if (!string.IsNullOrEmpty(request.Cursor))
                    {
                        param.Add("cursor", request.Cursor);
                    }
                }

                param.Add("limit", Math.Max(100, request?.Limit ?? 50).ToString());

                var ledgerUrl = new Uri(QueryHelpers.AddQueryString("ledger/", param), UriKind.Relative).ToString();

                using var response = await _httpClient.GetAsync(ledgerUrl, ct).ConfigureAwait(false);

                status = response.StatusCode;

                var responseStr = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                CheckForErrorInResponse(responseStr, status, requestId);

                var data = JsonConvert.DeserializeObject<List<LedgerLog>>(responseStr);

                var result = new PaginationResponse<List<LedgerLog>> { Data = data };

                if (response.Headers.TryGetValues("link", out var links))
                {
                    (result.Next, result.Previous) = GetCursors(links);
                }

                return result;
            }
            catch (Exception e)
            {
                _log.LogError(e, "ledger history - response exception: {jsonText}", new
                {
                    requestId,
                    StatusCode = status.ToString()
                }.ToJson());
                throw;
            }
        }

        public async Task<PaginationResponse<List<TradeLog>>> GetTradeHistoryAsync(TradesHistoryRequest request, CancellationToken ct = default(CancellationToken))
        {
            var requestId = Guid.NewGuid();

            var responseStr = string.Empty;

            HttpStatusCode status = HttpStatusCode.OK;
                
            try
            {
                var param = new Dictionary<string, string>();

                if (request != null)
                {
                    if (request.CreatedAfter.HasValue)
                    {
                        param.Add("created__gte", request.CreatedAfter.Value.ToString("yyyy-MM-ddThh:mm:ss"));
                    }

                    if (request.CreatedBefore.HasValue)
                    {
                        param.Add("created__lt", request.CreatedBefore.Value.ToString("yyyy-MM-ddThh:mm:ss"));
                    }

                    if (!string.IsNullOrEmpty(request.Instrument))
                    {
                        param.Add("instrument", request.Instrument);
                    }

                    if (request.Since.HasValue)
                    {
                        param.Add("since", request.Since.Value.ToString("yyyy-MM-ddThh:mm:ss"));
                    }

                    if (!string.IsNullOrEmpty(request.Cursor))
                    {
                        param.Add("cursor", request.Cursor);
                    }
                }

                param.Add("limit", Math.Max(100, request?.Limit ?? 50).ToString());

                var tradeUrl = new Uri(QueryHelpers.AddQueryString("trade/", param), UriKind.Relative).ToString();

                using var response = await _httpClient.GetAsync(tradeUrl, ct).ConfigureAwait(false);

                status = response.StatusCode;

                responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                _log.LogDebug("trade history - response", new { RequestId = requestId, Response = responseStr });

                CheckForErrorInResponse(responseStr, status, requestId);

                var data = JsonConvert.DeserializeObject<List<TradeLog>>(responseStr);

                var result = new PaginationResponse<List<TradeLog>> { Data = data };

                if (response.Headers.TryGetValues("link", out var links))
                {
                    (result.Next, result.Previous) = GetCursors(links);
                }

                return result;
            }
            catch (Exception e)
            {
                _log.LogError(e, "trade history - response exception: {jsonText}", new
                {
                    RequestId = requestId,
                    StatusCode = status.ToString(),
                    Response = responseStr
                }.ToJson());

                throw;
            }
        }

        public async Task<AccountInfo> GetAccountInfoAsync(CancellationToken ct = default(CancellationToken))
        {
            var requestId = Guid.NewGuid();

            _log.LogDebug("account info - request", requestId);

            var responseStr = string.Empty;
            HttpStatusCode status = HttpStatusCode.OK;

            try
            {
                using var response = await _httpClient.GetAsync($"account_info/", ct).ConfigureAwait(false);
                status = response.StatusCode;

                responseStr = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                _log.LogDebug("account info - response {requestId}; {responseStr}", requestId, responseStr);
                CheckForErrorInResponse(responseStr, status, requestId);

                var result = JsonConvert.DeserializeObject<AccountInfo>(responseStr);

                return result;
            }
            catch (Exception e)
            {
                _log.LogError(e, "account info - response exception: {jsonText}", new
                {
                    RequestId = requestId,
                    StatusCode = status.ToString(),
                    Response = responseStr
                }.ToJson());

                throw;
            }
        }

        public async Task<MarginRequirements> GetMarginRequirementsAsync(string currency, CancellationToken ct = default(CancellationToken))
        {
            var requestId = Guid.NewGuid();

            _log.LogDebug("margin_requirements - request", requestId);

            var responseStr = string.Empty;
            HttpStatusCode status = HttpStatusCode.OK;

            try
            {
                using var response = await _httpClient.GetAsync($"margin_requirements/", ct).ConfigureAwait(false);
                status = response.StatusCode;

                responseStr = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                _log.LogDebug("margin_requirements - response {requestId}; {responseStr}", requestId, responseStr);
                CheckForErrorInResponse(responseStr, status, requestId);

                var result = JsonConvert.DeserializeObject<MarginRequirements>(responseStr);

                return result;
            }
            catch (Exception e)
            {
                _log.LogWarning(e, "margin_requirements - response exception: {jsonText}", new
                {
                    RequestId = requestId,
                    StatusCode = status.ToString(),
                    Response = responseStr
                }.ToJson());

                throw;
            }
        }

        private (string, string) GetCursors(IEnumerable<string> values)
        {
            string next = null;
            string prev = null;

            foreach (var value in values)
            {
                var links = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var link in links)
                {
                    var data = link.Replace("<", string.Empty).Replace(">", string.Empty);
                    var parts = data.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length == 2)
                    {
                        var cursor = HttpUtility.ParseQueryString(new Uri(parts[0]).Query).Get("cursor");
                        if (parts[1].Contains("next"))
                        {
                            next = cursor;
                        }
                        else
                        {
                            prev = cursor;
                        }
                    }
                }
            }

            return (next, prev);
        }

        private void CheckForErrorInResponse(string response, HttpStatusCode status, Guid guid)
        {
            if (!response.Contains("errors"))
                return;

            ErrorResponse errorResponse;

            try
            {
                errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(response);

                errorResponse.Status = status;
            }
            catch (Exception e)
            {
                var message = $"Can't deserialize error response, status: {(int)status} {status}, guid: {guid}, response: {response}";

                throw new B2c2RestException(message, e, guid);
            }

            throw new B2c2RestException(errorResponse, guid);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
