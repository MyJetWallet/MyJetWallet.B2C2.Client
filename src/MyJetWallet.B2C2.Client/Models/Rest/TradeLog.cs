﻿using System;
using Newtonsoft.Json;

namespace MyJetWallet.B2C2.Client.Models.Rest
{
    public class TradeLog
    {
        [JsonProperty("trade_id")]
        public string TradeId { get; set; }

        [JsonProperty("rfq_id")]
        public string RequestForQuoteId { get; set; }

        [JsonProperty("quantity")]
        public decimal Volume { get; set; }

        [JsonProperty("side")]
        public string Direction { get; set; }

        [JsonProperty("instrument")]
        public string AssetPair { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }
    }
}
