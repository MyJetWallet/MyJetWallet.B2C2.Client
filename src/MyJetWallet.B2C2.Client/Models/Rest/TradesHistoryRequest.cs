using System;

namespace MyJetWallet.B2C2.Client.Models.Rest
{
    public class TradesHistoryRequest : PaginationRequest
    {
        public DateTime? CreatedBefore { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public string Instrument { get; set; }
        public DateTime? Since { get; set; }
    }
}
