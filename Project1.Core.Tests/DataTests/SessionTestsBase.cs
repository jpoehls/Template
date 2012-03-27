using System;
using NHibernate;
using NUnit.Framework;
using Project1.Core.Domain;

namespace Project1.Core.Tests.DataTests
{
    public class SessionTestsBase
    {
        private ITransaction _transaction;

        protected ISession Session { get; private set; }

        [SetUp]
        public void SetUp()
        {
            // new session on every test
            Session = FixtureSetup.Container.Resolve<ISessionFactory>().OpenSession();
            FixtureSetup.Container.Register(x => this.Session);
            _transaction = Session.BeginTransaction();
        }

        [TearDown]
        public void TearDown()
        {
            User.GetCurrent = () => null;
            //Session.CreateSQLQuery(@"delete from AliasToPersonLinks; delete from TransactionAliasLink; delete from Transactions; delete from Aliases; delete from UserToPersonLinks; delete from Users; delete from People").ExecuteUpdate();
            _transaction.Rollback();
            Session.Close();

        }

        private static readonly Random Random = new Random();

        protected static string RandomDigits(int length)
        {
            return Random.Next((int)Math.Pow(10, length - 1), (int)Math.Pow(10, length) - 1).ToString();
        }

        protected static string RandomLetters(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var buffer = new char[length];

            for (int i = 0; i < length; i++)
            {
                buffer[i] = chars[Random.Next(chars.Length)];
            }
            return new string(buffer);
        }

        protected static User NewUser()
        {
            var user = Fac.New<User>();
            using (User.Scope(user))
            {
                user.Password = "password";
            }
            return user;
        }

    }
}