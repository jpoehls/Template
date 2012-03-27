using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Project1.Core.Domain;

namespace Project1.Web.Infrastructure
{
    public class RoleProvider : System.Web.Security.RoleProvider
    {
        public static Func<string, User> GetUser { get; set; }

        public override bool IsUserInRole(string username, string roleName)
        {
            return GetRolesForUser(username).Contains(roleName);
        }

        public override string[] GetRolesForUser(string username)
        {
            return YieldRoles(username).ToArray();
        }
        IEnumerable<string> YieldRoles(string username)
        {
            var user = GetUser(username);
            if (user.IsAdmin) yield return "Admin";
            if (user.IsContentAdmin) yield return "Content Admin";
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName { get; set; }
    }
}