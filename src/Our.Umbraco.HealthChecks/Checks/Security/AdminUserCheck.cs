using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    [HealthCheck("42a3a15f-c2f0-48e7-ae5a-1237c5af5e35", "Admin User Check",
    Description = "Check the admin user isn't called 'admin'",
    Group = "Security")]
    public class AdminUserCheck : HealthCheck
    {
        protected readonly ILocalizedTextService TextService;
        private const string UserTypeAlias = "admin";

        public AdminUserCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            TextService = healthCheckContext.ApplicationContext.Services.TextService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckAdminUser() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new InvalidOperationException("AdminUserCheck has no executable actions");
        }

        private HealthCheckStatus CheckAdminUser()
        {
            StringBuilder message = new StringBuilder();

            var userService = ApplicationContext.Current.Services.UserService;
            int totalRecords = 0;
            var users = userService.GetAll(0, int.MaxValue, out totalRecords).Where(x => x.UserType.Alias.InvariantEquals(UserTypeAlias));
            var matchingUsers = users.Where(u => u.Name.InvariantEquals(UserTypeAlias) || u.Username.InvariantEquals(UserTypeAlias)).ToList();
            var success = !matchingUsers.Any();
            if (success)
            {
                message.AppendFormat(TextService.Localize("Our.Umbraco.HealthChecks/adminUserCheckNotFound"), UserTypeAlias);
            }
            else
            {
                message.AppendFormat(TextService.Localize("Our.Umbraco.HealthChecks/adminUserCheckFound"), UserTypeAlias);
                message.Append("<br/>");
                message.Append("<br/>");
                message.Append(TextService.Localize("Our.Umbraco.HealthChecks/adminUserRename"));
                message.Append("<br/>");
                message.Append("<br/>");
                message.Append("<ul>");
                foreach (var user in matchingUsers)
                {
                    message.Append("<li>");
                    message.AppendFormat(TextService.Localize("Our.Umbraco.HealthChecks/adminUserUser"), user.Id, user.Username, user.Name, user.Email);
                    message.Append("</li>");
                }
                message.Append("</ul>");

            }

            var actions = new List<HealthCheckAction>();

            return
                new HealthCheckStatus(message.ToString())
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }
    }
}