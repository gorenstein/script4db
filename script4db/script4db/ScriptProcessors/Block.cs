using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script4db.ScriptProcessors
{
    class Block
    {
        private BlockNames name;
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        private ArrayList logMessages = new ArrayList();

        private string[] cmdSimpleAllowed = new string[] { "type", "onErrorStop", "connection", "sql" };
        private string[] cmdTableExportAllowed = new string[] { "type", "onErrorStop", "connectionSource", "tableSource", "connectionTarget", "tableTarget" };

        private string[] cmdSimpleRequered = new string[] { "type", "connection", "sql" };
        private string[] cmdTableExportRequered = new string[] { "type", "connectionSource", "tableSource", "connectionTarget", "tableTarget" };


        public Block(BlockNames _name)
        {
            this.name = _name;
        }

        public bool Check()
        {
            if (this.name == BlockNames.constants) return true;
            if (this.name == BlockNames.command) return checkCommandParameters();

            this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, "Block name is not defined."));

            return false;
        }

        private bool checkCommandParameters()
        {
            // Get/Check command Type
            if (!this.parameters.ContainsKey("type"))
            {
                this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, "Command 'type' parameter is not defined."));
                return false;
            }
            string cmdType = this.parameters["type"];
            if (cmdType != "simple" && cmdType != "exportTable")
            {
                string msg = String.Format("Command type='{0}' is not supported.", cmdType);
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
                        string msg = String.Format("Parameter '{0}' is empty.", parameter.Key);
                        this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                        return false;
                    }
                    continue;
                }
            }

            // Check on requered parameters
            if (!isPresentRequeredCmdParameters(cmdType))
            {
                return false;
            }

            return true;
        }

        private bool isAllowedCmdParameter(string cmdType, string paramName)
        {
            switch (cmdType)
            {
                case "simple": return this.cmdSimpleAllowed.Contains(paramName);
                case "exportTable": return this.cmdSimpleAllowed.Contains(paramName);
                default: throw new System.ArgumentException("It's must be never reachable.", this.GetType().Name);
            }
        }

        private bool isPresentRequeredCmdParameters(string cmdType)
        {
            string[] requerdParameters;
            switch (cmdType)
            {
                case "simple": requerdParameters = this.cmdSimpleRequered; break;
                case "exportTable": requerdParameters = this.cmdTableExportRequered; break;
                default: throw new System.ArgumentException("It's must be never reachable.", this.GetType().Name);
            }

            foreach (var paramName in requerdParameters)
            {
                if (!this.parameters.Keys.Contains(paramName))
                {
                    string msg = String.Format("Parameter '{0}' is requered.", paramName);
                    this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                    return false;
                }
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
