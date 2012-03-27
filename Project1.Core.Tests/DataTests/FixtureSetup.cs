using System;
using Harden;
using Munq;
using Munq.LifetimeManagers;
using NUnit.Framework;
using Project1.Core.Infrastructure;

namespace Project1.Core.Tests.DataTests
{
    [SetUpFixture]
    public class FixtureSetup
    {
        public static IocContainer Container;

        [SetUp]
        public void RunBeforeAnyTests()
        {
            //NHibernateProfiler.Initialize();
            Container = new IocContainer();

            Fac.Default.Interceptors.Add(t => new HardenInterceptor());
            {
                Fac.Default.SetServiceLocator(Container.Resolve, Container.CanResolve);
            }

            NHibernateConfigurator.Configure(Container, new ThreadLocalStorageLifetime());
        }
    }
}