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
        bool ExecuteSQL(string sql);
        int Affected { get; }
        string GetTableFields(string tableName);
        LogMessageTypes ErrorLevel { get; set; }
        ArrayList LogMessages { get; }
    }
}
