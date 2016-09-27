using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace script4db
{
    class Logs
    {
        private RichTextBox RichTextBox;

        public Logs(RichTextBox richTextBox)
        {
            this.RichTextBox = richTextBox;
            //this.AppendMessage(new LogMessage(LogMessageTypes.Info, "Logs", "Initialize Logs"));
        }

        public void AppendMessage(LogMessage logMsg)
        {
            this.RichTextBox.AppendText(String.Format("{0:s}", DateTime.Now));
            this.RichTextBox.AppendText(" | ");
            this.RichTextBox.AppendText(logMsg.TypeNameNormalized, logMsg.Color);
            this.RichTextBox.AppendText(" | ");
            this.RichTextBox.AppendText(logMsg.Text);
            this.RichTextBox.AppendText(Environment.NewLine);
        }
    }
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}
