using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using script4db.Connections;
using script4db.ScriptProcessors;

namespace script4db
{
    public partial class FormMain : Form
    {
        String[] arguments = Environment.GetCommandLineArgs();
        String scriptPathFromArg = null;
        AutoCloseModes AutoClose = AutoCloseModes.disable;

        enum AutoCloseModes
        {
            onSuccess,
            onError,
            always,
            disable
        }

        enum AppStatuses
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

        AppStatuses currentStatus;
        string initialDirectory = Environment.SpecialFolder.Desktop.ToString();
        //string initialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
        Parser parser;
        BackgroundWorker worker;

        Logs Logs;

        public FormMain()
        {
            InitializeComponent();

            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;

            this.MinimumSize = new Size(this.Width, this.Height);
            this.Logs = new Logs(this.richTextBoxLogs);

            this.worker = new BackgroundWorker();
            this.worker.ProgressChanged += Bw_ProgressChanged;
            this.worker.WorkerSupportsCancellation = true;
            this.worker.WorkerReportsProgress = true;

            ParseCommandLineArguments();

            RefreshControls(AppStatuses.Init);
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
                this.ParseScript(openFileDialog.FileName);
            }
            else
            {
                textBoxScriptFile.Text = "";
                RefreshControls(AppStatuses.Init);
                statusLabel2.Text = "File was not selected.";
            }
        }

        private bool ParseScript(String FileName)
        {
            textBoxScriptFile.Text = FileName;
            initialDirectory = Path.GetDirectoryName(FileName);
            RefreshControls(AppStatuses.Parse);

            // Parcing Script
            this.parser = new Parser(FileName);

            // Show Result
            parser.FillBlocksTree(this.treeViewScriptBlocks);
            parser.FillRichTextBox(this.richTextBoxRaw);

            foreach (LogMessage logMsg in parser.LogMessages) this.Logs.AppendMessage(logMsg);

            if (parser.CurrentStatus == ParserStatuses.ParseSuccesse)
            {
                RefreshControls(AppStatuses.ReadyToRun);
            }
            else
            {
                RefreshControls(AppStatuses.Error);
            }

            return parser.CurrentStatus == ParserStatuses.ParseSuccesse;
        }

        private void RefreshAutoCloseLabel()
        {
            if (this.AutoClose == AutoCloseModes.disable)
            {
                this.labelAutoCloseMode.Hide();
            }
            else
            {
                this.labelAutoCloseMode.Text = "auto close " + this.AutoClose.ToString();
            }
        }

