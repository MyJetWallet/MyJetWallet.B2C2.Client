using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyJetWallet.B2C2.Client;
using MyJetWallet.B2C2.Client.Exceptions;
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
            var client = new B2С2RestClient(
                new B2C2ClientSettings(
                    "https://api.uat.b2c2.net",
                    token),
                serviceProvider.GetService<IHttpClientFactory>(), 
                loggerFactory.CreateLogger<B2С2RestClient>());


            var instruments = await client.InstrumentsAsync();

            Print(instruments);

            var balances = await client.BalanceAsync();

            Print(balances);
            
            var qote = await client.RequestForQuoteAsync(new RequestForQuoteRequest()
            {
                Instrument = "BTCUSD",
                Side = Side.Buy,
                Quantity = 1m,
                ClientRfqId = Guid.NewGuid().ToString()
            });

            var order = await client.OrderAsync(new OrderRequest()
            {
                ClientOrderId = Guid.NewGuid().ToString(),
                Instrument = "BTCUSD",
                Side = Side.Buy,
                Quantity = 0.001m,
                Price = 10000m,
                AcceptableSlippage = 1,
                OrderType = OrderType.FOK,
                
            });

            // await TestPublicKey();
            Console.ReadLine();
        }

        static void Print(object obj)
        {
            Console.WriteLine(obj.ToJson());
        }
    }
}
