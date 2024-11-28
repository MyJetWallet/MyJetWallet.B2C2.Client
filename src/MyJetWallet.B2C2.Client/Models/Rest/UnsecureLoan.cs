using System;
using System.Runtime.Serialization;

namespace MyJetWallet.B2C2.Client.Models.Rest;

[DataContract]
public class UnsecureLoan
{
    [DataMember(Order = 1)] public string trade_id { get; set; }
    [DataMember(Order = 2)] public string currency { get; set; }
    [DataMember(Order = 3)] public string side { get; set; }
    [DataMember(Order = 4)] public string notional { get; set; }
    [DataMember(Order = 5)] public string rate { get; set; }
    [DataMember(Order = 6)] public DateTime? created { get; set; }
    [DataMember(Order = 7)] public DateTime? start_date { get; set; }
    [DataMember(Order = 8)] public DateTime? end_date { get; set; }
    // etd...
}