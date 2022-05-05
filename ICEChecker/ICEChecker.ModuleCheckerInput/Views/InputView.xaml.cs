using System.Windows.Controls;

namespace ICEChecker.ModuleCheckerInput.Views {
    /// <summary>
    /// Interaction logic for InputView.xaml
    /// </summary>
    public partial class InputView : UserControl {
        public InputView() {
            InitializeComponent();
        }
        private void BtnFilter_Click(object sender, System.Windows.RoutedEventArgs e) {

        }

        private void Check_Click(object sender, System.Windows.RoutedEventArgs e) {
            CheckBtn.CommandParameter = ((TabItem)TraderTab.SelectedItem).Header;
        }

        private void LostFocus_datagrid(object sender, System.Windows.RoutedEventArgs e) {
            InputDatagrid.UnselectAll();
        }
    }
}
