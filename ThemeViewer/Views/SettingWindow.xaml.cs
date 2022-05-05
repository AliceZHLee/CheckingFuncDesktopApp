using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ThemeViewer.ViewModels;
using ThemeViewer.Views.SysSettingUsrCtrls;

namespace ThemeViewer.Views {
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window {
        private Dictionary<string, UserControl> SettingsControls;
        private bool isCrossBtnClicked = true;

        public SettingWindow() {
            InitializeComponent();
            SettingsControls = new Dictionary<string, UserControl>();
            SettingsControls.Add("Alert Setting", new AlertSetting());
        }

        private void OnWindowClosing(object? sender, CancelEventArgs e) {
            throw new NotImplementedException();
        }

        private void ChangeSettingCtrlView(object sender, RoutedPropertyChangedEventArgs<object> e) {
            TreeViewItem item = SettingsList.SelectedItem as TreeViewItem;
            if (item == null) return;
            string setting = item.Header.ToString();
            if (SettingsControls.ContainsKey(setting) && !panel.Children.Contains(SettingsControls[setting])) {
                panel.Children.Clear();
                SettingsControls[setting].SetValue(Grid.ColumnProperty, 2);
                panel.Children.Add(SettingsControls[setting]);
            }
        }

        private void CloseWin_Click(object sender, RoutedEventArgs e) {
            isCrossBtnClicked = false;
            Close();
        }

        private void Win_Close(object sender, EventArgs e) {
            if (isCrossBtnClicked) {
                CancelBtn.Command.Execute(CancelBtn.CommandParameter);
            }
        }
    }
}
