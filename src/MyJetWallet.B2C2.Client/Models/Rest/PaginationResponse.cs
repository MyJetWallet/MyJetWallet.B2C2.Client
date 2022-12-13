namespace MyJetWallet.B2C2.Client.Models.Rest
{
    public class PaginationResponse<T>
    {
        public T Data { get; set; }
        public string Previous { get; set; }
        public string Next { get; set; }
    }
}
