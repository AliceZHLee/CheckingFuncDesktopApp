using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using ThemeViewer.Models;
using ThemeViewer.ViewModels;

namespace ThemeViewer.Views {
    /// <summary>
    /// Interaction logic for MarketDataUsrCtrl.xaml
    /// </summary>
    public partial class MarketDataUsrCtrl : UserControl {
        private readonly MarketDataVM _vm;
        private bool isSelectAllOperated = false;
        private readonly int MAXNUM = int.MaxValue;
        private readonly int MINNUM = int.MinValue;
        public MarketDataUsrCtrl() {
            InitializeComponent();
            _vm = new MarketDataVM(ThmViewerWin.EA);
            DataContext = _vm;
            _vm.MarketDataViewS.Filter += Data_Filter;
            Loaded += MarketDataUsrCtrl_Loaded;
        }

        private void MarketDataUsrCtrl_Loaded(object sender, RoutedEventArgs e) {
            var win = Window.GetWindow(this);
            win.Closing += Win_Closing;
        }

        private void Win_Closing(object sender, CancelEventArgs e) {
            _vm.Dispose();
        }

        private void Data_Filter(object sender, FilterEventArgs e) {
            e.Accepted = true;
            if (e.Item is MarketRecord p) {
                foreach (var item in p.GetType().GetProperties()) {
                    string colName = item.Name;
                    if (Enum.IsDefined(typeof(Displaycolumn), colName)) {
                        var property = p.GetType().GetProperty(colName);
                        if (property != null) {
                            var obj = property.GetValue(p, null);
                            string value = obj == null ? "" : obj.ToString();
                            if (_vm.FilterDic.ContainsKey(colName)) {
                                ObservableCollection<FilterObj>? currentFilter;
                                _vm.FilterDic.TryGetValue(colName, out currentFilter);
                                if (currentFilter != null) {
                                    if (Enum.IsDefined(typeof(CheckboxFiltercolumn), colName)) {
                                        if (!currentFilter.Any(x => x.IsChecked == true && x.Label == value)) {
                                            e.Accepted = false;
                                            return;
                                        }
                                    }
                                    else if (Enum.IsDefined(typeof(NumberRangeFilterColumn), colName)) {
                                        if (currentFilter != null && currentFilter.Count() > 0) {
                                            if (string.IsNullOrEmpty(value)) {
                                                e.Accepted = false;
                                                return;
                                            }
                                            double min = currentFilter[0].MinLimit == null ? double.MinValue : (double)currentFilter[0].MinLimit;
                                            double max = currentFilter[0].MaxLimit == null ? double.MaxValue : (double)currentFilter[0].MaxLimit;
                                            double num = Convert.ToDouble(value);
                                            if (num < min || num > max) {
                                                e.Accepted = false;
                                                return;
                                            }
                                        }
                                    }
                                    else if (Enum.IsDefined(typeof(ComboDateTimeFilterColumn), colName) || Enum.IsDefined(typeof(DateTimeFilterColumn), colName)) {
                                        if (string.IsNullOrEmpty(value)) {
                                            e.Accepted = false;
                                            return;
                                        }
                                        DateTime from = currentFilter[0].DateFrom != null ? (DateTime)currentFilter[0].DateFrom : DateTime.MinValue;
                                        DateTime to = currentFilter[0].DateTo != null ? (DateTime)currentFilter[0].DateTo : DateTime.MaxValue;
                                        DateTime time = Convert.ToDateTime(value);
                                        if (time < from || time > to) {
                                            e.Accepted = false;
                                            return;
                                        }
                                    }
                                    else if (Enum.IsDefined(typeof(ComboTextFilterColumn), colName)) {
                                        if (!currentFilter.Any(x => x.Label == value)) {
                                            if (p == null || !p.NewlyReceived) {
                                                e.Accepted = false;
                                                return;
                                            }
                                            Popup popElement = FindName("pop_ComboFlt_" + colName) as Popup;
                                            bool isQualified = false;
                                            if (popElement != null) {
                                                string fltLabel = popElement.Uid;
                                                Border border = popElement.Child as Border;
                                                if (border != null) {
                                                    Grid grid = border.Child as Grid;
                                                    if (grid != null) {
                                                        ComboBox cb = null;
                                                        TextBox tb = null;
                                                        foreach (var control in grid.Children) {
                                                            if (control is ComboBox) {
                                                                cb = control as ComboBox;
                                                                continue;
                                                            }
                                                            if (control is TextBox) {
                                                                tb = control as TextBox;
                                                                continue;
                                                            }
                                                        }
                                                        if (tb != null && cb != null) {
                                                            string input = tb.Text;
                                                            string filterCtrl = (string)cb.SelectedItem;
                                                            switch (filterCtrl) {
                                                                case "Contains":
                                                                    isQualified = value.ToUpper().Contains(input.ToUpper());
                                                                    break;
                                                                case "Starts With":
                                                                    isQualified = value.ToUpper().StartsWith(input.ToUpper());
                                                                    break;
                                                                case "Ends With":
                                                                    isQualified = value.ToUpper().EndsWith(input.ToUpper());
                                                                    break;
                                                                case "Equals":
                                                                    isQualified = value.ToUpper() == input.ToUpper();
                                                                    break;
                                                                case "Does Not Contain":
                                                                    isQualified = value.ToUpper().Contains(input.ToUpper()) == false;
                                                                    break;
                                                                default:
                                                                    isQualified = value.ToUpper().Contains(input.ToUpper());
                                                                    break;
                                                            }
                                                            e.Accepted = isQualified;
                                                            return;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
                p.NewlyReceived = false;
            }
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e) {
            ToggleButton btn = (ToggleButton)sender;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            DockPanel dp = (DockPanel)btn.Parent;
            string colName = btn.Name;
            Popup popUpWin = new Popup();
            Border popUpInnerBorder = new Border();
            foreach (var ch in dp.Children) {
                if (ch is Popup pop) {
                    popUpWin = pop;
                    popUpInnerBorder = (Border)popUpWin.Child;
                    popUpWin.Uid = colName;
                    break;
                }
            }

            if (Enum.IsDefined(typeof(CheckboxFiltercolumn), colName)) {
                foreach (MarketRecord p in _vm.MarketDataViewS.View) {
                    if (p != null) {
                        var property = p.GetType().GetProperty(colName);
                        if (property != null) {
                            var obj = property.GetValue(p, null);
                            string value = obj == null ? "" : obj.ToString();
                            bool IsNew = true;
                            foreach (FilterObj flt in currentFilter) {
                                if (flt.Label == value) {
                                    IsNew = false;
                                    break;
                                }
                            }
                            if (IsNew) {
                                currentFilter.Add(new FilterObj() { IsChecked = true, Label = value });
                            }
                        }
                    }
                }
                if (_vm.FilterDic.ContainsKey(colName)) {
                    _vm.FilterDic[colName] = currentFilter;
                }
                else {
                    _vm.FilterDic.Add(colName, currentFilter);
                }
                CheckBox selectAllCheckbox = new CheckBox();
                Grid gd = (Grid)popUpInnerBorder.Child;
                bool findCheckBox = false;
                foreach (var item in gd.Children) {
                    if (item is StackPanel stp) {
                        foreach (var component in stp.Children) {
                            if (component is CheckBox cb) {
                                selectAllCheckbox = cb;
                                findCheckBox = true;
                                break;
                            }
                        }
                    }
                    if (findCheckBox) break;
                }
                if (!currentFilter.Any(x => x.IsChecked == false)) { // select all
                    selectAllCheckbox.IsChecked = true;
                }
                else if (!currentFilter.Any(x => x.IsChecked == true)) { //unselect all
                    selectAllCheckbox.IsChecked = false;
                }
                else { //partial select
                    selectAllCheckbox.IsChecked = null;
                }
            }
            else if (Enum.IsDefined(typeof(NumberRangeFilterColumn), colName)) {
                if (!_vm.FilterDic.ContainsKey(colName)) {
                    //currentFilter = new ObservableCollection<FilterObj>();
                    foreach (MarketRecord p in (ObservableCollection<MarketRecord>)_vm.MarketDataViewS.Source) {
                        if (p != null) {
                            var property = p.GetType().GetProperty(colName);
                            if (property != null) {
                                var obj = property.GetValue(p, null);
                                string strvalue = obj == null ? "" : obj.ToString();
                                bool IsNew = true;
                                foreach (FilterObj flt in currentFilter) {
                                    if (strvalue == flt.Label) {
                                        IsNew = false;
                                        break;
                                    }
                                }
                                if (IsNew) {
                                    currentFilter.Add(new FilterObj() { Label = strvalue });
                                }
                            }
                        }
                    }
                    if (_vm.FilterDic.ContainsKey(colName)) {
                        _vm.FilterDic[colName] = currentFilter;
                    }
                    else {
                        _vm.FilterDic.Add(colName, currentFilter);
                    }
                }
            }
            else if (Enum.IsDefined(typeof(ComboDateTimeFilterColumn), colName) || Enum.IsDefined(typeof(DateTimeFilterColumn), colName)) {
                if (!_vm.FilterDic.ContainsKey(colName)) {
                    //currentFilter = new ObservableCollection<FilterObj>();
                    foreach (MarketRecord p in (ObservableCollection<MarketRecord>)_vm.MarketDataViewS.Source) {
                        if (p != null) {
                            var property = p.GetType().GetProperty(colName);
                            if (property != null) {
                                var obj = property.GetValue(p, null);
                                string strvalue = obj != null ? obj.ToString() : "";
                                bool IsNew = true;
                                foreach (FilterObj flt in currentFilter) {
                                    if (strvalue == flt.Label) {
                                        IsNew = false;
                                        break;
                                    }
                                }
                                if (IsNew) {
                                    currentFilter.Add(new FilterObj() { Label = strvalue });
                                }
                            }
                        }
                    }
                    if (_vm.FilterDic.ContainsKey(colName)) {
                        _vm.FilterDic[colName] = currentFilter;
                    }
                    else {
                        _vm.FilterDic.Add(colName, currentFilter);
                    }
                }
            }
            foreach (var ch in dp.Children) {
                if (ch is Popup pop) {
                    pop.IsOpen = true;
                    var border = (Border)pop.Child;
                    var grid = (Grid)border.Child;
                    bool findListBox = false;
                    foreach (var control in grid.Children) {
                        if (control is StackPanel s) {
                            foreach (var item in s.Children) {
                                if (item is ListBox l) {
                                    l.ItemsSource = currentFilter;
                                    findListBox = true;
                                    break;
                                }
                            }
                        }
                        if (findListBox) break;
                    }
                }
            }
        }

        #region checkbox 3 status    
        private void CheckBox_Update(object sender, RoutedEventArgs e) {
            if (isSelectAllOperated) {
                return;
            }
            else {
                CheckBox cb = (CheckBox)sender;
                ListBox lb = GetParent(cb);
                StackPanel sp = (StackPanel)lb.Parent;
                if (sp != null) {
                    foreach (var item in sp.Children) {
                        if (item is CheckBox selectAllBox) {
                            if (cb.IsChecked == false) {
                                bool AnyItemChecked = false;
                                foreach (FilterObj flt in lb.Items) {
                                    if (flt.Label != "(Select All)") {
                                        AnyItemChecked |= (bool)flt.IsChecked;
                                    }
                                    if (AnyItemChecked) {
                                        selectAllBox.IsChecked = null;
                                        return;
                                    }
                                }
                                selectAllBox.IsChecked = false;
                            }
                            if (cb.IsChecked == true) {
                                bool AllItemChecked = true;
                                foreach (FilterObj flt in lb.Items) {
                                    if (flt.Label != "(Select All)") {
                                        AllItemChecked &= (bool)flt.IsChecked;
                                    }
                                    if (!AllItemChecked) {
                                        selectAllBox.IsChecked = null;
                                        return;
                                    }
                                }
                                selectAllBox.IsChecked = true;
                            }
                        }
                    }
                }
            }
        }

        private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e) {
            isSelectAllOperated = true;
            ListBox lbFilter = new ListBox();
            CheckBox cb = (CheckBox)sender;
            StackPanel sp = (StackPanel)cb.Parent;
            foreach (var control in sp.Children) {
                if (control is ListBox lb) {
                    lbFilter = lb;
                    break;
                }
            }
            if (cb.Content != null) {
                if (cb.Content.ToString() == "(Select All)") {
                    if (cb.IsChecked == true) {
                        foreach (FilterObj item in lbFilter.Items) {
                            item.IsChecked = true;
                        }
                    }
                    if (cb.IsChecked == false) {
                        foreach (FilterObj item in lbFilter.Items) {
                            item.IsChecked = false;
                        }
                    }
                    if (cb.IsChecked == null) {
                        cb.IsChecked = false;
                    }
                }
            }
            isSelectAllOperated = false;
        }

        private ListBox GetParent(Visual v) {
            while (v != null) {
                v = (Visual)VisualTreeHelper.GetParent(v);
                if (v is ListBox)
                    break;
            }
            if (v is ListBox lb) {
                return lb;
            }
            else {
                return new ListBox();
            }
        }
        #endregion

        private void CloseFltLabel_Click(object sender, RoutedEventArgs e) {
            Button btn = (Button)sender;
            if (btn != null) {
                string fltDisplaylabel = ((Button)sender).Tag.ToString();
                if (fltDisplaylabel == null) {
                    fltDisplaylabel = "";
                }
                _vm.RemoveDisplayLabel(fltDisplaylabel);
                if (_vm.FilterLabel.Count == 0) {
                    added_FilterDic_Label.Visibility = Visibility.Hidden;
                    Default_Filter_Label.Visibility = Visibility.Visible;
                }
                string fltlabel = _vm.PropertyColumnMapping.FirstOrDefault(x => x.Value == fltDisplaylabel).Key.ToString();

                _vm.FilterDic.Remove(fltlabel);
                switch (fltlabel) {
                    case "ExecutedSGT":
                        datePicker_ExecutedSGTFrom.SelectedDate = null;
                        datePicker_ExecutedSGTTo.SelectedDate = null;
                        ExecTime_From.SelectedItem = null;
                        ExecTime_To.SelectedItem = null;
                        break;
                    case "ClearedSGT":
                        datePicker_ClearedSGTFrom.SelectedDate = null;
                        datePicker_ClearedSGTTo.SelectedDate = null;
                        ClearedTime_From.SelectedItem = null;
                        ClearedTime_To.SelectedItem = null;
                        break;
                    case "DeletedSGT":
                        datePicker_DeletedSGTFrom.SelectedDate = null;
                        datePicker_DeletedSGTTo.SelectedDate = null;
                        DeletedTime_From.SelectedItem = null;
                        DeletedTime_To.SelectedItem = null;
                        break;
                    case "SubmittedSGT":
                        datePicker_SubmittedSGTFrom.SelectedDate = null;
                        datePicker_SubmittedSGTTo.SelectedDate = null;
                        SubmitTime_From.SelectedItem = null;
                        SubmitTime_To.SelectedItem = null;
                        break;
                    case "ClearedDate":
                        datePicker_ClearedDateFrom.SelectedDate = null;
                        datePicker_ClearedDateTo.SelectedDate = null;
                        break;
                    case "Price":
                        MinNum_Price.Text = "";
                        MaxNum_Price.Text = "";
                        break;
                    case "QtyInLots":
                        MinNum_QtyInLots.Text = "";
                        MaxNum_QtyInLots.Text = "";
                        break;
                    case "Amount":
                        MinNum_Amount.Text = "";
                        MaxNum_Amount.Text = "";
                        break;
                    case "Strike":
                        MinNum_Strike.Text = "";
                        MaxNum_Strike.Text = "";
                        break;
                    case "DealID":
                        DealID_Flt_Input.Text = "";
                        break;
                    default: break;
                }
                _vm.MarketDataViewS.View.Filter = null;
                _vm.MarketDataViewS.Filter += Data_Filter;
                _vm.MarketDataViewS.View.Refresh();

                //clear sort direction after remove one filter label
                ICollectionView view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
                if (view != null) {
                    foreach (DataGridColumn column in dataGrid.Columns) {
                        column.SortDirection = null;
                    }
                }
            }
        }

        private void Reset_CurrFlts(object sender, RoutedEventArgs e) {
            _vm.MarketDataViewS.View.Filter = null;
            _vm.MarketDataViewS.View.Refresh();

            #region //clear previous selection
            DealID_Flt_Input.Text = "";

            Checkbox_selectAll_ComboType.IsChecked = true;
            Checkbox_selectAll_Contract.IsChecked = true;
            Checkbox_selectAll_Instrument.IsChecked = true;
            Checkbox_selectAll_Legs.IsChecked = true;
            Checkbox_selectAll_Market.IsChecked = true;
            Checkbox_selectAll_Product.IsChecked = true;
            Checkbox_selectAll_Unit.IsChecked = true;
            Checkbox_selectAll_Session.IsChecked = true;
            Checkbox_selectAll_Status.IsChecked = true;

            MinNum_Amount.Text = "";
            MaxNum_Amount.Text = "";
            MinNum_Price.Text = "";
            MaxNum_Price.Text = "";
            MinNum_QtyInLots.Text = "";
            MaxNum_QtyInLots.Text = "";
            MinNum_Strike.Text = "";
            MaxNum_Strike.Text = "";

            datePicker_ClearedDateFrom.SelectedDate = null;
            datePicker_ClearedDateTo.SelectedDate = null;
            datePicker_ClearedSGTFrom.SelectedDate = null;
            datePicker_ClearedSGTTo.SelectedDate = null;
            datePicker_DeletedSGTFrom.SelectedDate = null;
            datePicker_DeletedSGTTo.SelectedDate = null;
            datePicker_ExecutedSGTFrom.SelectedDate = null;
            datePicker_ExecutedSGTTo.SelectedDate = null;
            datePicker_SubmittedSGTFrom.SelectedDate = null;
            datePicker_SubmittedSGTTo.SelectedDate = null;
            #endregion //clear previous selection

            #region //clear current filter label
            _vm.FilterLabel.Clear();
            _vm.FilterDic.Clear();
            added_FilterDic_Label.Visibility = Visibility.Hidden;
            Default_Filter_Label.Visibility = Visibility.Visible;
            #endregion //clear current filter label

            #region //clear sort direction
            ICollectionView view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
            if (view != null && view.SortDescriptions.Count > 0) {
                view.SortDescriptions.Clear();
                foreach (DataGridColumn column in dataGrid.Columns) {
                    column.SortDirection = null;
                }
            }
            #endregion //clear sort direction

            #region // re-display all columns    
            //_vm.MarketColumns.ForEach(x => x.IsChecked = true);
            checkbox_selectAll_CustomizeColumns.IsChecked = true;
            dataGrid.Columns.ToList().ForEach(x => x.Visibility = Visibility.Visible);
            #endregion // re-display all columns
        }

        #region number up and down
        private void NUDButtonUP_Click(object sender, RoutedEventArgs e) {
            string valueName = "";
            RepeatButton upBtn = (RepeatButton)sender;
            Grid numGrid = (Grid)upBtn.Parent;
            foreach (var ch in numGrid.Children) {
                if (ch is TextBox tb) {
                    valueName = tb.Name;
                    break;
                }
            }
            if (valueName != null) {
                if (valueName == "MinNum_Price") {
                    if (MinNum_Price.Text == "") {
                        MinNum_Price.Text = "0";
                    }
                    double Num;
                    bool isNumber = double.TryParse(MinNum_Price.Text, out Num);
                    if (!isNumber) {
                        MessageBox.Show("Input must be numeric"); return;
                    }
                    Num = (int)Num;
                    MinNum_Price.Text = (Num + 1).ToString();
                }
                else if (valueName == "MaxNum_Price") {
                    if (MaxNum_Price.Text == "") {
                        MaxNum_Price.Text = "0";
                    }
                    double Num;
                    bool isNumber = double.TryParse(MaxNum_Price.Text, out Num);
                    if (!isNumber) {
                        MessageBox.Show("Input must be numeric"); return;
                    }
                    Num = (int)Num;
                    MaxNum_Price.Text = (Num + 1).ToString();
                }
                else if (valueName == "MinNum_QtyInLots") {
                    if (MinNum_QtyInLots.Text == "") {
                        MinNum_QtyInLots.Text = "0";
                    }
                    double Num;
                    bool isNumber = double.TryParse(MinNum_QtyInLots.Text, out Num);
                    if (!isNumber) {
                        MessageBox.Show("Input must be numeric"); return;
                    }
                    Num = (int)Num;
                    MinNum_QtyInLots.Text = (Num + 1).ToString();
                }
                else if (valueName == "MaxNum_QtyInLots") {
                    if (MaxNum_QtyInLots.Text == "") {
                        MaxNum_QtyInLots.Text = "0";
                    }
                    double Num;
                    bool isNumber = double.TryParse(MaxNum_QtyInLots.Text, out Num);
                    if (!isNumber) {
                        MessageBox.Show("Input must be numeric"); return;
                    }
                    Num = (int)Num;
                    MaxNum_QtyInLots.Text = (Num + 1).ToString();
                }
                else if (valueName == "MinNum_Strike") {
                    if (MinNum_Strike.Text == "") {
                        MinNum_Strike.Text = "0";
                    }
                    double Num;
                    bool isNumber = double.TryParse(MinNum_Strike.Text, out Num);
                    if (!isNumber) {
                        MessageBox.Show("Input must be numeric"); return;
                    }
                    Num = (int)Num;
                    MinNum_Strike.Text = (Num + 1).ToString();
                }
                else if (valueName == "MaxNum_Strike") {
                    if (MaxNum_Strike.Text == "") {
                        MaxNum_Strike.Text = "0";
                    }
                    double Num;
                    bool isNumber = double.TryParse(MaxNum_Strike.Text, out Num);
                    if (!isNumber) {
                        MessageBox.Show("Input must be numeric"); return;
                    }
                    Num = (int)Num;
                    MaxNum_Strike.Text = (Num + 1).ToString();
                }
                else if (valueName == "MinNum_Amount") {
                    if (MinNum_Amount.Text == "") {
                        MinNum_Amount.Text = "0";
                    }
                    double Num;
                    bool isNumber = double.TryParse(MinNum_Amount.Text, out Num);
                    if (!isNumber) {
                        MessageBox.Show("Input must be numeric"); return;
                    }
                    Num = (int)Num;
                    MinNum_Amount.Text = (Num + 1).ToString();
                }
                else if (valueName == "MaxNum_Amount") {
                    if (MaxNum_Amount.Text == "") {
                        MaxNum_Amount.Text = "0";
                    }
                    double Num;
                    bool isNumber = double.TryParse(MaxNum_Amount.Text, out Num);
                    if (!isNumber) {
                        MessageBox.Show("Input must be numeric"); return;
                    }
                    Num = (int)Num;
                    MaxNum_Amount.Text = (Num + 1).ToString();
                }
            }
        }

        private void NUDButtonDown_Click(object sender, RoutedEventArgs e) {
            string valueName = "";
            RepeatButton upBtn = (RepeatButton)sender;
            if (upBtn != null) {
                Grid numGrid = (Grid)upBtn.Parent;
                if (numGrid != null) {
                    foreach (var ch in numGrid.Children) {
                        if (ch is TextBox tb) {
                            valueName = tb.Name;
                            break;
                        }
                    }
                    if (valueName != null) {
                        if (valueName == "MinNum_Price") {
                            if (MinNum_Price.Text == "") {
                                MinNum_Price.Text = "0";
                            }
                            double Num;
                            bool isNumber = double.TryParse(MinNum_Price.Text, out Num);
                            if (!isNumber) {
                                MessageBox.Show("Input must be numeric"); return;
                            }
                            Num = (int)Num;
                            MinNum_Price.Text = (Num - 1).ToString();
                        }
                        else if (valueName == "MaxNum_Price") {
                            if (MaxNum_Price.Text == "") {
                                MaxNum_Price.Text = "0";
                            }
                            double Num;
                            bool isNumber = double.TryParse(MaxNum_Price.Text, out Num);
                            if (!isNumber) {
                                MessageBox.Show("Input must be numeric"); return;
                            }
                            Num = (int)Num;
                            MaxNum_Price.Text = (Num - 1).ToString();
                        }
                        else if (valueName == "MinNum_QtyInLots") {
                            if (MinNum_QtyInLots.Text == "") {
                                MinNum_QtyInLots.Text = "0";
                            }
                            double Num;
                            bool isNumber = double.TryParse(MinNum_QtyInLots.Text, out Num);
                            if (!isNumber) {
                                MessageBox.Show("Input must be numeric"); return;
                            }
                            Num = (int)Num;
                            MinNum_QtyInLots.Text = (Num - 1).ToString();
                        }
                        else if (valueName == "MaxNum_QtyInLots") {
                            if (MaxNum_QtyInLots.Text == "") {
                                MaxNum_QtyInLots.Text = "0";
                            }
                            double Num;
                            bool isNumber = double.TryParse(MaxNum_QtyInLots.Text, out Num);
                            if (!isNumber) {
                                MessageBox.Show("Input must be numeric"); return;
                            }
                            Num = (int)Num;
                            MaxNum_QtyInLots.Text = (Num - 1).ToString();
                        }
                        else if (valueName == "MinNum_Strike") {
                            if (MinNum_Strike.Text == "") {
                                MinNum_Strike.Text = "0";
                            }
                            double Num;
                            bool isNumber = double.TryParse(MinNum_Strike.Text, out Num);
                            if (!isNumber) {
                                MessageBox.Show("Input must be numeric"); return;
                            }
                            Num = (int)Num;
                            MinNum_Strike.Text = (Num - 1).ToString();
                        }
                        else if (valueName == "MaxNum_Strike") {
                            if (MaxNum_Strike.Text == "") {
                                MaxNum_Strike.Text = "0";
                            }
                            double Num;
                            bool isNumber = double.TryParse(MaxNum_Strike.Text, out Num);
                            if (!isNumber) {
                                MessageBox.Show("Input must be numeric"); return;
                            }
                            Num = (int)Num;
                            MaxNum_Strike.Text = (Num - 1).ToString();
                        }
                        else if (valueName == "MinNum_Amount") {
                            if (MinNum_Amount.Text == "") {
                                MinNum_Amount.Text = "0";
                            }
                            double Num;
                            bool isNumber = double.TryParse(MinNum_Amount.Text, out Num);
                            if (!isNumber) {
                                MessageBox.Show("Input must be numeric"); return;
                            }
                            Num = (int)Num;
                            MinNum_Amount.Text = (Num - 1).ToString();
                        }
                        else if (valueName == "MaxNum_Amount") {
                            if (MaxNum_Amount.Text == "") {
                                MaxNum_Amount.Text = "0";
                            }
                            double Num;
                            bool isNumber = double.TryParse(MaxNum_Amount.Text, out Num);
                            if (!isNumber) {
                                MessageBox.Show("Input must be numeric"); return;
                            }
                            Num = (int)Num;
                            MaxNum_Amount.Text = (Num - 1).ToString();
                        }
                    }
                }
            }
        }
        #endregion

        #region Apply filter
        #region combo box filter
        private void DealID_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_DealID.Uid;
            string input = DealID_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<MarketRecord> filteredValue;
            string filterCtrl = (string)TextFltOptions_DealID.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.DealID)) {
                    currentFilter.Add(new FilterObj() { Label = item.DealID });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_DealID.IsOpen = false;
        }

        private IEnumerable<MarketRecord> ComboxBoxFilter(string fltLabel, string filterCtrl, string input) {
            IEnumerable<MarketRecord> filteredValue;

            switch (filterCtrl) {
                case "Contains":
                    filteredValue = _vm.AllMarketData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper().Contains(input.ToUpper()));
                    break;
                case "Starts With":
                    filteredValue = _vm.AllMarketData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper().StartsWith(input.ToUpper()));
                    break;
                case "Ends With":
                    filteredValue = _vm.AllMarketData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper().EndsWith(input.ToUpper()));
                    break;
                case "Equals":
                    filteredValue = _vm.AllMarketData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper() == input.ToUpper());
                    break;
                case "Does Not Contain":
                    filteredValue = _vm.AllMarketData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper().Contains(input.ToUpper()) == false);
                    break;
                default:
                    filteredValue = _vm.AllMarketData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper().Contains(input.ToUpper()));
                    break;
            }
            return filteredValue;

        }
        #endregion

        #region checkbox filter
        private void Market_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_Market.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_Market.IsOpen = false;
        }
        private void Instrument_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_Instrument.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_Instrument.IsOpen = false;
        }

