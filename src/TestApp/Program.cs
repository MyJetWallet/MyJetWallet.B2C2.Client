using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyJetWallet.B2C2.Client;
using MyJetWallet.B2C2.Client.Models.Rest;
using MyJetWallet.B2C2.Client.Settings;
using MyJetWallet.Sdk.Service;

namespace TestApp
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            //var body = "";
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClient();
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            using var loggerFactory = LogConfigurator.ConfigureElk_v2("MyJetWallet", null, null);
            var token = Environment.GetEnvironmentVariable("B2C2_TOKEN");
            var client = new B2C2RestClient(
                new B2C2ClientSettings(
                    "https://api.uat.b2c2.net",
                    token),
                serviceProvider.GetService<IHttpClientFactory>(), 
                loggerFactory.CreateLogger<B2C2RestClient>());

            var websocketClient = new B2C2WebSocketClient(
                new B2C2ClientSettings("wss://socket.uat.b2c2.net/quotes", token),
                loggerFactory.CreateLogger<B2C2WebSocketClient>(),
                TimeSpan.FromMilliseconds(10_000));

            var instruments = await client.InstrumentsAsync();

            Print(instruments);

            var balances = await client.BalanceAsync();

            Print(balances);

            var instrument = instruments.First(x => x.Name == "BTCUSD.SPOT");

            foreach (var item in instruments)
            {
                try
                {
                    await websocketClient.SubscribeAsync(item.Name, new[] { 0.01m, 0.5m }, async (price) =>
                    {
                        Print(price);
                    });
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            while (true)
            {
                Thread.Sleep(50);
            }

            await websocketClient.UnsubscribeAsync(instrument.Name);

            var quote = await client.RequestForQuoteAsync(new RequestForQuoteRequest()
            {
                Instrument = instrument.Name,
                Side = Side.Buy,
                Quantity = 0.01m,
                ClientRfqId = Guid.NewGuid().ToString()
            });

            Print(quote);
            
            var order = await client.OrderAsync(new OrderRequest()
            {
                ClientOrderId = Guid.NewGuid().ToString(),
                Instrument = instrument.Name,
                Side = Side.Buy,
                Quantity = quote.Quantity,
                Price = quote.Price,
                //AcceptableSlippage = 1,
                OrderType = OrderType.FOK,
                ValidUntil = quote.ValidUntil,
                //ExecutingUnit = 
                //ForceOpen = 
                
            });

            Print(order);

            // await TestPublicKey();
            Console.ReadLine();
        }

        static void Print(object obj)
        {
            Console.WriteLine(obj.ToJson());
        }
    }
}
