using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ThemeViewer.ViewModels;

namespace ThemeViewer.Views.SysSettingUsrCtrls {
    /// <summary>
    /// Interaction logic for AlertSetting.xaml
    /// </summary>    
    public partial class AlertSetting : UserControl {
        Thickness LeftSide = new Thickness(10, 0, 0, 0);
        Thickness RightSide = new Thickness(70, 0, 0, 0);
        SolidColorBrush Off = new SolidColorBrush(Color.FromRgb(160, 160, 160));
        SolidColorBrush On = new SolidColorBrush(Color.FromRgb(21, 87, 228));
        public AlertSetting() {
            InitializeComponent();
            if (SettingWinVM.OriginalAlertOn) {
                AlertSwitch.Toggled = true;
                AlertSwitch.Back.Fill = On;               
                AlertSwitch.Dot.Margin = RightSide;
            }
            else {
                AlertSwitch.Toggled = false;
                AlertSwitch.Back.Fill = Off;
                AlertSwitch.Dot.Margin = LeftSide;
            }

            if (SettingWinVM.OriginalExcludeOurs) {
                ExcludeOurCom.Toggled = true;
                ExcludeOurCom.Back.Fill = On;
                ExcludeOurCom.Dot.Margin = RightSide;
            }
            else {
                ExcludeOurCom.Toggled = false;
                ExcludeOurCom.Back.Fill = Off;
                ExcludeOurCom.Dot.Margin = LeftSide;
            }
        }

        private void AlertSwitch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (AlertSwitch.Toggled == true) {
                SwitchStatus.Text = "On";
                IsAlertOn.IsChecked = true;
            }
            else {
                SwitchStatus.Text = "Off";
                IsAlertOn.IsChecked = false;
            }
        }

        private void ExcludeOurCom_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (ExcludeOurCom.Toggled == true) {
                SwitchStatus_Exclude.Text = "On";
                IsExcluded.IsChecked = true;
            }
            else {
                SwitchStatus_Exclude.Text = "Off";
                IsExcluded.IsChecked = false;
            }
        }
    }
}
