using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Text.RegularExpressions;
using System.Configuration;
using PermissionManagement.Model;
using PermissionManagement.Utility;
using Dapper;

namespace PermissionManagement.Repository
{
    public static partial class SqlMapper
    {
        //avoiding varchar field in db to be converted to nvarchar before use in where clause.
        //,net code are unicode, but with the approach below, we specifically ask .NET to send the
        //string as non-unicode, which will cause a direct comparison with a varchar field in the db
        //without conversion which add processing cost and cause the index not be used.
        //var query = conn.Query<Stat>(
        //  "select * from Stats where Item = @queryPlanHash and EventName = 'query' and StatsDay > @startDate",
        //  new
        //  {
        //      queryPlanHash = new DbString() { Value = args[0], IsAnsi = true, Length = args[0].Length },
        //      startDate = DateTime.Today.AddDays(-7)
        //  });

        const string updateTemplate = @"UPDATE {0} SET {1} WHERE {2}";
        const string insertTemplate = @"INSERT INTO {0} ({1}) VALUES ({2})";
        const string selectTemplate = @"SELECT * FROM {0} WHERE {1}";
        const string selectTemplate2 = @"SELECT {2} * FROM {0} WHERE {1}";
        static SqlMapper.LinkII<string, string> queryInitCache;

        /// <summary>
        /// Get a record using the identitykey value.
        /// </summary>
        /// <typeparam name="T">Generic Entity T</typeparam>
        /// <param name="connection">Connection object</param>
        /// <param name="entityID">Identity key value</param>
        /// <param name="databaseTableName">required if the database table name is different from entity name</param>
        /// <param name="fieldNameMapping">required to map field (column) name in the database table to property names in the entity object</param>
        /// <returns>Instance of T with populated properties</returns>
        public async static Task<T> GetByIdAsync<T>(this IDbConnection connection, long entityID, string databaseTableName = null, IDictionary<string, string> fieldNameMapping = null, IDbTransaction transaction = null)
        {
            //var entity = connection.GetById<Transactions>(8000);
            //var entity = connection.Get<Transactions>(new Transactions() { ReferenceID = "111111188888" });

            var type = typeof(T);

            var idField = type.GetProperties().Where(g => (g.GetCustomAttributes(true).Any(b => b.GetType() == typeof(IdentityPrimaryKeyAttribute)))).FirstOrDefault();
            if (idField == null)
                throw new InvalidOperationException("Get cannot be performed without identity primary key");

            var identityFieldName = idField == null ? string.Empty : idField.Name;


            var keySet = new StringBuilder();
            var query = string.Empty;
            keySet.AppendFormat("{0} = {1}EntityID AND ", identityFieldName, GetParameterPrefix(connection.ConnectionString));

            if (fieldNameMapping != null)
            {
                var fieldMap = new StringBuilder();
                foreach (var k in fieldNameMapping)
                {
                    fieldMap.AppendFormat("{0} AS {1}, ", k.Key, k.Value);
                }
                query = string.Format(selectTemplate2, string.IsNullOrEmpty(databaseTableName) ? type.Name : databaseTableName, keySet.Remove(keySet.Length - 4, 4).ToString(), fieldMap.ToString());
            }
            else
            {
                query = string.Format(selectTemplate, string.IsNullOrEmpty(databaseTableName) ? type.Name : databaseTableName, keySet.Remove(keySet.Length - 4, 4).ToString());
            }

            var waiter = await connection.QueryAsync<T>(query, new { EntityID = entityID }, transaction: transaction);
            return waiter.FirstOrDefault();
        }

