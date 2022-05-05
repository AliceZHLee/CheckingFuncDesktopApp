using System.Windows;
using System.Windows.Controls;

namespace ICEChecker.ModuleCheckerOutput.Views {
    /// <summary>
    /// Interaction logic for OutputView.xaml
    /// </summary>
    public partial class OutputView : UserControl {
        public OutputView() {
            InitializeComponent();
        }
        private void TrdID_Filter_Apply(object sender, RoutedEventArgs e) {

        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e) {

        }

        private void ClosePopUp_Click(object sender, RoutedEventArgs e) {

        }
        private void LostFocus_datagrid1(object sender, RoutedEventArgs e) {
            ConsistentTrades.UnselectAll();
        }

        private void LostFocus_datagrid2(object sender, RoutedEventArgs e) {
            InconsistentTrades.UnselectAll();
        }

        private void Row_Selected(object sender, RoutedEventArgs e) {

        }

    }
}