        private void Legs_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_Legs.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_Legs.IsOpen = false;
        }

        private void ComboType_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_ComboType.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_ComboType.IsOpen = false;
        }

        private void Contract_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_Contract.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_Contract.IsOpen = false;
        }

        private void Product_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_Product.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_Product.IsOpen = false;
        }
        private void Unit_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_Unit.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_Unit.IsOpen = false;
        }

        private void Session_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_Session.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_Session.IsOpen = false;
        }

        private void Status_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_Status.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_Status.IsOpen = false;
        }

        private bool? GetTopSelectAllCheckStatus(object sender) {
            Button applyBtn = (Button)sender;
            StackPanel btnBar = (StackPanel)applyBtn.Parent;
            Grid gd = (Grid)btnBar.Parent;
            bool? isTopSelectAllChecked = false;
            foreach (var item in gd.Children) {
                if (item is StackPanel sp) {
                    foreach (var component in sp.Children) {
                        if (component is CheckBox ch) {
                            isTopSelectAllChecked = ch.IsChecked;
                            break;
                        }
                    }
                    break;
                }
            }
            return isTopSelectAllChecked;
        }
        #endregion

        #region number range filter
        private void Strike_Filter_Apply(object sender, RoutedEventArgs e) {
            if (MinNum_Strike.Text == "" && MaxNum_Strike.Text == "") {
                return;
            }
            string fltLabel = pop_RangeFlt_Strike.Uid;
            double parseOutput;
            if (!double.TryParse(MinNum_Strike.Text, out parseOutput) && MinNum_Strike.Text != "") {
                MessageBox.Show("Input must be numeric"); return;
            }
            if (!double.TryParse(MaxNum_Strike.Text, out parseOutput) && MaxNum_Strike.Text != "") {
                MessageBox.Show("Input must be numeric"); return;
            }
            double min = MinNum_Strike.Text != "" ? Convert.ToDouble(MinNum_Strike.Text) : MINNUM;
            double max = MaxNum_Strike.Text != "" ? Convert.ToDouble(MaxNum_Strike.Text) : MAXNUM;
            if (min > max) {
                MessageBox.Show("Strike's Minimum value cannot be greater than Maximum value!");
                return;
            }
            RefreshWindowAfterNumRangeFilter(fltLabel, min, max);
            pop_RangeFlt_Strike.IsOpen = false;
        }

        private void Price_Filter_Apply(object sender, RoutedEventArgs e) {
            if (MinNum_Price.Text == "" && MaxNum_Price.Text == "") {
                return;
            }
            string fltLabel = pop_RangeFlt_Price.Uid;
            double parseOutput;
            if (!double.TryParse(MinNum_Price.Text, out parseOutput) && MinNum_Price.Text != "") {
                MessageBox.Show("Input must be numeric"); return;
            }
            if (!double.TryParse(MaxNum_Price.Text, out parseOutput) && MaxNum_Price.Text != "") {
                MessageBox.Show("Input must be numeric"); return;
            }
            double min = MinNum_Price.Text != "" ? Convert.ToDouble(MinNum_Price.Text) : MINNUM;
            double max = MaxNum_Price.Text != "" ? Convert.ToDouble(MaxNum_Price.Text) : MAXNUM;
            if (min > max) {
                MessageBox.Show("Price's Minimum value cannot be greater than Maximum value!");
                return;
            }
            RefreshWindowAfterNumRangeFilter(fltLabel, min, max);
            pop_RangeFlt_Price.IsOpen = false;
        }

        private void Amount_Filter_Apply(object sender, RoutedEventArgs e) {
            if (MinNum_Amount.Text == "" && MaxNum_Amount.Text == "") {
                return;
            }
            string fltLabel = pop_RangeFlt_Amount.Uid;
            double parseOutput;
            if (!double.TryParse(MinNum_Amount.Text, out parseOutput) && MinNum_Amount.Text != "") {
                MessageBox.Show("Input must be numeric"); return;
            }
            if (!double.TryParse(MaxNum_Amount.Text, out parseOutput) && MaxNum_Amount.Text != "") {
                MessageBox.Show("Input must be numeric"); return;
            }
            double min = MinNum_Amount.Text != "" ? Convert.ToDouble(MinNum_Amount.Text) : MINNUM;
            double max = MaxNum_Amount.Text != "" ? Convert.ToDouble(MaxNum_Amount.Text) : MAXNUM;
            if (min > max) {
                MessageBox.Show("Amount's Minimum value cannot be greater than Maximum value!");
                return;
            }
            RefreshWindowAfterNumRangeFilter(fltLabel, min, max);
            pop_RangeFlt_Amount.IsOpen = false;
        }

        private void QtyInLots_Filter_Apply(object sender, RoutedEventArgs e) {
            if (MinNum_QtyInLots.Text == "" && MaxNum_QtyInLots.Text == "") {
                return;
            }
            string fltLabel = pop_RangeFlt_QtyInLots.Uid;
            double parseOutput;
            if (!double.TryParse(MinNum_QtyInLots.Text, out parseOutput) && MinNum_QtyInLots.Text != "") {
                MessageBox.Show("Input must be numeric"); return;
            }
            if (!double.TryParse(MaxNum_QtyInLots.Text, out parseOutput) && MaxNum_QtyInLots.Text != "") {
                MessageBox.Show("Input must be numeric"); return;
            }
            double min = MinNum_QtyInLots.Text != "" ? Convert.ToDouble(MinNum_QtyInLots.Text) : MINNUM;
            double max = MaxNum_QtyInLots.Text != "" ? Convert.ToDouble(MaxNum_QtyInLots.Text) : MAXNUM;
            if (min > max) {
                MessageBox.Show("Quantity In Lots's Minimum value cannot be greater than Maximum value!");
                return;
            }
            RefreshWindowAfterNumRangeFilter(fltLabel, min, max);
            pop_RangeFlt_QtyInLots.IsOpen = false;
        }
        #endregion

        #region combo and datetime filter
        private void ExecutedSGT_Filter_Apply(object sender, RoutedEventArgs e) {
            ObservableCollection<FilterObj>? currentFilter;
            string fltLabel = pop_ComboDateTimeFlt_ExecutedSGT.Uid;
            _vm.FilterDic.TryGetValue(fltLabel, out currentFilter);
            if (currentFilter != null) {
                string selectedValue;
                DateTime? ExecDateFrom = datePicker_ExecutedSGTFrom.SelectedDate;
                DateTime? ExecDateTo = datePicker_ExecutedSGTTo.SelectedDate;
                if (ExecDateFrom == null && ExecDateTo == null) {
                    pop_ComboDateTimeFlt_ExecutedSGT.IsOpen = false;
                    ExecTime_From.SelectedItem = null;
                    ExecTime_To.SelectedItem = null;
                    return;
                }
                else {
                    if (ExecDateFrom != null) {
                        DateTime dateStartFrom = ((DateTime)ExecDateFrom).Date;
                        TimeSpan timeStartFrom = Convert.ToDateTime(ExecTime_From.SelectedItem).TimeOfDay;
                        ExecDateFrom = ExecDateFrom == null ? null : dateStartFrom + timeStartFrom;
                    }

                    if (ExecDateTo != null) {
                        DateTime dateEndWith = ((DateTime)ExecDateTo).Date;
                        TimeSpan timeEndWith = Convert.ToDateTime(ExecTime_To.SelectedItem).TimeOfDay;
                        ExecDateTo = ExecDateTo == null ? null : dateEndWith + timeEndWith;
                    }

                    if (ExecDateTo == null) {
                        selectedValue = "From: " + ExecDateFrom.ToString();
                    }
                    else if (ExecDateFrom == null) {
                        selectedValue = "To: " + ExecDateTo.ToString();
                    }
                    else {
                        selectedValue = "From: " + ExecDateFrom.ToString() + " To: " + ExecDateTo.ToString();
                    }
                }

                foreach (var item in currentFilter) {
                    item.DateFrom = ExecDateFrom == null ? DateTime.MinValue : ExecDateFrom;
                    item.DateTo = ExecDateTo == null ? DateTime.MaxValue : ExecDateTo;
                }
                RefreshWindowAfterDateTimeFilter(fltLabel, currentFilter, selectedValue);

                pop_ComboDateTimeFlt_ExecutedSGT.IsOpen = false;
            }
        }

        private void SubmittedSGT_Filter_Apply(object sender, RoutedEventArgs e) {
            ObservableCollection<FilterObj>? currentFilter;
            string fltLabel = pop_ComboDateTimeFlt_SubmittedSGT.Uid;
            _vm.FilterDic.TryGetValue(fltLabel, out currentFilter);
            if (currentFilter != null) {
                string selectedValue;
                DateTime? SubmittedDateFrom = datePicker_SubmittedSGTFrom.SelectedDate;
                DateTime? SubmittedDateTo = datePicker_SubmittedSGTTo.SelectedDate;
                if (SubmittedDateFrom == null && SubmittedDateTo == null) {
                    pop_ComboDateTimeFlt_SubmittedSGT.IsOpen = false;
                    SubmitTime_From.SelectedItem = null;
                    SubmitTime_To.SelectedItem = null;
                    return;
                }
                else {
                    if (SubmittedDateFrom != null) {
                        DateTime dateStartFrom = ((DateTime)SubmittedDateFrom).Date;
                        TimeSpan timeStartFrom = Convert.ToDateTime(SubmitTime_From.SelectedItem).TimeOfDay;
                        SubmittedDateFrom = SubmittedDateFrom == null ? null : dateStartFrom + timeStartFrom;
                    }

                    if (SubmittedDateTo != null) {
                        DateTime dateEndWith = ((DateTime)SubmittedDateTo).Date;
                        TimeSpan timeEndWith = Convert.ToDateTime(SubmitTime_To.SelectedItem).TimeOfDay;
                        SubmittedDateTo = SubmittedDateTo == null ? null : dateEndWith + timeEndWith;
                    }

                    if (SubmittedDateTo == null) {
                        selectedValue = "From: " + SubmittedDateFrom.ToString();
                    }
                    else if (SubmittedDateFrom == null) {
                        selectedValue = "To: " + SubmittedDateTo.ToString();
                    }
                    else {
                        selectedValue = "From: " + SubmittedDateFrom.ToString() + " To: " + SubmittedDateTo.ToString();
                    }
                }
                foreach (var item in currentFilter) {
                    item.DateFrom = SubmittedDateFrom == null ? DateTime.MinValue : SubmittedDateFrom;
                    item.DateTo = SubmittedDateTo == null ? DateTime.MaxValue : SubmittedDateTo;
                }

                RefreshWindowAfterDateTimeFilter(fltLabel, currentFilter, selectedValue);
                pop_ComboDateTimeFlt_SubmittedSGT.IsOpen = false;
            }
        }

        private void ClearedSGT_Filter_Apply(object sender, RoutedEventArgs e) {
            ObservableCollection<FilterObj>? currentFilter;
            string fltLabel = pop_ComboDateTimeFlt_ClearedSGT.Uid;
            _vm.FilterDic.TryGetValue(fltLabel, out currentFilter);
            if (currentFilter != null) {
                string selectedValue;
                DateTime? ClearedSGTFrom = datePicker_ClearedSGTFrom.SelectedDate;
                DateTime? ClearedSGTTo = datePicker_ClearedSGTTo.SelectedDate;
                if (ClearedSGTFrom == null && ClearedSGTTo == null) {
                    pop_ComboDateTimeFlt_ClearedSGT.IsOpen = false;
                    ClearedTime_From.SelectedItem = null;
                    ClearedTime_To.SelectedItem = null;
                    return;
                }
                else {
                    if (ClearedSGTFrom != null) {//ClearedSGTFrom is not null                    
                        DateTime dateStartFrom = ((DateTime)ClearedSGTFrom).Date;
                        TimeSpan timeStartFrom = Convert.ToDateTime(ClearedTime_From.SelectedItem).TimeOfDay;
                        ClearedSGTFrom = ClearedSGTFrom == null ? null : dateStartFrom + timeStartFrom;
                    }

                    if (ClearedSGTTo != null) {//ClearedSGTTo is not null
                        DateTime dateEndWith = ((DateTime)ClearedSGTTo).Date;
                        TimeSpan timeEndWith = Convert.ToDateTime(ClearedTime_To.SelectedItem).TimeOfDay;
                        ClearedSGTTo = ClearedSGTTo == null ? null : dateEndWith + timeEndWith;
                    }

                    if (ClearedSGTTo == null) {
                        selectedValue = "From: " + ClearedSGTFrom.ToString();
                    }
                    else if (ClearedSGTFrom == null) {
                        selectedValue = "To: " + ClearedSGTTo.ToString();
                    }
                    else {
                        selectedValue = "From: " + ClearedSGTFrom.ToString() + " To: " + ClearedSGTTo.ToString();
                    }
                }

                foreach (var item in currentFilter) {
                    item.DateFrom = ClearedSGTFrom == null ? DateTime.MinValue : ClearedSGTFrom;
                    item.DateTo = ClearedSGTTo == null ? DateTime.MaxValue : ClearedSGTTo;
                }
                RefreshWindowAfterDateTimeFilter(fltLabel, currentFilter, selectedValue);
                pop_ComboDateTimeFlt_ClearedSGT.IsOpen = false;
            }
        }

        private void DeletedSGT_Filter_Apply(object sender, RoutedEventArgs e) {
            ObservableCollection<FilterObj>? currentFilter;
            string fltLabel = pop_ComboDateTimeFlt_DeletedSGT.Uid;
            _vm.FilterDic.TryGetValue(fltLabel, out currentFilter);
            if (currentFilter != null) {
                string selectedValue;
                DateTime? DeletedSGTFrom = datePicker_DeletedSGTFrom.SelectedDate;
                DateTime? DeletedSGTTo = datePicker_DeletedSGTTo.SelectedDate;
                if (DeletedSGTFrom == null && DeletedSGTTo == null) {
                    pop_ComboDateTimeFlt_DeletedSGT.IsOpen = false;
                    DeletedTime_From.SelectedItem = null;
                    DeletedTime_To.SelectedItem = null;
                    return;
                }
                else {
                    if (DeletedSGTFrom != null) {
                        DateTime dateStartFrom = ((DateTime)DeletedSGTFrom).Date;
                        TimeSpan timeStartFrom = Convert.ToDateTime(DeletedTime_From.SelectedItem).TimeOfDay;
                        DeletedSGTFrom = DeletedSGTFrom == null ? null : dateStartFrom + timeStartFrom;
                    }

                    if (DeletedSGTTo != null) {
                        DateTime dateEndWith = ((DateTime)DeletedSGTTo).Date;
                        TimeSpan timeEndWith = Convert.ToDateTime(DeletedTime_To.SelectedItem).TimeOfDay;
                        DeletedSGTTo = DeletedSGTTo == null ? null : dateEndWith + timeEndWith;
                    }

                    if (DeletedSGTTo == null) {
                        selectedValue = "From: " + DeletedSGTFrom.ToString();
                    }
                    else if (DeletedSGTFrom == null) {
                        selectedValue = "To: " + DeletedSGTTo.ToString();
                    }
                    else {
                        selectedValue = "From: " + DeletedSGTFrom.ToString() + " To: " + DeletedSGTTo.ToString();
                    }
                }

                foreach (var item in currentFilter) {
                    item.DateFrom = DeletedSGTFrom == null ? DateTime.MinValue : DeletedSGTFrom;
                    item.DateTo = DeletedSGTTo == null ? DateTime.MaxValue : DeletedSGTTo;
                }
                RefreshWindowAfterDateTimeFilter(fltLabel, currentFilter, selectedValue);
                pop_ComboDateTimeFlt_DeletedSGT.IsOpen = false;
            }
        }
        #endregion

        #region datetime filter
        private void ClearedDate_Filter_Apply(object sender, RoutedEventArgs e) {
            ObservableCollection<FilterObj>? currentFilter;
            string fltLabel = pop_DateTimeFlt_ClearedDate.Uid;
            _vm.FilterDic.TryGetValue(fltLabel, out currentFilter);
            if (currentFilter != null) {
                DateTime? clearedDateFrom = datePicker_ClearedDateFrom.SelectedDate;
                DateTime? clearedDateTo = datePicker_ClearedDateTo.SelectedDate;
                string selectedValue;
                if (clearedDateFrom == null && clearedDateTo == null) {
                    pop_DateTimeFlt_ClearedDate.IsOpen = false;
                    return;
                }
                if (clearedDateTo == null) {//clearedDateFrom has value
                    selectedValue = "From: " + ((DateTime)clearedDateFrom).ToString("dd/MM/yyyy");
                }
                else if (clearedDateFrom == null) {//clearedDateTo has value
                    selectedValue = "To: " + ((DateTime)clearedDateTo).ToString("dd/MM/yyyy");
                }
                else {//clearedDateTo and clearedDateFrom both have values
                    if (clearedDateFrom == clearedDateTo) {
                        selectedValue = " " + ((DateTime)clearedDateFrom).ToString("dd/MM/yyyy");
                    }
                    else {
                        selectedValue = "From: " + ((DateTime)clearedDateFrom).ToString("dd/MM/yyyy") + " To: " + ((DateTime)clearedDateTo).ToString("dd/MM/yyyy");
                    }
                }
                foreach (var item in currentFilter) {
                    item.DateFrom = clearedDateFrom == null ? DateTime.MinValue : clearedDateFrom;
                    item.DateTo = clearedDateTo == null ? DateTime.MaxValue : clearedDateTo;
                }
                RefreshWindowAfterDateTimeFilter(fltLabel, currentFilter, selectedValue);
                pop_DateTimeFlt_ClearedDate.IsOpen = false;
            }
        }
        #endregion

        #endregion

        #region Refresh Window after apply filter
        private void RefreshWindowAfterComboBoxFilter(string fltLabel, string filterCtrl, string input, ObservableCollection<FilterObj> currentFilter) {
            if (_vm.FilterDic.ContainsKey(fltLabel)) {
                _vm.FilterDic[fltLabel] = currentFilter;
            }
            else {
                _vm.FilterDic.Add(fltLabel, currentFilter);
            }

            if (Default_Filter_Label.IsVisible == true) {
                Default_Filter_Label.Visibility = Visibility.Collapsed;
                added_FilterDic_Label.Visibility = Visibility.Visible;
            }
            string fltValue = filterCtrl + " \"" + input + "\"";
            _vm.AddDisplayLabel(fltLabel, fltValue);
            Refresh();
        }

        private void RefreshWindowAfterDateTimeFilter(string fltLabel, ObservableCollection<FilterObj> currentFilter, string selectedValue) {
            if (_vm.FilterDic.ContainsKey(fltLabel)) {
                _vm.FilterDic[fltLabel] = currentFilter;
            }
            else {
                _vm.FilterDic.Add(fltLabel, currentFilter);
            }
            _vm.AddDisplayLabel(fltLabel, selectedValue);
            if (Default_Filter_Label.IsVisible == true) {
                Default_Filter_Label.Visibility = Visibility.Collapsed;
                added_FilterDic_Label.Visibility = Visibility.Visible;
            }
            Refresh();
        }

        private void RefreshWindowAfterNumRangeFilter(string fltLabel, double min, double max) {
            ObservableCollection<FilterObj>? currentFilter;
            _vm.FilterDic.TryGetValue(fltLabel, out currentFilter);
            if (currentFilter != null) {
                string selectedValue = "";
                if (min == max) {
                    selectedValue = "=" + min;
                }
                else {
                    if (min != MINNUM) {
                        selectedValue += ">=" + min;
                    }
                    if (max != MAXNUM) {
                        selectedValue += (selectedValue == "") ? ("<=" + max) : (" && " + "<=" + max);
                    }
                }
                foreach (var flt in currentFilter) {
                    flt.MinLimit = min;
                    flt.MaxLimit = max;
                }
                if (Default_Filter_Label.IsVisible == true) {
                    Default_Filter_Label.Visibility = Visibility.Collapsed;
                    added_FilterDic_Label.Visibility = Visibility.Visible;
                }
                _vm.AddDisplayLabel(fltLabel, selectedValue);
                Refresh();
            }
        }

        private void RefreshWindowAfterCheckBoxFilter(string fltLabel, bool? isTopSelectAllChecked) {//need to cleanse the code
            ObservableCollection<FilterObj>? currentFilter;
            _vm.FilterDic.TryGetValue(fltLabel, out currentFilter);
            if (currentFilter != null) {
                string selectedValue = "";
                if (Enum.IsDefined(typeof(CheckboxFiltercolumn), fltLabel)) {
                    bool allChecked = !currentFilter.Any(x => x.IsChecked == false);
                    bool allUnchecked = !currentFilter.Any(x => x.IsChecked == true);

                    if (allChecked && allUnchecked) {// no checkbox option in listbox
                        if (isTopSelectAllChecked == true) {
                            _vm.RemoveDisplayLabel(_vm.PropertyColumnMapping[fltLabel]);
                            _vm.FilterDic.Remove(fltLabel);
                            if (_vm.FilterLabel.Count == 0) {
                                added_FilterDic_Label.Visibility = Visibility.Hidden;
                                Default_Filter_Label.Visibility = Visibility.Visible;
                            }
                        }
                        else if (isTopSelectAllChecked == false) {
                            _vm.AddDisplayLabel(fltLabel, "None");
                        }
                    }
                    else {// have checkbox option in listbox
                        if (allChecked) {
                            _vm.RemoveDisplayLabel(_vm.PropertyColumnMapping[fltLabel]);
                            _vm.FilterDic.Remove(fltLabel);
                            if (_vm.FilterLabel.Count == 0) {
                                added_FilterDic_Label.Visibility = Visibility.Hidden;
                                Default_Filter_Label.Visibility = Visibility.Visible;
                            }
                        }
                        else {
                            if (allUnchecked) {
                                selectedValue = "None";
                            }
                            else {//partial checked
                                foreach (var flt in currentFilter) {
                                    if (flt.IsChecked == true) {
                                        selectedValue = (selectedValue == "") ? ("\"" + flt.Label + "\"") : (selectedValue + " or \"" + flt.Label + "\"");
                                    }
                                }
                                selectedValue = "Equals " + selectedValue;
                            }
                            _vm.AddDisplayLabel(fltLabel, selectedValue);
                            if (Default_Filter_Label.IsVisible == true) {
                                Default_Filter_Label.Visibility = Visibility.Collapsed;
                                added_FilterDic_Label.Visibility = Visibility.Visible;
                            }
                        }
                    }
                    Refresh();
                }
            }
        }

        private void Refresh() {
            if (_vm.MarketDataViewS.View.Filter == null) {
                _vm.MarketDataViewS.Filter += Data_Filter;
            }
            _vm.MarketDataViewS.View.Refresh();
        }
        #endregion

        private void ClosePopUp_Click(object sender, RoutedEventArgs e) {
            Button btn = (Button)sender;
            StackPanel sp = (StackPanel)btn.Parent;
            Grid gd = (Grid)sp.Parent;
            Border bd = (Border)gd.Parent;
            Popup pop = (Popup)bd.Parent;
            pop.IsOpen = false;
        }

        private void ClosePopUp(object sender, EventArgs e) {
            Popup pop = (Popup)sender;
            Border bd = (Border)pop.Child;
            Grid gd = (Grid)bd.Child;
            DockPanel dp = (DockPanel)pop.Parent;
            if (dp != null) {
                foreach (var item in dp.Children) {
                    if (item is ToggleButton tb) {
                        string column = tb.Name;
                        if (Enum.IsDefined(typeof(CheckboxFiltercolumn), column)) {
                            foreach (var component in gd.Children) {
                                if (component is StackPanel stp) {
                                    foreach (var m in stp.Children) {
                                        if (m is CheckBox SelectAllCheckbox) {
                                            SelectAllCheckbox.IsChecked = null;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }

        private void CustomizePopUpClose(object sender, EventArgs e) {
            var visibleColumns = dataGrid.Columns.Where(x => x.Visibility == Visibility.Visible);
            if (visibleColumns.Count() == _vm.MarketColumns.Where(x => x.IsChecked == true).Count()) {
                return;
            }
            int debugIndex = 0;
            foreach (FilterObj item in _vm.MarketColumns) {
                if (visibleColumns.Any(x => _vm.PropertyColumnMapping[x.SortMemberPath] == item.Label)) {
                    item.IsChecked = true;
                    debugIndex++;
                }
                else {
                    item.IsChecked = false;
                }
            }
            if (!_vm.MarketColumns.Any(x => x.IsChecked == false)) {
                //sometimes, if UI cannot be fully loaded in screen, some code-behind event handler(like CheckBox_Update)
                //cannot be triggered
                checkbox_selectAll_CustomizeColumns.IsChecked = true;
            }
        }

        #region Customize feature
        private void CustomizeColumn_Click(object sender, RoutedEventArgs e) {
            pop_CustomizeColumn.IsOpen = true;
        }

        private void UpdateColumn_Click(object sender, RoutedEventArgs e) {
            ObservableCollection<DataGridColumn> item = dataGrid.Columns;
            foreach (FilterObj option in lstColumnOptions.Items) {
                if (option.IsChecked == false) {
                    item.First(x => _vm.PropertyColumnMapping[x.SortMemberPath] == option.Label).Visibility = Visibility.Collapsed;
                }
                else if (option.IsChecked == true) {
                    item.First(x => _vm.PropertyColumnMapping[x.SortMemberPath] == option.Label).Visibility = Visibility.Visible;
                }
            }
            pop_CustomizeColumn.IsOpen = false;
        }
        #endregion

        #region Column enumerator
        public enum Displaycolumn {
            Market,
            Instrument,
            Product,
            Contract,
            ComboType,
            Legs,
            Unit,
            Session,
            Status,
            Price,
            Amount,
            QtyInLots,
            Strike,
            ClearedDate,
            ExecutedSGT,
            SubmittedSGT,
            ClearedSGT,
            DeletedSGT,
            DealID
        }

        public enum ComboTextFilterColumn {
            DealID
        }

        public enum ComboDateTimeFilterColumn {
            ExecutedSGT,
            SubmittedSGT,
            ClearedSGT,
            DeletedSGT
        }

        public enum CheckboxFiltercolumn {
            Market,
            Instrument,
            Product,
            Contract,
            ComboType,
            Legs,
            Unit,
            Session,
            Status
        }

        public enum DateTimeFilterColumn {
            ClearedDate
        }

        public enum NumberRangeFilterColumn {
            Price,
            Amount,
            QtyInLots,
            Strike
        }
        #endregion

        private void textChangedEventHandler(object sender, TextChangedEventArgs e) {
            TextBox tb = (TextBox)sender;
            if (tb != null) {
                string? colName = tb.Tag.ToString();
                if (colName != null) {
                    ObservableCollection<FilterObj>? currentFilter;
                    _vm.FilterDic.TryGetValue(colName, out currentFilter);
                    if (currentFilter != null) {
                        DockPanel dp = (DockPanel)tb.Parent;
                        Grid gd = (Grid)dp.Parent;
                        bool anyValueFltered = true;
                        foreach (var item in gd.Children) {
                            if (item is StackPanel sp) {
                                foreach (var control in sp.Children) {
                                    if (control is ListBox lb) {
                                        lb.ItemsSource = currentFilter.Where(x => x.Label != null && x.Label.IndexOf(tb.Text, System.StringComparison.OrdinalIgnoreCase) >= 0);
                                        if (lb.Items.Count == 0) {
                                            anyValueFltered = false;
                                        }
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                        foreach (var item in gd.Children) {
                            if (item is StackPanel sp) {
                                var items = sp.Children;
                                CheckBox chkSelectAll = (CheckBox)items[0];
                                CheckBox chkSelectAllSearched = (CheckBox)items[1];
                                CheckBox chkAddSelectToCurrFlter = (CheckBox)items[2];
                                if (tb.Text != "") {
                                    if (anyValueFltered) {
                                        chkSelectAll.Visibility = Visibility.Collapsed;
                                        chkSelectAllSearched.Visibility = Visibility.Visible;
                                        chkAddSelectToCurrFlter.Visibility = Visibility.Visible;
                                    }
                                    else {
                                        chkSelectAll.Visibility = Visibility.Collapsed;
                                        chkSelectAllSearched.Visibility = Visibility.Collapsed;
                                        chkAddSelectToCurrFlter.Visibility = Visibility.Collapsed;
                                    }
                                }
                                else {
                                    chkSelectAll.Visibility = Visibility.Visible;
                                    chkSelectAllSearched.Visibility = Visibility.Collapsed;
                                    chkAddSelectToCurrFlter.Visibility = Visibility.Collapsed;
                                }
                                break;
                            }
                        }
                    }
                }

            }
        }

        private void SelectAllSearchedCheckBox_Checked(object sender, RoutedEventArgs e) {

        }

        private void AddSelectionToFilter_Checked(object sender, RoutedEventArgs e) {
            CheckBox chkAddSelection = (CheckBox)sender;
            if (chkAddSelection.IsChecked == true) {

            }
            else if (chkAddSelection.IsChecked == false) {

            }
        }
        private void LostFocus_datagrid(object sender, RoutedEventArgs e) {
            dataGrid.UnselectAll();
        }

        private void ExportToExcel(object sender, RoutedEventArgs e) {
            if (dataGrid.Items.Count > 0) {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV (*.csv)|*.csv";
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sfd.FileName = "Market Report_" + DateTime.Today.ToString("yyyyMMdd") + ".csv";
                bool fileError = false;
                if (sfd.ShowDialog() == true) {
                    if (File.Exists(sfd.FileName)) {
                        try {
                            File.Delete(sfd.FileName);//override previous file(with same name)
                        }
                        catch (IOException ex) {
                            fileError = true;
                            MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
                        }
                    }
                    if (!fileError) {
                        try {
                            dataGrid.SelectAllCells();
                            ApplicationCommands.Copy.Execute(null, dataGrid);
                            dataGrid.UnselectAllCells();
                            string result = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);

                            string columnNames = "";
                            foreach (var column in dataGrid.Columns) {
                                if (column.Visibility == Visibility.Visible) {
                                    DockPanel header = column.Header as DockPanel;
                                    foreach (var item in header.Children) {
                                        if (item is TextBlock tb && !string.IsNullOrEmpty(tb.Text)) {
                                            columnNames += tb.Text + ",";
                                            break;
                                        }
                                    }
                                }
                            }

                            StreamWriter sw = new StreamWriter(sfd.FileName);
                            sw.WriteLine(columnNames);
                            sw.WriteLine(result);
                            sw.Close();

                            MessageBox.Show("Market Data Exported Successfully !!!", "Info");
                        }
                        catch (Exception ex) {
                            MessageBox.Show("Error :" + ex.Message);
                        }
                    }
                }
            }
            else {
                MessageBox.Show("No Record To Export !!!", "Info");
            }
        }
    }
}
