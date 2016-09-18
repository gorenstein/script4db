using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace script4db
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum Status {
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
        //String InitialDirectory = Environment.SpecialFolder.Desktop.ToString();
        String InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

        public MainWindow()
        {
            InitializeComponent();
            this.MinHeight = this.Height;
            this.MinWidth = this.Width;
            refreshView(Status.Init);
        }

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.InitialDirectory = InitialDirectory;

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".script4db";
            dlg.Filter = "Script Files (*.script4db)|*.script4db";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open script 
                string filename = dlg.FileName;
                textBoxScriptFile.Text = filename;

                InitialDirectory = System.IO.Path.GetDirectoryName(filename);
                refreshView(Status.Parse);
            }
            else
            {
                refreshView(Status.Init);
            }

        }


        private void buttonRun_Click(object sender, RoutedEventArgs e)
        {
            refreshView(Status.Run);
        }

        private void buttonPauseContinue_Click(object sender, RoutedEventArgs e)
        {
            Status NewStatus;
            if (CurrentStatus == Status.Run || CurrentStatus == Status.Pause)
                NewStatus = Status.Continue;
            else if (CurrentStatus == Status.Continue)
                NewStatus = Status.Pause;
            else
                throw new System.ArgumentException("Error application status: " + CurrentStatus.ToString() + " - Must be Pause or Continue.",  "appStatusError" ); 

            refreshView(NewStatus);
        }

        private void buttonBreak_Click(object sender, RoutedEventArgs e)
        {
            refreshView(Status.Break);
        }

        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void refreshView(Status newStatus)
        {

            CurrentStatus = newStatus;

            buttonOpen.IsEnabled = false;
            buttonRun.IsEnabled = false;
            buttonPauseContinue.IsEnabled = false;
            buttonBreak.IsEnabled = false;
            buttonExit.IsEnabled = false;

            switch (CurrentStatus)
            {
                case Status.Init:
                    buttonOpen.IsEnabled = true;
                    buttonExit.IsEnabled = true;
                    break;
                case Status.Parse:
                    //this.
                    buttonExit.IsEnabled = true;
                    break;
                case Status.Break:
                case Status.Error:
                    buttonOpen.IsEnabled = true;
                    buttonExit.IsEnabled = true;
                    break;
                case Status.Run:
                    buttonPauseContinue.IsEnabled = true;
                    buttonBreak.IsEnabled = true;
                    break;
                case Status.Finish:
                    buttonOpen.IsEnabled = true;
                    buttonExit.IsEnabled = true;
                    break;
                case Status.Pause:
                case Status.Continue:
                    buttonPauseContinue.Content = CurrentStatus.ToString();
                    buttonPauseContinue.ToolTip = buttonPauseContinue.Content + " script";
                    buttonPauseContinue.IsEnabled = true;
                    buttonBreak.IsEnabled = true;
                    break;
                default:
                    throw new System.ArgumentException("Default switch case must not be never reachable by refreshView.", "appStatusError");
            }
        }
    }
}