        /// <summary>
        /// Get a record using the identitykey value.
        /// </summary>
        /// <typeparam name="T">Generic Entity T</typeparam>
        /// <param name="connection">Connection object</param>
        /// <param name="entityID">Identity key value</param>
        /// <param name="databaseTableName">required if the database table name is different from entity name</param>
        /// <param name="fieldNameMapping">required to map field (column) name in the database table to property names in the entity object</param>
        /// <returns>Instance of T with populated properties</returns>
        public static T GetById<T>(this IDbConnection connection, long entityID, string databaseTableName = null, IDictionary<string, string> fieldNameMapping = null, IDbTransaction transaction = null)
        {
            //var entity = connection.GetById<Transactions>(8000);
            //var entity = connection.Get<Transactions>(new Transactions() { ReferenceID = "111111188888" });

            var type = typeof(T);

            var idField = type.GetProperties().Where(g => (g.GetCustomAttributes(true).Any(b => b.GetType() == typeof(IdentityPrimaryKeyAttribute)))).FirstOrDefault();
            if (idField == null)
                throw new InvalidOperationException("Get cannot be performed without identity primary key");

            var identityFieldName = idField == null ? string.Empty : idField.Name;


            var keySet = new StringBuilder();
            var query = string.Empty;
            keySet.AppendFormat("{0} = {1}EntityID AND ", identityFieldName, GetParameterPrefix(connection.ConnectionString));

            if (fieldNameMapping != null)
            {
                var fieldMap = new StringBuilder();
                foreach (var k in fieldNameMapping)
                {
                    fieldMap.AppendFormat("{0} AS {1}, ", k.Key, k.Value);
                }
                query = string.Format(selectTemplate2, string.IsNullOrEmpty(databaseTableName) ? type.Name : databaseTableName, keySet.Remove(keySet.Length - 4, 4).ToString(), fieldMap.ToString());
            }
            else
            {
                query = string.Format(selectTemplate, string.IsNullOrEmpty(databaseTableName) ? type.Name : databaseTableName, keySet.Remove(keySet.Length - 4, 4).ToString());
            }

            return connection.Query<T>(query, new { EntityID = entityID }, transaction: transaction).FirstOrDefault();
        }


        /// <summary>
        /// Get a record using the identitykey value.
        /// </summary>
        /// <typeparam name="T">Generic Entity T</typeparam>
        /// <param name="connection">Connection object</param>
        /// <param name="entityToGet">An instance of entity to get, with the key properties to be used in where clause set.</param>
        /// <param name="databaseTableName">required if the database table name is different from entity name</param>
        /// <param name="fieldNameMapping">required to map field (column) name in the database table to property names in the entity object</param>
        /// <returns>Instance of T with populated properties</returns>
        public async static Task<T> GetAsync<T>(this IDbConnection connection, T entityToGet, string databaseTableName = null, IDictionary<string, string> fieldNameMapping = null, string[] primaryKeyList = null, IDbTransaction transaction = null)
        {

            if (primaryKeyList == null)
                throw new InvalidOperationException("Get cannot be performed without primary key(s)");

            var type = typeof(T);
            var keySet = new StringBuilder();
            var query = string.Empty;

            var parameterPrefix = GetParameterPrefix(connection.ConnectionString);
            foreach (var d in primaryKeyList)
            {
                keySet.AppendFormat("{0} = {1}{0} AND ", d, parameterPrefix);
            }

            if (fieldNameMapping != null)
            {
                var fieldMap = new StringBuilder();
                foreach (var k in fieldNameMapping)
                {
                    fieldMap.AppendFormat("{0} AS {1}, ", k.Key, k.Value);
                }
                query = string.Format(selectTemplate2, string.IsNullOrEmpty(databaseTableName) ? type.Name : databaseTableName, keySet.Remove(keySet.Length - 4, 4).ToString(), fieldMap.ToString());
            }
            else
            {
                query = string.Format(selectTemplate, string.IsNullOrEmpty(databaseTableName) ? type.Name : databaseTableName, keySet.Remove(keySet.Length - 4, 4).ToString());
            }

            var waiter = await connection.QueryAsync<T>(query, entityToGet, transaction: transaction);
            return waiter.FirstOrDefault();
        }

        /// <summary>
        /// Get a record using the identitykey value.
        /// </summary>
        /// <typeparam name="T">Generic Entity T</typeparam>
        /// <param name="connection">Connection object</param>
        /// <param name="entityToGet">An instance of entity to get, with the key properties to be used in where clause set.</param>
        /// <param name="databaseTableName">required if the database table name is different from entity name</param>
        /// <param name="fieldNameMapping">required to map field (column) name in the database table to property names in the entity object</param>
        /// <returns>Instance of T with populated properties</returns>
        public static T Get<T>(this IDbConnection connection, T entityToGet, string databaseTableName = null, IDictionary<string, string> fieldNameMapping = null, string[] primaryKeyList = null, IDbTransaction transaction = null)
        {

            if (primaryKeyList == null)
                throw new InvalidOperationException("Get cannot be performed without primary key(s)");

            var type = typeof(T);
            var keySet = new StringBuilder();
            var query = string.Empty;

            var parameterPrefix = GetParameterPrefix(connection.ConnectionString);
            foreach (var d in primaryKeyList)
            {
                keySet.AppendFormat("{0} = {1}{0} AND ", d, parameterPrefix);
            }

            if (fieldNameMapping != null)
            {
                var fieldMap = new StringBuilder();
                foreach (var k in fieldNameMapping)
                {
                    fieldMap.AppendFormat("{0} AS {1}, ", k.Key, k.Value);
                }
                query = string.Format(selectTemplate2, string.IsNullOrEmpty(databaseTableName) ? type.Name : databaseTableName, keySet.Remove(keySet.Length - 4, 4).ToString(), fieldMap.ToString());
            }
            else
            {
                query = string.Format(selectTemplate, string.IsNullOrEmpty(databaseTableName) ? type.Name : databaseTableName, keySet.Remove(keySet.Length - 4, 4).ToString());
            }

            return connection.Query<T>(query, entityToGet, transaction: transaction).FirstOrDefault();
        }


