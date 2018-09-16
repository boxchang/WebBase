using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;


namespace BoxCommonLib
{
    public class DBController : IDisposable
    {
        // Fields
        private List<IDbCommand> _iDbCommandList;
        private string connectionString;
        private DatabaseExecutiveService dbes;
        private const string DefaultSingleSelectSqlStringForOracle = "SELECT {0} FROM {1} WHERE {2} =:{3} ";
        private const string DefaultSingleSelectSqlStringForSqlServer = "SELECT {0} FROM {1} WHERE {2} =@{3} ";
        private string provider;

        // Methods
        public DBController()
        {
            this._iDbCommandList = new List<IDbCommand>();
            this.dbes = new DatabaseExecutiveService();
            this.dbes.SetDatabaseExecutiveServiceConnectionString();
            this.dbes.OpenConnection();
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["defaultConnectionString"]];
            this.connectionString = settings.ConnectionString;
            this.provider = settings.ProviderName;
        }

        public DBController(ConnectionStringSettings connectionStringSettings)
        {
            this._iDbCommandList = new List<IDbCommand>();
            DatabaseConnectionString connectionString = new DatabaseConnectionString();
            connectionString.ConnectionString = connectionStringSettings.ConnectionString;
            connectionString.ProviderName = connectionStringSettings.ProviderName;
            this.dbes = new DatabaseExecutiveService();
            this.dbes.SetDatabaseExecutiveServiceConnectionString(connectionString);
            this.dbes.OpenConnection();
            this.connectionString = connectionStringSettings.ConnectionString;
            this.provider = connectionStringSettings.ProviderName;
        }

        public void AddCommand(IDbCommand iDbCommand)
        {
            this._iDbCommandList.Add(iDbCommand);
        }

        public void AddCommandParameter(List<IDbDataParameter> parameters, string parameterName, object parameterValue)
        {
            parameters.Add(this.CreateCommandParameter(parameterName, parameterValue));
        }

        public void AddCommandParameter(IDataParameterCollection parameters, string parameterName, object parameterValue)
        {
            if (parameterValue == null)
            {
                parameterValue = string.Empty;
            }
            string provider = this.provider;
            if (provider != null)
            {
                if (!(provider == "System.Data.OracleClient"))
                {
                    if (provider == "System.Data.SqlClient")
                    {
                        parameters.Add(new SqlParameter(parameterName, parameterValue));
                    }
                }
                else
                {
                    parameters.Add(new OracleParameter(parameterName, parameterValue));
                }
            }
        }

        public DataTable BuildTableCloneByTableName(string tableName)
        {
            return this.dbes.BuildTableCloneByTableName(tableName);
        }

        public void CloseConnection()
        {
            if (this.dbes != null)
            {
                this.dbes.CloseConnection();
            }
        }

        public IDbCommand CreateCommand()
        {
            return this.dbes.GetDatabaseConnection().CreateCommand();
        }

        public IDbCommand CreateCommand(string sql, List<IDbDataParameter> parameters)
        {
            IDbCommand command = this.dbes.GetDatabaseConnection().CreateCommand();
            command.CommandText = this.GetCommandText(sql, SQLStringType.OracleSQLString);
            command.Parameters.Clear();
            foreach (IDbDataParameter parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }
            return command;
        }

        public IDbDataParameter CreateCommandParameter(string parameterName, object parameterValue)
        {
            switch (this.provider)
            {
                case "System.Data.OracleClient":
                    parameterName = parameterName.Replace("@", ":");
                    return new OracleParameter(parameterName, parameterValue);

                case "System.Data.SqlClient":
                    parameterName = parameterName.Replace(":", "@");
                    return new SqlParameter(parameterName, parameterValue);
            }
            throw new Exception(string.Format("Database Provider {0} is not support.", this.provider));
        }

        public void Dispose()
        {
            if (this.dbes != null)
            {
                this.dbes.CloseConnection();
                this.dbes = null;
            }
            this.provider = null;
            this._iDbCommandList = null;
            GC.SuppressFinalize(this);
        }

