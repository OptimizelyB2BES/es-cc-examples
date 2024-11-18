namespace Extensions.Plugins.PaymentGateway.Models.InvoicePayment
{
    public class WalletPageConnectionModel
    {
        public string CustomerNumber { get; set; }

        public string Tender { get; set; }

        public string Token { get; set; }
    }
}