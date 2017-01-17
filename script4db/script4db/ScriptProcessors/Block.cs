using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Globalization;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using script4db.Connections;
using System.ComponentModel;

namespace script4db.ScriptProcessors
{
    enum BlockStatuses
    {
        Init,
        ReadyToRun,
        Working,
        Done,
        Warning,
        Error
    }

    class Block
    {
        private BlockNames _name;
        private BlockStatuses _status;
        public int order;
        public TreeNode node;
        private string _nodeName;
        private Color[] ColorByStatus = new Color[] { Color.DarkGray, Color.Black, Color.DarkOrange, Color.DarkGreen, Color.DarkOrange, Color.Red };
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        private ArrayList logMessages = new ArrayList();

        private string[] cmdSimpleRequered = new string[] { "type", "connection", "sql" };
        private string[] cmdSimpleOptional = new string[] { "onErrorContinue" };

        private string[] cmdTableExportRequered = new string[] { "type", "connectionSource", "tableSource", "connectionTarget", "tableTarget" };
        private string[] cmdTableExportOptional = new string[] { "onErrorContinue", "maxPerLoop" };

        private string[] onErrorContinueValues = new string[] { "true", "false" };
        private string[] connectionParameterNames = new string[] { "connection", "connectionSource", "connectionTarget" };

        Dictionary<string, string> integratedConstants = new Dictionary<string, string>();
        public Block(BlockNames blockName)
        {
            _name = blockName;
            _status = BlockStatuses.Init;

            DateTime dtNow = DateTime.Now;
            integratedConstants.Add("${TIMESTAMP}", dtNow.ToString("yyyyMMddHHmmssffff"));
            integratedConstants.Add("${YEAR}", dtNow.ToString("yyyy"));
            integratedConstants.Add("${MONTH}", dtNow.ToString("MM"));
            integratedConstants.Add("${DAY}", dtNow.ToString("dd"));

            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;
            integratedConstants.Add("${WEEK}", cal.GetWeekOfYear(dtNow, dfi.CalendarWeekRule, dfi.FirstDayOfWeek).ToString());
            integratedConstants.Add("${WEEK_PREV}", cal.GetWeekOfYear(dtNow.AddDays(-7), dfi.CalendarWeekRule, dfi.FirstDayOfWeek).ToString());
            integratedConstants.Add("${WEEK_NEXT}", cal.GetWeekOfYear(dtNow.AddDays(7), dfi.CalendarWeekRule, dfi.FirstDayOfWeek).ToString());
        }

        public bool Run(BackgroundWorker worker)
        {
            if (_name != BlockNames.command)
            {
                string msg = String.Format("Command '{0}' can't be run", _name);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                return false;
            }

            bool success;
            LogMessageTypes executeErrorLevel;

            if ("true" == this.parameters["onErrorContinue"]) executeErrorLevel = LogMessageTypes.Warning;
            else executeErrorLevel = LogMessageTypes.Error;

            Status = BlockStatuses.Working;

            switch (this.parameters["type"])
            {
                case "simple":
                    node.Text += " : 1 of 1 working...";
                    success = RunSimlpe(executeErrorLevel);
                    break;
                case "exportTable":
                    success = RunExportTable(executeErrorLevel, worker);
                    break;
                default:
                    string msg = String.Format("Value for Command parameter type='{0}' is not supported", this.parameters["type"]);
                    this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                    return false;
            }

            if (success)
            {
                //Status = BlockStatuses.Done;
            }
            else if (!success && "true" == this.parameters["onErrorContinue"])
            {
                //Status = BlockStatuses.Warning;
                Status = BlockStatuses.Done;
                node.Text += " : Skipped";
                success = true;
            }
            else
            {
                Status = BlockStatuses.Error;
            }

            return success;
        }

