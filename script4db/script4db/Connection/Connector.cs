using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script4db.Connection
{
    class Connector
    {
        enum ConnTypes { ODBC, MySQL, OleAccess };
        public string rawConnString;
        public bool isCorrectRawConnString;
        public string[] connParams;
        private ArrayList logMessages = new ArrayList();

        public Connector(string _rawConnString)
        {
            // connectionStringRaw: Type>>source>>user>>password
            this.rawConnString = _rawConnString;
            this.connParams = this.rawConnString.Split(new String[] { ">>" }, StringSplitOptions.None);
            this.isCorrectRawConnString = this.CheckRawConnString();
        }

        private bool CheckRawConnString()
        {
            if (this.connParams.Count() < 2)
            {
                string msg = String.Format("Too few parameters '{0}'", this.rawConnString);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                return false;
            }
            if (!Enum.GetNames(typeof(ConnTypes)).Contains(this.connParams[0]))
            {
                string msg = String.Format("Connection Type '{0}' is not supported", this.connParams[0]);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                return false;
            }
            if (String.IsNullOrWhiteSpace(this.connParams[1]))
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
    }
}
