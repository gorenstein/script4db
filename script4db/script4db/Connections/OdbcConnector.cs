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
        public int ExecuteSQL(string sql)
        {
            int result = -42;

            OdbcConnection conn = new OdbcConnection();
            conn.ConnectionString = connString;

            try
            {
                conn.Open();
                if (String.IsNullOrWhiteSpace(sql)) result = 0;
                else
                {
                    //TODO Run command
                    result = 1;
                }
            }
            catch (Exception ex)
            {
                string msg = String.Format("Connection error '{0}'", ex.Message);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                result = -1;
            }
            finally
            {
                conn.Close();
            }

            return result;
        }

        public bool IsLive()
        {
            if (ExecuteSQL("") == 0) return true;
            else
            {
                string msg = String.Format("Can't connected to '{0}'", connString);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                return false;
            }
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
