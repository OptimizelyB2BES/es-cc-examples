using Insite.Core.Interfaces.Dependency;
using Insite.Core.SystemSetting;
using Insite.Core.SystemSetting.Groups;

namespace Extensions.CustomSettings
{
    [SettingsGroup(PrimaryGroupName = "BMO ACH", Label = "BMO ACH Payment Settings")]
    public class BmoSettings : BaseSettingsGroup, IExtension
    {
        [SettingsField(DisplayName = "BMO URL", Description = "The API URL for the BMO integration.")]
        public virtual string BmoUrl => GetValue(string.Empty);
        [SettingsField(DisplayName = "BMO API Key", Description = "The API key for the BMO integration.")]
        public virtual string ApiKey => GetValue(string.Empty);

    }
}