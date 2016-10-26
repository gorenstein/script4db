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

        public OdbcConnector(string _source, string _login, string _password)
        {
            source = _source;
            login = _login;
            password = _password;
            connString = String.Format("DSN={0};", source);
            if (!String.IsNullOrEmpty(login))
            {
                connString += String.Format(" UID={0}; PWD={1};", login, password);
            }
        }

        //OdbcDataReader myReader = myCommand.ExecuteReader();
        public bool ExecuteSQL(string sqlText, LogMessageTypes executeErrorLevel = LogMessageTypes.Error, bool keepAlive = false)
        {
            bool result;

            try
            {
                DbOpenIfClosed();
                if (String.IsNullOrWhiteSpace(sqlText)) result = true; // Only testing Live connection 
                else
                {
                    try
                    {
                        result = this.ExecuteQuery(sqlText, Conn);
                    }
                    catch (OdbcException odbcEx)
                    {
                        for (int i = 0; i < odbcEx.Errors.Count; i++)
                        {
                            string msg = "By exicute SQL ERROR #" + i + " " +
                                          "Message: " + odbcEx.Errors[i].Message + " :: " +
                                          "Native: " + odbcEx.Errors[i].NativeError.ToString() + " :: " +
                                          "Source: " + odbcEx.Errors[i].Source + " :: " +
                                          "SQL: " + odbcEx.Errors[i].SQLState;
                            this.LogMessages.Add(new LogMessage(executeErrorLevel, this.GetType().Name, msg));
                        }
                        result = false;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = String.Format("By open connection '{0}'", ex.Message);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                result = false;
            }
            finally
            {
                if (!keepAlive) DbCloseIfOpen();
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
                string msg = String.Format("Not supported sql command '{0}'", cmdName);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                result = false;
            }

            return result;
        }

        public bool IsLive()
        {
            if (ExecuteSQL("")) return true;
            else
            {
                string msg = String.Format("Can't connected to '{0}'", connString);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
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
    }
}
