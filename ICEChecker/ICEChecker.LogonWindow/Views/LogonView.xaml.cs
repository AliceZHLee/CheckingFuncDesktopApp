using ICEChecker.Core;
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
using System.Windows.Shapes;

namespace ICEChecker.LogonWindow.Views {
    /// <summary>
    /// Interaction logic for LogonView.xaml
    /// </summary>
    public partial class LogonView : Window {
        internal static string SelectedAccount;

        public LogonView() {
            InitializeComponent();
        }

        private void RedirectToThmViewer_Click(object sender, RoutedEventArgs e) {           
            UpdateButtonVisualState(sender);
            GoToMainWin();
        }

      

        private async Task GoToMainWin() {
            await Task.Delay(TimeSpan.FromSeconds(1));
            SelectedAccount = Accounts.SelectedItem.ToString();
            DialogResult = true;
        }


        private async Task UpdateButtonVisualState(object btn) {
            VisualStateManager.GoToState((FrameworkElement)btn, "Pressed", useTransitions: true);
            await Task.Delay(TimeSpan.FromMilliseconds(200));
            VisualStateManager.GoToState((FrameworkElement)btn, "Normal", useTransitions: true);
        }

        private void Window_Closing(object sender, System.EventArgs e) {
            CloseProgram();
        }

        private async Task CloseProgram() {
            await Task.Delay(TimeSpan.FromSeconds(1));
            Close();
        }

        private void CloseWind_Click(object sender, RoutedEventArgs e) {
            UpdateButtonVisualState(sender);
            CloseProgram();
        }
    }
}
