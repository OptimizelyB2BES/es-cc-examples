using Insite.Core.Plugins.PaymentGateway.Dtos;
using System.Collections.Generic;

namespace Extensions.Plugins.PaymentGateway.Models.InvoicePayment
{
    public class InvoicePaymentViewModel
    {
        public InvoicePaymentViewModel()
        {
            this.SelectedInvoice = new List<InvoicePaymentModel>();
            this.CreditCardModel = new CreditCardDto();
        }

        public List<InvoicePaymentModel> SelectedInvoice { get; set; }
        public CreditCardDto CreditCardModel { get; set; }
        public string AchAccountId { get; set; }
    }
}