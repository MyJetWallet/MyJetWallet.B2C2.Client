using System.Collections.Generic;
using Newtonsoft.Json;

namespace MyJetWallet.B2C2.Client.Models.WebSocket
{
    public class Levels
    {
        [JsonProperty("buy")]
        public IReadOnlyList<QuantityPrice> Buy { get; set; } = new List<QuantityPrice>();

        [JsonProperty("sell")]
        public IReadOnlyList<QuantityPrice> Sell { get; set; } = new List<QuantityPrice>();
    }
}
