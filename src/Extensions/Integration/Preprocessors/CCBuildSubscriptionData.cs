using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Interfaces.Providers;
using Insite.Data.Entities;
using Insite.Integration.WebService.Interfaces;

namespace Extensions.Integration.Preprocessors
{
    [DependencyName("ESCBuildSubscriptionData")]
    public class JobPreprocessorBuildSubscriptionData : IJobPreprocessor
    {
        protected readonly IUnitOfWork UnitOfWork;

        public JobPreprocessorBuildSubscriptionData(IUnitOfWorkFactory unitOfWorkFactory)
        {
            UnitOfWork = unitOfWorkFactory.GetUnitOfWork();
        }

        public IJobLogger JobLogger { get; set; }

        public IntegrationJob IntegrationJob { get; set; }

        public virtual IntegrationJob Execute()
        {

            using (var sqlConnection = new SqlConnection(ConnectionStringProvider.Current.ConnectionString))
            {
                sqlConnection.Open();

                var command = sqlConnection.CreateCommand();

                command.CommandText = @"SELECT pss.ProductErpNumber, pss.CreatedOn, pss.UserId, pss.WarehouseName 
                                        FROM [Extensions].[ProductStockSubscriptions] pss;";
                var reader = command.ExecuteReader();
                var builder = new StringBuilder();
                builder.AppendLine($"ProductErpNumber,CreatedOn,UserName,P21ContactId,WarehouseName");
                while (reader.Read())
                {
                    try
                    {
                        var userId = Guid.Parse(reader["UserId"].ToString());
                        var user = UnitOfWork.GetRepository<UserProfile>().Get(userId);
                        var erpNumber = reader["ProductErpNumber"].ToString();
                        var createdOn = reader["CreatedOn"].ToString();
                        var userName = user.UserName;
                        var contactId = user.CustomProperties?.FirstOrDefault(o => o.Name == "ContactId");
                        var warehouseName = reader["WarehouseName"].ToString();
                        JobLogger?.Debug($"Subscription: {erpNumber},{createdOn},{userName},{contactId},{warehouseName}");
                        builder.AppendLine($"{erpNumber},{createdOn},{userName},{contactId},{warehouseName}");
                    }
                    catch (Exception)
                    {
                        JobLogger?.Error($"Error reading subscription info. Skipping record.");
                        continue;
                    }
                }

                IntegrationJob.InitialData = builder.ToString();
            }


            return IntegrationJob;
        }

    }
}