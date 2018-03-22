using System;
using System.Drawing;
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
            DateTime now = DateTime.Now;

            String msg = String.Format("{0:yyyy-MM-dd H:mm:ss}", now);
            msg += "  " + logMsg.TypeNameNormalized + " " + logMsg.Text + " (" + logMsg.Source + ")";
            Console.WriteLine(msg);

            this.RichTextBox.AppendText(String.Format("{0:yyyy-MM-dd H:mm:ss}", now));
            this.RichTextBox.AppendText(" | ");

            this.RichTextBox.AppendText(logMsg.TypeNameNormalized, logMsg.Color);
            this.RichTextBox.AppendText(" | ");
            this.RichTextBox.AppendText(logMsg.Text);
            this.RichTextBox.AppendText(" (" + logMsg.Source + ")", Color.DarkGray);
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
        public static void AppendText(this RichTextBox box, string text, Color color, Font font)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.SelectionFont = font;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}
