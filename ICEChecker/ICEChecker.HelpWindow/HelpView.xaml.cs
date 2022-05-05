using System.Windows;

namespace ICEChecker.HelpWindow {
    /// <summary>
    /// Interaction logic for HelpView.xaml
    /// </summary>
    public partial class HelpView : Window {
        public HelpView() {
            InitializeComponent();
        }

        private void Close_click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
