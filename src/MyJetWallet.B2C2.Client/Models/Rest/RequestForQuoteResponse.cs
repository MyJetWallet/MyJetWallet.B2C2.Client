﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MyJetWallet.B2C2.Client.Models.Rest
{
    public class RequestForQuoteResponse
    {
        [JsonProperty("rfq_id")]
        public string Id { get; set; }

        /// A universally unique identifier that was set in request.
        [JsonProperty("client_rfq_id")]
        public string ClientRfqId { get; set; }

        [JsonProperty("instrument")]
        public string Instrument { get; set; }

        [JsonProperty("side"), JsonConverter(typeof(StringEnumConverter))]
        public Side Side { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        /// Quantity in base currency (maximum 4 decimals).
        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        // Present in documentation but absent in real data
        //[JsonProperty("created"), JsonConverter(typeof(IsoDateTimeConverter))]
        //public DateTime Created { get; set; }

        [JsonProperty("valid_until"), JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime ValidUntil { get; set; }

        public RequestForQuoteResponse()
        {
        }

        public RequestForQuoteResponse(string id, string clientRfqId, string instrument, Side side, double price, double quantity, DateTime validUntil)
        {
            Id = id;
            ClientRfqId = clientRfqId;
            Instrument = instrument;
            Side = side;
            Price = (decimal)price;
            Quantity = (decimal)quantity;
            ValidUntil = validUntil;
        }
    }
}
