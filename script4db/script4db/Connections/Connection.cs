using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Threading.Tasks;

namespace script4db.Connections
{
    //enum ConnTypes { ODBC, ODBC_String, MySQL, OleAccess };
    enum ConnTypes { ODBC, OdbcString };

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
                this.login = (connParams.Count() < 3) ? "" : connParams[2];
                this.password = (connParams.Count() < 4) ? "" : connParams[3];
            }
        }

        public string GetInsertSql(OdbcDataReader dataReader, string tableTarget, DbType targetDbType)
        {
            string sql = connector.GetInsertSql(dataReader, tableTarget, targetDbType);

            if (string.IsNullOrWhiteSpace(sql))
            {
                foreach (LogMessage logMsg in Connector.LogMessages) this.LogMessages.Add(logMsg);
                string msg = string.Format("Insert skeleton is not defined for table '{0}' ", tableTarget);
                this.LogMessages.Add(new LogMessage(connector.ErrorLevel, this.GetType().Name, msg));
            }

            return sql;
        }

        public int CountOfRecords(string tableName)
        {
            string sql = string.Format("SELECT COUNT(*) FROM {0}", tableName);
            bool scalar = true;

            connector.ExecuteSQL(sql, scalar);

            if (connector.ScalarResult == null)
            {
                return 0;
            }
            else
            {
                return int.Parse(connector.ScalarResult);
            }
        }

        public string GetCreateTableSql(string tableSource, string tableTarget, DbType targetDbType)
        {
            string sql;
            string tableFields = connector.GetTableFieldsListAsSqlSyntax(tableSource, targetDbType);

            if (string.IsNullOrWhiteSpace(tableFields))
            {
                sql = "";
                foreach (LogMessage logMsg in Connector.LogMessages) this.LogMessages.Add(logMsg);
                string msg = string.Format("Can't determine structure of table '{0}' ", tableSource);
                this.LogMessages.Add(new LogMessage(connector.ErrorLevel, this.GetType().Name, msg));
            }
            else
            {
                sql = string.Format("CREATE TABLE {0} ({1});", tableTarget, tableFields);
            }

            return sql;
        }

        public bool ExecuteSQL(string sqlText, bool scalar = false)
        {
            if (Connector.ExecuteSQL(sqlText, scalar)) return true;
            else
            {
                foreach (LogMessage logMsg in Connector.LogMessages) this.LogMessages.Add(logMsg);
                return false;
            }
        }

        public bool IsLive()
        {
            if (!this.Connector.IsLive())
            {
                foreach (LogMessage logMsg in Connector.LogMessages) this.LogMessages.Add(logMsg);
                return false;
            }

            return true;
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
            get { return logMessages; }
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
                            this.connector = new OdbcConnector(this.source, this.login, this.password);
                            break;
                        case ConnTypes.OdbcString:
                            this.connector = new OdbcConnector(this.source);
                            break;
                        //case ConnTypes.MySQL:
                        //case ConnTypes.OleAccess:
                        default:
                            throw new System.ArgumentException("It's must be never reachable", this.GetType().Name);
                    }
                }

                return this.connector;
            }
        }
    }
}