        /// <summary>
        /// This will create insert statement using the property names of the passed
        /// Entity. 
        /// </summary>
        /// <param name="connection">IDbConnection instance</param>
        /// <param name="entityToInsert">Entity to insert</param>
        /// <param name="excludeFieldList">list of field names in the entity to be ignored, null otherwise</param>
        /// <param name="databaseTableName">the database table name, if not supplied, database table name is infered from the entity type name</param>
        /// <param name="fieldNameMapping">a dictionary that allows field name in the entity to different to the database table field name. 
        /// The entity field name is the key while the corresponding database table field name is the value, null otherwise</param>
        /// <param name="transaction">IDbTransaction object, null otherwise.</param>
        /// <returns
        /// ></returns>
        public async static Task<long> InsertAsync<T>(this IDbConnection connection, T entityToInsert, string[] excludeFieldList = null, string databaseTableName = null, IDictionary<string, string> fieldNameMapping = null, IDbTransaction transaction = null, string provider = null)
        {
            PropertyInfo idField = null;
            var type = typeof(T);  // entityToInsert.GetType();
            var query = BuildInsert(type, connection.ConnectionString, out idField, excludeFieldList, databaseTableName, fieldNameMapping);

            long id = 0;
            var parameterPrefix = GetParameterPrefix(connection.ConnectionString);

            if (transaction == null)
            {
                if (idField != null)
                {
                    if (parameterPrefix == "@")
                    {
                        string identitySelect = IsMSSQLProvider(connection.ConnectionString) ? "SELECT SCOPE_IDENTITY() AS Id" : "SELECT LAST_INSERT_ID() as Id;";
                        query = string.Format("{0};{1}", query, identitySelect);
                        var waiter = await connection.QueryAsync<Int64>(query, entityToInsert);
                        id = waiter.FirstOrDefault();

                        idField.SetValue(entityToInsert, id, null);
                    }
                    else
                    {

                        ////for Oracle 12c, create an identity column. 
                        ///create table t1 (
                        ////c1 NUMBER GENERATED ALWAYS as IDENTITY(START with 1 INCREMENT by 1),
                        ////        c2 VARCHAR2(10)
                        ///);
                        ///
                        ////for Oracle 12, create a sequence and in the create
                        ////statement, reference the sequence to populate the identity column
                        ///CREATE SEQUENCE dept_seq START WITH 1;
                        ////CREATE TABLE departments(
                        ////  ID           NUMBER(10)    DEFAULT dept_seq.nextval NOT NULL,
                        ////  DESCRIPTION  VARCHAR2(50)  NOT NULL);
                        ////ALTER TABLE departments ADD(CONSTRAINT dept_pk PRIMARY KEY (ID));

                        ////for oracle 12 and above, use code below
                        //var param = new DynamicParameters(entityToInsert);
                        //param.Add(name: "Id", dbType: DbType.Int64, direction: ParameterDirection.Output);
                        //query = string.Format("{0} {1}", query, "returning Id into :Id");
                        //connection.Execute(query, param, transaction: transaction);
                        //id = param.Get<Int64>("Id");
                        //idField.SetValue(entityToInsert, id, null);

                        ////for Oracle version below 12, 
                        ////create a sequence with tablename_seq format
                        ///CREATE TABLE departments (
                        ////ID NUMBER(10)    NOT NULL,
                        ////DESCRIPTION  VARCHAR2(50)  NOT NULL);
                        ////ALTER TABLE departments ADD(
                        ////  CONSTRAINT departments_pk PRIMARY KEY (ID));
                        ////CREATE SEQUENCE departments_seq START WITH 1;
                        var param = new DynamicParameters(entityToInsert);
                        param.Add(name: "Id", dbType: DbType.Int64, direction: ParameterDirection.Output);
                        query = string.Format("{0} {1}", query, "returning Id into :Id");
                        await connection.ExecuteAsync(query, param);
                        id = param.Get<Int64>("Id");
                        idField.SetValue(entityToInsert, id, null);
                    }
                }
                else
                {
                    var waiter = await connection.ExecuteAsync(query, entityToInsert);
                    id = waiter;
                }
            }
            else
            {
                if (idField != null)
                {
                    if (parameterPrefix == "@")
                    {
                        string identitySelect = IsMSSQLProvider(connection.ConnectionString) ? "SELECT SCOPE_IDENTITY() AS Id" : "SELECT LAST_INSERT_ID() as Id;";
                        query = string.Format("{0};{1}", query, identitySelect);
                        var waiter = await connection.QueryAsync<Int64>(query, entityToInsert, transaction: transaction);
                        id = waiter.FirstOrDefault();
                        idField.SetValue(entityToInsert, id, null);
                    }
                    else
                    {
                        ////for Oracle 12c, create an identity column. 
                        ///create table t1 (
                        ////c1 NUMBER GENERATED ALWAYS as IDENTITY(START with 1 INCREMENT by 1),
                        ////        c2 VARCHAR2(10)
                        ///);
                        ///
                        ////for Oracle 12, create a sequence and in the create
                        ////statement, reference the sequence to populate the identity column
                        ///CREATE SEQUENCE dept_seq START WITH 1;
                        ////CREATE TABLE departments(
                        ////  ID           NUMBER(10)    DEFAULT dept_seq.nextval NOT NULL,
                        ////  DESCRIPTION  VARCHAR2(50)  NOT NULL);
                        ////ALTER TABLE departments ADD(CONSTRAINT dept_pk PRIMARY KEY (ID));

                        ////for oracle 12 and above, use code below
                        //var param = new DynamicParameters(entityToInsert);
                        //param.Add(name: "Id", dbType: DbType.Int64, direction: ParameterDirection.Output);
                        //query = string.Format("{0} {1}", query, "returning Id into :Id");
                        //connection.Execute(query, param, transaction: transaction);
                        //id = param.Get<Int64>("Id");
                        //idField.SetValue(entityToInsert, id, null);

                        ////for Oracle version below 12, 
                        ////create a sequence with tablename_seq format
                        ///CREATE TABLE departments (
                        ////ID NUMBER(10)    NOT NULL,
                        ////DESCRIPTION  VARCHAR2(50)  NOT NULL);
                        ////ALTER TABLE departments ADD(
                        ////  CONSTRAINT departments_pk PRIMARY KEY (ID));
                        ////CREATE SEQUENCE departments_seq START WITH 1;
                        var param = new DynamicParameters(entityToInsert);
                        param.Add(name: "Id", dbType: DbType.Int64, direction: ParameterDirection.Output);
                        query = string.Format("{0} {1}", query, "returning Id into :Id");
                        await connection.ExecuteAsync(query, param, transaction: transaction);
                        id = param.Get<Int64>("Id");
                        idField.SetValue(entityToInsert, id, null);
                    }
                }
                else
                {
                    id = await connection.ExecuteAsync(query, entityToInsert, transaction: transaction);
                }
            }
            return id;
        }

