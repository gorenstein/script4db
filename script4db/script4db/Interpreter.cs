﻿using System;
using System.Collections;
using System.Windows.Forms;
using script4db.ScriptProcessors;

namespace script4db
{
    class Interpreter
    {
        private String processorName;
        public ArrayList LogMessages = new ArrayList();
        private IScriptProcessor scriptProcessor;

        public Interpreter(String procName, String textRaw)
        {
            this.processorName = procName;

            switch (this.processorName)
            {
                case ".script4db":
                    this.LogMessages.Add(new LogMessage(LogMessageTypes.Info, this.GetType().Name, "Using Interpreter processor 'Default'"));
                    this.scriptProcessor = new ScriptProcessors.Default.Processor(textRaw);
                    break;
                default:
                    string msg = String.Format("File extension '{0}' is NOT supported", this.processorName);
                    this.LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                    break;
            }

            foreach (LogMessage logMsg in scriptProcessor.LogMessages) this.LogMessages.Add(logMsg);
        }

        public void FillRichTextBox(RichTextBox richTextBox)
        {
            this.scriptProcessor.FillRichTextBox(richTextBox);
        }

        public bool HasError()
        {
            foreach (LogMessage logMsg in this.LogMessages)
            {
                if (logMsg.Type == LogMessageTypes.Error) return true;
            }
            return false;
        }

        public IScriptProcessor ScriptProcessor
        {
            get { return scriptProcessor; }
        }

        public string ProcessorName
        {
            get { return processorName; }
        }
    }
}
