using Insite.Core.Interfaces.Dependency;
using Insite.Core.SystemSetting;
using Insite.Core.SystemSetting.Groups;

namespace Extensions.CustomSettings
{
    [SettingsGroup(PrimaryGroupName = "RegisterForm", Label = "Register Form Settings")]
    public class RegisterFormSettings : BaseSettingsGroup, IExtension
    {
        [SettingsField(DisplayName = "Destination Email", Description = "Email the new customer registration forms will be sent to.")]
        public virtual string DestinationEmail => GetValue(string.Empty);

        [SettingsField(DisplayName = "From Email", Description = "The from address for new customer emails.")]
        public virtual string FromEmail => GetValue(string.Empty);

    }
}