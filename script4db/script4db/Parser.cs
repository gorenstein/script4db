using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using script4db.ScriptProcessors;

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

        private ParserStatuses currentStatus;
        private Interpreter interpreter;
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

        public void FillRichTextBox(RichTextBox richTextBox)
        {
            this.interpreter.FillRichTextBox(richTextBox);
        }

        public bool LoadFile(string fileForParce)
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

        public void FillBlocksTree(TreeView treeView)
        {
            treeView.BeginUpdate();
            treeView.Nodes.Clear();
            TreeNode rootNode = new TreeNode("Script Blocks");
            rootNode.NodeFont = new Font("Arial", 11, FontStyle.Regular);
            Font font;
            Color color;

            foreach (var item in this.interpreter.ScriptProcessor.Blocks.BlocksGroup)
            {
                // item => Dictionary<BlockNames, ArrayList>
                int itemValueCount = item.Value.Count;
                string countEm = " set";
                if (itemValueCount > 1) countEm += "s";

                TreeNode childNode = new TreeNode(item.Key.ToString() + " (" + itemValueCount.ToString() + countEm + ")");
                if (item.Key.ToString() == "command")
                {
                    childNode.Expand();
                    font = new Font("Arial", 11, FontStyle.Bold);
                    color = Color.Black; //Color.SlateBlue;
                }
                else
                {
                    font = new Font("Arial", 11, FontStyle.Regular);
                    color = Color.Black; //Color.SteelBlue;
                }
                childNode.NodeFont = font;
                childNode.ForeColor = color;
                this.AddSubNodes(childNode, item.Value);

                rootNode.Nodes.Add(childNode);
            }

            treeView.Nodes.Add(rootNode);
            rootNode.Expand();
            treeView.EndUpdate();
        }

        private void AddSubNodes(TreeNode @node, ArrayList subList)
        {
            int i = 0;
            foreach (Block block in subList)
            {
                i++;
                block.order = i;
                block.node = new TreeNode(i.ToString());
                block.node.NodeFont = new Font("Arial", 11);
                this.AddNodeParameters(block);
                node.Nodes.Add(block.node);

                //TreeNode childNode = new TreeNode(i.ToString());
                //this.AddNodeParameters(childNode, block);
                //node.Nodes.Add(childNode);

                block.Status = BlockStatuses.ReadyToRun;
            }
        }

        private void AddNodeParameters(Block block)
        {
            foreach (var item in block.Parameters)
            {
                TreeNode childNode = new TreeNode(item.Key + " = " + item.Value);
                childNode.NodeFont = new Font("Arial", 10);
                block.node.Nodes.Add(childNode);
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

            this.interpreter = new Interpreter(Path.GetExtension(this.fileName), this.textRaw);
            this.textRaw = this.interpreter.ScriptProcessor.TextRaw;
            foreach (LogMessage logMsg in interpreter.LogMessages) this.LogMessages.Add(logMsg);

            if (interpreter.hasError())
            {
                LogMessages.Add(new LogMessage(LogMessageTypes.Warning, this.GetType().Name, "Break parsing - has error"));
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
            get { return fileName; }
        }

        public ParserStatuses CurrentStatus
        {
            get { return currentStatus; }
        }

        public string TextRaw
        {
            get { return textRaw; }
        }

        public ArrayList ConnectionsStrings()
        {
            return this.interpreter.ScriptProcessor.ConnectionsStrings();
        }

        public ArrayList Commands()
        {
            return this.interpreter.ScriptProcessor.Blocks.BlocksGroup[BlockNames.command];
        }
    }
}
