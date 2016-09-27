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
        // Script file with formated conntent - RichText
        private String textRich;
        // Actuel Parser step - status   
        private ParserStatuses currentStatus;
        // LogMessages
        public ArrayList LogMessages = new ArrayList();

        public Parser(String fileName)
        {
            this.currentStatus = ParserStatuses.Init;
            if (this.LoadFile(fileName))
            {
                this.DoParse();
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
            this.textRich = "";
            this.LogMessages.Clear();
            this.currentStatus = ParserStatuses.Init;
        }

        public bool DoParse()
        {
            this.currentStatus = ParserStatuses.Parsing;
            LogMessages.Add(new LogMessage(LogMessageTypes.Info, "Parser", "Start parsing"));
            // TODO ... parsing
            LogMessages.Add(new LogMessage(LogMessageTypes.Info, "Parser", "Finish parsing"));
            this.currentStatus = ParserStatuses.ParseSuccesse;
            return true;
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

        public string TextRich
        {
            get
            {
                return textRich;
            }
        }
    }
}
