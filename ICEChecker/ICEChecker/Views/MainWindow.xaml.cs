using ICEChecker.DictWindow.Views;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;

namespace ICEChecker.Views {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow {
        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Close(object sender, CancelEventArgs e) {
            Environment.Exit(0);
        }

        private void OpenAbbrDictionary_Click(object sender, MouseButtonEventArgs e) {
            try {
                var win = new AbbrDictWindow() { Owner = this };
                win.ShowDialog();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
                Close();
            }
        }
    }
}
