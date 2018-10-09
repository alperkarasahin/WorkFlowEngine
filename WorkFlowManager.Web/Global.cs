using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Extensions;

namespace WorkFlowManager.Web
{

    public static class Global
    {

        public static string GetUserName()
        {

            var User = HttpContext.Current.User;
            return User.Identity.GetUserName();
        }

        public static List<ProjectRole> GetSystemRoles()
        {
            return ProjectRole.Admin.GetAllValues().Where(x => x.GetDescription() == "SYSTEM").ToList();
        }

        public static List<ProjectRole> GetUnitRoles()
        {
            return ProjectRole.Admin.GetAllValues().Where(x => x.GetDescription() == "UNIT").ToList();
        }

        public static List<ProjectRole> GetAllRoles()
        {
            return ProjectRole.Admin.GetAllValues().Where(x => x != ProjectRole.System).ToList();
        }

    }
}