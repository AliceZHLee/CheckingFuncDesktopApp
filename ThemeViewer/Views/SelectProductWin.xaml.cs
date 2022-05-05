using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using ThemeViewer.ViewModels;

namespace ThemeViewer.Views {
    /// <summary>
    /// Interaction logic for SelectProductWin.xaml
    /// </summary>
    public partial class SelectProductWin : Window {
        private SelectProductWinVM _vm;
        //internal static string SelectedProduct;
        internal static List<string> SelectedAccounts = new();
        internal static List<string> SelectedProducts = new();

        public SelectProductWin() {
            InitializeComponent();
            _vm = new SelectProductWinVM();
            DataContext = _vm;
            //productChoice.ItemsSource = ConfigHelper.LoadProductData(@"config\config.json");
        }

        private void RedirectToThmViewer_Click(object sender, RoutedEventArgs e) {
            UpdateButtonVisualState(sender);
            GoToThemeViewWin();
        }

        private async Task GoToThemeViewWin() {
            await Task.Delay(TimeSpan.FromSeconds(1));
            foreach (var item in Accounts.SelectedItems.Keys) {
                SelectedAccounts.Add(item.ToString());
            }
            foreach (var item in productChoice.SelectedItems.Keys) {
                SelectedProducts.Add(item.ToString());
            }
            //SelectedProduct = productChoice.SelectedItem.ToString();
            this.DialogResult = true;
        }


        private async Task UpdateButtonVisualState(object btn) {
            VisualStateManager.GoToState((FrameworkElement)btn, "Pressed", useTransitions: true);
            await Task.Delay(TimeSpan.FromMilliseconds(200));
            VisualStateManager.GoToState((FrameworkElement)btn, "Normal", useTransitions: true);
        }

        private void CloseWind_Click(object sender, RoutedEventArgs e) {
            UpdateButtonVisualState(sender);
            CloseProgram();
        }

        private void Window_Closing(object sender, System.EventArgs e) {
            CloseProgram();
        }

        private async Task CloseProgram() {
            await Task.Delay(TimeSpan.FromSeconds(1));
            Close();
        }
    }
}
