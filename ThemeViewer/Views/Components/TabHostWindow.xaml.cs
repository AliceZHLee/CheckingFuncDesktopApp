using System.Windows;

namespace ThemeViewer.Views.Components {
    /// <summary>
    /// Interaction logic for TabHostWindow.xaml
    /// </summary>
    public partial class TabHostWindow : Window {
        public TabHostWindow() {
            InitializeComponent();
        }

        private void CloseWind_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
