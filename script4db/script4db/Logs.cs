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
        public enum Type
        {
            Info = 0,
            Warning,
            Error
        }

        Color[] TypeColor = new Color[] { Color.DarkGreen, Color.DarkOrange, Color.Red };

        private RichTextBox RichTextBox;

        public Logs(RichTextBox richTextBox)
        {
            this.RichTextBox = richTextBox;
            this.AppendMessage(Type.Info, "Initialize Logs");
        }

        public void AppendMessage(Type type, String message)
        {
            this.RichTextBox.AppendText(String.Format("{0:s}", DateTime.Now));
            this.RichTextBox.AppendText(" | ");
            this.RichTextBox.AppendText(type.ToString().PadRight(7), TypeColor[(int)type]);
            this.RichTextBox.AppendText(" | ");
            this.RichTextBox.AppendText(message);
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
