using Insite.Core.Interfaces.Dependency;
using Insite.Core.SystemSetting;
using Insite.Core.SystemSetting.Groups;

namespace Extensions.CustomSettings
{
    [SettingsGroup(PrimaryGroupName = "Unified AR", Label = "Unified AR Settings")]
    public class UnifiedARSettings : BaseSettingsGroup, IExtension
    {
        [SettingsField(DisplayName = "UAR URL", Description = "The API URL for the UAR Integration.")]
        public virtual string UARUrl => GetValue(string.Empty);


        [SettingsField(DisplayName = "UAR Private Key", Description = "The Private key for the UAR integration.", IsEncrypted = true)]
        public virtual string UARPrivateKey => GetValue(string.Empty);


        [SettingsField(DisplayName = "Merchant Key", Description = "The Merchant key for the UAR integration.")]
        public virtual string MerchantKey => GetValue(string.Empty);


        [SettingsField(DisplayName = "PayNow Key", Description = "The PayNow key for the UAR integration.")]
        public virtual string PayNowKey => GetValue(string.Empty);
    }
}