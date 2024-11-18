namespace Extensions.Modules.Account.Services.Pipelines.Pipes.GetRoles
{
    using System.Collections.Generic;

    using Insite.Account.Services.Pipelines.Parameters;
    using Insite.Account.Services.Pipelines.Results;
    using Insite.Core.Interfaces.Data;
    using Insite.Core.Plugins.Pipelines;
    using Insite.Core.Security;
    using Insite.Core.SystemSetting.Groups.OrderManagement;

    public class GetRoles : IPipe<GetRolesParameter, GetRolesResult>
    {
        private readonly BudgetsAndOrderApprovalSettings budgetsAndOrderApprovalSettings;

        private readonly RequisitionsSettings requisitionsSettings;

        public int Order => 100;

        public GetRoles(BudgetsAndOrderApprovalSettings budgetsAndOrderApprovalSettings, RequisitionsSettings requisitionsSettings)
        {
            this.budgetsAndOrderApprovalSettings = budgetsAndOrderApprovalSettings;
            this.requisitionsSettings = requisitionsSettings;
        }

        public GetRolesResult Execute(IUnitOfWork unitOfWork, GetRolesParameter parameter, GetRolesResult result)
        {
            var standardRoles = new List<string> { BuiltInRoles.Administrator };

            if (budgetsAndOrderApprovalSettings.BudgetManagementEnabled)
            {
                standardRoles.Add(BuiltInRoles.Buyer1);
                standardRoles.Add(BuiltInRoles.Buyer2);
                standardRoles.Add(BuiltInRoles.Buyer3);
            }

            if (requisitionsSettings.Enabled)
            {
                standardRoles.Add(BuiltInRoles.Requisitioner);
            }

            //AMR-337 Add accounting role to user administration
            standardRoles.Add(Constants.AccountingRole);
            standardRoles.Sort();

            return new GetRolesResult
            {
                StandardRoles = standardRoles,
                RolesThatCanBeApprovers = new List<string> { BuiltInRoles.Administrator, BuiltInRoles.Buyer3 },
                RolesThatRequireApprover = new List<string> { BuiltInRoles.Requisitioner, BuiltInRoles.Buyer1, BuiltInRoles.Buyer2 },
                RolesThatCanViewApprovals = new List<string> { BuiltInRoles.Administrator, BuiltInRoles.Buyer1, BuiltInRoles.Buyer2, BuiltInRoles.Buyer3 },
                RolesThatCanViewUsers = new List<string> { BuiltInRoles.Administrator },
                RolesThatCanAddUsers = new List<string> { BuiltInRoles.Administrator },
                RolesThatCanUpdateUsers = new List<string> { BuiltInRoles.Administrator }
            };
        }
    }
}