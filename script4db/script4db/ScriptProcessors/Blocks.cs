using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script4db.ScriptProcessors
{
    enum BlockNames
    {
        constants,
        command
    }

    class Blocks
    {
        // All script blocks from file, grouped by block name
        private Dictionary<BlockNames, ArrayList> blocksGroup;
        // Rules for blocks
        private Dictionary<BlockNames, bool> isAllowedMutipleBlocks;
        // LogMessages
        private ArrayList logMessages = new ArrayList();

        public Blocks()
        {
            blocksGroup = new Dictionary<BlockNames, ArrayList>();
            isAllowedMutipleBlocks = new Dictionary<BlockNames, bool>();
            isAllowedMutipleBlocks.Add(BlockNames.constants, false);
            isAllowedMutipleBlocks.Add(BlockNames.command, true);
        }

        public Dictionary<BlockNames, ArrayList> BlocksGroup
        {
            get
            {
                return blocksGroup;
            }
        }

        public ArrayList LogMessages
        {
            get
            {
                return logMessages;
            }
        }

        public bool TestDbConnections()
        {
            foreach (var blocksGroup in this.BlocksGroup)
            {
                foreach (Block block in blocksGroup.Value)
                {
                    if (block.Name == BlockNames.command)
                    {
                        if (!block.TestDbConnection())
                        {
                            foreach (LogMessage logMsg in block.LogMessages) this.LogMessages.Add(logMsg);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public bool addBlock(Block _block)
        {
            if (!blocksGroup.ContainsKey(_block.Name))
            {
                // Create group for blocks with current block name 
                blocksGroup.Add(_block.Name, new ArrayList());
            }

            //ArrayList blockList = blocksGroup[_block.Name];
            ArrayList blockList;
            if (blocksGroup.TryGetValue(_block.Name, out blockList))
            {
                // Check multiple blocks
                if (blockList.Count > 0 && !isAllowedMutipleBlocks[_block.Name])
                {
                    string msg = "Multiple blocks for block '" + _block.Name.ToString() + "' not allowed";
                    this.logMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                    return false;
                }
                else if (_block.Check())
                {
                    blockList.Add(_block);
                    string msg = String.Format("Added parsed block '{0}'", _block.Name);
                    this.logMessages.Add(new LogMessage(LogMessageTypes.Info, this.GetType().Name, msg));
                    return true;
                }
                else
                {
                    foreach (LogMessage logMsg in _block.LogMessages) this.LogMessages.Add(logMsg);
                    string msg = String.Format("Can't add parsed block '{0}'", _block.Name);
                    this.logMessages.Add(new LogMessage(LogMessageTypes.Warning, this.GetType().Name, msg));
                    return false;
                }
            }
            else
            {
                throw new System.ArgumentException("Can't add block '" + _block.Name.ToString() + "'.", this.GetType().Name);
            }
        }

        public static int BlockNamesID(string _name)
        {
            foreach (BlockNames blockName in Enum.GetValues(typeof(BlockNames)))
            {
                if (blockName.ToString() == _name) return (int)blockName;

            }
            //block not supported
            return -1;
        }

        public bool FillPlaceHolders()
        {
            foreach (var blocksGroup in this.BlocksGroup)
            {
                foreach (Block block in blocksGroup.Value)
                {
                    foreach (KeyValuePair<string, string> parameter in block.Parameters.ToList())
                    {
                        if (parameter.Value.IndexOf("${") != -1)
                        {
                            if (!ReplacePlaceHolders(block, parameter))
                            {
                                string msg = String.Format("Can't set Constant value in parameter string: '{0}'", parameter.Value);
                                LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        private bool ReplacePlaceHolders(Block _block, KeyValuePair<string, string> _parameter)
        {
            ArrayList constantsBlockList;
            if (this.BlocksGroup.TryGetValue(BlockNames.constants, out constantsBlockList))
            {
                // Costants Block exist
                switch (constantsBlockList.Count)
                {
                    case 0: return true;
                    case 1: // Allowed only Only One set of parameters
                        string newParameterValue = _parameter.Value;
                        Block constantsBlock = (Block)constantsBlockList[0];
                        foreach (var parameter in constantsBlock.Parameters)
                        {
                            string placeHolderKey = "${" + parameter.Key + "}";
                            string placeHolderValue = parameter.Value;
                            newParameterValue = newParameterValue.Replace(placeHolderKey, placeHolderValue);
                        }

                        if (newParameterValue.IndexOf("${") != -1)
                        {
                            string msg = String.Format("Not all Constants are defined: '{0}'", newParameterValue);
                            LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                            return false;
                        }
                        return _block.UpdateParameter(new KeyValuePair<string, string>(_parameter.Key, newParameterValue));
                    default: // Allowed only Only One set of parameters or none
                        throw new System.ArgumentException("It's must be never reachable", this.GetType().Name);
                }
            }
            return true;
        }
    }
}
