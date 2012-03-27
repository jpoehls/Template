using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using Harden.ValidationAttributes;
using Munq;
using NHibernate;
using NHibernate.Instantiation;
using NHibernate.Tool.hbm2ddl;
using NhCodeFirst;
using NhCodeFirst.Conventions;
using Project1.Core.Domain;
using Environment = NHibernate.Cfg.Environment;

namespace Project1.Core.Infrastructure
{
    public class NHibernateConfigurator
    {
        public static void Configure(IocContainer container, ILifetimeManager lifetime)
        {
            CreateComponentMappedProperties
                .AddRuleForIdentifyingComponents(
                    x => x.ReturnType().GetCustomAttributes(typeof(ValueTypeAttribute), false).Any());

            var cs = ConfigurationManager.ConnectionStrings["Project1"].ConnectionString;

            var configuration = ConfigurationBuilder.New(true)
                .ForSql2008(cs)
                .MapEntities(typeof (User).Assembly.GetTypes(), MatchEntities.WithIdProperty);
            //Environment.UseReflectionOptimizer = false;
            configuration.SetProperty(Environment.GenerateStatistics, "true");
            configuration.SetProperty(Environment.BatchSize, "10");
            //configuration.SetProperty(Environment.ConnectionDriver,
            //                          typeof(ProfiledSql2008ClientDriver).AssemblyQualifiedName);

            new SchemaUpdate(configuration).Execute(s => Debug.WriteLine(s), true);

            {
                //Set up some black magic that allows us to use Fac to create our entities
                Fac.Default.Interceptors.Add(t => new MarkerInterceptor(t));
                Fac.Default.TypesToImplement.Add(t => typeof (IMarkerInterface));
                configuration.InterceptObjectInstantiation(container.Resolve<ISessionFactory>, Fac.New);
            }
            var sf = configuration.BuildSessionFactory();
            NullabilityRules.AddNotNullAttribute<NotNullAttribute>();

            //NhCodeFirst.UserTypes.SerializedUserType.Deserialize = JsonSerializer.DeserializeFromString;
            //NhCodeFirst.UserTypes.SerializedUserType.Serialize = JsonSerializer.SerializeToString;

            configuration.ExecuteAuxilliaryDatabaseScripts(sf);


            container.RegisterInstance(sf);
            container.Register(k => k.Resolve<ISessionFactory>().OpenSession()).WithLifetimeManager(lifetime);
        }
    }
}
