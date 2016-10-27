using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Diagnostics;
using script4db.Connections;

namespace script4db.ScriptProcessors
{
    class Block
    {
        private BlockNames name;
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        private ArrayList logMessages = new ArrayList();

        private string[] cmdSimpleRequered = new string[] { "type", "connection", "sql" };
        private string[] cmdSimpleOptional = new string[] { "onErrorContinue" };

        private string[] cmdTableExportRequered = new string[] { "type", "connectionSource", "tableSource", "connectionTarget", "tableTarget" };
        private string[] cmdTableExportOptional = new string[] { "onErrorContinue", "maxPerLoop" };

        private string[] onErrorContinueValues = new string[] { "true", "false" };
        private string[] connectionParameterNames = new string[] { "connection", "connectionSource", "connectionTarget" };
        public Block(BlockNames _name)
        {
            this.name = _name;
        }

        public bool Run()
        {
            if (this.name != BlockNames.command)
            {
                string msg = String.Format("Command '{0}' can't be run", this.name);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                return false;
            }

            bool result;
            LogMessageTypes executeErrorLevel;

            if ("true" == this.parameters["onErrorContinue"]) executeErrorLevel = LogMessageTypes.Warning;
            else executeErrorLevel = LogMessageTypes.Error;

            switch (this.parameters["type"])
            {
                case "simple":
                    result = RunSimlpe(executeErrorLevel);
                    break;
                case "exportTable":
                    result = RunExportTable(executeErrorLevel);
                    break;
                default:
                    string msg = String.Format("Value for Command parameter type='{0}' is not supported", this.parameters["type"]);
                    this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                    return false;
            }

            if ("true" == this.parameters["onErrorContinue"]) return true;
            else return result;
        }