        /// <summary>
        /// This will create insert statement using the property names of the passed
        /// Entity. 
        /// </summary>
        /// <param name="connection">IDbConnection instance</param>
        /// <param name="entityToInsert">Entity to insert</param>
        /// <param name="excludeFieldList">list of field names in the entity to be ignored, null otherwise</param>
        /// <param name="databaseTableName">the database table name, if not supplied, database table name is infered from the entity type name</param>
        /// <param name="fieldNameMapping">a dictionary that allows field name in the entity to different to the database table field name. 
        /// The entity field name is the key while the corresponding database table field name is the value, null otherwise</param>
        /// <param name="transaction">IDbTransaction object, null otherwise.</param>
        /// <returns
        /// ></returns>
        public static long Insert<T>(this IDbConnection connection, T entityToInsert, string[] excludeFieldList = null, string databaseTableName = null, IDictionary<string, string> fieldNameMapping = null, IDbTransaction transaction = null, string provider = null)
        {
            PropertyInfo idField = null;
            var type = typeof(T);  // entityToInsert.GetType();
            var query = BuildInsert(type, connection.ConnectionString, out idField, excludeFieldList, databaseTableName, fieldNameMapping);

            long id = 0;
            var parameterPrefix = GetParameterPrefix(connection.ConnectionString);

            if (transaction == null)
            {
                if (idField != null)
                {
                    if (parameterPrefix == "@")
                    {
                        string identitySelect = IsMSSQLProvider(connection.ConnectionString) ? "SELECT SCOPE_IDENTITY() AS Id" : "SELECT LAST_INSERT_ID() as Id;";
                        query = string.Format("{0};{1}", query, identitySelect);
                        id = connection.Query<Int64>(query, entityToInsert).FirstOrDefault();
                        idField.SetValue(entityToInsert, id, null);
                    }
                    else
                    {

                        ////for Oracle 12c, create an identity column. 
                        ///create table t1 (
                        ////c1 NUMBER GENERATED ALWAYS as IDENTITY(START with 1 INCREMENT by 1),
                        ////        c2 VARCHAR2(10)
                        ///);
                        ///
                        ////for Oracle 12, create a sequence and in the create
                        ////statement, reference the sequence to populate the identity column
                        ///CREATE SEQUENCE dept_seq START WITH 1;
                        ////CREATE TABLE departments(
                        ////  ID           NUMBER(10)    DEFAULT dept_seq.nextval NOT NULL,
                        ////  DESCRIPTION  VARCHAR2(50)  NOT NULL);
                        ////ALTER TABLE departments ADD(CONSTRAINT dept_pk PRIMARY KEY (ID));

                        ////for oracle 12 and above, use code below
                        //var param = new DynamicParameters(entityToInsert);
                        //param.Add(name: "Id", dbType: DbType.Int64, direction: ParameterDirection.Output);
                        //query = string.Format("{0} {1}", query, "returning Id into :Id");
                        //connection.Execute(query, param, transaction: transaction);
                        //id = param.Get<Int64>("Id");
                        //idField.SetValue(entityToInsert, id, null);

                        ////for Oracle version below 12, 
                        ////create a sequence with tablename_seq format
                        ///CREATE TABLE departments (
                        ////ID NUMBER(10)    NOT NULL,
                        ////DESCRIPTION  VARCHAR2(50)  NOT NULL);
                        ////ALTER TABLE departments ADD(
                        ////  CONSTRAINT departments_pk PRIMARY KEY (ID));
                        ////CREATE SEQUENCE departments_seq START WITH 1;
                        var param = new DynamicParameters(entityToInsert);
                        param.Add(name: "Id", dbType: DbType.Int64, direction: ParameterDirection.Output);
                        query = string.Format("{0} {1}", query, "returning Id into :Id");
                        connection.Execute(query, param);
                        id = param.Get<Int64>("Id");
                        idField.SetValue(entityToInsert, id, null);
                    }
                }
                else
                {
                    id = connection.Execute(query, entityToInsert);
                }
            }
            else
            {
                if (idField != null)
                {
                    if (parameterPrefix == "@")
                    {
                        string identitySelect = IsMSSQLProvider(connection.ConnectionString) ? "SELECT SCOPE_IDENTITY() AS Id" : "SELECT LAST_INSERT_ID() as Id;";
                        query = string.Format("{0};{1}", query, identitySelect);
                        id = connection.Query<Int64>(query, entityToInsert, transaction: transaction).FirstOrDefault();
                        idField.SetValue(entityToInsert, id, null);
                    }
                    else
                    {
                        ////for Oracle 12c, create an identity column. 
                        ///create table t1 (
                        ////c1 NUMBER GENERATED ALWAYS as IDENTITY(START with 1 INCREMENT by 1),
                        ////        c2 VARCHAR2(10)
                        ///);
                        ///
                        ////for Oracle 12, create a sequence and in the create
                        ////statement, reference the sequence to populate the identity column
                        ///CREATE SEQUENCE dept_seq START WITH 1;
                        ////CREATE TABLE departments(
                        ////  ID           NUMBER(10)    DEFAULT dept_seq.nextval NOT NULL,
                        ////  DESCRIPTION  VARCHAR2(50)  NOT NULL);
                        ////ALTER TABLE departments ADD(CONSTRAINT dept_pk PRIMARY KEY (ID));

                        ////for oracle 12 and above, use code below
                        //var param = new DynamicParameters(entityToInsert);
                        //param.Add(name: "Id", dbType: DbType.Int64, direction: ParameterDirection.Output);
                        //query = string.Format("{0} {1}", query, "returning Id into :Id");
                        //connection.Execute(query, param, transaction: transaction);
                        //id = param.Get<Int64>("Id");
                        //idField.SetValue(entityToInsert, id, null);

                        ////for Oracle version below 12, 
                        ////create a sequence with tablename_seq format
                        ///CREATE TABLE departments (
                        ////ID NUMBER(10)    NOT NULL,
                        ////DESCRIPTION  VARCHAR2(50)  NOT NULL);
                        ////ALTER TABLE departments ADD(
                        ////  CONSTRAINT departments_pk PRIMARY KEY (ID));
                        ////CREATE SEQUENCE departments_seq START WITH 1;
                        var param = new DynamicParameters(entityToInsert);
                        param.Add(name: "Id", dbType: DbType.Int64, direction: ParameterDirection.Output);
                        query = string.Format("{0} {1}", query, "returning Id into :Id");
                        connection.Execute(query, param, transaction: transaction);
                        id = param.Get<Int64>("Id");
                        idField.SetValue(entityToInsert, id, null);
                    }
                }
                else
                {
                    id = connection.Execute(query, entityToInsert, transaction: transaction);
                }
            }
            return id;
        }

