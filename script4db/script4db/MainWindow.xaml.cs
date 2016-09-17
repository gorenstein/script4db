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
            Run,
            Continue,
            Finish,
            Pause,
            Break,
            Error
        }

        Status CurrentStatus = Status.Init;

        public MainWindow()
        {
            InitializeComponent();
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
            switch (CurrentStatus)
            {
                case Status.Init:
                case Status.Break:
                case Status.Error:
                    buttonRun.IsEnabled = true;
                    buttonPauseContinue.IsEnabled = false;
                    buttonBreak.IsEnabled = false;
                    buttonExit.IsEnabled = true;
                    break;
                case Status.Run:
                    buttonRun.IsEnabled = false;
                    buttonPauseContinue.IsEnabled = true;
                    buttonBreak.IsEnabled = true;
                    buttonExit.IsEnabled = false;
                    break;
                case Status.Finish:
                    buttonRun.IsEnabled = false;
                    buttonPauseContinue.IsEnabled = false;
                    buttonBreak.IsEnabled = false;
                    buttonExit.IsEnabled = true;
                    break;
                case Status.Pause:
                case Status.Continue:
                    buttonPauseContinue.Content = CurrentStatus.ToString();
                    buttonPauseContinue.ToolTip = buttonPauseContinue.Content + " script";
                    buttonRun.IsEnabled = false;
                    buttonPauseContinue.IsEnabled = true;
                    buttonBreak.IsEnabled = true;
                    buttonExit.IsEnabled = false;
                    break;
            }
        }
    }
}
