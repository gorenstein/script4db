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

        public bool addBlock(Block _block)
        {
            if (!blocksGroup.ContainsKey(_block.Name))
            {
                // Create group for blocks with current block name 
                blocksGroup.Add(_block.Name, new ArrayList());
            }

            ArrayList blockList = blocksGroup[_block.Name];
            if (blocksGroup.TryGetValue(_block.Name, out blockList))
            {
                // Check multiple blocks
                if (blockList.Count > 0 && !isAllowedMutipleBlocks[_block.Name])
                {
                    string msg = "Multiple blocks for block '" + _block.Name.ToString() + "' not allowed.";
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
                    string msg = String.Format("Can't add parsed block '{0}'", _block.Name);
                    this.logMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
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

    }
}
