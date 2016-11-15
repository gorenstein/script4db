using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Data.Odbc;

namespace script4db.Connections
{
    interface IConnector
    {
        void DbCloseIfOpen();
        bool KeepAlive { get; set; }
        bool IsLive();
        bool ExecuteSQL(string sql, bool scalar);
        string ScalarResult { get; }
        int Affected { get; }
        string DataSource { get; }
        string GetTableFieldsListAsSqlSyntax(string tableName, string targetSqlSyntax);
        string GetInsertSql(OdbcDataReader dataReader, string tableTarget, string targetSqlSyntax);
        OdbcDataReader GetDataReader(string tableName);
        LogMessageTypes ErrorLevel { get; set; }
        ArrayList LogMessages { get; }
    }
}