        public int DoTransaction(List<IDbCommand> commands)
        {
            DatabaseReturn return2 = this.dbes.Execute(commands);
            if (return2.ReturnException != null)
            {
                throw new Exception(this.GetAllInnerExceptionString(return2.ErrorSQL, return2.ReturnException));
            }
            return return2.EffectRowCount;
        }

        public int DoTransaction(string[] sqls)
        {
            DatabaseReturn return2 = this.dbes.Execute(sqls);
            if (return2.ReturnException != null)
            {
                throw new Exception(this.GetAllInnerExceptionString(return2.ErrorSQL, return2.ReturnException));
            }
            return return2.EffectRowCount;
        }

        public int DoTransaction(List<IDbCommand> commands, IDbTransaction transaction)
        {
            DatabaseReturn return2 = this.dbes.Execute(commands, transaction);
            if (return2.ReturnException != null)
            {
                throw new Exception(this.GetAllInnerExceptionString(return2.ErrorSQL, return2.ReturnException));
            }
            return return2.EffectRowCount;
        }

        public int Execute(string sql)
        {
            return this.Execute(sql, new List<IDbDataParameter>());
        }

        public int Execute(string sql, List<IDbDataParameter> parameters)
        {
            return this.Execute(sql, parameters, false);
        }

        public int Execute(string sql, List<IDbDataParameter> parameters, bool showErrorSQL)
        {
            DatabaseReturn return2 = this.dbes.Execute(sql, parameters);
            if (return2.ReturnException != null)
            {
                throw new Exception(this.GetAllInnerExceptionString(return2.ErrorSQL, return2.ReturnException));
            }
            return return2.EffectRowCount;
        }

        [Obsolete("此方法已過期，禁止呼叫；已呼叫的物件，請改正為呼叫 DoTransaction(List<IDbCommand> commands)。")]
        public int ExecuteCommandAtOnce(List<IDbCommand> commands)
        {
            DatabaseReturn return2 = this.dbes.ExecuteCommandAtOnce(commands);
            if (return2.ReturnException != null)
            {
                throw new Exception(this.GetAllInnerExceptionString(return2.ErrorSQL, return2.ReturnException));
            }
            return return2.EffectRowCount;
        }

        public int ExecuteCommands()
        {
            DatabaseReturn return2 = this.dbes.Execute(this._iDbCommandList);
            if (return2.ReturnException != null)
            {
                throw new DBException(return2.ErrorSQL, return2.ReturnException);
            }
            return return2.EffectRowCount;
        }

        public string[] GetAllColumnName(string sql)
        {
            IDataReader reader = this.dbes.SelectReader(sql, null);
            int index = 0;
            string[] strArray = new string[reader.FieldCount];
            while (index < reader.FieldCount)
            {
                strArray[index] = reader.GetName(index).ToString();
                index++;
            }
            return strArray;
        }

        public string GetAllColumnsByTableName(string tableName)
        {
            return this.GetAllDBColumnsByTableName(tableName);
        }

        private string GetAllDBColumnsByTableName(string tableName)
        {
            string str = string.Empty;
            DataTable tableColumnsByTableName = this.dbes.GetTableColumnsByTableName(tableName);
            if (tableColumnsByTableName != null)
            {
                for (int i = 0; i < tableColumnsByTableName.Rows.Count; i++)
                {
                    if (i == (tableColumnsByTableName.Rows.Count - 1))
                    {
                        str = str + tableColumnsByTableName.Rows[i][0].ToString();
                    }
                    else
                    {
                        str = str + tableColumnsByTableName.Rows[i][0].ToString() + ", ";
                    }
                }
            }
            return str;
        }

        public string GetAllInnerExceptionString(string message, Exception innerException)
        {
            if (innerException == null)
            {
                return message;
            }
            return (message + " innerException:" + innerException.Message);
        }

