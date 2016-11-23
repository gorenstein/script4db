using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Data.Odbc;

namespace script4db.Connections
{
    enum DbType
    {
        undefined,
        unknow,
        Access,
        MySQL,
        MSSQL,
        Oracle
    }

    interface IConnector
    {
        void DbCloseIfOpen();
        bool KeepAlive { get; set; }
        bool IsLive();
        bool ExecuteSQL(string sql, bool scalar);
        string ScalarResult { get; }
        int Affected { get; }
        DbType DataBaseType { get; }
        string GetTableFieldsListAsSqlSyntax(string tableName, DbType targetDbType);
        string GetInsertSql(OdbcDataReader dataReader, string tableTarget, DbType targetDbType);
        OdbcDataReader GetDataReader(string tableName);
        LogMessageTypes ErrorLevel { get; set; }
        ArrayList LogMessages { get; }
    }
}
