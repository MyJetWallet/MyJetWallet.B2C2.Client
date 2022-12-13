using Newtonsoft.Json;

namespace MyJetWallet.B2C2.Client.Models.WebSocket
{
    public class UnsubscribeRequest : IRequest
    {
        [JsonProperty("event")]
        public string Event { get; set; } = "unsubscribe";

        [JsonProperty("instrument")]
        public string Instrument { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }
    }
}
