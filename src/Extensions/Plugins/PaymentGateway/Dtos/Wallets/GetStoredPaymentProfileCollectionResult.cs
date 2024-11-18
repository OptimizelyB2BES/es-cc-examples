using System.Collections.Generic;
using Insite.Core.Plugins.PaymentGateway.Dtos;
using Insite.Core.Services;

namespace Extensions.Plugins.PaymentGateway.Dtos.Wallets
{
    public class GetStoredPaymentProfileCollectionResult : ResultBase
    {
        public GetStoredPaymentProfileCollectionResult() => ResponseMessages = new List<string>();

        public ICollection<PaymentProfileDto> PaymentProfiles { get; set; }

        /// <summary>Gets or sets a value indicating whether the operation was successful.</summary>
        public bool Success { get; set; }

        public ICollection<string> ResponseMessages { get; set; }
    }
}