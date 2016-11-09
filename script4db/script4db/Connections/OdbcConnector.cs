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
        private OdbcConnection conn;
        private OdbcDataReader dataReader;
        private OdbcTableStructure2 tableStructure;
        private int affected;
        private string scalarResult;
        private string[] nonQueryCmdNames = new string[] { "DROP", "CREATE", "INSERT", "UPDATE", "DELETE", "RENAME", "ALTER" };
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
        public OdbcDataReader GetDataReader(string tableName)
        {
            string sql = string.Format("SELECT * FROM {0}", tableName);
            bool scalarNonQuery = false;
            keepAlive = true;
            ExecuteSQL(sql, scalarNonQuery);

            return dataReader;
        }

        public bool ExecuteSQL(string sqlText, bool scalar = false)
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
                        result = this.ExecuteQuery(sqlText, Conn, scalar);
                    }
                    catch (OdbcException odbcEx)
                    {
                        result = false;
                        string msg = String.Format("SQL '{0}'", sqlText);
                        this.LogMessages.Add(new LogMessage(errorLevel, this.GetType().Name, msg));
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

        private bool ExecuteQuery(string sqlText, OdbcConnection Conn, bool scalar = false)
        {
            // Reset previous result values
            this.affected = 0;
            this.scalarResult = null;
            this.dataReader = null;

            bool result = false;
            sqlText = sqlText.TrimStart();
            string cmdName = sqlText.Substring(0, sqlText.IndexOf(" ")).ToUpper();
            OdbcCommand command = new OdbcCommand(sqlText, Conn);

            if (cmdName == "SELECT")
            {
                if (scalar)
                {
                    var firstColumn = command.ExecuteScalar();
                    if (firstColumn != null)
                    {
                        this.scalarResult = firstColumn.ToString();
                        this.affected = 1;
                    }
                    else
                    {
                        this.scalarResult = null;
                        this.affected = 0;
                    }
                }
                else
                {
                    this.dataReader = command.ExecuteReader();
                    this.affected = dataReader.RecordsAffected;
                }
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

        public string ScalarResult
        {
            get { return this.scalarResult; }
        }

        public string GetTableFields(string tableName)
        {
            string tableFields = "";

            try
            {
                DbOpenIfClosed();
                try
                {
                    this.tableStructure = new OdbcTableStructure2(Conn, tableName);
                    tableFields = tableStructure.SkeletonCreate;
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

        public string GetInsertSql(OdbcDataReader dataReader, string tableTarget)
        {
            string fieldNames = tableStructure.FieldNames;
            string fieldValues = tableStructure.FieldValues;

            if (string.IsNullOrWhiteSpace(fieldNames) || string.IsNullOrWhiteSpace(fieldValues)) // error
            {
                string msg = string.Format("Insert skeleton is not defined for table '{0}' ", tableTarget);
                this.LogMessages.Add(new LogMessage(ErrorLevel, this.GetType().Name, msg));
                return "";
            }

            string oldValue, newValue;
            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                oldValue = ":" + dataReader.GetName(i);
                newValue = ValueToString(dataReader, i);
                Console.WriteLine("-----------");
                Console.WriteLine(dataReader.GetName(i) + " = " + dataReader.GetValue(i).ToString());
                Console.WriteLine(dataReader.GetFieldType(i).ToString());
                Console.WriteLine("===========");
                fieldValues = fieldValues.Replace(oldValue, newValue);
            }

            return string.Format("INSERT INTO {0} ({1}) VALUES ({2});", tableTarget, fieldNames, fieldValues);
        }

        private string ValueToString(OdbcDataReader dataReader, int fieldNum)
        {
            string result = dataReader.GetValue(fieldNum).ToString();

            switch (dataReader.GetFieldType(fieldNum).FullName)
            {
                case "System.Single":
                case "System.Double":
                case "System.Decimal":
                    result = result.Replace(",", ".");
                    break;
                default:
                    break;
            }

            return result;
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
