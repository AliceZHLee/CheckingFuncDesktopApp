using ICEChecker.ModuleDictionary.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ICEChecker.ModuleDictionary.Views {
    /// <summary>
    /// Interaction logic for ProductAbbrView.xaml
    /// </summary>
    public partial class ProductAbbrView : UserControl {
        public ProductAbbrView() {
            InitializeComponent();
        }

        private void LostFocus_datagrid(object sender, RoutedEventArgs e) {
            ProductAbbrTable.UnselectAll();
        }

        private void DisplayEditDeleteBtn(object sender, RoutedEventArgs e) {
            //EditBtn.Visibility = Visibility.Visible;
            //DeleteBtn.Visibility = Visibility.Visible;
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e) {
            try {
                var win = new NewProductView() { };
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
                var win = new NewProductView() { };
                win.ShowDialog();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
