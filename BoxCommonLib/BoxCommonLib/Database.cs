using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace BoxCommonLib
{
    [DataContract]
    public sealed class DatabaseConnectionString
    {
        // Fields
        private string connectionStr;
        private string provider;

        // Properties
        [DataMember]
        public string ConnectionString
        {
            get
            {
                return this.connectionStr;
            }
            set
            {
                this.connectionStr = value;
            }
        }

        [DataMember]
        public string ProviderName
        {
            get
            {
                return this.provider;
            }
            set
            {
                this.provider = value;
            }
        }
    }


    public class DatabaseExecutiveService : IDatabaseExecutiveService
    {
        // Fields
        private bool beginTxn;
        private IDbConnection conn;
        private DatabaseConnectionString connString;
        private int count;
        private DatabaseType databaseType;
        private string errorSQL;
        private IDbTransaction tx;

        // Methods
        public DataTable BuildTableCloneByTableName(string tableName)
        {
            DataTable table = new DataTable(tableName);
            string sql = "";
            List<IDbDataParameter> parameterList = new List<IDbDataParameter>();
            switch (this.databaseType)
            {
                case DatabaseType.Oracle:
                    sql = "SELECT COLUMN_NAME, DATA_TYPE FROM USER_TAB_COLUMNS WHERE TABLE_NAME = :TABLE_NAME";
                    parameterList.Add(new OracleParameter("TABLE_NAME", tableName));
                    break;

                case DatabaseType.SQLServer:
                    sql = "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TABLE_NAME";
                    parameterList.Add(new SqlParameter("TABLE_NAME", tableName));
                    break;
            }
            DataTable table2 = this.Select(sql, parameterList);
            if (((table2 != null) && (table2.Rows != null)) && (table2.Rows.Count != 0))
            {
                for (int i = 0; i < table2.Rows.Count; i++)
                {
                    string columnName = table2.Rows[i]["COLUMN_NAME"].ToString();
                    string str4 = table2.Rows[i]["DATA_TYPE"].ToString().ToUpper();
                    if (str4 == null)
                    {
                        goto Label_0168;
                    }
                    if ((!(str4 == "NUMBER") && !(str4 == "DECIMAL")) && !(str4 == "INT"))
                    {
                        if ((str4 == "DATE") || (str4 == "DATETIME"))
                        {
                            goto Label_014E;
                        }
                        goto Label_0168;
                    }
                    table.Columns.Add(columnName, typeof(decimal));
                    goto Label_0182;
                Label_014E:
                    table.Columns.Add(columnName, typeof(DateTime));
                    goto Label_0182;
                Label_0168:
                    table.Columns.Add(columnName, typeof(string));
                Label_0182:;
                }
            }
            return table;
        }

        public void CloseConnection()
        {
            if ((this.connString == null) || (this.conn == null))
            {
                throw new Exception(Message.NoConnection);
            }
            if (this.conn != null)
            {
                this.conn.Close();
            }
        }

        private DataTable ConvertDataReaderToTable(IDataReader reader)
        {
            DataTable table = new DataTable();
            if (!reader.Read())
            {
                table = null;
            }
            else
            {
                table = new DataTable();
                table.TableName = "QUERY_DATA_TABLE";
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    DataColumn column = new DataColumn(reader.GetName(i), reader.GetFieldType(i));
                    table.Columns.Add(column);
                }
                object[] values = new object[reader.FieldCount];
                do
                {
                    DataRow row = table.NewRow();
                    reader.GetValues(values);
                    row.ItemArray = values;
                    table.Rows.Add(row);
                }
                while (reader.Read());
            }
            reader.Close();
            return table;
        }

        public DatabaseReturn Execute(List<IDbCommand> commandList)
        {
            this.errorSQL = "";
            this.count = 0;
            if ((this.connString == null) || (this.conn == null))
            {
                throw new Exception(Message.NoConnection);
            }
            DatabaseReturn return2 = new DatabaseReturn();
            IDbTransaction transaction = this.conn.BeginTransaction();
            if ((commandList != null) && (commandList.Count != 0))
            {
                try
                {
                    foreach (IDbCommand command in commandList)
                    {
                        if ((command != null) && !(command.CommandText == ""))
                        {
                            this.errorSQL = command.CommandText;
                            command.Connection = this.conn;
                            command.Transaction = transaction;
                            this.count = command.ExecuteNonQuery();
                            if (this.count <= 0)
                            {
                                throw new Exception(Message.NoRowEffected);
                            }
                            return2.EffectRowCount += this.count;
                        }
                    }
                    this.errorSQL = "";
                    transaction.Commit();
                    return2.ReturnException = null;
                }
                catch (Exception exception)
                {
                    transaction.Rollback();
                    return2.EffectRowCount = 0;
                    return2.ReturnException = exception;
                    return2.ErrorSQL = this.errorSQL;
                }
            }
            return return2;
        }

        public DatabaseReturn Execute(string sql)
        {
            return this.Execute(sql, new List<IDbDataParameter>());
        }

        public DatabaseReturn Execute(string[] sqls)
        {
            this.errorSQL = "";
            this.count = 0;
            if ((this.connString == null) || (this.conn == null))
            {
                throw new Exception(Message.NoConnection);
            }
            DatabaseReturn return2 = new DatabaseReturn();
            IDbTransaction transaction = this.conn.BeginTransaction();
            IDbCommand command = this.conn.CreateCommand();
            command.Transaction = transaction;
            try
            {
                foreach (string str in sqls)
                {
                    if (str.Trim() != "")
                    {
                        this.errorSQL = str;
                        command.CommandText = str;
                        this.count = command.ExecuteNonQuery();
                        if (this.count <= 0)
                        {
                            throw new Exception(Message.NoRowEffected);
                        }
                        return2.EffectRowCount += this.count;
                    }
                }
                this.errorSQL = "";
                transaction.Commit();
                return2.ReturnException = null;
            }
            catch (Exception exception)
            {
                transaction.Rollback();
                return2.EffectRowCount = 0;
                return2.ReturnException = exception;
                return2.ErrorSQL = this.errorSQL;
            }
            return return2;
        }

        public DatabaseReturn Execute(List<IDbCommand> commandList, IDbTransaction transaction)
        {
            this.errorSQL = "";
            this.count = 0;
            if ((this.connString == null) || (this.conn == null))
            {
                throw new Exception(Message.NoConnection);
            }
            DatabaseReturn return2 = new DatabaseReturn();
            if ((commandList != null) && (commandList.Count != 0))
            {
                try
                {
                    foreach (IDbCommand command in commandList)
                    {
                        if ((command != null) && !(command.CommandText == ""))
                        {
                            this.errorSQL = command.CommandText;
                            command.Connection = this.conn;
                            command.Transaction = transaction;
                            this.count = command.ExecuteNonQuery();
                            if (this.count <= 0)
                            {
                                throw new Exception(Message.NoRowEffected);
                            }
                            return2.EffectRowCount += this.count;
                        }
                    }
                    this.errorSQL = "";
                    return2.ReturnException = null;
                }
                catch (Exception exception)
                {
                    return2.EffectRowCount = 0;
                    return2.ReturnException = exception;
                    return2.ErrorSQL = this.errorSQL;
                }
            }
            return return2;
        }

        public DatabaseReturn Execute(string sql, List<IDbDataParameter> parameterList)
        {
            this.errorSQL = "";
            if ((this.connString == null) || (this.conn == null))
            {
                throw new Exception(Message.NoConnection);
            }
            DatabaseReturn return2 = new DatabaseReturn();
            IDbCommand command = this.conn.CreateCommand();
            command.CommandText = sql;
            try
            {
                command.Parameters.Clear();
                if ((parameterList != null) && (parameterList.Count != 0))
                {
                    foreach (IDbDataParameter parameter in parameterList)
                    {
                        command.Parameters.Add(parameter);
                    }
                }
                return2.EffectRowCount = command.ExecuteNonQuery();
                if (return2.EffectRowCount <= 0)
                {
                    throw new Exception(Message.NoRowEffected);
                }
                return2.ReturnException = null;
            }
            catch (Exception exception)
            {
                this.errorSQL = command.CommandText;
                return2.EffectRowCount = 0;
                return2.ReturnException = exception;
                return2.ErrorSQL = this.errorSQL;
            }
            finally
            {
                command.Dispose();
                command = null;
            }
            return return2;
        }

        public DatabaseReturn ExecuteCommandAtOnce(List<IDbCommand> commandList)
        {
            this.errorSQL = "";
            this.count = 0;
            if ((this.connString == null) || (this.conn == null))
            {
                throw new Exception(Message.NoConnection);
            }
            DatabaseReturn return2 = new DatabaseReturn();
            if ((commandList != null) && (commandList.Count != 0))
            {
                try
                {
                    foreach (IDbCommand command in commandList)
                    {
                        if ((command != null) && !(command.CommandText == ""))
                        {
                            this.errorSQL = command.CommandText;
                            command.Connection = this.conn;
                            this.count = command.ExecuteNonQuery();
                            if (this.count <= 0)
                            {
                                throw new Exception(Message.NoRowEffected);
                            }
                            return2.EffectRowCount += this.count;
                        }
                    }
                    this.errorSQL = "";
                    return2.ReturnException = null;
                }
                catch (Exception exception)
                {
                    return2.EffectRowCount = 0;
                    return2.ReturnException = exception;
                    return2.ErrorSQL = this.errorSQL;
                }
            }
            return return2;
        }

        public IDbConnection GetDatabaseConnection()
        {
            if ((this.connString == null) || (this.conn == null))
            {
                throw new Exception(Message.NoConnection);
            }
            return this.conn;
        }

        public DatabaseConnectionString GetDatabaseConnectionString()
        {
            if (this.connString == null)
            {
                throw new Exception(Message.NoConnection);
            }
            return this.connString;
        }

        public DatabaseType GetDatabaseType()
        {
            if ((this.connString == null) || (this.conn == null))
            {
                throw new Exception(Message.NoConnection);
            }
            return this.databaseType;
        }

        public DateTime GetDBTime()
        {
            string sql = "";
            switch (this.databaseType)
            {
                case DatabaseType.Oracle:
                    sql = "SELECT SYSDATE FROM DUAL";
                    break;

                case DatabaseType.SQLServer:
                    sql = "SELECT CONVERT(DATETIME, SUBSTRING(CONVERT(NVARCHAR, GETDATE(), 121), 0, 20), 121)";
                    break;
            }
            return Convert.ToDateTime(this.Select(sql).Rows[0][0]);
        }

        public string GetExecutiveErrorSQL()
        {
            return this.errorSQL;
        }

        public string GetSystemSID()
        {
            DataTable table;
            string sql = "";
            switch (this.databaseType)
            {
                case DatabaseType.Oracle:
                    sql = "SELECT GetSystemSID FROM DUAL";
                    table = this.Select(sql);
                    return ((((table == null) || (table.Rows == null)) || (table.Rows.Count == 0)) ? "" : Convert.ToString(table.Rows[0][0]));

                case DatabaseType.SQLServer:
                    sql = " DECLARE @SEQ int ";
                    sql = (sql + " EXEC @SEQ =  GetNewSIDSequenceValue ") + " PRINT @SEQ " + " SELECT dbo.GetSystemSID() + RIGHT('00000' + CAST(@SEQ AS varchar(5)), 5) ";
                    table = this.Select(sql);
                    return ((((table == null) || (table.Rows == null)) || (table.Rows.Count == 0)) ? "" : Convert.ToString(table.Rows[0][0]));
            }
            return "";
        }

        public DataTable GetTableColumnsByTableName(string tableName)
        {
            string sql = "";
            List<IDbDataParameter> parameterList = new List<IDbDataParameter>();
            switch (this.databaseType)
            {
                case DatabaseType.Oracle:
                    sql = "SELECT COLUMN_NAME FROM USER_TAB_COLUMNS WHERE TABLE_NAME = :TABLE_NAME";
                    parameterList.Add(new OracleParameter("TABLE_NAME", tableName));
                    break;

                case DatabaseType.SQLServer:
                    sql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TABLE_NAME";
                    parameterList.Add(new SqlParameter("TABLE_NAME", tableName));
                    break;
            }
            return this.Select(sql, parameterList);
        }

        public IDbTransaction GetTransaction()
        {
            this.tx = this.conn.BeginTransaction();
            this.beginTxn = true;
            return this.tx;
        }

        public void OpenConnection()
        {
            if ((this.connString == null) || (this.conn == null))
            {
                throw new Exception(Message.NoConnection);
            }
            if (this.conn != null)
            {
                this.conn.Open();
            }
        }

        public DataTable Select(string sql)
        {
            return this.Select(sql, new List<IDbDataParameter>());
        }

        public DataTable Select(string sql, List<IDbDataParameter> parameterList)
        {
            DataTable table;
            this.errorSQL = "";
            try
            {
                if ((this.connString == null) || (this.conn == null))
                {
                    throw new Exception(Message.NoConnection);
                }
                IDbCommand command = this.conn.CreateCommand();
                command.CommandText = sql;
                if (this.beginTxn && (this.tx != null))
                {
                    command.Transaction = this.tx;
                }
                command.Parameters.Clear();
                if ((parameterList != null) && (parameterList.Count != 0))
                {
                    foreach (IDbDataParameter parameter in parameterList)
                    {
                        command.Parameters.Add(parameter);
                    }
                }
                IDataReader reader = command.ExecuteReader();
                table = this.ConvertDataReaderToTable(reader);
            }
            catch (Exception exception)
            {
                this.errorSQL = sql;
                throw exception;
            }
            return table;
        }

        public IDataReader SelectReader(string sql, List<IDbDataParameter> parameterList)
        {
            IDataReader reader;
            this.errorSQL = "";
            try
            {
                if ((this.connString == null) || (this.conn == null))
                {
                    throw new Exception(Message.NoConnection);
                }
                IDbCommand command = this.conn.CreateCommand();
                command.CommandText = sql;
                if (this.beginTxn && (this.tx != null))
                {
                    command.Transaction = this.tx;
                }
                command.Parameters.Clear();
                if ((parameterList != null) && (parameterList.Count != 0))
                {
                    foreach (IDbDataParameter parameter in parameterList)
                    {
                        command.Parameters.Add(parameter);
                    }
                }
                reader = command.ExecuteReader();
            }
            catch (Exception exception)
            {
                this.errorSQL = sql;
                throw exception;
            }
            return reader;
        }

        public void SetDatabaseExecutiveServiceConnectionString()
        {
            string str = ConfigurationManager.AppSettings["defaultConnectionString"];
            ConnectionStringSettingsCollection connectionStrings = ConfigurationManager.ConnectionStrings;
            this.connString = new DatabaseConnectionString();
            this.connString.ConnectionString = connectionStrings[str].ConnectionString;
            this.connString.ProviderName = connectionStrings[str].ProviderName;
            switch (this.connString.ProviderName)
            {
                case "System.Data.OracleClient":
                    this.conn = new OracleConnection(this.connString.ConnectionString);
                    this.databaseType = DatabaseType.Oracle;
                    return;

                case "System.Data.SqlClient":
                    this.conn = new SqlConnection(this.connString.ConnectionString);
                    this.databaseType = DatabaseType.SQLServer;
                    return;
            }
            throw new Exception(Message.NoSupportDatabaseType);
        }

        public void SetDatabaseExecutiveServiceConnectionString(DatabaseConnectionString connectionString)
        {
            this.connString = connectionString;
            this.connString.ConnectionString = connectionString.ConnectionString;
            this.connString.ProviderName = connectionString.ProviderName;
            switch (this.connString.ProviderName)
            {
                case "System.Data.OracleClient":
                    this.conn = new OracleConnection(this.connString.ConnectionString);
                    this.databaseType = DatabaseType.Oracle;
                    return;

                case "System.Data.SqlClient":
                    this.conn = new SqlConnection(this.connString.ConnectionString);
                    this.databaseType = DatabaseType.SQLServer;
                    return;
            }
            throw new Exception(Message.NoSupportDatabaseType);
        }
    }


    [DataContract]
    public sealed class DatabaseReturn
    {
        // Fields
        private int count;
        private Exception ex;
        private string sql;

        // Properties
        [DataMember]
        public int EffectRowCount
        {
            get
            {
                return this.count;
            }
            set
            {
                this.count = value;
            }
        }

        public string ErrorSQL
        {
            get
            {
                return this.sql;
            }
            set
            {
                this.sql = value;
            }
        }

        [DataMember]
        public Exception ReturnException
        {
            get
            {
                return this.ex;
            }
            set
            {
                this.ex = value;
            }
        }
    }

    [DataContract]
    public sealed class DatabaseTransaction
    {
        // Fields
        private List<IDbCommand> commandList = new List<IDbCommand>();
        private DatabaseExecutiveService dbes;

        // Properties
        [DataMember]
        public List<IDbCommand> DatabaseCommandList
        {
            get
            {
                return this.commandList;
            }
        }

        [DataMember]
        public IDbConnection DatabaseConnection
        {
            get
            {
                return this.dbes.GetDatabaseConnection();
            }
        }

        [DataMember]
        public DatabaseExecutiveService DatabaseExecutiveService
        {
            get
            {
                return this.dbes;
            }
            set
            {
                this.dbes = value;
            }
        }
    }

    [DataContract]
    public enum DatabaseType
    {
        Oracle,
        SQLServer
    }

    [DebuggerNonUserCode, GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0"), CompilerGenerated]
    internal class Message
    {
        // Fields
        private static CultureInfo resourceCulture;
        private static ResourceManager resourceMan;

        // Methods
        internal Message() { }

        // Properties
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture { get; set; }
        internal static string NoConnection { get; }
        internal static string NoRowEffected { get; }
        internal static string NoSupportDatabaseType { get; }
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager { get; }

        internal static string IllegalRights
        {
            get
            {
                return ResourceManager.GetString("IllegalRights", resourceCulture);
            }
        }
    }


}







