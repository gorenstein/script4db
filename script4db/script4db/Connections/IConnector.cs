using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace script4db.Connections
{
    interface IConnector
    {
        ArrayList LogMessages { get; }
        bool IsLive();
        int ExecuteSQL(string sql);
    }
}