        public string GetCommandText(string sql, SQLStringType type)
        {
            string provider = this.provider;
            if (provider != null)
            {
                if (!(provider == "System.Data.OracleClient"))
                {
                    if (provider == "System.Data.SqlClient")
                    {
                        switch (type)
                        {
                            case SQLStringType.OracleSQLString:
                                sql = sql.Replace(":", "@");
                                sql = sql.Replace("||", "+");
                                sql = sql.Replace("NVL", "ISNULL");
                                sql = sql.Replace("SYSDATE", "SYSDATETIME()");
                                return sql;

                            case SQLStringType.SqlServerSQLString:
                                return sql;
                        }
                    }
                    return sql;
                }
                switch (type)
                {
                    case SQLStringType.OracleSQLString:
                        return sql;

                    case SQLStringType.SqlServerSQLString:
                        sql = sql.Replace("@", ":");
                        sql = sql.Replace("+", "||");
                        sql = sql.Replace("ISNULL", "NVL");
                        sql = sql.Replace("SYSDATETIME()", "SYSDATE");
                        return sql;
                }
            }
            return sql;
        }

        public IDbConnection GetConnection()
        {
            return this.dbes.GetDatabaseConnection();
        }

        public static DBController GetDBController()
        {
            return new DBController();
        }

        public string GetDBID()
        {
            string format = string.Empty;
            string provider = this.provider;
            if (provider != null)
            {
                if (!(provider == "System.Data.OracleClient"))
                {
                    if (provider == "System.Data.SqlClient")
                    {
                        string str2 = string.Empty;
                        string[] strArray = this.dbes.GetDatabaseConnectionString().ConnectionString.Split(new char[] { ';' });
                        for (int i = 0; i < strArray.Length; i++)
                        {
                            string[] strArray2 = strArray[i].Split(new char[] { '=' });
                            if (strArray2[0] == "Initial Catalog")
                            {
                                str2 = strArray2[1];
                                break;
                            }
                        }
                        format = "SELECT DB_ID('{0}')";
                        format = string.Format(format, str2);
                        goto Label_00EA;
                    }
                }
                else
                {
                    format = " SELECT DBID FROM V$DATABASE ";
                    goto Label_00EA;
                }
            }
            throw new Exception(string.Format("Database Provider {0} is not support.", this.provider));
        Label_00EA:
            return Convert.ToString(this.Select(format).Rows[0][0]);
        }

        public DateTime GetDBMicroTime()
        {
            string sql = string.Empty;
            string provider = this.provider;
            if (provider != null)
            {
                if (!(provider == "System.Data.OracleClient"))
                {
                    if (provider == "System.Data.SqlClient")
                    {
                        sql = "SELECT GETDATE()";
                    }
                }
                else
                {
                    sql = "SELECT SYSTIMESTAMP FROM DUAL";
                }
            }
            return Convert.ToDateTime(this.Select(sql).Rows[0][0]);
        }

        public DateTime GetDBTime()
        {
            return this.dbes.GetDBTime();
        }

        public DataTable GetEmptyTableCloneByTableName(string tableName)
        {
            DataTable table = new DataTable(tableName);
            string allDBColumnsByTableName = this.GetAllDBColumnsByTableName(tableName);
            if (allDBColumnsByTableName != null)
            {
                string[] strArray = allDBColumnsByTableName.Split(new char[] { ',' });
                if ((strArray == null) || (strArray.Length == 0))
                {
                    return table;
                }
                for (int i = 0; i < strArray.Length; i++)
                {
                    table.Columns.Add(strArray[i]);
                }
            }
            return table;
        }

