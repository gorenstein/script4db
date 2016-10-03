using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace script4db
{
    // Evalible Statuses
    enum ParserStatuses
    {
        Init,
        FileLoading,
        FileLoaded,
        FileNotExist,
        Parsing,
        ParseSuccesse,
        ParseError
    }

    class Parser
    {
        // Script fullpath File name for parce
        private String fileName;
        // Equal to script file conntent on disk
        private String textRaw;
        // Actuel Parser step - status   
        private ParserStatuses currentStatus;
        // LogMessages
        public ArrayList LogMessages = new ArrayList();

        public Parser(String fileName)
        {
            this.currentStatus = ParserStatuses.Init;
            if (this.LoadFile(fileName))
            {
                if (this.DoParse())
                {
                    this.currentStatus = ParserStatuses.ParseSuccesse;
                }
                else
                {
                    this.currentStatus = ParserStatuses.ParseError;
                }
            }
        }

        public bool LoadFile(String fileForParce)
        {
            this.Clear();
            this.currentStatus = ParserStatuses.FileLoading;

            if (File.Exists(fileForParce))
            {
                this.fileName = fileForParce;
                this.textRaw = File.ReadAllText(this.fileName);
                this.currentStatus = ParserStatuses.FileLoaded;
                LogMessages.Add(new LogMessage(LogMessageTypes.Info, "Parser", "Loaded script file: " + this.fileName));
                return true;
            }
            else
            {
                this.currentStatus = ParserStatuses.FileNotExist;
                LogMessages.Add(new LogMessage(LogMessageTypes.Error, "Parser", "Can't load script file: " + this.fileName));
                return false;
            }
        }

        public void Clear()
        {
            this.fileName = "";
            this.textRaw = "";
            this.LogMessages.Clear();
            this.currentStatus = ParserStatuses.Init;
        }

        public bool DoParse()
        {
            this.currentStatus = ParserStatuses.Parsing;
            LogMessages.Add(new LogMessage(LogMessageTypes.Info, this.GetType().Name, "Start parsing"));

            Interpreter interpreter = new Interpreter(Path.GetExtension(this.fileName), this.textRaw);
            this.textRaw = interpreter.ScriptProcessor.TextRaw;
            foreach (LogMessage logMsg in interpreter.LogMessages) this.LogMessages.Add(logMsg);

            if (interpreter.hasError())
            {
                LogMessages.Add(new LogMessage(LogMessageTypes.Info, this.GetType().Name, "Break parsing - has error"));
                return false;
            }
            else
            {
                LogMessages.Add(new LogMessage(LogMessageTypes.Info, this.GetType().Name, "Finish parsing"));
                return true;
            }
        }

        public string FileName
        {
            get
            {
                return fileName;
            }
        }

        public ParserStatuses CurrentStatus
        {
            get
            {
                return currentStatus;
            }

        }

        public string TextRaw
        {
            get
            {
                return textRaw;
            }
        }
    }
}
