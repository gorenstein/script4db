using System;
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

        public Block(BlockNames _name)
        {
            this.name = _name;
        }

        public bool Check()
        {
            // TODO Check syntax
            //throw new System.ArgumentException("Multiple blocks for block '" + _block.Name.ToString() + "' not allowed.", this.GetType().Name);

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
    }
}
