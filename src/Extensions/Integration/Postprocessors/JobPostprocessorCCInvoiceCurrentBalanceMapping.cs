using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Data.Entities;
using Insite.Integration.WebService.Interfaces;
using Insite.Integration.WebService.PlugIns.Postprocessor.FieldMap;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace Extensions.Integration.Postprocessors
{
    [DependencyName("JobPostprocessorInvoiceCurrentBalanceMapping")]
    public class JobPostprocessorInvoiceCurrentBalanceMapping : IJobPostprocessor
    {
        public IntegrationJob IntegrationJob { get; set; }
        public IJobLogger JobLogger { get; set; }
        public IUnitOfWork UnitOfWork { get; }
        public JobPostprocessorFieldMap JobPostprocessorFieldMap { get; }

        public JobPostprocessorInvoiceCurrentBalanceMapping(IUnitOfWorkFactory unitOfWorkFactory, JobPostprocessorFieldMap jobPostprocessorFieldMap)
        {
            UnitOfWork = unitOfWorkFactory.GetUnitOfWork();
            JobPostprocessorFieldMap = jobPostprocessorFieldMap;
        }

        public void Execute(DataSet dataSet, CancellationToken cancellationToken)
        {
            JobPostprocessorFieldMap.IntegrationJob = IntegrationJob;
            JobPostprocessorFieldMap.JobLogger = JobLogger;

            if (dataSet.Tables == null || dataSet.Tables.Count < 1)
            {
                JobLogger.Debug($"No tables in dataset. Exiting.");
                return;
            }

            JobLogger.Debug($"Invoice_hdr row count: {dataSet.Tables[0].Rows.Count}");
            var index = 0;
            decimal invoiceBalance;

            var invoiceTable = UnitOfWork.GetRepository<InvoiceHistory>().GetTableAsNoTracking();

            var incomingInvoiceNumbers = dataSet.Tables[0]
                .Select("invoice_no is not null")
                .Select(o => o["invoice_no"].ToString().ToLower());

            var incomingInvoices = invoiceTable
                .Where(o => incomingInvoiceNumbers.Contains(o.InvoiceNumber))
                .ToDictionary(o => o.InvoiceNumber.ToLower());

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                try
                {
                    if (row["invoice_no"] != null)
                    {
                        string invoiceNumber = row["invoice_no"].ToString().ToLower();
                        if (!string.IsNullOrWhiteSpace(invoiceNumber) &&
                            incomingInvoices.TryGetValue(invoiceNumber, out InvoiceHistory invoice))
                        {
                            invoiceBalance = 0M;
                            if (row["InvoiceBalance"] != null)
                            {
                                invoiceBalance = Math.Abs(Convert.ToDecimal(row["InvoiceBalance"]));
                                if (invoiceBalance > Math.Abs(invoice.CurrentBalance))
                                {
                                    row["InvoiceBalance"] = invoice.CurrentBalance;
                                }
                            }

                            index++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    JobLogger.Error($"Error occurred processing row: {ex.Message} {ex.InnerException} Index: {index}");
                }
            }
            JobPostprocessorFieldMap.Execute(dataSet, cancellationToken);
        }

        public void Cancel()
        {
        }
    }
}