        /// <summary>
        /// This will create insert statement using the property names of the passed
        /// Entity. 
        /// </summary>
        /// <param name="connection">IDbConnection instance</param>
        /// <param name="entitiesToInsert">IENumerable of entities to insert</param>
        /// <param name="excludeFieldList">list of field names in the entity to be ignored, null otherwise</param>
        /// <param name="databaseTableName">the database table name, if not supplied, database table name is infered from the entity type name</param>
        /// <param name="fieldNameMapping">a dictionary that allows field name in the entity to different to the database table field name. 
        /// The entity field name is the key while the corresponding database table field name is the value, null otherwise</param>
        /// <param name="transaction">IDbTransaction object, null otherwise.</param>
        /// <returns></returns>
        public static long InsertMany<T>(this IDbConnection connection, IEnumerable<T> entitiesToInsert, string[] excludeFieldList = null, string databaseTableName = null, IDictionary<string, string> fieldNameMapping = null, IDbTransaction transaction = null)
        {
            PropertyInfo idField = null;
            var type = typeof(T);
            var query = BuildInsert(type, connection.ConnectionString, out idField, excludeFieldList, databaseTableName, fieldNameMapping);
            return transaction == null ?
                   connection.Execute(query, entitiesToInsert) :
                   connection.Execute(query, entitiesToInsert, transaction: transaction);
        }

