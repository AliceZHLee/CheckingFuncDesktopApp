using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ThemeViewer.Views.Components {
    /// <summary>
    /// Interaction logic for ToggleSwitch.xaml
    /// </summary>
    public partial class ToggleSwitch : UserControl {
        Thickness LeftSide = new Thickness(10, 0, 0, 0);
        Thickness RightSide = new Thickness(70, 0, 0, 0);
        SolidColorBrush Off = new SolidColorBrush(Color.FromRgb(160, 160, 160));
        SolidColorBrush On = new SolidColorBrush(Color.FromRgb(21,87,228));
        private bool _toggled = false;

        public ToggleSwitch() {
            InitializeComponent();
            Back.Fill = Off;
            _toggled = false;
            Dot.Margin = LeftSide;
        }

        public bool Toggled { get => _toggled; set => _toggled = value; }

        private void Dot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (!_toggled) {
                Back.Fill = On;
                _toggled = true;
                Dot.Margin = RightSide;
            }
            else {
                Back.Fill = Off;
                _toggled = false;
                Dot.Margin = LeftSide;
            }
        }

        private void Back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (!_toggled) {
                Back.Fill = On;
                _toggled = true;
                Dot.Margin = RightSide;
            }
            else {
                Back.Fill = Off;
                _toggled = false;
                Dot.Margin = LeftSide;
            }
        }
    }
}
