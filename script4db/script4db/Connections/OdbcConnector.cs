using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Odbc;


namespace script4db.Connections
{
    class OdbcConnector : IConnector
    {
        private string source;
        private string login;
        private string password;
        private string connString;
        private OdbcConnection conn;
        private OdbcDataReader dataReader;
        private int affected;
        private string[] nonQueryCmdNames = new string[] { "DROP", "CREATE", "INSERT", "UPDATE", "DELETE" };
        private ArrayList logMessages = new ArrayList();
        private bool keepAlive;
        private LogMessageTypes errorLevel;

        public OdbcConnector(string _source)
        {
            source = _source;
            keepAlive = false;
            ErrorLevel = LogMessageTypes.Error;

            connString = _source;
        }

        public OdbcConnector(string _source, string _login, string _password)
            : this(_source)
        {
            password = _password;
            login = _login;

            connString = string.Format("DSN={0};", source);
            if (!string.IsNullOrEmpty(login))
            {
                connString += string.Format(" UID={0}; PWD={1};", login, password);
            }
        }

        //OdbcDataReader myReader = myCommand.ExecuteReader();
        public bool ExecuteSQL(string sqlText)
        {
            bool result;

            try
            {
                DbOpenIfClosed();
                if (string.IsNullOrWhiteSpace(sqlText)) result = true; // Only testing Live connection 
                else
                {
                    try
                    {
                        result = this.ExecuteQuery(sqlText, Conn);
                    }
                    catch (OdbcException odbcEx)
                    {
                        result = false;
                        AddToLogOdbcError(odbcEx);
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                string msg = String.Format("By open connection '{0}'", ex.Message);
                this.LogMessages.Add(new LogMessage(errorLevel, this.GetType().Name, msg));
            }
            finally
            {
                if (!KeepAlive) DbCloseIfOpen();
            }

            return result;
        }

        private bool ExecuteQuery(string sqlText, OdbcConnection Conn)
        {
            bool result;
            sqlText = sqlText.TrimStart();
            string cmdName = sqlText.Substring(0, sqlText.IndexOf(" ")).ToUpper();
            OdbcCommand command = new OdbcCommand(sqlText, Conn);

            if (cmdName == "SELECT")
            {
                this.dataReader = command.ExecuteReader();
                this.affected = dataReader.RecordsAffected;
                result = true;
            }
            else if (this.nonQueryCmdNames.Contains(cmdName))
            {
                // DROP, CREATE  => return -1
                // INSERT, UPDATE, DELETE => return count
                this.affected = Math.Abs(command.ExecuteNonQuery());
                result = true;
            }
            else
            {
                string msg = string.Format("Not supported sql command '{0}'", cmdName);
                this.LogMessages.Add(new LogMessage(errorLevel, this.GetType().Name, msg));
                result = false;
            }

            return result;
        }

        public bool IsLive()
        {
            if (ExecuteSQL("")) return true;
            else
            {
                string msg = string.Format("Can't connected to '{0}'", connString);
                this.LogMessages.Add(new LogMessage(errorLevel, this.GetType().Name, msg));
                return false;
            }
        }

        private void DbOpenIfClosed()
        {
            if (Conn.State == ConnectionState.Closed) Conn.Open();
        }

        public void DbCloseIfOpen()
        {
            if (Conn.State != ConnectionState.Closed) Conn.Close();
        }

        public ArrayList LogMessages
        {
            get { return logMessages; }
        }

        public OdbcDataReader DataReader
        {
            get { return dataReader; }
        }

        public int Affected
        {
            get { return affected; }
        }

        private OdbcConnection Conn
        {
            get
            {
                if (null == this.conn)
                {
                    this.conn = new OdbcConnection();
                    this.conn.ConnectionString = this.connString;
                }
                return this.conn;
            }
        }

        public bool KeepAlive
        {
            get { return keepAlive; }
            set { keepAlive = value; }
        }

        public LogMessageTypes ErrorLevel
        {
            get { return errorLevel; }
            set { errorLevel = value; }
        }

        public string GetTableFields(string tableName)
        {
            string tableFields = "";

            try
            {
                DbOpenIfClosed();
                try
                {
                    // TODO tableFields
                }
                catch (OdbcException odbcEx)
                {
                    tableFields = "";
                    AddToLogOdbcError(odbcEx);
                }
            }
            catch (Exception ex)
            {
                tableFields = "";
                string msg = string.Format("By open connection '{0}'", ex.Message);
                this.LogMessages.Add(new LogMessage(errorLevel, this.GetType().Name, msg));
            }
            finally
            {
                if (!KeepAlive) DbCloseIfOpen();
            }

            return tableFields;
        }

        private void AddToLogOdbcError(OdbcException odbcEx)
        {
            for (int i = 0; i < odbcEx.Errors.Count; i++)
            {
                string msg = "By exicute SQL ERROR #" + i + " " +
                              "Message: " + odbcEx.Errors[i].Message + " :: " +
                              "Native: " + odbcEx.Errors[i].NativeError.ToString() + " :: " +
                              "Source: " + odbcEx.Errors[i].Source + " :: " +
                              "SQL: " + odbcEx.Errors[i].SQLState;
                this.LogMessages.Add(new LogMessage(ErrorLevel, this.GetType().Name, msg));
            }
        }
    }
}
