using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Text;
using NHibernate.AdoNet;
using NHibernate.AdoNet.Util;
using NHibernate.Driver;
using NHibernate.Exceptions;
using NHibernate.Util;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace MiniProfilerContrib.NHibernate
{
    // SOURCE: https://gist.github.com/1110153
    public partial class ProfiledSql2008ClientDriver : Sql2008ClientDriver
    {
        public override IDbCommand CreateCommand()
        {
            IDbCommand command = base.CreateCommand();

            if (MiniProfiler.Current != null)
                command = DbCommandProxy.CreateProxy(command);

            return command;
        }
    }

    public class DbCommandProxy : RealProxy
    {
        private DbCommand instance;
        private IDbProfiler profiler;

        private DbCommandProxy(DbCommand instance) : base(typeof(DbCommand))
        {
            this.instance = instance;
            this.profiler = MiniProfiler.Current as IDbProfiler;
        }

        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage methodMessage = new MethodCallMessageWrapper((IMethodCallMessage)msg);

            var executeType = GetExecuteType(methodMessage);

            if (executeType != ExecuteType.None)
                profiler.ExecuteStart(instance, executeType);

            object returnValue = methodMessage.MethodBase.Invoke(instance, methodMessage.Args);

            if (executeType == ExecuteType.Reader)
                returnValue = new ProfiledDbDataReader((DbDataReader)returnValue, instance.Connection, profiler);

            IMessage returnMessage = new ReturnMessage(returnValue, methodMessage.Args, methodMessage.ArgCount, methodMessage.LogicalCallContext, methodMessage);

            if (executeType == ExecuteType.Reader)
                profiler.ExecuteFinish(instance, executeType, (DbDataReader)returnValue);
            else if (executeType != ExecuteType.None)
                profiler.ExecuteFinish(instance, executeType, null);

            return returnMessage;
        }

        private static ExecuteType GetExecuteType(IMethodCallMessage message)
        {
            switch (message.MethodName)
            {
                case "ExecuteNonQuery":
                    return ExecuteType.NonQuery;
                case "ExecuteReader":
                    return ExecuteType.Reader;
                case "ExecuteScalar":
                    return ExecuteType.Scalar;
                default:
                    return ExecuteType.None;
            }
        }

        public static IDbCommand CreateProxy(IDbCommand instance)
        {
            var proxy = new DbCommandProxy(instance as DbCommand);

            return proxy.GetTransparentProxy() as IDbCommand;
        }
    }

    // FIX: Unable to cast transparent proxy to type 'System.Data.SqlClient.SqlCommand'
    // SOURCE: http://www.adverseconditionals.com/2011/08/fixing-miniprofiler-with-nhibernate.html
    public partial class ProfiledSql2008ClientDriver : IEmbeddedBatcherFactoryProvider
    {
        System.Type IEmbeddedBatcherFactoryProvider.BatcherFactoryClass
        {
            get { return typeof(ProfiledSqlClientBatchingBatcherFactory); }
        }
    }
    public class ProfiledSqlClientBatchingBatcherFactory : SqlClientBatchingBatcherFactory
    {
        public override global::NHibernate.Engine.IBatcher CreateBatcher(ConnectionManager connectionManager, global::NHibernate.IInterceptor interceptor)
        {
            return new ProfiledSqlClientBatchingBatcher(connectionManager, interceptor);
        }
    }
    public class ProfiledSqlClientBatchingBatcher : AbstractBatcher
    {
        private int batchSize;
        private int totalExpectedRowsAffected;
        private SqlClientSqlCommandSet currentBatch;
        private StringBuilder currentBatchCommandsLog;
        private readonly int defaultTimeout;

        public ProfiledSqlClientBatchingBatcher(ConnectionManager connectionManager, global::NHibernate.IInterceptor interceptor)
            : base(connectionManager, interceptor)
        {
            batchSize = Factory.Settings.AdoBatchSize;
            defaultTimeout = PropertiesHelper.GetInt32(global::NHibernate.Cfg.Environment.CommandTimeout, global::NHibernate.Cfg.Environment.Properties, -1);

            currentBatch = CreateConfiguredBatch();
            //we always create this, because we need to deal with a scenario in which
            //the user change the logging configuration at runtime. Trying to put this
            //behind an if(log.IsDebugEnabled) will cause a null reference exception 
            //at that point.
            currentBatchCommandsLog = new StringBuilder().AppendLine("Batch commands:");
        }

        public override int BatchSize
        {
            get { return batchSize; }
            set { batchSize = value; }
        }

        protected override int CountOfStatementsInCurrentBatch
        {
            get { return currentBatch.CountOfCommands; }
        }

        public override void AddToBatch(IExpectation expectation)
        {
            totalExpectedRowsAffected += expectation.ExpectedRowCount;
            IDbCommand batchUpdate = CurrentCommand;
            Driver.AdjustCommand(batchUpdate);
            string lineWithParameters = null;
            var sqlStatementLogger = Factory.Settings.SqlStatementLogger;
            if (sqlStatementLogger.IsDebugEnabled || log.IsDebugEnabled)
            {
                lineWithParameters = sqlStatementLogger.GetCommandLineWithParameters(batchUpdate);
                var formatStyle = sqlStatementLogger.DetermineActualStyle(FormatStyle.Basic);
                lineWithParameters = formatStyle.Formatter.Format(lineWithParameters);
                currentBatchCommandsLog.Append("command ")
                    .Append(currentBatch.CountOfCommands)
                    .Append(":")
                    .AppendLine(lineWithParameters);
            }
            if (log.IsDebugEnabled)
            {
                log.Debug("Adding to batch:" + lineWithParameters);
            }
            if (batchUpdate is System.Data.SqlClient.SqlCommand)
                currentBatch.Append((System.Data.SqlClient.SqlCommand)batchUpdate);
            else
            {
                var sqlCommand = new System.Data.SqlClient.SqlCommand(
                batchUpdate.CommandText,
                (SqlConnection)batchUpdate.Connection,
                (SqlTransaction)batchUpdate.Transaction);
                foreach (SqlParameter p in batchUpdate.Parameters)
                {
                    sqlCommand.Parameters.Add(
                    new SqlParameter(
                    p.ParameterName,
                    p.SqlDbType,
                    p.Size,
                    p.Direction,
                    p.IsNullable,
                    p.Precision,
                    p.Scale,
                    p.SourceColumn,
                    p.SourceVersion,
                    p.Value));
                }
                currentBatch.Append(sqlCommand);
            }

            if (currentBatch.CountOfCommands >= batchSize)
            {
                ExecuteBatchWithTiming(batchUpdate);
            }
        }

        protected override void DoExecuteBatch(IDbCommand ps)
        {
            log.DebugFormat("Executing batch");
            CheckReaders();
            Prepare(currentBatch.BatchCommand);
            if (Factory.Settings.SqlStatementLogger.IsDebugEnabled)
            {
                Factory.Settings.SqlStatementLogger.LogBatchCommand(currentBatchCommandsLog.ToString());
                currentBatchCommandsLog = new StringBuilder().AppendLine("Batch commands:");
            }

            int rowsAffected;
            try
            {
                rowsAffected = currentBatch.ExecuteNonQuery();
            }
            catch (DbException e)
            {
                throw ADOExceptionHelper.Convert(Factory.SQLExceptionConverter, e, "could not execute batch command.");
            }

            Expectations.VerifyOutcomeBatched(totalExpectedRowsAffected, rowsAffected);

            currentBatch.Dispose();
            totalExpectedRowsAffected = 0;
            currentBatch = CreateConfiguredBatch();
        }

        private SqlClientSqlCommandSet CreateConfiguredBatch()
        {
            var result = new SqlClientSqlCommandSet();
            if (defaultTimeout > 0)
            {
                try
                {
                    result.CommandTimeout = defaultTimeout;
                }
                catch (Exception e)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn(e.ToString());
                    }
                }
            }

            return result;
        }
    }
}