        private bool RunSimlpe(LogMessageTypes executeErrorLevel)
        {
            string sql = this.parameters["sql"];
            string rawConnString = this.parameters["connection"];
            Connection connection = new Connection(rawConnString);
            connection.Connector.ErrorLevel = executeErrorLevel;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool scalarOrNonQuery = true;
            bool success = connection.ExecuteSQL(sql, scalarOrNonQuery);
            sw.Stop();
            if (success)
            {
                string msg = String.Format("Elapsed {0:0.000}s : Affected {1} rec : '{2}'", sw.Elapsed.TotalSeconds, connection.Connector.Affected, sql);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Info, this.GetType().Name, msg));

                Status = BlockStatuses.Done;
                node.Text = String.Format("{0} - Ok : Affected {1} rec : Elapsed {2:0.000}s", NodeName, connection.Connector.Affected, sw.Elapsed.TotalSeconds);

                return true;
            }
            else
            {
                node.Text = String.Format("{0} - error", NodeName);
                foreach (LogMessage logMsg in connection.LogMessages) this.LogMessages.Add(logMsg);
                return false;
            }
        }

        private bool RunExportTable(LogMessageTypes executeErrorLevel, BackgroundWorker worker)
        {
            // Define Connections for Source & Target
            Connection connSource = new Connection(this.parameters["connectionSource"]);
            Connection connTarget = new Connection(this.parameters["connectionTarget"]);
            connSource.Connector.KeepAlive = true;
            connSource.Connector.ErrorLevel = executeErrorLevel;
            connTarget.Connector.KeepAlive = true;
            connTarget.Connector.ErrorLevel = executeErrorLevel;

            bool success;
            bool scalarOrNonQuery;
            string msg;
            string sql;
            string nodeBaseText = NodeName + " " + BlockStatuses.Working.ToString();
            string tableSource = parameters["tableSource"];
            string tableTarget = parameters["tableTarget"];
            Stopwatch sw = new Stopwatch();

            sql = connSource.GetCreateTableSql(tableSource, tableTarget, connTarget.Connector.DataBaseType);
            if (string.IsNullOrWhiteSpace(sql))
            {
                foreach (LogMessage logMsg in connSource.LogMessages) this.LogMessages.Add(logMsg);
                success = false;
            }
            else // Create Target table as copy of Source table structure
            {
                //Console.WriteLine(sql);
                scalarOrNonQuery = true;
                sw.Start();
                success = connTarget.ExecuteSQL(sql, scalarOrNonQuery);
                sw.Stop();
            }

            if (success)
            {
                node.Text = String.Format("{0} : Copy table structure : Elapsed {1:0.000}s", nodeBaseText, sw.Elapsed.TotalSeconds);
                msg = String.Format("Elapsed {0:0.000}s : '{1}'", sw.Elapsed.TotalSeconds, sql);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Info, this.GetType().Name, msg));
            }
            else
            {
                msg = String.Format("SQL: '{0}'", sql);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Info, this.GetType().Name, msg));
                foreach (LogMessage logMsg in connTarget.LogMessages) this.LogMessages.Add(logMsg);
                connSource.Connector.DbCloseIfOpen();
                connTarget.Connector.DbCloseIfOpen();
                return false;
            }

            // Copy Data
            int recordCountSource = connSource.CountOfRecords(tableSource);
            int restToCopy;
            Stopwatch swRead = new Stopwatch();
            Stopwatch swWrite = new Stopwatch();
            double avReadSec = 0;
            double avWriteSec = 0;
            double restSec = 0;

            int perLoop; // Limit for max Read/Insert records per interation
            if (connTarget.Connector.DataBaseType == Connections.DbType.MySQL || connTarget.Connector.DataBaseType == Connections.DbType.MSSQL)
            {
                int n;
                bool isNumeric = int.TryParse(parameters["maxPerLoop"], out n);
                if (!isNumeric || n < 0)
                {
                    perLoop = 1;
                    msg = String.Format("Set 'maxPerLoop' to default value '1'. Check on Error the script file.");
                    this.LogMessages.Add(new LogMessage(LogMessageTypes.Warning, this.GetType().Name, msg));
                }
                else perLoop = n;  // Use value from script
            }
            else
            {
                perLoop = 1; // Access NOT supported multiple rows INSERT
            }
            int maxLoops = (int)Math.Ceiling((decimal)recordCountSource / perLoop);
            int updRate = (int)(Math.Exp(Math.Log10(maxLoops)));

            OdbcDataReader dataReader = connSource.Connector.GetDataReader(tableSource);
            if (!dataReader.HasRows) // No data for copy
            {
                msg = String.Format("No data for copy from table '{0}'", tableSource);
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Info, this.GetType().Name, msg));
                foreach (LogMessage logMsg in connTarget.LogMessages) this.LogMessages.Add(logMsg);
                connSource.Connector.DbCloseIfOpen();
                connTarget.Connector.DbCloseIfOpen();
                return false;
            }

            String insertSqlHead = connSource.GetInsertSqlHead(tableTarget);
            String insertSqlValues;

            for (int loop = 0; loop < maxLoops; loop++)
            {
                // User sended Cancel ?
                if (worker.CancellationPending == true)
                {
                    connSource.Connector.DbCloseIfOpen();
                    connTarget.Connector.DbCloseIfOpen();
                    return false;
                }

                avReadSec = (double)swRead.Elapsed.TotalSeconds / (double)((loop + 1) * perLoop);
                avWriteSec = (double)swWrite.Elapsed.TotalSeconds / (double)((loop + 1) * perLoop);
                restToCopy = recordCountSource - (loop * perLoop);
                restSec = (double)restToCopy * (avReadSec + avWriteSec);

                if ((loop % updRate) == 0)
                {
                    node.Text = String.Format(
                        "{0} : Rest {1} rec / {3:0.00} s : Average r/w {4:0.00000} / {5:0.00000} s per record",
                        nodeBaseText, restToCopy, recordCountSource, restSec, avReadSec, avWriteSec);
                }

                // Read
                insertSqlValues = "";
                swRead.Start();
                for (int i = 0; i < perLoop; i++)
                {
                    if (dataReader.Read())
                    {
                        if (!String.IsNullOrWhiteSpace(insertSqlValues)) insertSqlValues += ",";
                        insertSqlValues += connSource.GetInsertSqlValues(dataReader, tableTarget, connTarget.Connector.DataBaseType);
                    }
                    else break;
                }
                swRead.Stop();

                sql = insertSqlHead + insertSqlValues + ";";

                // Write
                if (string.IsNullOrWhiteSpace(sql)) success = false;
                else // Insert to target - copy next scope of records
                {
                    //Console.WriteLine(sql);
                    scalarOrNonQuery = true;
                    swWrite.Start();
                    success = connTarget.ExecuteSQL(sql, scalarOrNonQuery);
                    swWrite.Stop();
                }

                if (!success)
                {
                    msg = String.Format("Copy scope from record '{0}' next '{1}'", loop, perLoop);
                    this.LogMessages.Add(new LogMessage(LogMessageTypes.Info, this.GetType().Name, msg));
                    foreach (LogMessage logMsg in connSource.LogMessages) this.LogMessages.Add(logMsg);
                    foreach (LogMessage logMsg in connTarget.LogMessages) this.LogMessages.Add(logMsg);
                    connSource.Connector.DbCloseIfOpen();
                    connTarget.Connector.DbCloseIfOpen();
                    return false;
                }
            }

            connSource.Connector.DbCloseIfOpen();
            connTarget.Connector.DbCloseIfOpen();

            Status = BlockStatuses.Done;

            avReadSec = (double)swRead.Elapsed.TotalSeconds / (double)recordCountSource;
            avWriteSec = (double)swWrite.Elapsed.TotalSeconds / (double)recordCountSource;

            node.Text = String.Format(
                    "{0} - Ok : Copied {1} records : Average r/w {2:0.00000} / {3:0.00000} s per record : Elapsed {4:0.00}s",
                      NodeName, recordCountSource, avReadSec, avWriteSec, swRead.Elapsed.TotalSeconds + swWrite.Elapsed.TotalSeconds);

            msg = String.Format("Elapsed {0:0.000}s : Copied in to table '{1}' {2} records",
                                    swRead.Elapsed.TotalSeconds + swWrite.Elapsed.TotalSeconds, tableTarget, recordCountSource);
            //Console.WriteLine(msg);
            this.LogMessages.Add(new LogMessage(LogMessageTypes.Info, this.GetType().Name, msg));
            return true;
        }

        public bool Check()
        {
            if (_name == BlockNames.constants) return true;
            if (_name == BlockNames.command) return CheckCommandParameters();

            this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, "Block name is not defined"));

            return false;
        }

        public ArrayList ConnectionsStrings()
        {
            ArrayList connections = new ArrayList();

            if (_name != BlockNames.command) return connections;

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
            if (_name != BlockNames.command) return true;

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
                defValue = "1";

                if (this.parameters.Keys.Contains(paramName))
                {
                    string paramValue = this.parameters[paramName];
                    if (IsPlaceHolder(paramValue)) return true;
                    else
                    {
                        int n;
                        bool isNumeric = int.TryParse(paramValue, out n);

                        if (!isNumeric || n < 0)
                        {
                            string msg = String.Format("Value '{0}' for parameter '{1}' is not supported", paramValue, paramName);
                            this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                            return false;
                        }
                    }
                }
                else this.AddParameter(paramName, defValue);
            }

            return true;
        }

        private bool IsPlaceHolder(String value)
        {
            value = value.Trim();
            if (value.StartsWith("${") && value.EndsWith("}")) return true;
            else return false;
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
                this.parameters.Add(key, FillIntegratedConstants(value));
            }

            return this.parameters.ContainsKey(key);
        }

        private string FillIntegratedConstants(string value)
        {
            if (value.IndexOf("${") != -1)
            {
                foreach (KeyValuePair<string, string> constant in integratedConstants)
                {
                    value = value.Replace(constant.Key, constant.Value);
                }
            }

            return value;
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
            get { return _name; }
        }

        public Dictionary<string, string> Parameters
        {
            get { return parameters; }
        }

        public ArrayList LogMessages
        {
            get { return logMessages; }
        }

        public BlockStatuses Status
        {
            get { return _status; }
            set
            {
                _status = value;
                if (_name == BlockNames.command)
                {
                    //node.Text = NodeName + " - " + value.ToString();
                    node.ForeColor = ColorByStatus[(int)value];
                }
            }
        }

        public string NodeName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_nodeName))
                {
                    _nodeName = order.ToString();

                    if (Name == BlockNames.command)
                    {
                        switch (this.parameters["type"])
                        {
                            case "simple":
                                int len = parameters["sql"].IndexOf(" "); // DROP TABLE Xyz
                                string word = parameters["sql"].Substring(0, Math.Max(len, 4));
                                word = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(word.ToLower());
                                _nodeName += " " + word;
                                break;
                            case "exportTable":
                                _nodeName += " Export";
                                break;
                            default:
                                _nodeName += " " + parameters["type"];
                                break;
                        }
                    }
                }
                return _nodeName;
            }
        }
    }
}
public static class extensions
{
    public static int IndexOfNth(this string str, string value, int nth = 1)
    {
        if (nth <= 0)
            throw new ArgumentException("Can not find the zeroth index of substring in string. Must start with 1");
        int offset = str.IndexOf(value);
        for (int i = 1; i < nth; i++)
        {
            if (offset == -1) return -1;
            offset = str.IndexOf(value, offset + 1);
        }
        return offset;
    }
}
