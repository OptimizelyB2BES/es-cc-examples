using Insite.Core.Interfaces.Dependency;
using Insite.Core.SystemSetting;
using Insite.Core.SystemSetting.Groups;

namespace Extensions.CustomSettings
{ 
    [SettingsGroup(PrimaryGroupName = "Unified AR Invoice", Label = "Unified AR Invoice Settings")]
    public class UnifiedARInvoiceSettings : BaseSettingsGroup, IExtension
    {
        [SettingsField(DisplayName = "UAR URL", Description = "The API URL for the UAR Integration.")]
        public virtual string UARUrl => this.GetValue(string.Empty);


        [SettingsField(DisplayName = "UAR Private Key", Description = "The Private key for the UAR integration.", IsEncrypted = true)]
        public virtual string UARPrivateKey => this.GetValue(string.Empty);


        [SettingsField(DisplayName = "Merchant Key", Description = "The Merchant key for the UAR integration.")]
        public virtual string MerchantKey => this.GetValue(string.Empty);


        [SettingsField(DisplayName = "Invoice API Base URL", Description = "The Base URL for the Invoice API.")]
        public virtual string InvoiceURL => this.GetValue(string.Empty);
    }
}
