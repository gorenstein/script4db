using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public bool Check()
        {
            if (this.name == BlockNames.constants) return true;
            if (this.name == BlockNames.command) return CheckCommandParameters();

            this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, "Block name is not defined"));

            return false;
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
            get
            {
                return name;
            }
        }

        public Dictionary<string, string> Parameters
        {
            get
            {
                return parameters;
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