        public DataTable GetOneColumnDataBySID(string SelectColumnName, string TableName, string SIDColumnName, string sidvalue)
        {
            string sql = string.Empty;
            string providerName = this.ProviderName;
            if (providerName != null)
            {
                if (!(providerName == "System.Data.SqlClient"))
                {
                    if (providerName == "System.Data.OracleClient")
                    {
                    }
                }
                else
                {
                    sql = string.Format("SELECT {0} FROM {1} WHERE {2} =@{3} ", new object[] { SelectColumnName, TableName, SIDColumnName, SIDColumnName });
                    goto Label_0087;
                }
            }
            sql = string.Format("SELECT {0} FROM {1} WHERE {2} =:{3} ", new object[] { SelectColumnName, TableName, SIDColumnName, SIDColumnName });
        Label_0087:
            sql = this.GetCommandText(sql, SQLStringType.OracleSQLString);
            List<IDbDataParameter> parameters = new List<IDbDataParameter>();
            this.AddCommandParameter(parameters, SIDColumnName, sidvalue);
            return this.Select(sql, parameters);
        }

        public string GetOneColumnValueBySID(string SelectColumnName, string TableName, string SIDColumnName, string sidvalue)
        {
            DataTable table = this.GetOneColumnDataBySID(SelectColumnName, TableName, SIDColumnName, sidvalue);
            if ((table == null) || (table.Rows.Count == 0))
            {
                return "";
            }
            return Convert.ToString(table.Rows[0][SelectColumnName]);
        }

        public string GetRegister(string encryptedData, string saltkey)
        {
            return Encrypter.DecryptAES(encryptedData, saltkey);
        }

        public string GetSID()
        {
            return this.dbes.GetSystemSID();
        }

