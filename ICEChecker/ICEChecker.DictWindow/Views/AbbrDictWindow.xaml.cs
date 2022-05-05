using ICEChecker.DictWindow.Models;
using ICEChecker.ModuleDictionary.Views;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ICEChecker.DictWindow.Views {
    /// <summary>
    /// Interaction logic for AbbrDictWindow.xaml
    /// </summary>
    public partial class AbbrDictWindow : Window {
        private Dictionary<string, UserControl> SettingsControls;
        private bool isCrossBtnClicked = true;
        ExcelDataService _objExcelSer;
        AbbrMapping _stud = new AbbrMapping();
        public AbbrDictWindow() {
            InitializeComponent();
            SettingsControls = new Dictionary<string, UserControl>();
            SettingsControls.Add("Security-Abbreviation", new ProductAbbrView());
            SettingsControls.Add("Shop-Broker", new ShopBRKView());
        }

        private void CloseWin_Click(object sender, RoutedEventArgs e) {
            isCrossBtnClicked = false;
            Close();
        }

        private void ChangeSettingCtrlView(object sender, RoutedPropertyChangedEventArgs<object> e) {
            TreeViewItem item = SettingsList.SelectedItem as TreeViewItem;
            if (item == null) return;
            string setting = item.Header.ToString();
            //switch (setting) {
            //    case "Product-Abbreviation":
            //        NavigateToProdAbbr.Command.Execute(NavigateToProdAbbr.CommandParameter);
            //        break;
            //    case "Shop-Broker":
            //        NavigateToShopBRK.Command.Execute(NavigateToShopBRK.CommandParameter);
            //        break;
            //    default: break;
            //}

            if (SettingsControls.ContainsKey(setting) && !panel.Children.Contains(SettingsControls[setting])) {
                panel.Children.Clear();
                SettingsControls[setting].SetValue(Grid.ColumnProperty, 2);
                panel.Children.Add(SettingsControls[setting]);
            }
        }
    }
}