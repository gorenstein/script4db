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
        string initialDirectory = Environment.SpecialFolder.Desktop.ToString();
        //string initialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
        string baseTextStatusLabel2;

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
            toolStripStatusLabel2.Text = "Selecting a script file...";

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
                    this.tabControl1.SelectTab(this.tabPageTree);
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
                toolStripStatusLabel2.Text = "File was not selected.";
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
                    toolStripStatusLabel2.Text = "Please open a script file.";
                    break;
                case appStatuses.Parse:
                    //this.
                    buttonExit.Enabled = true;
                    toolStripStatusLabel2.Text = "Parsing in process...";
                    break;
                case appStatuses.Break:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    toolStripStatusLabel2.Text = "Canseled";
                    break;
                case appStatuses.Error:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    toolStripStatusLabel2.Text = "Look please a Logs for details";
                    break;
                case appStatuses.ReadyToRun:
                    buttonOpen.Enabled = true;
                    buttonRun.Enabled = true;
                    buttonExit.Enabled = true;
                    toolStripStatusLabel2.Text = "Click 'Run' to start srcript.";
                    break;
                case appStatuses.Run:
                    buttonPauseContinue.Enabled = true;
                    buttonBreak.Enabled = true;
                    toolStripStatusLabel2.Text = "Running...";
                    toolStripProgressBar1.Visible = true;
                    break;
                case appStatuses.Finish:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    toolStripStatusLabel2.Text = "Ended. You can open a next script file.";
                    toolStripProgressBar1.Visible = false;
                    break;
                case appStatuses.Pause:
                    buttonPauseContinue.Text = appStatuses.Continue.ToString();
                    buttonPauseContinue.Enabled = true;
                    buttonBreak.Enabled = true;
                    toolStripStatusLabel2.Text = "Pause...";
                    break;
                case appStatuses.Continue:
                    buttonPauseContinue.Text = appStatuses.Pause.ToString();
                    buttonPauseContinue.Enabled = true;
                    buttonBreak.Enabled = true;
                    toolStripStatusLabel2.Text = "Continue running...";
                    break;
                default:
                    throw new System.ArgumentException("Default switch case must be never reachable by refreshControls.", "appStatusError");
            }
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

        private void buttonRun_Click(object sender, EventArgs e)
        {
            RefreshControls(appStatuses.Run);
            baseTextStatusLabel2 = "Checking DB connection";

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += ((object doWorkSender, DoWorkEventArgs doWorkArgs) =>
            {
                for (int i = 0; i < 100; i++)
                {
                    //Check Connection
                    bw.ReportProgress(i);
                    System.Threading.Thread.Sleep(50);
                }
            });
            bw.ProgressChanged += Bw_ProgressChanged;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            bw.WorkerReportsProgress = true;
            bw.RunWorkerAsync();
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripStatusLabel2.Text = String.Format("{0}: {1,3} %", baseTextStatusLabel2, e.ProgressPercentage);
            toolStripProgressBar1.Value = e.ProgressPercentage;
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RefreshControls(appStatuses.Finish);
        }
    }
}
