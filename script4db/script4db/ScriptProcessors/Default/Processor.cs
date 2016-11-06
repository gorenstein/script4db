using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace script4db.ScriptProcessors.Default
{
    class Processor : IScriptProcessor
    {

        // Data for Pass
        ///////////////
        private string originalText;
        // content of script file
        private string textRaw;
        // Splited textRaw by line
        private string[] scriptLines;
        // Current passing block
        private Block block = null;

        private Blocks blocks = new Blocks();
        private ArrayList logMessages = new ArrayList();

        public Processor(string scriptText)
        {
            this.originalText = scriptText;
            // Support multyline - concatenated two line when Next Line started with '<<' and to previous line enden '<<'
            this.TextRaw = scriptText.Replace("<<\r\n<<", "");
            // Split with empty/space line for keep number of lines
            this.scriptLines = this.TextRaw.Split(new String[] { "\r\n" }, StringSplitOptions.None);

            this.Run();
        }

        public void FillRichTextBox(RichTextBox richTextBox)
        {
            string[] lines = this.originalText.Split(new String[] { "\r\n" }, StringSplitOptions.None);

            int maxLineNumberLen = lines.Count().ToString().Length;
            int lineNumber = 0;
            Color color;
            Font font;

            foreach (string scriptLine in this.scriptLines)
            {
                ++lineNumber;
                string line = scriptLine.Trim();

                if (String.IsNullOrWhiteSpace(line))
                {
                    color = Color.Black;
                    font = new Font(richTextBox.Font, FontStyle.Regular);
                }
                else if (line.First().ToString() == ";")
                {
                    color = Color.OliveDrab;
                    font = new Font(richTextBox.Font, FontStyle.Italic);
                }
                else if (line == "[constants]")
                {
                    color = Color.SteelBlue;
                    font = new Font(richTextBox.Font, FontStyle.Bold);
                }
                else if (line.First().ToString() == "[" && line.Trim().Last().ToString() == "]")
                {
                    color = Color.SlateBlue;
                    font = new Font(richTextBox.Font, FontStyle.Bold);
                }
                else
                {
                    color = Color.Black;
                    font = new Font(richTextBox.Font, FontStyle.Regular);
                }

                richTextBox.AppendText(String.Format("{0} ", lineNumber.ToString().PadLeft(maxLineNumberLen, '0')), Color.DarkGray);
                richTextBox.AppendText(line, color, font);
                richTextBox.AppendText(Environment.NewLine);
            }
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
                string msg = String.Format("Fill place holders checking error on Pass #2");
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

                if (String.IsNullOrWhiteSpace(line)) continue;
                // It's comment line
                if (line.First().ToString() == ";") continue;

                // It's begin of block line
                if (line.First().ToString() == "[" && line.Trim().Last().ToString() == "]")
                {
                    // If it's nexte block then try to save preview block
                    if (false == AddBlockIfAny(block)) return false;

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
                string key = line.Substring(0, IndexOfSplitChar).Trim();
                string value = line.Substring(IndexOfSplitChar + 1).Trim();

                if (false == this.block.AddParameter(key, value))
                {
                    foreach (LogMessage logMsg in this.blocks.LogMessages) this.LogMessages.Add(logMsg);
                    string msg = String.Format("Line No: {0} - Can't add parameter: '{1}'. Maybe Duplicated?", lineNumber, key);
                    LogMessages.Add(new LogMessage(LogMessageTypes.Error, this.GetType().Name, msg));
                    return false;
                }
            }
            // It's Last block, save if any
            return AddBlockIfAny(block);
        }

        // Pass for replace variable placeholders 
        private bool Pass2()
        {
            if (!this.blocks.FillPlaceHolders())
            {
                foreach (LogMessage logMsg in this.blocks.LogMessages) this.LogMessages.Add(logMsg);
                return false;
            }
            return true;
        }

        private bool AddBlockIfAny(Block block)
        {
            if (block != null)
            {
                if (false == this.Blocks.addBlock(block))
                {
                    foreach (LogMessage logMsg in this.blocks.LogMessages) this.LogMessages.Add(logMsg);
                    return false;
                }
            }
            return true;
        }

        public string TextRaw
        {
            get { return textRaw; }
            set { textRaw = value; }
        }

        public ArrayList LogMessages
        {
            get { return logMessages; }
        }

        public Blocks Blocks
        {
            get { return blocks; }
        }

        public ArrayList ConnectionsStrings()
        {
            return this.blocks.ConnectionsStrings();
        }
    }
}
