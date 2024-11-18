using System;
using System.ComponentModel;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.SystemSetting;
using Insite.Core.SystemSetting.Groups;

namespace Extensions.CustomSettings
{
    [SettingsGroup(PrimaryGroupName = "AccountManagement", Label = "Co-op Statements")]
    public class CoopStatementSettings : BaseSettingsGroup, IExtension
    {
        [SettingsField(DisplayName = "Co-op Statement API Username", Description = "Username for the Co-op Statement API user account.")]
        public virtual string CoopStatementUserName => GetValue(string.Empty);
        [SettingsField(DisplayName = "Co-op Statement API Password", Description = "Password for the Co-op Statement API user account.", IsEncrypted = true)]
        public virtual string CoopStatementPassword => GetValue(string.Empty);
    }
}