        public async static Task<long> InsertManyAsync<T>(this IDbConnection connection, IEnumerable<T> entitiesToInsert, string[] excludeFieldList = null, string databaseTableName = null, IDictionary<string, string> fieldNameMapping = null, IDbTransaction transaction = null)
        {
            PropertyInfo idField = null;
            var type = typeof(T);
            var query = BuildInsert(type, connection.ConnectionString, out idField, excludeFieldList, databaseTableName, fieldNameMapping);
            return transaction == null ?
                   await connection.ExecuteAsync(query, entitiesToInsert) :
                   await connection.ExecuteAsync(query, entitiesToInsert, transaction: transaction);
        }

        public async static Task<int> UpdateAsync<T>(this IDbConnection connection, T entityToUpdate, string[] excludeFieldList = null, string databaseTableName = null, IDictionary<string, string> fieldNameMapping = null, string[] primaryKeyList = null, IDbTransaction transaction = null, bool concurrencyCheck = false)
        {
            if (primaryKeyList == null)
                throw new InvalidOperationException("Update cannot be performed without primary key(s)");

            var type = typeof(T);
            var query = BuildUpdate(type, connection.ConnectionString, excludeFieldList, databaseTableName, fieldNameMapping, primaryKeyList, concurrencyCheck);
            return transaction == null ? await connection.ExecuteAsync(query, entityToUpdate) :
                   await connection.ExecuteAsync(query, entityToUpdate, transaction: transaction);
        }

        public static int Update<T>(this IDbConnection connection, T entityToUpdate, string[] excludeFieldList = null, string databaseTableName = null, IDictionary<string, string> fieldNameMapping = null, string[] primaryKeyList = null, IDbTransaction transaction = null, bool concurrencyCheck = false)
        {
            if (primaryKeyList == null)
                throw new InvalidOperationException("Update cannot be performed without primary key(s)");

            var type = typeof(T);
            var query = BuildUpdate(type, connection.ConnectionString, excludeFieldList, databaseTableName, fieldNameMapping, primaryKeyList, concurrencyCheck);
            return transaction == null ?
                   connection.Execute(query, entityToUpdate) :
                   connection.Execute(query, entityToUpdate, transaction: transaction);
        }

