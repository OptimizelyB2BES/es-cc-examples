using Insite.Core.Security;
using Insite.Core.SystemSetting;
using Insite.Core.SystemSetting.Groups;
using Insite.Core.SystemSetting.Groups.Integration;

namespace Extensions.CustomSettings
{
    [SettingsGroup(PrimaryGroupName = "Integration", Label = "Order Submit", SortOrder = 4)]
    public class ESCOrderSubmitSettings : BaseSettingsGroup
    {
        [SettingsField(DisplayName = "Wait for Real Time Order Submit Results", Description = "Determines if the system should wait for the real-time order submit results before continuing the order submit process.", IsGlobal = false, SortOrder = 4)]
        [SettingsFieldAuthorizationRequired(AllowedRoles = new[] { BuiltInRoles.ISC_System, BuiltInRoles.ISC_Implementer })]
        [SettingsFieldDependency(typeof(OrderSubmitSettings), "UseRealTimeOrderSubmit", "true")]
        public virtual bool WaitforRealTimeOrderSubmitResults=> this.GetValue(true);

        [SettingsField(DisplayName = "Call Wallet ID Job", Description = "Determines if the system should call the wallet ID job as part of order submit.", IsGlobal = false, SortOrder = 5)]
        [SettingsFieldAuthorizationRequired(AllowedRoles = new[] { BuiltInRoles.ISC_System, BuiltInRoles.ISC_Implementer })]
        public virtual bool CallWalletIdJob => this.GetValue(false);
    }
}