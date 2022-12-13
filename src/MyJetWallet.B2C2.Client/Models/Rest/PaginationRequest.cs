namespace MyJetWallet.B2C2.Client.Models.Rest
{
    public class PaginationRequest
    {
        public string Cursor { get; set; }
        public int Limit { get; set; } = 50;
    }
}
