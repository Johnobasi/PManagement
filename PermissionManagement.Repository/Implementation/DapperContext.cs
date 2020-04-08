using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using Dapper;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace PermissionManagement.Repository
{ 
    public class DapperContext : IDisposable
    {
        private string connectionString;
        private bool connectionOnDemand = ConfigurationManager.AppSettings["ConnectionOnDemand"] == null || ConfigurationManager.AppSettings["ConnectionOnDemand"] == "true" ? true : false;
        private readonly object @lock = new object();
        private IDbConnection cn;
        public IDbConnection Connection { get { return GetConnection(); } }
        public bool ConnectionOnDemand { get { return connectionOnDemand; } set { connectionOnDemand = value; } }
        public string ParameterPrefix  { get; private set; }
        private IDbTransaction transaction;
        public string ProviderName { get; private set; }

        public DapperContext()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string providerName = ConfigurationManager.ConnectionStrings["DefaultConnection"].ProviderName;
            ParameterPrefix = providerName == "Oracle.ManagedDataAccess.Client" ? ":" : "@";
            ProviderName = providerName == "Oracle.ManagedDataAccess.Client" ? "ORCL" :
                           providerName == "System.Data.SqlClient" ? "MSSQL" : "MYSQL";
        }
        public DapperContext(string connectionStringName)
        {
            connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            string providerName = ConfigurationManager.ConnectionStrings[connectionStringName].ProviderName;
            ParameterPrefix = providerName == "Oracle.ManagedDataAccess.Client" ? ":" : "@";
            ProviderName = providerName == "Oracle.ManagedDataAccess.Client" ? "ORCL" :
                           providerName == "System.Data.SqlClient" ? "MSSQL" : "MYSQL";
        }
        public bool IsTransactionInProgress()
        {
            return (cn != null && transaction != null);
        }
        public IDbTransaction GetTransaction()
        {
            if (cn != null && transaction != null) { return transaction; }
            if (cn != null)
            {
                if (cn.State == ConnectionState.Open) { cn.Close(); }
                cn.Open();
                transaction = cn.BeginTransaction();
                return transaction;
            }
            return null;
        }

        public void CommitTransaction()
        {
            if (cn == null || transaction == null) { return; }
            transaction.Commit();
            cn.Close();
            if (!connectionOnDemand)
                cn.Open();

            transaction = null;
        }

        public void RollbackTransaction()
        {
            if (cn == null || transaction == null) { return; }
            if (cn.State != ConnectionState.Closed  && cn.State !=  ConnectionState.Broken) 
                transaction.Rollback();
            cn.Close();
            if (!connectionOnDemand)
                cn.Open();

            transaction = null;
        }

        public void OpenIfNot()
        {

            if (cn != null && cn.State != ConnectionState.Broken && cn.State == ConnectionState.Closed) { cn.Open(); }
        }

        public void CloseIfNot()
        {
            if (cn != null && cn.State != ConnectionState.Closed) { cn.Close(); }
        }

        private IDbConnection GetConnection()
        {
            if (cn == null)
            {
                lock (@lock)
                {
                    if (cn == null)
                    {
                        if (ProviderName == "MSSQL")
                        {
                            cn = new SqlConnection(connectionString);
                        }
                        else if (ProviderName == "ORCL")
                        {
                            cn = new OracleConnection(connectionString);
                            
                        }
                        else if (ProviderName == "MYSQL")
                        {
                            cn = new MySqlConnection(connectionString);
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException("providerName", "Unable to resolve ADO.NET provider name");
                        }
                        if (!connectionOnDemand)
                            cn.Open();
                    }
                }
            }

            return cn;
        }

        #region IDisposable

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (transaction != null)
                    { transaction.Dispose(); }
                    if (cn != null)
                    { cn.Dispose(); }
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
