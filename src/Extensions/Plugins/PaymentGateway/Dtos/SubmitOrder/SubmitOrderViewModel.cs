using Insite.Core.Plugins.PaymentGateway.Dtos;

namespace Extensions.Plugins.PaymentGateway.Dtos.SubmitOrder
{
    public class SubmitOrderViewModel
    {
        public string OrderNumber { get; set; }
        public decimal Amount { get; set; }
        public CreditCardDto CreditCardModel { get; set; }
        public string ACHAccountId { get; set; }
        public string PaymentType { get; set; }
    }
}