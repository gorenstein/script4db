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
        enum Status
        {
            Init,
            Parse,
            Run,
            Continue,
            Finish,
            Pause,
            Break,
            Error
        }

        Status CurrentStatus;
        String InitialDirectory = Environment.SpecialFolder.Desktop.ToString();
        //String InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

        Logs Logs;

        public FormMain()
        {
            InitializeComponent();
            this.MinimumSize = new Size(this.Width, this.Height);
            this.Logs = new Logs(this.richTextBoxLogs);
            refreshControls(Status.Init);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {

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
            // Displays an OpenFileDialog so the user can select a Cursor.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Script Files (*.script4db)|*.script4db";
            openFileDialog.Title = "Select a Script File";
            openFileDialog.InitialDirectory = InitialDirectory;
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;

            // Show the Dialog.
            // If the user clicked OK in the dialog and
            // a script file was selected, open it.
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxScriptFile.Text = openFileDialog.FileName;
                InitialDirectory = Path.GetDirectoryName(openFileDialog.FileName);

                if (File.Exists(openFileDialog.FileName))
                {
                    this.Logs.AppendMessage(Logs.Type.Info, "Open script " + openFileDialog.FileName);
                    richTextBoxRaw.LoadFile(openFileDialog.FileName, RichTextBoxStreamType.PlainText);
                }

                refreshControls(Status.Parse);
            }
            else
            {
                textBoxScriptFile.Text = "";
                refreshControls(Status.Init);
            }
        }

        private void refreshControls(Status newStatus)
        {
            this.Logs.AppendMessage(Logs.Type.Info, "Change status to " + newStatus.ToString());
            CurrentStatus = newStatus;
            toolStripStatusLabel1.Text = CurrentStatus.ToString();

            buttonOpen.Enabled = false;
            buttonRun.Enabled = false;
            buttonPauseContinue.Enabled = false;
            buttonBreak.Enabled = false;
            buttonExit.Enabled = false;

            switch (CurrentStatus)
            {
                case Status.Init:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    break;
                case Status.Parse:
                    //this.
                    buttonExit.Enabled = true;
                    break;
                case Status.Break:
                case Status.Error:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    break;
                case Status.Run:
                    buttonPauseContinue.Enabled = true;
                    buttonBreak.Enabled = true;
                    break;
                case Status.Finish:
                    buttonOpen.Enabled = true;
                    buttonExit.Enabled = true;
                    break;
                case Status.Pause:
                    buttonPauseContinue.Text = Status.Continue.ToString();
                    buttonPauseContinue.Enabled = true;
                    buttonBreak.Enabled = true;
                    break;
                case Status.Continue:
                    buttonPauseContinue.Text = Status.Run.ToString();
                    buttonPauseContinue.Enabled = true;
                    buttonBreak.Enabled = true;
                    break;
                default:
                    throw new System.ArgumentException("Default switch case must not be never reachable by refreshControls.", "appStatusError");
            }
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            refreshControls(Status.Run);
        }

        private void buttonPauseContinue_Click(object sender, EventArgs e)
        {
            Status NewStatus;
            //switch button
            if (CurrentStatus == Status.Run || CurrentStatus == Status.Continue)
                NewStatus = Status.Pause;
            else if (CurrentStatus == Status.Pause)
                NewStatus = Status.Continue;
            else
                throw new System.ArgumentException("Error application status: " + CurrentStatus.ToString() + " - Must be Pause or Continue.", "appStatusError");

            refreshControls(NewStatus);
        }

        private void buttonBreak_Click(object sender, EventArgs e)
        {
            refreshControls(Status.Break);
        }
    }
}
