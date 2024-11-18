using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Pipelines.Pricing;
using Insite.Core.Plugins.PromotionEngine;
using Insite.Core.Plugins.Utilities;
using Insite.Data.Entities;

namespace Extensions.Plugins.PromotionEngine
{

    [DependencyName("AmountOffOrder")]
    public class DiscountAmountOffOrder : PromotionResultServiceBase
    {
        public DiscountAmountOffOrder(IPromotionAmountProvider promotionProvider, IPricingPipeline pricingPipeline)
            : base(pricingPipeline, promotionProvider)
        {
        }

        public override string DisplayName => "Discount Amount Off Order";

        /// <summary>The parameter descriptions.</summary>
        public override Dictionary<string, PromotionResultParameter> ParameterDescriptions => new Dictionary<string, PromotionResultParameter>
        {
            { nameof(PromotionResult.Amount), new PromotionResultParameter { Label = "Amount", ValueType = "number" } },
        };

        /// <summary>The amount off order.</summary>
        /// <param name="customerOrder">The customer order.</param>
        /// <returns>The <see cref="decimal"/>.</returns>
        public override decimal AmountOffOrder(CustomerOrder customerOrder)
        {
            //if (!this.PromotionResult.Amount.HasValue)
            //{
            //    throw new ArgumentNullException(nameof(this.PromotionResult.Amount));
            //}

            //if (this.PromotionResult.Amount.Value == 0)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(this.PromotionResult.Amount));
            //}

            return PromotionResult.Amount.Value;
        }

        /// <summary>The apply promotion result.</summary>
        /// <param name="customerOrder">The customer order.</param>
        public override void ApplyPromotionResult(CustomerOrder customerOrder)
        {
            var appliedPromotion = customerOrder.CustomerOrderPromotions.FirstOrDefault(p => p.PromotionId == PromotionResult.PromotionId && p.PromotionResultId == PromotionResult.Id);
            AddOrUpdateCustomerOrderPromotion(customerOrder, appliedPromotion, null, PromotionResult);
        }

        /// <summary>The clear promotion result.</summary>
        /// <param name="customerOrder">The customer order.</param>
        public override void ClearPromotionResult(CustomerOrder customerOrder)
        {
        }
    }
}