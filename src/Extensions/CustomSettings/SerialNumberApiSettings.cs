using Insite.Core.Interfaces.Dependency;
using Insite.Core.SystemSetting;
using Insite.Core.SystemSetting.Groups;

namespace Extensions.CustomSettings
{
    [SettingsGroup(PrimaryGroupName = "SerialNumber", Label = "Serial Number Settings")]
    public class SerialNumberApiSettings : BaseSettingsGroup, IExtension
    {
        [SettingsField(DisplayName = "Serial Number API Client Id", Description = "Client Id used for the serial number API.")]
        public virtual string ClientId => GetValue(string.Empty);

        [SettingsField(DisplayName = "Serial Number Client Secret", Description = "Password used for the serial number API.", IsEncrypted = true)]
        public virtual string ClientSecret => GetValue(string.Empty);

        [SettingsField(DisplayName = "Serial Number API Token URL", Description = "URL used to obtain an access token.")]
        public virtual string SerialNumberApiTokenUrl => GetValue("https://erp.escommerce.com:4001/token");

        [SettingsField(DisplayName = "Serial Naumber API URL", Description = "URL used to call the serial number API.")]
        public virtual string SerialNumberApiUrl => GetValue("https://erp.escommerce.com:4001");

        [SettingsField(DisplayName = "Minimum Serial Number Length", Description = "The minimum number of characters needed before a Serial Number API search is triggered.")]
        public virtual int MinimumSerialNumberLength => GetValue(4);
    }
}