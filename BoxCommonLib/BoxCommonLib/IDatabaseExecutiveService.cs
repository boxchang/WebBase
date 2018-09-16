using BoxCommonLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;

[ServiceContract]
public interface IDatabaseExecutiveService
{
    // Methods
    [OperationContract]
    DataTable BuildTableCloneByTableName(string tableName);
    [OperationContract]
    void CloseConnection();
    [OperationContract(Name = "ExecuteTransactionWithCommands")]
    DatabaseReturn Execute(List<IDbCommand> commandList);
    [OperationContract(Name = "ExecuteSQL")]
    DatabaseReturn Execute(string sql);
    [OperationContract(Name = "ExecuteTransactionWithSQLs")]
    DatabaseReturn Execute(string[] sqls);
    [OperationContract(Name = "ExecuteTransactionByTransaction")]
    DatabaseReturn Execute(List<IDbCommand> commandList, IDbTransaction transaction);
    [OperationContract(Name = "ExecuteWithParameters")]
    DatabaseReturn Execute(string sql, List<IDbDataParameter> parameterList);
    [OperationContract(Name = "ExecuteCommandAtOnce")]
    DatabaseReturn ExecuteCommandAtOnce(List<IDbCommand> commandList);
    [OperationContract]
    IDbConnection GetDatabaseConnection();
    [OperationContract]
    DatabaseConnectionString GetDatabaseConnectionString();
    [OperationContract]
    DatabaseType GetDatabaseType();
    [OperationContract]
    DateTime GetDBTime();
    [OperationContract]
    string GetExecutiveErrorSQL();
    [OperationContract]
    string GetSystemSID();
    [OperationContract]
    DataTable GetTableColumnsByTableName(string tableName);
    [OperationContract]
    IDbTransaction GetTransaction();
    [OperationContract]
    void OpenConnection();
    [OperationContract(Name = "Query")]
    DataTable Select(string sql);
    [OperationContract(Name = "QueryWithParameters")]
    DataTable Select(string sql, List<IDbDataParameter> parameterList);
    IDataReader SelectReader(string sql, List<IDbDataParameter> parameterList);
    [OperationContract(Name = "SetDatabaseExecutiveServiceDefaultConnectionString")]
    void SetDatabaseExecutiveServiceConnectionString();
    [OperationContract]
    void SetDatabaseExecutiveServiceConnectionString(DatabaseConnectionString connectionString);
}


