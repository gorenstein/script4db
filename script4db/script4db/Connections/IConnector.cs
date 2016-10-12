using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script4db.Connections
{
    interface IConnector
    {
        bool IsLive { get; }
    }
}