        private bool RunSimlpe(LogMessageTypes executeErrorLevel)
        {
            string sql = this.parameters["sql"];
            string rawConnString = this.parameters["connection"];
            Connection connection = new Connection(rawConnString);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool success = connection.ExecuteSQL(sql);
            sw.Stop();
            if (success)
            {
                string msg = String.Format("Elapsed {0:0.000}s : Affected {1} : '{2}'", sw.Elapsed.TotalSeconds, connection.Connector.Affected, sql);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Info, this.GetType().Name, msg));
                return true;
            }
            else
            {
                foreach (LogMessage logMsg in connection.LogMessages) this.LogMessages.Add(logMsg);
                return false;
            }
        }

        private bool RunExportTable(LogMessageTypes errorLevel)
        {
            // Define Connections for Source & Target
            Connection connSource = new Connection(this.parameters["connectionSource"]);
            Connection connTarget = new Connection(this.parameters["connectionTarget"]);
            connSource.Connector.KeepAlive = true;
            connSource.Connector.ErrorLevel = errorLevel;
            connTarget.Connector.KeepAlive = true;
            connTarget.Connector.ErrorLevel = errorLevel;

            bool success;
            string sql;

            // Get Create Table sql string for table
            sql = connSource.GetCreateTableSql(parameters["tableSource"], parameters["tableTarget"]);
            if (string.IsNullOrWhiteSpace(sql))
            {
                foreach (LogMessage logMsg in connSource.LogMessages) this.LogMessages.Add(logMsg);
                connSource.Connector.DbCloseIfOpen();
                connTarget.Connector.DbCloseIfOpen();
                return false;
            }

            // Create Target table as copy of Source table structure
            success = connTarget.ExecuteSQL(sql);
            if (!success)
            {
                foreach (LogMessage logMsg in connTarget.LogMessages) this.LogMessages.Add(logMsg);
                connSource.Connector.DbCloseIfOpen();
                connTarget.Connector.DbCloseIfOpen();
                return false;
            }

            // Select form source

            // Insert to target
            return true;
        }

        //private void Test2()
        //{
        //    SqlDataAdapter adapter1 = new SqlDataAdapter("select * from Table1", sqlConnectionString);
        //}

        //private void Test()
        //{
        //    //The connection strings needed: One for SQL and one for Access
        //    String accessConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\...\\test.accdb;";
        //    String sqlConnectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=Your_Catalog;Integrated Security=True";

        //    //Make adapters for each table we want to export
        //    SqlDataAdapter adapter1 = new SqlDataAdapter("select * from Table1", sqlConnectionString);
        //    SqlDataAdapter adapter2 = new SqlDataAdapter("select * from Table2", sqlConnectionString);

        //    //Fills the data set with data from the SQL database
        //    DataSet dataSet = new DataSet();
        //    adapter1.Fill(dataSet, "Table1");
        //    adapter2.Fill(dataSet, "Table2");

        //    //Create an empty Access file that we will fill with data from the data set
        //    ADOX.Catalog catalog = new ADOX.Catalog();
        //    catalog.Create(accessConnectionString);

        //    //Create an Access connection and a command that we'll use
        //    OleDbConnection accessConnection = new OleDbConnection(accessConnectionString);
        //    OleDbCommand command = new OleDbCommand();
        //    command.Connection = accessConnection;
        //    command.CommandType = CommandType.Text;
        //    accessConnection.Open();

        //    //This loop creates the structure of the database
        //    foreach (DataTable table in dataSet.Tables)
        //    {
        //        String columnsCommandText = "(";
        //        foreach (DataColumn column in table.Columns)
        //        {
        //            String columnName = column.ColumnName;
        //            String dataTypeName = column.DataType.Name;
        //            String sqlDataTypeName = getSqlDataTypeName(dataTypeName);
        //            columnsCommandText += "[" + columnName + "] " + sqlDataTypeName + ",";
        //        }
        //        columnsCommandText = columnsCommandText.Remove(columnsCommandText.Length - 1);
        //        columnsCommandText += ")";

        //        command.CommandText = "CREATE TABLE " + table.TableName + columnsCommandText;

        //        command.ExecuteNonQuery();
        //    }

        //    //This loop fills the database with all information
        //    foreach (DataTable table in dataSet.Tables)
        //    {
        //        foreach (DataRow row in table.Rows)
        //        {
        //            String commandText = "INSERT INTO " + table.TableName + " VALUES (";
        //            foreach (var item in row.ItemArray)
        //            {
        //                commandText += "'" + item.ToString() + "',";
        //            }
        //            commandText = commandText.Remove(commandText.Length - 1);
        //            commandText += ")";

        //            command.CommandText = commandText;
        //            command.ExecuteNonQuery();
        //        }
        //    }

        //    accessConnection.Close();
        //}

        public bool Check()
        {
            if (this.name == BlockNames.constants) return true;
            if (this.name == BlockNames.command) return CheckCommandParameters();

            this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, "Block name is not defined"));

            return false;
        }

        public ArrayList ConnectionsStrings()
        {
            ArrayList connections = new ArrayList();

            if (this.name != BlockNames.command) return connections;

            foreach (string conParam in connectionParameterNames)
            {
                if (this.parameters.ContainsKey(conParam))
                {
                    string rawConnString = this.parameters[conParam];
                    if (!connections.Contains(rawConnString))
                    {
                        connections.Add(rawConnString);
                    }
                }
            }

            return connections;
        }

        public bool TestDbConnection()
        {
            if (this.name != BlockNames.command) return true;

            foreach (string conParam in connectionParameterNames)
            {
                if (this.parameters.ContainsKey(conParam))
                {
                    string rawConnString = this.parameters[conParam];
                    Connection connection = new Connection(rawConnString);
                    if (!connection.isCorrectRawConnString)
                    {
                        foreach (LogMessage logMsg in connection.LogMessages) this.LogMessages.Add(logMsg);
                        return false;
                    }

                    if (!connection.IsLive())
                    {
                        foreach (LogMessage logMsg in connection.LogMessages) this.LogMessages.Add(logMsg);
                        return false;
                    }

                    return true;
                }
            }

            return true;
        }

        private bool CheckCommandParameters()
        {
            // Get/Check command Type
            if (!this.parameters.ContainsKey("type"))
            {
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, "Requered parameter 'type' is not defined"));
                return false;
            }
            string cmdType = this.parameters["type"];
            if (cmdType != "simple" && cmdType != "exportTable")
            {
                string msg = String.Format("Value for Command parameter type='{0}' is not supported", cmdType);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                return false;
            }

            // Check on case of All parameters are allowed
            foreach (var parameter in this.parameters)
            {
                if (this.isAllowedCmdParameter(cmdType, parameter.Key))
                {
                    if (String.IsNullOrWhiteSpace(parameter.Value))
                    {
                        string msg = String.Format("Value of parameter '{0}' is empty", parameter.Key);
                        this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                        return false;
                    }
                    continue;
                }
                else
                {
                    string msg = String.Format("Parameter '{0}' is not supported", parameter.Key);
                    this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                    return false;
                }
            }

            // Check on requered parameters
            if (!isPresentRequeredCmdParameters(cmdType)) return false;

            return true;
        }

        private bool isAllowedCmdParameter(string cmdType, string paramName)
        {
            switch (cmdType)
            {
                case "simple": return (this.cmdSimpleOptional.Contains(paramName) || this.cmdSimpleRequered.Contains(paramName));
                case "exportTable": return (this.cmdTableExportOptional.Contains(paramName) || this.cmdTableExportRequered.Contains(paramName));
                default: throw new System.ArgumentException("It's must be never reachable", this.GetType().Name);
            }
        }

        private bool isPresentRequeredCmdParameters(string cmdType)
        {
            string[] requerdParameters;
            switch (cmdType)
            {
                case "simple": requerdParameters = this.cmdSimpleRequered; break;
                case "exportTable": requerdParameters = this.cmdTableExportRequered; break;
                default: throw new System.ArgumentException("It's must be never reachable", this.GetType().Name);
            }

            foreach (var paramName in requerdParameters)
            {
                if (!this.parameters.Keys.Contains(paramName))
                {
                    string msg = String.Format("Parameter '{0}' is requered", paramName);
                    this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                    return false;
                }
            }

            return checkCmdDefaultParameters(cmdType);
        }

        private bool checkCmdDefaultParameters(string cmdType)
        {
            // Set/Check default value for optional parameters
            string paramName = "onErrorContinue";
            string defValue = "false";
            string[] allowedValues = this.onErrorContinueValues;

            if (this.parameters.Keys.Contains(paramName))
            {
                string paramValue = this.parameters[paramName];
                if (!allowedValues.Contains(paramValue))
                {
                    string msg = String.Format("Value '{0}' for parameter '{1}' is not supported", paramValue, paramName);
                    this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                    return false;
                }
            }
            else this.AddParameter(paramName, defValue);

            if (cmdType == "exportTable")
            {
                paramName = "maxPerLoop";
                defValue = "1000";

                if (this.parameters.Keys.Contains(paramName))
                {
                    string paramValue = this.parameters[paramName];
                    int n;
                    bool isNumeric = int.TryParse(paramValue, out n);

                    if (!isNumeric || n < 0)
                    {
                        string msg = String.Format("Value '{0}' for parameter '{1}' is not supported", paramValue, paramName);
                        this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                        return false;
                    }
                }
                else this.AddParameter(paramName, defValue);
            }

            return true;
        }

        public bool AddParameter(string key, string value)
        {
            if (this.parameters.ContainsKey(key))
            {
                // Key already exist - duplicate or overwriting not allowed 
                return false;
            }
            else
            {
                this.parameters.Add(key, value);
            }

            return this.parameters.ContainsKey(key);
        }

        public bool UpdateParameter(KeyValuePair<string, string> _parameter)
        {
            if (this.parameters.ContainsKey(_parameter.Key))
            {
                this.parameters[_parameter.Key] = _parameter.Value;
                return true;
            }

            // Key not exist 
            return false;
        }

        public BlockNames Name
        {
            get { return name; }
        }

        public Dictionary<string, string> Parameters
        {
            get { return parameters; }
        }

        public ArrayList LogMessages
        {
            get { return logMessages; }
        }
    }
}
