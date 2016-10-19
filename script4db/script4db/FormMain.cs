using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using script4db.Connections;

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
            Cancel,
            Error
        }

        appStatuses currentStatus;
        string initialDirectory = Environment.SpecialFolder.Desktop.ToString();
        //string initialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
        Parser parser;
        BackgroundWorker worker = new BackgroundWorker();

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
            statusLabel2.Text = "Selecting a script file...";

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
                this.parser = new Parser(openFileDialog.FileName);

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
                statusLabel2.Text = "File was not selected.";
            }
        }

        private void RefreshControls(appStatuses newStatus)
        {
            this.Logs.AppendMessage(new LogMessage(LogMessageTypes.Info, "main", "Change app status to " + newStatus.ToString()));
            this.currentStatus = newStatus;
            statusLabel1.Text = this.currentStatus.ToString();

            buttonOpen.Enabled = false;
            buttonRun.Enabled = false;
            buttonPauseContinue.Enabled = false;
            buttonCancel.Enabled = false;
            buttonExit.Enabled = false;

            switch (this.currentStatus)
            {
                case appStatuses.Init:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    statusLabel2.Text = "Please open a script file.";
                    break;
                case appStatuses.Parse:
                    //this.
                    buttonExit.Enabled = true;
                    statusLabel2.Text = "Parsing in process...";
                    break;
                case appStatuses.Cancel:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    statusLabel2.Text = "Canseled";
                    break;
                case appStatuses.Error:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    statusLabel2.Text = "Look please a Logs for details";
                    break;
                case appStatuses.ReadyToRun:
                    buttonOpen.Enabled = true;
                    buttonRun.Enabled = true;
                    buttonExit.Enabled = true;
                    statusLabel2.Text = "Click 'Run' to start srcript.";
                    break;
                case appStatuses.Run:
                    buttonPauseContinue.Enabled = true;
                    buttonCancel.Enabled = true;
                    statusLabel2.Text = "Running...";
                    statusLabel3.Visible = true;
                    progressBar1.Visible = true;
                    break;
                case appStatuses.Finish:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    statusLabel2.Text = "Ended. You can open a next script file.";
                    statusLabel3.Visible = false;
                    progressBar1.Visible = false;
                    break;
                case appStatuses.Pause:
                    buttonPauseContinue.Text = appStatuses.Continue.ToString();
                    buttonPauseContinue.Enabled = true;
                    buttonCancel.Enabled = true;
                    statusLabel2.Text = "Pause...";
                    break;
                case appStatuses.Continue:
                    buttonPauseContinue.Text = appStatuses.Pause.ToString();
                    buttonPauseContinue.Enabled = true;
                    buttonCancel.Enabled = true;
                    statusLabel2.Text = "Continue running...";
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
            // Cancel the asynchronous operation.
            this.worker.CancelAsync();

            // Disable the Cancel button.
            buttonCancel.Enabled = false;
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            RefreshControls(appStatuses.Run);
            statusLabel2.Text = "Checking DB connection";

            worker.DoWork += Bw_DoWorkCheckConnection;

            worker.ProgressChanged += Bw_ProgressChanged;
            worker.RunWorkerCompleted += Bw_RunWorkerCompleted;
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync();
        }

        private void Bw_DoWorkCheckConnection(object sender, DoWorkEventArgs e)
        {
            ArrayList workerMsgs = new ArrayList();
            int connCount = parser.ConnectionsStrings().Count;
            int progressStep = 100 / connCount;
            int progress = progressStep / 2;

            foreach (string connString in parser.ConnectionsStrings())
            {
                // User send Cancel ?
                if (worker.CancellationPending == true)
                {
                    workerMsgs.Add(new LogMessage(LogMessageTypes.Warning, "Check Connection", "Aborted by user"));
                    e.Result = workerMsgs;
                    e.Cancel = true;
                    return;
                }

                // Do next step of Work
                worker.ReportProgress(progress);
                Connection connection = new Connection(connString);
                if (!connection.isCorrectRawConnString)
                {
                    foreach (LogMessage logMsg in connection.LogMessages) workerMsgs.Add(logMsg);
                    e.Result = workerMsgs;
                    e.Cancel = true;
                    return;
                }

                if (!connection.IsLive())
                {
                    foreach (LogMessage logMsg in connection.LogMessages) workerMsgs.Add(logMsg);
                    e.Result = workerMsgs;
                    e.Cancel = true;
                    return;
                }

                progress += progressStep;
            }

            string msg = String.Format("Success checked {0} {1}", connCount, "connection" + (connCount > 1 ? "s" : ""));
            workerMsgs.Add(new LogMessage(LogMessageTypes.Info, "Check Connection", msg));
            e.Result = workerMsgs;
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            statusLabel3.Text = String.Format("{0,3} %", e.ProgressPercentage);
            progressBar1.Value = e.ProgressPercentage;
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ArrayList logMsgs = (ArrayList)e.Result;
                foreach (LogMessage logMsg in logMsgs) this.Logs.AppendMessage(logMsg);

                if (e.Cancelled)
                {
                    RefreshControls(appStatuses.Cancel);
                }
                else
                {
                    RefreshControls(appStatuses.Finish);
                }
            }
            else
            {
                LogMessage logMsg = new LogMessage(LogMessageTypes.Error, "Check Connection", e.Error.ToString());
                this.Logs.AppendMessage(logMsg);
            }
        }
    }
}
