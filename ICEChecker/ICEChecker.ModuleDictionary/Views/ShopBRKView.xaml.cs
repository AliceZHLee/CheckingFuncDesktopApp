using System;
using System.Windows;
using System.Windows.Controls;

namespace ICEChecker.ModuleDictionary.Views {
    /// <summary>
    /// Interaction logic for ShopBRKView.xaml
    /// </summary>
    public partial class ShopBRKView : UserControl {
        public ShopBRKView() {
            InitializeComponent();
        }

        private void LostFocus_datagrid(object sender, System.Windows.RoutedEventArgs e) {
            BrkTable.UnselectAll();
        }
        private void DisplayEditDeleteBtn(object sender, RoutedEventArgs e) {
            // EditBtn.Visibility = Visibility.Visible;
            //DeleteBtn.Visibility = Visibility.Visible;
        }
        private void EditProduct_Click(object sender, RoutedEventArgs e) {
            try {
                var win = new NewShopBrkView() { };
                win.ShowDialog();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e) {

        }

        private void AddProduct_Click(object sender, RoutedEventArgs e) {
            try {
                var win = new NewShopBrkView() { };
                win.ShowDialog();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
