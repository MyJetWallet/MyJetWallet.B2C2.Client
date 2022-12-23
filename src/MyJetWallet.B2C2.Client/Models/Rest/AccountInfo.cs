using Newtonsoft.Json;

namespace MyJetWallet.B2C2.Client.Models.Rest;

public class AccountInfo
{
    [JsonProperty("risk_exposure")]
    public decimal RiskExposure { get; set; }

    [JsonProperty("max_risk_exposure")]
    public decimal MaxRiskExposure { get; set; }

    [JsonProperty("btc_max_qty_per_trade")] public decimal BtcMaxQtyPerTrade { get; set; }
    [JsonProperty("ust_max_qty_per_trade")] public decimal UstMaxQtyPerTrade { get; set; }
    [JsonProperty("eth_max_qty_per_trade")] public decimal EthMaxQtyPerTrade { get; set; }
    [JsonProperty("ltc_max_qty_per_trade")] public decimal LtcMaxQtyPerTrade { get; set; }
    [JsonProperty("bch_max_qty_per_trade")] public decimal BchMaxQtyPerTrade { get; set; }
    [JsonProperty("xrp_max_qty_per_trade")] public decimal XrpMaxQtyPerTrade { get; set; }
    [JsonProperty("eos_max_qty_per_trade")] public decimal EosMaxQtyPerTrade { get; set; }
    [JsonProperty("xau_max_qty_per_trade")] public decimal XauMaxQtyPerTrade { get; set; }
    [JsonProperty("zec_max_qty_per_trade")] public decimal ZecMaxQtyPerTrade { get; set; }
    [JsonProperty("xlm_max_qty_per_trade")] public decimal XlmMaxQtyPerTrade { get; set; }
    [JsonProperty("ada_max_qty_per_trade")] public decimal AdaMaxQtyPerTrade { get; set; }
    [JsonProperty("dot_max_qty_per_trade")] public decimal DotMaxQtyPerTrade { get; set; }
    [JsonProperty("usc_max_qty_per_trade")] public decimal UscMaxQtyPerTrade { get; set; }
    [JsonProperty("lnk_max_qty_per_trade")] public decimal LnkMaxQtyPerTrade { get; set; }
    [JsonProperty("aud_max_qty_per_trade")] public decimal AudMaxQtyPerTrade { get; set; }
    [JsonProperty("cad_max_qty_per_trade")] public decimal CadMaxQtyPerTrade { get; set; }
    [JsonProperty("chf_max_qty_per_trade")] public decimal ChfMaxQtyPerTrade { get; set; }
    [JsonProperty("cnh_max_qty_per_trade")] public decimal CnhMaxQtyPerTrade { get; set; }
    [JsonProperty("eur_max_qty_per_trade")] public decimal EurMaxQtyPerTrade { get; set; }
    [JsonProperty("gbp_max_qty_per_trade")] public decimal GbpMaxQtyPerTrade { get; set; }
    [JsonProperty("jpy_max_qty_per_trade")] public decimal JpyMaxQtyPerTrade { get; set; }
    [JsonProperty("mxn_max_qty_per_trade")] public decimal MxnMaxQtyPerTrade { get; set; }
    [JsonProperty("sgd_max_qty_per_trade")] public decimal SgdMaxQtyPerTrade { get; set; }
    [JsonProperty("usd_max_qty_per_trade")] public decimal UsdMaxQtyPerTrade { get; set; }
    [JsonProperty("bnb_max_qty_per_trade")] public decimal BnbMaxQtyPerTrade { get; set; }
    [JsonProperty("ksm_max_qty_per_trade")] public decimal KsmMaxQtyPerTrade { get; set; }
    [JsonProperty("trx_max_qty_per_trade")] public decimal TrxMaxQtyPerTrade { get; set; }
    [JsonProperty("uni_max_qty_per_trade")] public decimal UniMaxQtyPerTrade { get; set; }
    [JsonProperty("xtz_max_qty_per_trade")] public decimal XtzMaxQtyPerTrade { get; set; }
    [JsonProperty("dog_max_qty_per_trade")] public decimal DogMaxQtyPerTrade { get; set; }
    [JsonProperty("icp_max_qty_per_trade")] public decimal IcpMaxQtyPerTrade { get; set; }
    [JsonProperty("sol_max_qty_per_trade")] public decimal SolMaxQtyPerTrade { get; set; }
    [JsonProperty("mat_max_qty_per_trade")] public decimal MatMaxQtyPerTrade { get; set; }
    [JsonProperty("nzd_max_qty_per_trade")] public decimal NzdMaxQtyPerTrade { get; set; }
    [JsonProperty("bus_max_qty_per_trade")] public decimal BusMaxQtyPerTrade { get; set; }
    [JsonProperty("avx_max_qty_per_trade")] public decimal AvxMaxQtyPerTrade { get; set; }
    [JsonProperty("lun_max_qty_per_trade")] public decimal LunMaxQtyPerTrade { get; set; }
    [JsonProperty("bsv_max_qty_per_trade")] public decimal BsvMaxQtyPerTrade { get; set; }
    [JsonProperty("etc_max_qty_per_trade")] public decimal EtcMaxQtyPerTrade { get; set; }
    [JsonProperty("glm_max_qty_per_trade")] public decimal GlmMaxQtyPerTrade { get; set; }
    [JsonProperty("shb_max_qty_per_trade")] public decimal ShbMaxQtyPerTrade { get; set; }
    [JsonProperty("dai_max_qty_per_trade")] public decimal DaiMaxQtyPerTrade { get; set; }
    [JsonProperty("cmp_max_qty_per_trade")] public decimal CmpMaxQtyPerTrade { get; set; }
    [JsonProperty("etw_max_qty_per_trade")] public decimal EtwMaxQtyPerTrade { get; set; }
    [JsonProperty("atm_max_qty_per_trade")] public decimal AtmMaxQtyPerTrade { get; set; }
    [JsonProperty("ner_max_qty_per_trade")] public decimal NerMaxQtyPerTrade { get; set; }
    [JsonProperty("zar_max_qty_per_trade")] public decimal ZarMaxQtyPerTrade { get; set; }
    [JsonProperty("seh_max_qty_per_trade")] public decimal SehMaxQtyPerTrade { get; set; }
    [JsonProperty("reh_max_qty_per_trade")] public decimal RehMaxQtyPerTrade { get; set; }
    [JsonProperty("aav_max_qty_per_trade")] public decimal AavMaxQtyPerTrade { get; set; }
    [JsonProperty("try_max_qty_per_trade")] public decimal TryMaxQtyPerTrade { get; set; }
}