        public static int UpdateMany<T>(this IDbConnection connection, IEnumerable<T> entitiesToUpdate, string[] excludeFieldList = null, string databaseTableName = null, IDictionary<string, string> fieldNameMapping = null, string[] primaryKeyList = null, IDbTransaction transaction = null, bool concurrencyCheck = false)
        {
            if (primaryKeyList == null)
                throw new InvalidOperationException("Update cannot be performed without primary key(s)");

            var type = typeof(T);
            var query = BuildUpdate(type, connection.ConnectionString, excludeFieldList, databaseTableName, fieldNameMapping, primaryKeyList, concurrencyCheck);
            return transaction == null ?
                   connection.Execute(query, entitiesToUpdate) :
                   connection.Execute(query, entitiesToUpdate, transaction: transaction);
        }

        public async static Task<int> UpdateManyAsync<T>(this IDbConnection connection, IEnumerable<T> entitiesToUpdate, string[] excludeFieldList = null, string databaseTableName = null, IDictionary<string, string> fieldNameMapping = null, string[] primaryKeyList = null, IDbTransaction transaction = null, bool concurrencyCheck = false)
        {
            if (primaryKeyList == null)
                throw new InvalidOperationException("Update cannot be performed without primary key(s)");

            var type = typeof(T);
            var query = BuildUpdate(type, connection.ConnectionString, excludeFieldList, databaseTableName, fieldNameMapping, primaryKeyList, concurrencyCheck);
            return transaction == null ?
                   await connection.ExecuteAsync(query, entitiesToUpdate) :
                   await connection.ExecuteAsync(query, entitiesToUpdate, transaction: transaction);
        }

        private static string BuildUpdate(Type type, string connectionString, string[] excludeFieldList = null, string databaseTableName = null, IDictionary<string, string> fieldNameMapping = null, string[] primaryKeyList = null, bool concurrencyCheck = false)
        {
            var parameterPrefix = GetParameterPrefix(connectionString);

            var s = string.Format("{0}-{1}UPDATE", type.FullName, parameterPrefix);
            string internedCmd = string.IsInterned(s) ?? string.Intern(s);
            string queryText = null;
            if (SqlMapper.LinkII<string, string>.TryGet(queryInitCache, internedCmd, out queryText))
            {
                return queryText;
            }

            var updateSet = new StringBuilder();
            var keySet = new StringBuilder();

            var list = new List<string>();
            if (excludeFieldList != null)
                list.AddRange(excludeFieldList);


            //Convention based RowVersionNo2. Should not be part of update list
            list.Add("RowVersionNo2");
            list.AddRange(primaryKeyList);
            var properties = type.GetProperties().Select(p => p.Name).ToArray().Except(list).ToArray();

            var status = false;
            if (fieldNameMapping != null)
            {
                for (var i = 0; i < properties.Length; i++)
                {
                    var temp = properties[i];
                    var temp2 = string.Empty;
                    status = fieldNameMapping.TryGetValue(temp, out temp2);
                    if (status)
                    {
                        properties[i] = temp2;
                    }
                    updateSet.AppendFormat("{0} = {2}{1}, ", status ? temp2 : properties[i], properties[i], parameterPrefix);
                }
            }
            else
            {
                for (var i = 0; i < properties.Length; i++)
                {
                    updateSet.AppendFormat("{0} = {1}{0}, ", properties[i], parameterPrefix);
                }
            }

            foreach (var d in primaryKeyList)
            {
                keySet.AppendFormat("{0} = {1}{0} AND ", d, parameterPrefix);
            }
            if (concurrencyCheck)
            {
                keySet.AppendFormat("CONVERT(bigint,{0}) = {1}RowVersionNo2 AND ", "RowVersionNo", parameterPrefix);
            }

            var query = string.Format(updateTemplate, string.IsNullOrEmpty(databaseTableName) ? type.Name : databaseTableName, updateSet.Remove(updateSet.Length - 2, 2).ToString(), keySet.Remove(keySet.Length - 4, 4).ToString());
            SqlMapper.LinkII<string, string>.TryAdd(ref queryInitCache, internedCmd, ref query);

            return query;
        }

