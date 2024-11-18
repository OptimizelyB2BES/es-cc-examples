using Insite.Core.Interfaces.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Services.Handlers;
using Insite.Websites.Services.Parameters;
using Insite.Websites.Services.Results;
using Insite.Core.Interfaces.Data;
using System.Threading;
using Insite.Core.SystemSetting.Groups.OrderManagement;
using Insite.Common.Providers;
using Extensions.CustomSettings;

namespace Extensions.Modules.Websites.Services.Handlers.GetSettingsCollection
{
    [DependencyName(nameof(ESCGetSettingsCollection))]
    public class ESCGetSettingsCollection : HandlerBase<GetSettingsCollectionParameter, GetSettingsCollectionResult>
    {
        public override int Order => 401;

        private readonly CheckoutSettings checkoutSettings;
        private readonly SerialNumberApiSettings serialNumberApiSettings;

        public ESCGetSettingsCollection(CheckoutSettings checkoutSettings, SerialNumberApiSettings serialNumberApiSettings)
        {
            this.checkoutSettings = checkoutSettings;
            this.serialNumberApiSettings = serialNumberApiSettings;
        }

        public override GetSettingsCollectionResult Execute(IUnitOfWork unitOfWork, GetSettingsCollectionParameter parameter, GetSettingsCollectionResult result)
        {
            result.Properties.Add("acceptedCreditCards", this.checkoutSettings.AcceptedCreditCards.Split('|').Select(o => new KeyValuePair<string, string>(o, o.ToUpper())).ToList().ToJson());
            var dateTimeFormat = Thread.CurrentThread.CurrentCulture.DateTimeFormat;
            result.Properties.Add("expirationMonths", Enumerable.Range(1, 12).Select(o => new KeyValuePair<string, int>(dateTimeFormat.GetMonthName(o), o)).ToList().ToJson());
            result.Properties.Add("expirationYears", Enumerable.Range(DateTimeProvider.Current.Now.Year, 10).Select(o => new KeyValuePair<int, int>(o, o)).ToList().ToJson());
            result.Properties.Add("minimumSerialNumberLength", this.serialNumberApiSettings.MinimumSerialNumberLength.ToString());

            return this.NextHandler.Execute(unitOfWork, parameter, result);
        }
    }
}