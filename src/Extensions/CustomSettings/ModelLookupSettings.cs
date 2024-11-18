using System;
using System.ComponentModel;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.SystemSetting;
using Insite.Core.SystemSetting.Groups;

namespace Extensions.CustomSettings
{
    [SettingsGroup(PrimaryGroupName = "ModelLookup", Label = "Model Lookup Settings")]
    public class ModelLookupSettings : BaseSettingsGroup, IExtension
    {
        [SettingsField(DisplayName = "Token Request URL", Description = "URL used to make the authorization token request.")]
        public virtual string TokenRequestUrl => GetValue(string.Empty);

        [SettingsField(DisplayName = "Token Request Username", Description = "Username used for the token request.")]
        public virtual string TokenRequestUsername => GetValue(string.Empty);

        [SettingsField(DisplayName = "Token Request Password", Description = "Password used for the token request.", IsEncrypted = true)]
        public virtual string TokenRequestPassword => GetValue(string.Empty);

        [SettingsField(DisplayName = "Token Authorization Username", Description = "Username used for the token request authorization.")]
        public virtual string TokenAuthorizationUsername => GetValue(string.Empty);

        [SettingsField(DisplayName = "Token Authorization Password", Description = "Password used for the token request authorization.", IsEncrypted = true)]
        public virtual string TokenAuthorizationPassword => GetValue(string.Empty);

        [SettingsField(DisplayName = "Model Lookup URL", Description = "URL used to look up model numbers against the Epic API.")]
        public virtual string ModelLookupUrl => GetValue(string.Empty);
    }
}