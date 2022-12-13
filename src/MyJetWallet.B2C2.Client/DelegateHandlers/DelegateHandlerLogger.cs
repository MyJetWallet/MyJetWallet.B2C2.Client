using System.Net.Http;
using System.Threading.Tasks;

namespace MyJetWallet.B2C2.Client.DelegateHandlers
{
    //TODO: Complete logging and telemetry
    public class DelegateHandlerLogger : DelegatingHandler
    {
        public DelegateHandlerLogger()
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            //System.Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(request));

            var contentAsStr = request.Content?.ReadAsStringAsync(cancellationToken).Result;
            System.Console.WriteLine(contentAsStr);
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            //System.Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(request));

            contentAsStr = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            System.Console.WriteLine(contentAsStr);

            return response;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }
}
