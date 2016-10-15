﻿using System;
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
        public int ExecuteSQL(string sqlText)
        {
            int result = -42;

            OdbcConnection conn = new OdbcConnection();
            conn.ConnectionString = connString;

            try
            {
                conn.Open();
                if (String.IsNullOrWhiteSpace(sqlText)) result = 0;
                else
                {
                    try
                    {
                        result = this.ExecuteQuery(sqlText, conn);
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
                            this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                        }
                        result = -2;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = String.Format("By open connection error '{0}'", ex.Message);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                result = -1;
            }
            finally
            {
                conn.Close();
            }

            return result;
        }

        private int ExecuteQuery(string sqlText, OdbcConnection conn)
        {
            int result;
            sqlText = sqlText.TrimStart();
            string cmdName = sqlText.Substring(0, sqlText.IndexOf(" "));
            OdbcCommand command = new OdbcCommand(sqlText, conn);

            if (cmdName.ToUpper() == "SELECT")
            {
                OdbcDataReader dataReader = command.ExecuteReader();
                result = dataReader.RecordsAffected;
            }
            else
            {
                // DROP, CREATE  => return -1
                // INSERT, UPDATE, DELETE => return count
                result = Math.Abs(command.ExecuteNonQuery());
            }

            return result;
        }

        public bool IsLive()
        {
            string sql = "";
            if (ExecuteSQL(sql) >= 0) return true;
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
