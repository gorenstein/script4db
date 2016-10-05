using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace script4db
{
    public partial class FormMain : Form
    {
        enum appStatuses
        {
            Init,
            Parse,
            ReadyToRun,
            Run,
            Continue,
            Finish,
            Pause,
            Break,
            Error
        }

        appStatuses currentStatus;
        String initialDirectory = Environment.SpecialFolder.Desktop.ToString();
        //String initialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

        Logs Logs;

        public FormMain()
        {
            InitializeComponent();

            this.MinimumSize = new Size(this.Width, this.Height);
            this.Logs = new Logs(this.richTextBoxLogs);

            RefreshControls(appStatuses.Init);
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void buttonAbout_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            this.richTextBoxRaw.Clear();

            // Displays an OpenFileDialog so the user can select a Cursor.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Script Files (*.script4db)|*.script4db";
            openFileDialog.Title = "Select a Script File";
            openFileDialog.InitialDirectory = initialDirectory;
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;

            // Show the Dialog.
            // If the user clicked OK in the dialog and
            // a script file was selected, open it.
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxScriptFile.Text = openFileDialog.FileName;
                initialDirectory = Path.GetDirectoryName(openFileDialog.FileName);
                RefreshControls(appStatuses.Parse);

                // Parcing Script
                Parser parser = new Parser(openFileDialog.FileName);

                // Show Result
                parser.RefreshBlocksTree(this.treeViewScriptBlocks);
                this.richTextBoxRaw.AppendText(parser.TextRaw);
                foreach (LogMessage logMsg in parser.LogMessages) this.Logs.AppendMessage(logMsg);

                if (parser.CurrentStatus == ParserStatuses.ParseSuccesse)
                {
                    RefreshControls(appStatuses.ReadyToRun);
                }
                else
                {
                    RefreshControls(appStatuses.Error);
                    this.tabControl1.SelectTab(this.tabPageLogs);
                }
            }
            else
            {
                textBoxScriptFile.Text = "";
                RefreshControls(appStatuses.Init);
            }
        }

        private void RefreshControls(appStatuses newStatus)
        {
            this.Logs.AppendMessage(new LogMessage(LogMessageTypes.Info, "main", "Change app status to " + newStatus.ToString()));
            this.currentStatus = newStatus;
            toolStripStatusLabel1.Text = this.currentStatus.ToString();

            buttonOpen.Enabled = false;
            buttonRun.Enabled = false;
            buttonPauseContinue.Enabled = false;
            buttonBreak.Enabled = false;
            buttonExit.Enabled = false;

            switch (this.currentStatus)
            {
                case appStatuses.Init:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    break;
                case appStatuses.Parse:
                    //this.
                    buttonExit.Enabled = true;
                    break;
                case appStatuses.Break:
                case appStatuses.Error:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    break;
                case appStatuses.ReadyToRun:
                    buttonOpen.Enabled = true;
                    buttonRun.Enabled = true;
                    buttonExit.Enabled = true;
                    break;
                case appStatuses.Run:
                    buttonPauseContinue.Enabled = true;
                    buttonBreak.Enabled = true;
                    break;
                case appStatuses.Finish:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    break;
                case appStatuses.Pause:
                    buttonPauseContinue.Text = appStatuses.Continue.ToString();
                    buttonPauseContinue.Enabled = true;
                    buttonBreak.Enabled = true;
                    break;
                case appStatuses.Continue:
                    buttonPauseContinue.Text = appStatuses.Pause.ToString();
                    buttonPauseContinue.Enabled = true;
                    buttonBreak.Enabled = true;
                    break;
                default:
                    throw new System.ArgumentException("Default switch case must be never reachable by refreshControls.", "appStatusError");
            }
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            RefreshControls(appStatuses.Run);
        }

        private void buttonPauseContinue_Click(object sender, EventArgs e)
        {
            appStatuses NewStatus;
            //switch button
            if (currentStatus == appStatuses.Run || currentStatus == appStatuses.Continue)
                NewStatus = appStatuses.Pause;
            else if (currentStatus == appStatuses.Pause)
                NewStatus = appStatuses.Continue;
            else
                throw new System.ArgumentException("Error application status: " + currentStatus.ToString() + " - Must be Pause or Continue.", "appStatusError");

            RefreshControls(NewStatus);
        }

        private void buttonBreak_Click(object sender, EventArgs e)
        {
            RefreshControls(appStatuses.Break);
        }
    }
}
