using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script4db.Connections
{
    enum ConnTypes { ODBC, MySQL, OleAccess };

    class Connection
    {
        public string rawConnString;
        public bool isCorrectRawConnString;
        private ConnTypes type;
        private string source;
        private string login;
        private string password;
        private IConnector connector;
        private ArrayList logMessages = new ArrayList();

        public Connection(string _rawConnString)
        {
            // connectionStringRaw: Type>>source>>user>>password
            this.rawConnString = _rawConnString;
            string[] connParams = this.rawConnString.Split(new String[] { ">>" }, StringSplitOptions.None);
            this.isCorrectRawConnString = this.CheckRawConnString(connParams);
            if (this.isCorrectRawConnString)
            {
                this.type = (ConnTypes)Enum.Parse(typeof(ConnTypes), connParams[0]);
                this.source = connParams[1];
                this.login = connParams[2];
                this.password = connParams[3];
            }
        }

        public bool IsLive()
        {
            // TODO Add logs
            //foreach (LogMessage logMsg in connector.LogMessages) this.LogMessages.Add(logMsg);
            return this.Connector.IsLive;
        }

        private bool CheckRawConnString(string[] connParams)
        {
            if (connParams.Count() < 2)
            {
                string msg = String.Format("Too few parameters '{0}'", this.rawConnString);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                return false;
            }
            if (!Enum.GetNames(typeof(ConnTypes)).Contains(connParams[0]))
            {
                string msg = String.Format("Connection Type '{0}' is not supported", connParams[0]);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                return false;
            }
            if (String.IsNullOrWhiteSpace(connParams[1]))
            {
                string msg = String.Format("Connection Source is not defined '{0}'", this.rawConnString);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                return false;
            }

            return true;
        }

        public ArrayList LogMessages
        {
            get
            {
                return logMessages;
            }
        }

        public IConnector Connector
        {
            get
            {
                if (null == this.connector)
                {
                    switch (this.type)
                    {
                        case ConnTypes.ODBC:
                        case ConnTypes.MySQL:
                        case ConnTypes.OleAccess:
                            connector = new OdbcConnector(this.source, this.login, this.password);
                            break;
                        default:
                            throw new System.ArgumentException("It's must be never reachable", this.GetType().Name);
                    }
                }
                return this.connector;
            }
        }
    }
}
