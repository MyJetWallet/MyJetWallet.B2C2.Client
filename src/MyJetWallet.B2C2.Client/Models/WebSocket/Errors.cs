using Newtonsoft.Json;

namespace MyJetWallet.B2C2.Client.Models.WebSocket
{
    public class Errors
    {
        [JsonProperty("instrument")]
        public string[] Instrument { get; set; }

        [JsonProperty("levels")]
        public string[] Levels { get; set; }
    }
}
