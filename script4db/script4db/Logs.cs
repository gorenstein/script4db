using System;
using System.Collections.Generic;
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
            Info,
            Warning,
            Error
        }

        private RichTextBox RichTextBox;

        public Logs(RichTextBox richTextBox) {
            this.RichTextBox = richTextBox;
            this.AppendMessage(Type.Info, "Initialize Logs");
        }

        public void AppendMessage(Type type, String message)
        {
            var logMsg = String.Format("{0:s} | {1,-7} | {2}{3}",
                DateTime.Now, type, message, Environment.NewLine);
            this.RichTextBox.AppendText(logMsg);
        }
    }
}
