using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script4db.ScriptProcessors.Default
{
    class Processor : IScriptProcessor
    {

        // Data for Pass
        ///////////////
        // content of script file
        private string textRaw;
        // Splited textRaw by line
        private String[] scriptLines;
        // Current passing block
        private Block block = null;

        private Blocks blocks = new Blocks();
        private ArrayList logMessages = new ArrayList();

        public Processor(String scriptText)
        {
            this.TextRaw = scriptText;
            this.scriptLines = this.TextRaw.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            this.Run();
        }

        private bool Run()
        {
            if (!this.Pass1())
            {
                string msg = String.Format("Syntax error on Pass #1");
                LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                return false;
            }
            if (!this.Pass2())
            {
                string msg = String.Format("Connection checking error on Pass #2");
                LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                return false;
            }

            return true;
        }

        // Pass RAW text line by line
        private bool Pass1()
        {
            int lineNumber = 0;
            foreach (String scriptLine in this.scriptLines)
            {
                string line = scriptLine.Trim();
                lineNumber++;

                // It's comment line
                if (line.First().ToString() == ";") continue;

                // It's begin of block line
                if (line.First().ToString() == "[" && line.Trim().Last().ToString() == "]")
                {
                    // If it's nexte block then save preview block
                    if (block != null)
                    {
                        if (false == this.Blocks.addBlock(block))
                        {
                            foreach (LogMessage logMsg in this.blocks.LogMessages) this.LogMessages.Add(logMsg);
                            return false;
                        }
                    }

                    string blockName = line.Substring(1, line.Length - 2).Trim();
                    int blockID = Blocks.BlockNamesID(blockName);
                    if (blockID == -1)
                    {
                        foreach (LogMessage logMsg in this.blocks.LogMessages) this.LogMessages.Add(logMsg);
                        string msg = String.Format("Line No: {0} - Not supported block name: '{1}'", lineNumber, blockName);
                        LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                        return false;
                    }
                    this.block = new Block((BlockNames)blockID);
                    continue;
                }

                // It's line with paar key=value
                int IndexOfSplitChar = line.IndexOf("=");
                string key = line.Substring(0, IndexOfSplitChar);
                string value = line.Substring(IndexOfSplitChar + 1);

                if (false == this.block.AddParameter(key, value))
                {
                    foreach (LogMessage logMsg in this.blocks.LogMessages) this.LogMessages.Add(logMsg);
                    string msg = String.Format("Line No: {0} - Can't add parameter: '{1}'. Maybe Duplicated?", lineNumber, key);
                    LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                    return false;
                }
            }
            return true;
        }

        // Pass to Check evalible of DB connection 
        private bool Pass2()
        {
            return false;
        }

        private void addParameterToBlock(string blockName, int blockNumber)
        {
        }

        public string TextRaw
        {
            get
            {
                return textRaw;
            }
            set
            {
                textRaw = value;
            }
        }

        public ArrayList LogMessages
        {
            get
            {
                return logMessages;
            }
        }

        public Blocks Blocks
        {
            get
            {
                return blocks;
            }
        }
    }
}
