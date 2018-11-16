using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    [HealthCheck("42a3a15f-c2f0-48e7-ae5a-1237c5af5e35", "Admin User Check",
    Description = "Check the admin user isn't called 'admin'",
    Group = "Security")]
    public class AdminUserCheck : HealthCheck
    {
        public AdminUserCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
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
            const string userType = "admin";

            var userService = ApplicationContext.Current.Services.UserService;
            int totalRecords = 0;
            var users = userService.GetAll(0, int.MaxValue, out totalRecords).Where(x => x.UserType.Alias.InvariantEquals(userType));
            var matchingUsers = users.Where(u => u.Name.InvariantEquals(userType) || u.Username.InvariantEquals(userType)).ToList();
            var success = !matchingUsers.Any();
            if (success)
            {
                message.AppendFormat("There are no Administrator users with the name or username '<strong>{0}</strong>'", userType);
            }
            else
            {
                message.AppendFormat("We found an Administrator user with the name or username '<strong>{0}</strong>'", userType);
                message.Append("<br/>");
                message.Append("<br/>");
                message.Append("Please rename these users:");
                message.Append("<br/>");
                message.Append("<br/>");
                message.Append("<ul>");
                foreach (var user in matchingUsers)
                {
                    message.Append("<li>");
                    message.AppendFormat("<strong>Id</strong>: {0}, <strong>Username</strong>: {1}, <strong>Name</strong>: {2}, <strong>Email</strong>: {3}", user.Id, user.Username, user.Name, user.Email);
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