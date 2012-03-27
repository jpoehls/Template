using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Project1.Core.Domain;

namespace Project1.Web.Areas.Admin.Models
{
    public class ManageUsersModel
    {
        public User.SearchParameters SearchParameters { get; set; }
        public IEnumerable<User> Users { get; set; }
    }
}