        private static string BuildInsert(Type type, string connectionString, out PropertyInfo idField, string[] excludeFieldList = null, string databaseTableName = null, IDictionary<string, string> fieldNameMapping = null)
        {
            idField = type.GetProperties().Where(g => (g.GetCustomAttributes(true).Any(b => b.GetType() == typeof(IdentityPrimaryKeyAttribute)))).FirstOrDefault();
            var parameterPrefix = GetParameterPrefix(connectionString);

            var s = string.Format("{0}-{1}INSERT", type.FullName, parameterPrefix);
            string internedCmd = string.IsInterned(s) ?? string.Intern(s);
            string queryText = null;
            if (SqlMapper.LinkII<string, string>.TryGet(queryInitCache, internedCmd, out queryText))
            {
                return queryText;
            }

            var identityFieldName = idField == null ? string.Empty : idField.Name;

            var list = new List<string>();
            if (excludeFieldList != null)
                list.AddRange(excludeFieldList);

            //Convention based RowVersionNo2. Should not be part of insert list
            list.Add("RowVersionNo2");
            var properties = type.GetProperties().Where(f => f.Name != identityFieldName).Select(p => p.Name).ToArray().Except(list).ToArray();

            var tableName = string.IsNullOrEmpty(databaseTableName) ? type.Name : databaseTableName;

            var values = string.Join(",", properties.Select(n => parameterPrefix + n).ToArray());
            values = !string.IsNullOrEmpty(identityFieldName) && parameterPrefix == ":" ?
                     string.Format("{0}_seq.NEXTVAL,{1}", tableName, values)
                     : values;

            var status = false;
            if (fieldNameMapping != null)
            {
                for (var i = 0; i < properties.Length; i++)
                {
                    var temp = properties[i];
                    var temp2 = string.Empty;
                    status = fieldNameMapping.TryGetValue(temp, out temp2);
                    if (status)
                    {
                        properties[i] = temp2;
                    }
                }
            }
            var names = string.Join(",", properties);
            names = !string.IsNullOrEmpty(identityFieldName) && parameterPrefix == ":" ?
                     string.Format("{0},{1}", identityFieldName, names)
                     : names;

            var query = string.Format(insertTemplate, tableName, names, values);
            SqlMapper.LinkII<string, string>.TryAdd(ref queryInitCache, internedCmd, ref query);
            return query;
        }

        /// <summary>
        /// Uses connection string to determine if the connection object is a SqlClient or Oracle.ManagedDataAccess and return : or @ as parameter prefix
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns>@ for SqlClient | : for Oracle</returns>
        private static string GetParameterPrefix(string connectionString)
        {
            return connectionString.IndexOf("catalog", StringComparison.InvariantCultureIgnoreCase) > 0 ? "@" : ":";
        }
        private static bool IsMSSQLProvider(string connectionString)
        {
            return connectionString.IndexOf("catalog", StringComparison.InvariantCultureIgnoreCase) > 0 ? true : false;
        }
        /// <summary>
        /// This is a micro-cache; suitable when the number of terms is controllable (a few hundred, for example),
        /// and strictly append-only; you cannot change existing values.
        /// equality. The type is fully thread-safe.
        /// </summary>
        internal partial class LinkII<TKey, TValue> where TKey : class
        {
            public static bool TryGet(LinkII<TKey, TValue> link, TKey key, out TValue value)
            {
                while (link != null)
                {
                    if ((object)key == (object)link.Key)
                    {
                        value = link.Value;
                        return true;
                    }
                    link = link.Tail;
                }
                value = default(TValue);
                return false;
            }
            public static bool TryAdd(ref LinkII<TKey, TValue> head, TKey key, ref TValue value)
            {
                bool tryAgain;
                do
                {
                    var snapshot = Interlocked.CompareExchange(ref head, null, null);
                    TValue found;
                    if (TryGet(snapshot, key, out found))
                    { // existing match; report the existing value instead
                        value = found;
                        return false;
                    }
                    var newNode = new LinkII<TKey, TValue>(key, value, snapshot);
                    // did somebody move our cheese?
                    tryAgain = Interlocked.CompareExchange(ref head, newNode, snapshot) != snapshot;
                } while (tryAgain);
                return true;
            }
            private LinkII(TKey key, TValue value, LinkII<TKey, TValue> tail)
            {
                Key = key;
                Value = value;
                Tail = tail;
            }
            public TKey Key { get; private set; }
            public TValue Value { get; private set; }
            public LinkII<TKey, TValue> Tail { get; private set; }
        }
    }
}
