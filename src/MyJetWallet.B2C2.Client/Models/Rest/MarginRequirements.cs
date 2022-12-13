using Newtonsoft.Json;

namespace MyJetWallet.B2C2.Client.Models.Rest
{
    public class MarginRequirements
    {
        [JsonProperty("margin_requirement")]
        public decimal MarginRequirement { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("margin_usage")]
        public decimal MarginUsage { get; set; }

        [JsonProperty("equity")]
        public decimal Equity { get; set; }
    }
}
