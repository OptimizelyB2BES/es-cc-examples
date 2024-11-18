using System.Collections.Generic;

namespace Extensions
{
    public class Constants
    {
        public static string RfqAvailable = "rfqAvailable";
        public static string AccountingRole = "Accounting";
        public static string CpnList = "cpnList";
        public static string ESCommerceWarehouseBankNumber = "escommerce_warehouseBankNumber";
        public static string IsRma = "isRma";
        public static string ContactId = "ContactId";
        public static string IsPromotional = "IsPromotional";
        public static string PromotionalBadge = "PromotionalBadge";
        public static string PromotionalBadges = "PromotionalBadges";
        public static string P21RmaReasonCode = "P21RmaReasonCode";
        public static string SerialNumber = "serialNumber";
        public static string JobName = "jobName";
        public static string ProductErpNumber = "productErpNumber";
        public static string OrderTotalOperator = "orderTotalOperator";
        public static string OrderTotal = "orderTotal";

        // Invoice Payment Constants
        public static readonly string InvoicePaymentConnectionName = "ESCPayFabric_Test";

        public static readonly decimal VerificationAmount = 0.01M;
        public static readonly string PaymentBatch = "payment_batch";
        public static readonly string CompanyNumber = "company_no";
        public static readonly string CustomerNumber = "customer_id";
        public static readonly string InvoiceNumber = "invoice_no";
        public static readonly string AmountToPay = "amount_to_pay";
        public static readonly string TermsAmount = "terms_amount";
        public static readonly string AllowedAmount = "allowed_amount";
        public static readonly string TotalAmount = "total_amount";
        public static readonly string OrderNumber = "order_no";
        public static readonly string PONumber = "po_no";
        public static readonly string InvoiceDate = "invoice_date";
        public static readonly string NetDueDate = "net_due_date";
        public static readonly string TermsDueDate = "terms_due_date";
        public static readonly string PaymentType = "payment_type";
        public static readonly string PaymentAccountNo = "payment_account_no";
        public static readonly string WalletID = "escommerce_achAccountId";
        public static readonly string JobNameCustomerOrder = "escommerce_jobName";

        public static readonly string IntegrationJobParameter_TransactionLogLookbackDaysParamName = "Transaction Lookback Days";

        public static readonly string MailingAddress1CustomProperty = "mailingAddress1";
        public static readonly string MailingAddress2CustomProperty = "mailingAddress2";
        public static readonly string MailingAddress3CustomProperty = "mailingAddress3";
        public static readonly string MailingAddress4CustomProperty = "mailingAddress4";
        public static readonly string MailingCityCustomProperty = "mailingCity";
        public static readonly string MailingStateCustomProperty = "mailingState";
        public static readonly string MailingPostalCodeCustomProperty = "mailingPostalCode";
        public static readonly string MailingCountryCustomProperty = "mailingCountry";
        public static readonly string MailingLabelCustomProperty = "mailingLabel";

        public static readonly string IsMultiBrandCustomProperty = "isMultiBrand";
        public static readonly string SubBrandsListCustomProperty = "subBrandsList";
        public static readonly string SubBrandsListCartLinesCustomProperty = "subBrandsListCartLines";
        public static readonly List<string> CustomerMailingCustomPropertyNames = new List<string> { MailingAddress1CustomProperty, MailingAddress2CustomProperty, MailingAddress3CustomProperty, MailingAddress4CustomProperty, MailingCityCustomProperty, MailingStateCustomProperty, MailingPostalCodeCustomProperty, MailingCountryCustomProperty };

        public static readonly string CurrentBillToNumber = "currentBillToNumber";
        public static readonly string CurrentUserUserName = "currentUserUserName";

        public static readonly string CategoryAttributeName = "Categories";
        public static readonly string JobNameOrderHistory = "escommerce_oh_jobName";

    }
}