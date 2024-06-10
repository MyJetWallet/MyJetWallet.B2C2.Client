using System;

namespace MyJetWallet.B2C2.Client.Models.Rest;

public class UnsecureLoan
{
    public string trade_id { get; set; }
    public string currency { get; set; }
    public string side { get; set; }
    public string notional { get; set; }
    public string rate { get; set; }
    public DateTime? created { get; set; }
    public DateTime? start_date { get; set; }
    public DateTime? end_date { get; set; }
    // etd...
}