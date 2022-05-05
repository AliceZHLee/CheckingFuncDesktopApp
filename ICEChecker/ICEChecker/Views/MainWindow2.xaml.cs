using ICEChecker.DictWindow.Views;
using ICEChecker.HelpWindow;
using ICEChecker.LogonWindow.ViewModels;
using ICEChecker.LogonWindow.Views;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ICEChecker.Views {
    /// <summary>
    /// Interaction logic for MainWindow2.xaml
    /// </summary>
    public partial class MainWindow2 : Window {
        public MainWindow2() {
            InitializeComponent();
        }

        private void Tg_Btn_Unchecked(object sender, RoutedEventArgs e) {
            //img_bg.Opacity = 1;
        }

        private void Tg_Btn_Checked(object sender, RoutedEventArgs e) {
            //img_bg.Opacity = 0.3;
        }

        private void OpenSettingWin_Click(object sender, MouseButtonEventArgs e) {
            try {
                var win = new AbbrDictWindow() { Owner = this };
                //var win = new AbbrDictWindow();

                win.ShowDialog();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
                Close();
            }
        }
        private void OpenHelpWin_Click(object sender, MouseButtonEventArgs e) {
            try {
                var win = new HelpView() { Owner = this };
                win.ShowDialog();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
                Close();
            }
        }

        private void Window_Close(object sender, CancelEventArgs e) {
            Environment.Exit(0);
        }

        private void ListViewItem_MouseEnter(object sender, MouseEventArgs e) {
            // Set tooltip visibility
            if (Tg_Btn.IsChecked == true) {
                tt_home.Visibility = Visibility.Collapsed;
                tt_settings.Visibility = Visibility.Collapsed;
                tt_helps.Visibility = Visibility.Collapsed;
                //tt_signout.Visibility = Visibility.Collapsed;
            }
            else {
                tt_home.Visibility = Visibility.Visible;
                tt_settings.Visibility = Visibility.Visible;
                tt_helps.Visibility = Visibility.Visible;
                //tt_signout.Visibility = Visibility.Visible;
            }
        }
        private void BG_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Tg_Btn.IsChecked = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            try {
                var win = new LogonView() {
                    Owner = this
                };
                if (LogonViewModel.FirstTimeLogin) {
                    var rlt = win.ShowDialog();
                    if (rlt != null && rlt.Value) {
                        return;
                    }
                    Close();
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
                Close();
            }
        }
    }
}
