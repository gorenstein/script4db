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
        OdbcDataReader DataReader { get; }
        int Affected { get; }
        ArrayList LogMessages { get; }
        bool IsLive();
        void DbCloseIfOpen();
        bool ExecuteSQL(string sql, LogMessageTypes executeErrorLevel, bool keepAlive);
    }
}
