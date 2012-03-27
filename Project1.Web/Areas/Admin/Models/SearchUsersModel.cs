using System.Collections.Generic;
using Project1.Core.Domain;

namespace Project1.Web.Areas.Admin.Models
{
    public class SearchUsersModel
    {
        public string SearchString { get; set; }
        public IEnumerable<User> Users { get; set; }
    }
}