using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ThemeViewer.Models;

namespace ThemeViewer.Views.Components {
    /// <summary>
    /// Interaction logic for MultiSelectComboBox.xaml
    /// </summary>
    public partial class MultiSelectComboBox : UserControl {
        private ObservableCollection<FilterObj> _optionList;
        private bool isInitialized;
        public MultiSelectComboBox() {
            InitializeComponent();
            _optionList = new ObservableCollection<FilterObj>();
            isInitialized = true;
        }

        #region Dependency Properties

        public static readonly DependencyProperty ItemsSourceProperty =
             DependencyProperty.Register("ItemsSource", typeof(Dictionary<string, object>), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(null,
        new PropertyChangedCallback(MultiSelectComboBox.OnItemsSourceChanged)));

        public static readonly DependencyProperty SelectedItemsProperty =
             DependencyProperty.Register("SelectedItems", typeof(Dictionary<string, object>), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(null,
         new PropertyChangedCallback(MultiSelectComboBox.OnSelectedItemsChanged)));

        public static readonly DependencyProperty TextProperty =
             DependencyProperty.Register("Text", typeof(string), typeof(MultiSelectComboBox), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(MultiSelectComboBox), new UIPropertyMetadata(string.Empty));

        public Dictionary<string, object> ItemsSource {
            get { return (Dictionary<string, object>)GetValue(ItemsSourceProperty); }
            set {
                SetValue(ItemsSourceProperty, value);
            }
        }

        public Dictionary<string, object> SelectedItems {
            get {
                return (Dictionary<string, object>)GetValue(SelectedItemsProperty);
            }
            set {
                SetValue(SelectedItemsProperty, value);
            }
        }

        public string Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string DefaultText {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }
        #endregion

        #region Events
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            MultiSelectComboBox control = (MultiSelectComboBox)d;
            control.DisplayInControl();
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            MultiSelectComboBox control = (MultiSelectComboBox)d;
            control.SelectOptions();
            control.SetText();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e) {
            CheckBox clickedBox = (CheckBox)sender;
            if (clickedBox != null) {
                if (clickedBox.Content == "All") {
                    if (clickedBox.IsChecked.Value) {
                        foreach (FilterObj FilterObj in _optionList) {
                            FilterObj.IsChecked = true;
                        }
                    }
                    else {
                        foreach (FilterObj FilterObj in _optionList) {
                            FilterObj.IsChecked = false;
                        }
                    }

                }
                else {
                    int _selectedCount = 0;
                    foreach (FilterObj s in _optionList) {
                        if (s.IsChecked && s.Label != "All")
                            _selectedCount++;
                    }
                    if (_selectedCount == _optionList.Count - 1) {
                        _optionList.FirstOrDefault(i => i.Label == "All").IsChecked = true;
                    }
                    else {
                        _optionList.FirstOrDefault(i => i.Label == "All").IsChecked = false;
                    }
                }
            }
            SetSelectedItems();
            SetText();
        }
        #endregion

        #region Methods
        private void SelectOptions() {
            foreach (KeyValuePair<string, object> keyValue in SelectedItems) {
                FilterObj? FilterObj = _optionList.FirstOrDefault(i => i.Label == keyValue.Key);
                if (FilterObj != null)
                    FilterObj.IsChecked = true;
            }
        }

        private void SetSelectedItems() {
            if (SelectedItems == null) {
                SelectedItems = new Dictionary<string, object>();
            }
            SelectedItems.Clear();
            foreach (FilterObj FilterObj in _optionList) {
                if (FilterObj.IsChecked && FilterObj.Label != "All") {
                    if (ItemsSource.Count > 0) {
                        SelectedItems.Add(FilterObj.Label, ItemsSource[FilterObj.Label]);
                    }
                }
            }
        }

        private void DisplayInControl() {
            _optionList.Clear();
            if (ItemsSource.Count > 0) {
                _optionList.Add(new FilterObj() { Label = "All" });
            }
            foreach (KeyValuePair<string, object> keyValue in ItemsSource) {
                FilterObj filterObj = new FilterObj() { Label = keyValue.Key };
                _optionList.Add(filterObj);
            }
            if (isInitialized) {
                foreach (FilterObj item in _optionList) {
                    item.IsChecked = true;
                }
                if (SelectedItems == null) {
                    SelectedItems = new Dictionary<string, object>();
                }
                foreach (FilterObj FilterObj in _optionList) {
                    if (FilterObj.IsChecked && FilterObj.Label != "All") {
                        if (ItemsSource.Count > 0) {
                            SelectedItems.Add(FilterObj.Label, ItemsSource[FilterObj.Label]);
                        }
                    }
                }
                isInitialized = false;
            }
            MultiSelectCombo.ItemsSource = _optionList;
        }

        private void SetText() {
            if (SelectedItems != null) {
                StringBuilder displayText = new StringBuilder();
                foreach (FilterObj s in _optionList) {
                    if (s.IsChecked == true && s.Label == "All") {
                        displayText = new StringBuilder();
                        displayText.Append("All");
                        break;
                    }
                    else if (s.IsChecked == true && s.Label != "All") {
                        displayText.Append(s.Label);
                        displayText.Append(',');
                    }
                }
                Text = displayText.ToString().TrimEnd(new char[] { ',' });
            }
            // set DefaultText if nothing else selected
            if (string.IsNullOrEmpty(Text)) {
                Text = DefaultText;
            }
        }
        #endregion
    }
}
