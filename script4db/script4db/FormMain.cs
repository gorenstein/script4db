using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
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

        internal const int SC_CLOSE = 0xF060;           //close button's code in Windows API
        internal const int MF_ENABLED = 0x00000000;     //enabled button status
        internal const int MF_GRAYED = 0x1;             //disabled button status (enabled = false)
        internal const int MF_DISABLED = 0x00000002;    //disabled button status

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr HWNDValue, bool isRevert);

        [DllImport("user32.dll")]
        private static extern int EnableMenuItem(IntPtr tMenu, int targetItem, int targetStatus);

        private void EnableCloseItem(bool value)
        {
            IntPtr tMenu = GetSystemMenu(this.Handle, false);
            EnableMenuItem(tMenu, SC_CLOSE, value ? MF_ENABLED : MF_DISABLED);
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
                }
                else
                {
                    RefreshControls(appStatuses.Error);
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
            statusLabel1.ForeColor = Color.Black;

            buttonOpen.Enabled = false;
            buttonRun.Enabled = false;
            buttonPauseContinue.Enabled = false;
            buttonCancel.Enabled = false;
            buttonExit.Enabled = false;
            EnableCloseItem(false);

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
                    statusLabel2.Text = "Canceled";
                    statusLabel3.Visible = false;
                    progressBar1.Visible = false;
                    break;
                case appStatuses.Error:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    this.tabControl1.SelectTab(this.tabPageLogs);
                    statusLabel1.ForeColor = Color.Red;
                    statusLabel2.Text = "Look please a Logs for details";
                    statusLabel3.Visible = false;
                    progressBar1.Visible = false;
                    break;
                case appStatuses.ReadyToRun:
                    buttonOpen.Enabled = true;
                    buttonRun.Enabled = true;
                    buttonExit.Enabled = true;
                    statusLabel2.Text = "Click 'Run' to start srcript.";
                    this.tabControl1.SelectTab(this.tabPageTree);
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

            EnableCloseItem(buttonExit.Enabled);
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
            var result = MessageBox.Show("Are you sure to cancel runnig script ?",
                                     "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes && this.worker.IsBusy)
            {
                this.worker.CancelAsync();
                statusLabel1.Text = "Canceling...";
                statusLabel1.ForeColor = Color.DarkOrange;
                buttonCancel.Enabled = false;
            }

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
                    e.Result = new WorkerResult(WorkerResultStatuses.Cancel, workerMsgs);
                    return;
                }

                // Do next step of Work
                worker.ReportProgress(progress);
                Connection connection = new Connection(connString);
                if (!connection.isCorrectRawConnString)
                {
                    foreach (LogMessage logMsg in connection.LogMessages) workerMsgs.Add(logMsg);
                    e.Result = new WorkerResult(WorkerResultStatuses.Error, workerMsgs);
                    return;
                }

                if (!connection.IsLive())
                {
                    foreach (LogMessage logMsg in connection.LogMessages) workerMsgs.Add(logMsg);
                    e.Result = new WorkerResult(WorkerResultStatuses.Error, workerMsgs);
                    return;
                }

                progress += progressStep;
            }

            string msg = String.Format("Success checked {0} {1}", connCount, "connection" + (connCount > 1 ? "s" : ""));
            workerMsgs.Add(new LogMessage(LogMessageTypes.Info, "Check Connection", msg));
            e.Result = new WorkerResult(WorkerResultStatuses.Success, workerMsgs);

            // TODO Run command here
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
                if (e.Cancelled)
                {
                    RefreshControls(appStatuses.Cancel);
                }
                else
                {
                    WorkerResult result = e.Result as WorkerResult;
                    foreach (LogMessage logMsg in result.LogMsgs) this.Logs.AppendMessage(logMsg);
                    switch (result.Status)
                    {
                        case WorkerResultStatuses.Cancel:
                            RefreshControls(appStatuses.Cancel);
                            break;
                        case WorkerResultStatuses.Success:
                            RefreshControls(appStatuses.Finish);
                            break;
                        case WorkerResultStatuses.Error:
                        default:
                            RefreshControls(appStatuses.Error);
                            break;
                    }
                }
            }
            else
            {
                LogMessage logMsg = new LogMessage(LogMessageTypes.Error, "Worker", e.Error.ToString());
                this.Logs.AppendMessage(logMsg);
            }
        }
    }
}
