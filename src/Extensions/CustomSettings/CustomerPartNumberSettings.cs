using Insite.Core.Interfaces.Dependency;
using Insite.Core.SystemSetting;
using Insite.Core.SystemSetting.Groups;

namespace Extensions.CustomSettings
{
    [SettingsGroup(PrimaryGroupName = "CustomerPartNumber", Label = "Customer Part Number Settings")]
    public class CustomerPartNumberSettings : BaseSettingsGroup, IExtension
    {
        [SettingsField(DisplayName = "Customer Part Number API Username", Description = "Username used for the customer part number API.")]
        public virtual string ApiUsername => GetValue(string.Empty);

        [SettingsField(DisplayName = "Customer Part Number API Password", Description = "Password used for the customer part number API.", IsEncrypted = true)]
        public virtual string ApiPassword => GetValue(string.Empty);

        [SettingsField(DisplayName = "Customer Part Number Maximum Length", Description = "Maximum number of characters allowed for a customer part number.")]
        public virtual int CustomerPartNumberMaximumLength => GetValue(40);

        [SettingsField(
            DisplayName = "Customer Part Number Disallowed Characters",
            Description = "Characters not allow for customer part numbers. Internal data separated by |#|.",
            ControlType = SystemSettingControlType.MultipleValue,
            ValueSeparator = "|#|"
        )]
        public virtual string DisallowedCharacters => GetValue("~|#|`|#|!|#|@|#|%|#|^|#|_|#|\\|#|:|#|'|#|\"|#|&");
    }
}