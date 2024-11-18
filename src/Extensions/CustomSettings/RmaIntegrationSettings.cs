using Insite.Core.Interfaces.Dependency;
using Insite.Core.SystemSetting;
using Insite.Core.SystemSetting.Groups;

namespace Extensions.CustomSettings
{
    [SettingsGroup(PrimaryGroupName = "RMAIntegration", Label = "RMA Integration")]
    public class RmaIntegrationSettings : BaseSettingsGroup, IExtension
    {
        [SettingsField(DisplayName = "Warranty Claim Rma Code", Description = "The value to use when submitting a Warranty Claim to P21", IsGlobal = false)]
        public virtual string WarrantyClaimRmaCode => this.GetValue("Warranty");

        [SettingsField(DisplayName = "Wisconsin Customers Warranty URL", Description = "The warranty claim URL for Wisconsin customers.", IsGlobal = false)]
        public virtual string WarrantyWiUrl => this.GetValue("https://escwiwarranty.freshdesk.com/support/login");

        [SettingsField(DisplayName = "Minnesota Customers Warranty URL", Description = "The warranty claim URL for Minnesota customers.", IsGlobal = false)]
        public virtual string WarrantyMnUrl => this.GetValue("https://escmnwarranty.freshdesk.com/support/login");
    }
}