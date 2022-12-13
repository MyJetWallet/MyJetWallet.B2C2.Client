using System.Collections.Generic;
using Newtonsoft.Json;

namespace MyJetWallet.B2C2.Client.Models.WebSocket
{
    public class ConnectResponse
    {
        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("tradable_instruments")]
        public IReadOnlyList<string> Instruments { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
