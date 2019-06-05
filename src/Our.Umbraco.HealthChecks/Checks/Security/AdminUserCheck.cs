using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;

namespace Our.Umbraco.HealthChecks.Checks.Security
{
    [HealthCheck("42a3a15f-c2f0-48e7-ae5a-1237c5af5e35", "Admin User Check - (from Our.Umbraco.HealthChecks)",
    Description = "Check the admin user isn't called 'admin'",
    Group = "Security")]
    public class AdminUserCheck : HealthCheck
    {
        protected readonly IUserService _userService;
        protected readonly ILocalizedTextService _textService;
        private const string UserTypeAlias = "admin";

        public AdminUserCheck(ILocalizedTextService textService, IUserService userService)
        {
            _userService = userService;
            _textService = textService;
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

            var users = _userService.GetAll(0, 2, out long totalRecords, "username", Direction.Ascending, includeUserGroups: new[] { UserTypeAlias });
            var matchingUsers = users.Where(u => u.Name.InvariantEquals(UserTypeAlias) || u.Username.InvariantEquals(UserTypeAlias)).ToList();
            var success = !matchingUsers.Any();
            if (success)
            {
                message.AppendFormat(_textService.Localize("Our.Umbraco.HealthChecks/adminUserCheckNotFound"), UserTypeAlias);
            }
            else
            {
                message.AppendFormat(_textService.Localize("Our.Umbraco.HealthChecks/adminUserCheckFound"), UserTypeAlias);
                message.Append("<br/>");
                message.Append("<br/>");
                message.Append(_textService.Localize("Our.Umbraco.HealthChecks/adminUserRename"));
                message.Append("<br/>");
                message.Append("<br/>");
                message.Append("<ul>");
                foreach (var user in matchingUsers)
                {
                    message.Append("<li>");
                    message.AppendFormat(_textService.Localize("Our.Umbraco.HealthChecks/adminUserUser"), user.Id, user.Username, user.Name, user.Email);
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