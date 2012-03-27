using System.Linq;
using NUnit.Framework;
using Project1.Core.Domain;

namespace Project1.Core.Tests.DataTests
{
    public class UserTests : SessionTestsBase
    {
        [Test]
        public void SaveNewUser()
        {
            var user = NewUser();
            Session.Save(user);
            Session.Flush();
        }

        [Test]
        public void SearchUsers()
        {
            var user = NewUser();
            user.Email = "test_email@example.com";
            Session.Save(user);
            Session.Flush();

            Assert.Contains(user,
                            User.Search(Session, new User.SearchParameters {Q = "test_email@example.com"}).ToList());
            Assert.Contains(user,
                            User.Search(Session, new User.SearchParameters {Q = "%@example.com"}).ToList());
            Assert.Contains(user,
                            User.Search(Session, new User.SearchParameters {Q = "test_email%"}).ToList());

            Assert.True(
                User.Search(Session, new User.SearchParameters {IsAdmin = false}).ToList()
                    .Contains(user));
            Assert.False(
                User.Search(Session, new User.SearchParameters {IsAdmin = true}).ToList()
                    .Contains(user));

            user.IsAdmin = true;
            Session.Save(user);
            Session.Flush();

            Assert.True(
                User.Search(Session, new User.SearchParameters {IsAdmin = true}).ToList()
                    .Contains(user));
            Assert.False(
                User.Search(Session, new User.SearchParameters {IsAdmin = false}).ToList()
                    .Contains(user));
        }
    }
}