        private void RefreshControls(AppStatuses newStatus)
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
                case AppStatuses.Init:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    statusLabel2.Text = "Please open a script file.";
                    break;
                case AppStatuses.Parse:
                    buttonExit.Enabled = true;
                    statusLabel2.Text = "Parsing in process...";
                    break;
                case AppStatuses.Cancel:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    statusLabel2.Text = "Canceled";
                    statusLabel3.Visible = false;
                    progressBar1.Visible = false;
                    break;
                case AppStatuses.Error:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    this.tabControl1.SelectTab(this.tabPageLogs);
                    statusLabel1.ForeColor = Color.Red;
                    statusLabel2.Text = "Look please a Logs for details";
                    statusLabel3.Visible = false;
                    progressBar1.Visible = false;
                    RefreshAutoCloseLabel();
                    break;
                case AppStatuses.ReadyToRun:
                    buttonOpen.Enabled = true;
                    buttonRun.Enabled = true;
                    buttonExit.Enabled = true;
                    statusLabel2.Text = "Click 'Run' to start srcript.";
                    break;
                case AppStatuses.Run:
                    buttonPauseContinue.Enabled = true;
                    buttonCancel.Enabled = true;
                    statusLabel2.Text = "Running...";
                    statusLabel3.Visible = true;
                    progressBar1.Visible = true;
                    this.tabControl1.SelectTab(this.tabPageTree);
                    break;
                case AppStatuses.Finish:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    statusLabel2.Text = "Ended. You can open a next script file.";
                    statusLabel3.Visible = false;
                    progressBar1.Visible = false;
                    break;
                case AppStatuses.Pause:
                    buttonPauseContinue.Text = AppStatuses.Continue.ToString();
                    buttonPauseContinue.Enabled = true;
                    buttonCancel.Enabled = true;
                    statusLabel2.Text = "Pause...";
                    break;
                case AppStatuses.Continue:
                    buttonPauseContinue.Text = AppStatuses.Pause.ToString();
                    buttonPauseContinue.Enabled = true;
                    buttonCancel.Enabled = true;
                    statusLabel2.Text = "Continue running...";
                    break;
                default:
                    throw new System.ArgumentException("Default switch case must be never reachable by refreshControls.", "appStatusError");
            }

            EnableCloseItem(buttonExit.Enabled);
        }

        private void ButtonPauseContinue_Click(object sender, EventArgs e)
        {
            AppStatuses NewStatus;
            //switch button
            if (currentStatus == AppStatuses.Run || currentStatus == AppStatuses.Continue)
                NewStatus = AppStatuses.Pause;
            else if (currentStatus == AppStatuses.Pause)
                NewStatus = AppStatuses.Continue;
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
            RefreshControls(AppStatuses.Run);
            statusLabel2.Text = String.Format("Checking DB connection ({0})", parser.ConnectionsStrings().Count);

            worker.DoWork += Bw_DoWorkCheckConnection;

            worker.RunWorkerCompleted += Bw_RunWorkerCheckCompleted;
            worker.RunWorkerAsync();
        }

        private void Bw_DoWorkCheckConnection(object sender, DoWorkEventArgs e)
        {
            ArrayList workerMsgs = new ArrayList();
            int connCount = parser.ConnectionsStrings().Count;
            int progressStep = 100 / connCount;
            int progress = progressStep / 2;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (string connString in parser.ConnectionsStrings())
            {
                // User sended Cancel ?
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

            sw.Stop();

            string msg = String.Format("Elapsed {0:0.000}s : Success checked {1} {2}", sw.Elapsed.TotalSeconds, connCount, "connection" + (connCount > 1 ? "s" : ""));
            workerMsgs.Add(new LogMessage(LogMessageTypes.Info, "Check Connection", msg));


            e.Result = new WorkerResult(WorkerResultStatuses.Success, workerMsgs);
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.ProgressBar(e.ProgressPercentage);
        }

        private void ProgressBar(int value)
        {
            progressBar1.Value = value;
            statusLabel3.Text = String.Format("{0,3} %", value);
        }

        private void Bw_RunWorkerCheckCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Cancelled)
                {
                    RefreshControls(AppStatuses.Cancel);
                }
                else
                {
                    WorkerResult result = e.Result as WorkerResult;
                    foreach (LogMessage logMsg in result.LogMsgs) this.Logs.AppendMessage(logMsg);
                    switch (result.Status)
                    {
                        case WorkerResultStatuses.Cancel:
                            RefreshControls(AppStatuses.Cancel);
                            break;
                        case WorkerResultStatuses.Success:
                            RunScriptCommand();
                            break;
                        case WorkerResultStatuses.Error:
                        default:
                            RefreshControls(AppStatuses.Error);
                            break;
                    }
                }
            }
            else
            {
                LogMessage logMsg = new LogMessage(LogMessageTypes.Error, "Worker", e.Error.ToString());
                this.Logs.AppendMessage(logMsg);
            }

            worker.DoWork -= Bw_DoWorkCheckConnection;
            worker.RunWorkerCompleted -= Bw_RunWorkerCheckCompleted;
        }

        private void RunScriptCommand()
        {
            statusLabel2.Text = String.Format("Run Script commands ({0})", parser.Commands().Count);
            this.ProgressBar(0);

            worker.DoWork += Bw_DoWorkScriptCommand;
            worker.RunWorkerCompleted += Bw_RunWorkerScriptCompleted;
            worker.RunWorkerAsync();
        }

        private void Bw_DoWorkScriptCommand(object sender, DoWorkEventArgs e)
        {
            ArrayList workerMsgs = new ArrayList();
            int count = parser.Commands().Count;
            int progressStep = 100 / count;
            int progress = progressStep / 2;

            foreach (Block block in parser.Commands())
            {
                // User sended Cancel ?
                if (worker.CancellationPending == true)
                {
                    workerMsgs.Add(new LogMessage(LogMessageTypes.Warning, "Run Script command", "Aborted by user"));
                    e.Result = new WorkerResult(WorkerResultStatuses.Cancel, workerMsgs);
                    return;
                }

                // Do next step of Work
                worker.ReportProgress(progress);
                bool success = block.Run(worker);
                //foreach (LogMessage logMsg in block.LogMessages) workerMsgs.Add(logMsg);
                foreach (LogMessage logMsg in block.LogMessages) this.Logs.AppendMessage(logMsg);

                if (!success)
                {
                    if (worker.CancellationPending == true)
                    {
                        workerMsgs.Add(new LogMessage(LogMessageTypes.Warning, "Run Script command", "Aborted by user"));
                        e.Result = new WorkerResult(WorkerResultStatuses.Cancel, workerMsgs);
                    }
                    else
                    {
                        workerMsgs.Add(new LogMessage(LogMessageTypes.Warning, "Worker", "Error by Run command"));
                        e.Result = new WorkerResult(WorkerResultStatuses.Error, workerMsgs);
                    }
                    return;
                }
                progress += progressStep;
            }

            string msg = String.Format("Success run {0} script command{1}", count, (count > 1 ? "s" : ""));
            workerMsgs.Add(new LogMessage(LogMessageTypes.Info, "Worker", msg));
            e.Result = new WorkerResult(WorkerResultStatuses.Success, workerMsgs);
            return;
        }

        private void Bw_RunWorkerScriptCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Cancelled)
                {
                    RefreshControls(AppStatuses.Cancel);
                }
                else
                {
                    WorkerResult result = e.Result as WorkerResult;
                    foreach (LogMessage logMsg in result.LogMsgs) this.Logs.AppendMessage(logMsg);
                    switch (result.Status)
                    {
                        case WorkerResultStatuses.Cancel:
                            RefreshControls(AppStatuses.Cancel);
                            break;
                        case WorkerResultStatuses.Success:
                            worker.DoWork -= Bw_DoWorkScriptCommand;
                            RefreshControls(AppStatuses.Finish);
                            if (this.AutoClose == AutoCloseModes.onSuccess || this.AutoClose == AutoCloseModes.always)
                            {
                                Application.Exit();
                            }
                            break;
                        case WorkerResultStatuses.Error:
                        default:
                            RefreshControls(AppStatuses.Error);
                            if (this.AutoClose == AutoCloseModes.onError || this.AutoClose == AutoCloseModes.always)
                            {
                                Application.Exit();
                            }
                            break;
                    }
                }
            }
            else
            {
                LogMessage logMsg = new LogMessage(LogMessageTypes.Error, "Worker", e.Error.ToString());
                this.Logs.AppendMessage(logMsg);
            }
            worker.DoWork -= Bw_DoWorkScriptCommand;
            worker.RunWorkerCompleted -= Bw_RunWorkerScriptCompleted;
        }

        private void ParseCommandLineArguments()
        {
            int argCount = 0;
            // Check command line parameters
            foreach (String arg in arguments)
            {
                argCount++;

                if (arg.Substring(0, Math.Min(11, arg.Length)).ToLower() == "/autoclose=")
                {
                    this.Logs.AppendMessage(new LogMessage(LogMessageTypes.Info, "main", "Auto close as cmd line arg: " + arg.Substring(11)));

                    String mode = arg.Substring(11).ToLower();

                    if (mode == AutoCloseModes.always.ToString().ToLower())
                    {
                        this.AutoClose = AutoCloseModes.always;
                    }
                    else if (mode == AutoCloseModes.disable.ToString().ToLower())
                    {
                        this.AutoClose = AutoCloseModes.disable;
                    }
                    else if (mode == AutoCloseModes.onError.ToString().ToLower())
                    {
                        this.AutoClose = AutoCloseModes.onError;
                    }
                    else if (mode == AutoCloseModes.onSuccess.ToString().ToLower())
                    {
                        this.AutoClose = AutoCloseModes.onSuccess;
                    }
                    else
                    {
                        this.AutoClose = AutoCloseModes.disable;
                        String allowedModes = string.Join(",", Enum.GetNames(typeof(AutoCloseModes)));
                        this.Logs.AppendMessage(new LogMessage(LogMessageTypes.Warning, "main", "Not supported auto close mode: " + arg + " - Allowed: " + allowedModes + ". Auto close disabled."));
                        MessageBox.Show("Not supported auto close mode: " + arg + "\n\nallowed: " + allowedModes + "\n\n P.S. Auto close DISABLED.", "Requested unknown parameter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (arg.Substring(0, Math.Min(8, arg.Length)).ToLower() == "/script=")
                {
                    this.scriptPathFromArg = arg.Substring(8);
                    this.Logs.AppendMessage(new LogMessage(LogMessageTypes.Info, "main", "Script as cmd line arg: " + this.scriptPathFromArg));
                }
                else if (argCount > 1) // ignore runnig program path argument
                {
                    this.AutoClose = AutoCloseModes.disable;
                    this.Logs.AppendMessage(new LogMessage(LogMessageTypes.Warning, "main", "Not supported cmd line argument: " + arg + " - Auto close disabled."));
                    MessageBox.Show("Not supported cmd line argument: " + arg, "Requested unknown parameter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            RefreshAutoCloseLabel();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            if (null != this.scriptPathFromArg) // script file as command line argument defined
            {
                if (!String.IsNullOrWhiteSpace(this.scriptPathFromArg) && File.Exists(this.scriptPathFromArg))
                {
                    bool parseResult = this.ParseScript(this.scriptPathFromArg);
                    if (parseResult)
                    {
                        this.buttonRun.PerformClick();
                    }
                    else
                    {
                        if (this.AutoClose == AutoCloseModes.disable || this.AutoClose == AutoCloseModes.onSuccess)
                        {
                            MessageBox.Show("Check log for details.", "Script parse error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                else
                {
                    String msg = "Script file not found.";
                    if (this.AutoClose == AutoCloseModes.onSuccess)
                    {
                        this.AutoClose = AutoCloseModes.disable;
                        msg += " Auto close disabled.";
                    }

                    this.Logs.AppendMessage(new LogMessage(LogMessageTypes.Warning, "main", msg));
                    if (this.AutoClose == AutoCloseModes.disable || this.AutoClose == AutoCloseModes.onSuccess)
                    {
                        MessageBox.Show("Script file not found." + "\n path: " + this.scriptPathFromArg, "Script not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }

            RefreshAutoCloseLabel();
        }
    }
}

public static class ControlExtensions
{
    /// <summary>
    /// Executes the Action asynchronously on the UI thread, does not block execution on the calling thread.
    /// Using: this.UIThread(() => this.myLabel.Text = "Text Goes Here");
    /// </summary>
    /// <param name="control"></param>
    /// <param name="code"></param>
    public static void UIThread(this Control @this, Action code)
    {
        if (@this.InvokeRequired)
        {
            @this.BeginInvoke(code);
        }
        else
        {
            code.Invoke();
        }
    }
}