using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script4db.Connections
{
    class OdbcConnector : IConnector
    {
        private bool isLive;
        private string source;
        private string login;
        private string password;

        public OdbcConnector(string _source, string _login, string _password)
        {
            source = _source;
            login = _login;
            password = _password;
        }

        public bool IsLive
        {
            get
            {
                return false;
            }
        }

    }
}
