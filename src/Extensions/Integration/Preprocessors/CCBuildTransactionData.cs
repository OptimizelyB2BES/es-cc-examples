using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Data.Entities;
using Insite.Data.Extensions;
using Insite.Integration.WebService.Interfaces;

namespace Extensions.Integration.Preprocessors
{
    [DependencyName("ESCBuildTransactionData")]
    public class JobPreprocessorBuildTransactionData : IJobPreprocessor
    {
        protected readonly IUnitOfWork UnitOfWork;

        public JobPreprocessorBuildTransactionData(IUnitOfWorkFactory unitOfWorkFactory)
        {
            UnitOfWork = unitOfWorkFactory.GetUnitOfWork();
        }

        public IJobLogger JobLogger { get; set; }

        public IntegrationJob IntegrationJob { get; set; }

        public virtual IntegrationJob Execute()
        {
            JobLogger?.Debug($"{nameof(JobPreprocessorBuildTransactionData)} Job Started");
            var transactionLookbackDaysParamVal = IntegrationJob.IntegrationJobParameters
                .FirstOrDefault(o => o.JobDefinitionParameter.Name == Constants.IntegrationJobParameter_TransactionLogLookbackDaysParamName)?.Value;
            double transactionLookbackDays = 1; // default to 1.

            if (!string.IsNullOrWhiteSpace(transactionLookbackDaysParamVal))
            {
                double.TryParse(transactionLookbackDaysParamVal, out transactionLookbackDays);
            }

            var targetDate = DateTime.Now.AddDays(transactionLookbackDays * -1);
            List<CreditCardTransaction> creditCardTransactionList = UnitOfWork.GetRepository<CreditCardTransaction>().GetTableAsNoTracking()
                .Include(c => c.CustomProperties)
                .Where(c => c.TransactionDate >= targetDate)
                .ToList(); // only retrieve transactions from 1 day back.

            var builder = new StringBuilder();
            builder.AppendLine($"invoice_no,amount,contact,transaction_date,transaction_id,reason");
            creditCardTransactionList.ForEach(c =>
            {
                try
                {
                    var user = UnitOfWork.GetRepository<UserProfile>().GetTableAsNoTracking().Expand(u => u.CustomProperties).Expand(u => u.Customers).FirstOrDefault(u => u.Customers.Select(cn => cn.CustomerNumber == c.CustomerNumber).FirstOrDefault());
                    var contactId = user?.GetProperty("ContactId", string.Empty);
                    var transactionId = c.GetProperty("transactionId", string.Empty);
                    var reason = c.GetProperty("reason", string.Empty);
                    JobLogger?.Debug($"Transaction: {c.InvoiceNumber}, {c.Amount}, {contactId}, {c.TransactionDate}, {transactionId}, {reason}");
                    builder.AppendLine($"{c.InvoiceNumber},{c.Amount},{contactId},{c.TransactionDate},{transactionId},{reason}");
                }
                catch (Exception)
                {
                    JobLogger?.Error($"Error reading transaction info. Skipping record.");
                }
            });
            IntegrationJob.InitialData = builder.ToString();

            JobLogger?.Debug($"{nameof(JobPreprocessorBuildTransactionData)} Job Completed");

            return IntegrationJob;
        }
    }
}