        public string GetSQLServerIdentification(string idType)
        {
            string format = string.Empty;
            string str5 = idType;
            if (str5 != null)
            {
                if (!(str5 == "DBID"))
                {
                    if (str5 == "SERVERNAME")
                    {
                        format = "SELECT @@SERVERNAME";
                        goto Label_00F9;
                    }
                    if (str5 == "SERVICENAME")
                    {
                        format = "SELECT @@SERVICENAME";
                        goto Label_00F9;
                    }
                }
                else
                {
                    string str2 = string.Empty;
                    string[] strArray = this.dbes.GetDatabaseConnectionString().ConnectionString.Split(new char[] { ';' });
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        string[] strArray2 = strArray[i].Split(new char[] { '=' });
                        if (strArray2[0] == "Initial Catalog")
                        {
                            str2 = strArray2[1];
                            break;
                        }
                    }
                    format = "SELECT DB_ID('{0}')";
                    format = string.Format(format, str2);
                    goto Label_00F9;
                }
            }
            throw new Exception(string.Format("Unknown identification type {0}, please input DBID, SERVERNAME or SERVICENAME", idType));
        Label_00F9:
            return Convert.ToString(this.Select(format).Rows[0][0]);
        }

        public IDbTransaction GetTransaction()
        {
            return this.dbes.GetTransaction();
        }

        public string IsExecuteVaild(string sql)
        {
            DatabaseReturn return2 = this.dbes.Execute(sql);
            if (return2.ReturnException == null)
            {
                return null;
            }
            return return2.ReturnException.Message.Replace("沒有資料被影響異動。", "").Replace("没有数据被影响异动。", "").Replace("No data is affecting changes.", "");
        }

        public bool IsTableExist(string tableName)
        {
            List<IDbDataParameter> list;
            string sql = string.Empty;
            string provider = this.provider;
            if (provider != null)
            {
                if (!(provider == "System.Data.OracleClient"))
                {
                    if (provider == "System.Data.SqlClient")
                    {
                        sql = " SELECT COUNT(NAME) AS CNT FROM SYSOBJECTS WHERE NAME = @TABLE_NAME ";
                        goto Label_0057;
                    }
                }
                else
                {
                    sql = " SELECT COUNT(TABLE_NAME) AS CNT FROM USER_TABLES WHERE TABLE_NAME = :TABLE_NAME ";
                    goto Label_0057;
                }
            }
            throw new Exception(string.Format("Database Provider {0} is not support.", this.provider));
        Label_0057:
            list = new List<IDbDataParameter>();
            this.AddCommandParameter(list, "TABLE_NAME", tableName);
            DataTable table = this.Select(sql, list);
            if ((((table == null) || (table.Rows == null)) || (table.Rows.Count == 0)) || (Convert.ToDecimal(table.Rows[0]["CNT"]) == 0M))
            {
                return false;
            }
            return true;
        }

        public void OpenConnection()
        {
            if (((ConnectionState)this.DatabaseConnectionState) != ConnectionState.Open)
            {
                this.dbes.OpenConnection();
            }
        }

        public DataTable Select(string sql)
        {
            string provider = this.provider;
            if (provider != null)
            {
                if (!(provider == "System.Data.OracleClient"))
                {
                    if (provider == "System.Data.SqlClient")
                    {
                        sql = sql.Replace(":", "@");
                        sql = sql.Replace("||", "+");
                        sql = sql.Replace("NVL", "ISNULL");
                        sql = sql.Replace("SYSDATE", "SYSDATETIME()");
                    }
                }
                else
                {
                    sql = sql.Replace("@", ":");
                    sql = sql.Replace("+", "||");
                    sql = sql.Replace("ISNULL", "NVL");
                    sql = sql.Replace("SYSDATETIME()", "SYSDATE");
                }
            }
            return this.dbes.Select(sql);
        }

        public DataTable Select(string sql, List<IDbDataParameter> parameters)
        {
            string provider = this.provider;
            if (provider != null)
            {
                if (!(provider == "System.Data.OracleClient"))
                {
                    if (provider == "System.Data.SqlClient")
                    {
                        sql = sql.Replace(":", "@");
                        sql = sql.Replace("||", "+");
                        sql = sql.Replace("NVL", "ISNULL");
                        sql = sql.Replace("SYSDATE", "SYSDATETIME()");
                    }
                }
                else
                {
                    sql = sql.Replace("@", ":");
                    sql = sql.Replace("+", "||");
                    sql = sql.Replace("ISNULL", "NVL");
                    sql = sql.Replace("SYSDATETIME()", "SYSDATE");
                }
            }
            return this.dbes.Select(sql, parameters);
        }

        public DataTable SelectNoChangeTxt(string sql)
        {
            return this.dbes.Select(sql);
        }

        public DataTable SelectNoChangeTxt(string sql, List<IDbDataParameter> parameters)
        {
            return this.dbes.Select(sql, parameters);
        }

        public IDataReader SelectReader(string sql, List<IDbDataParameter> parameters)
        {
            string provider = this.provider;
            if (provider != null)
            {
                if (!(provider == "System.Data.OracleClient"))
                {
                    if (provider == "System.Data.SqlClient")
                    {
                        sql = sql.Replace(":", "@");
                        sql = sql.Replace("||", "+");
                        sql = sql.Replace("NVL", "ISNULL");
                        sql = sql.Replace("SYSDATE", "SYSDATETIME()");
                    }
                }
                else
                {
                    sql = sql.Replace("@", ":");
                    sql = sql.Replace("+", "||");
                    sql = sql.Replace("ISNULL", "NVL");
                    sql = sql.Replace("SYSDATETIME()", "SYSDATE");
                }
            }
            return this.dbes.SelectReader(sql, parameters);
        }

        // Properties
        internal DatabaseExecutiveService _DBES
        {
            get
            {
                return this.dbes;
            }
        }

        public string ConnectionString
        {
            get
            {
                return this.connectionString;
            }
        }

        public ConnectionState? DatabaseConnectionState
        {
            get
            {
                if (this.dbes == null)
                {
                    return null;
                }
                return new ConnectionState?(this.dbes.GetDatabaseConnection().State);
            }
        }

        public string ProviderName
        {
            get
            {
                return this.provider;
            }
        }
    }

    public class DBException : Exception
    {
        // Fields
        private Exception ex;
        private string sql;

        // Methods
        public DBException(string errorSQL, Exception e)
        {
            this.sql = errorSQL;
            this.ex = e;
        }

        // Properties
        public string ErrorSQL
        {
            get
            {
                return this.sql;
            }
        }

        public Exception Exception
        {
            get
            {
                return this.ex;
            }
        }
    }

    public enum SQLStringType
    {
        OracleSQLString,
        SqlServerSQLString
    }

}
