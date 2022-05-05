using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ThemeViewer.ViewModels;
using ThemeViewer.Views;

namespace ThemeViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ThmViewerWin : Window
    {
        private readonly ThmViewWinVM _vm;
        internal static IEventAggregator EA { get; } = new EventAggregator();
        private readonly SettingWinVM _settingWinVM;

        public ThmViewerWin()
        {
            InitializeComponent();
            _vm = new ThmViewWinVM();
            _settingWinVM = new SettingWinVM(EA);
            DataContext = _vm;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.isProductChosen)
            {
                try
                {
                    var rlt = new SelectProductWin()
                    {
                        Owner = this
                    }.ShowDialog();

                    if (rlt != null && rlt.Value)
                    {
                        List<string> selectedProduct = SelectProductWin.SelectedProducts == null ? null : SelectProductWin.SelectedProducts;
                        string displayedProducts = "";
                        _vm.Start();
                        foreach(var item in selectedProduct) {
                            displayedProducts += " "+item+";";
                        }
                        displayedProducts=displayedProducts.TrimStart();
                        productName.Text = displayedProducts;
                        App.isProductChosen = true;
                        return;
                    }
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Close();
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _vm.Dispose();
            Environment.Exit(0);
        }

        private void ListViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            // Set tooltip visibility
            if (Tg_Btn.IsChecked == true)
            {
                tt_home.Visibility = Visibility.Collapsed;
                tt_settings.Visibility = Visibility.Collapsed;
                tt_signout.Visibility = Visibility.Collapsed;
            }
            else
            {
                tt_home.Visibility = Visibility.Visible;
                tt_settings.Visibility = Visibility.Visible;
                tt_signout.Visibility = Visibility.Visible;
            }
        }

        private void Tg_Btn_Unchecked(object sender, RoutedEventArgs e)
        {
            //img_bg.Opacity = 1;
        }

        private void Tg_Btn_Checked(object sender, RoutedEventArgs e)
        {
            //img_bg.Opacity = 0.3;
        }

        private void BG_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Tg_Btn.IsChecked = false;
        }

        private void OpenSettingWin_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var win = new SettingWindow() { Owner = this };
                win.DataContext = _settingWinVM;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
        }

        private void CloseWind_Click(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
