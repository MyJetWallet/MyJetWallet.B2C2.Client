using Newtonsoft.Json;

namespace MyJetWallet.B2C2.Client.Models.Rest
{
    public class Instrument
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("underlier")]
        public string Underlier { get; set; }

        [JsonProperty("is_tradable")]
        public bool IsTradable { get; set; }

        [JsonProperty("quantity_precision")]
        public decimal QuantityPrecision { get; set; }

        [JsonProperty("max_quantity_per_trade")]
        public decimal MaxQuantityPerTrade { get; set; }

        [JsonProperty("min_quantity_per_trade")]
        public decimal MinQuantityPerTrade { get; set; }

        [JsonProperty("price_significant_digits")]
        public int PriceSignificantDigits { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
