using ICEChecker.ModuleICEData.Models;
using ICEChecker.ModuleICEData.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ICEChecker.ModuleICEData.Views {
    /// <summary>
    /// Interaction logic for ICETCApiView.xaml
    /// </summary>
    public partial class ICETCApiView : UserControl {
        private readonly ICETCApiViewModel _vm;
        private bool isSelectAllOperated = false;
        private readonly int MAXNUM = int.MaxValue;
        private readonly int MINNUM = int.MinValue;
        public ICETCApiView() {
            InitializeComponent();
            ICEAPIDatagrid.UnselectAll();

            _vm = (ICETCApiViewModel)DataContext;
            ICEAPIDatagrid.DataContext = _vm.APICollectionView;
            _vm.APICollectionView.Filter += Data_Filter;
            Loaded += APIFeed_Load;
           
        }
        private void LostFocus_datagrid(object sender, RoutedEventArgs e) {
            ICEAPIDatagrid.UnselectAll();
          
        }

        private void APIFeed_Load(object sender, RoutedEventArgs e) {
            var win = Window.GetWindow(this);
        }

        private void Data_Filter(object sender, FilterEventArgs e) {
            e.Accepted = true;
            if (e.Item is TradeCaptureData p) {
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
                foreach (TradeCaptureData p in _vm.APIData) {
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
                    foreach (TradeCaptureData p in (ObservableCollection<TradeCaptureData>)_vm.APIData) {
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
                    foreach (TradeCaptureData p in (ObservableCollection<TradeCaptureData>)_vm.APIData) {
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
                    case "TrdTime":
                        datePicker_TrdTimeFrom.SelectedDate = null;
                        datePicker_TrdTimeTo.SelectedDate = null;
                        TrdTime_From.SelectedItem = null;
                        TrdTime_To.SelectedItem = null;
                        break;
                    case "TrdDate":
                        datePicker_TrdDateFrom.SelectedDate = null;
                        datePicker_TrdDateTo.SelectedDate = null;
                        break;
                    case "BeginDate":
                        datePicker_BeginDateFrom.SelectedDate = null;
                        datePicker_BeginDateTo.SelectedDate = null;
                        break;
                    case "EndDate":
                        datePicker_EndDateFrom.SelectedDate = null;
                        datePicker_EndDateTo.SelectedDate = null;
                        break;
                    case "Price":
                        MinNum_Price.Text = "";
                        MaxNum_Price.Text = "";
                        break;
                    case "Lots":
                        MinNum_Lots.Text = "";
                        MaxNum_Lots.Text = "";
                        break;
                    case "DealID":
                        DealID_Flt_Input.Text = "";
                        break;
                    case "OrigID":
                        OrigID_Flt_Input.Text = "";
                        break;
                    case "Memo":
                        Memo_Flt_Input.Text = "";
                        break;
                    case "ClearingVenue":
                        ClearingVenue_Flt_Input.Text = "";
                        break;
                    default: break;
                }
                _vm.APICollectionView.View.Filter = null;
                _vm.APICollectionView.Filter += Data_Filter;
                _vm.APICollectionView.View.Refresh();

                //clear sort direction after remove one filter label
                ICollectionView view = CollectionViewSource.GetDefaultView(ICEAPIDatagrid.ItemsSource);
                if (view != null) {
                    foreach (DataGridColumn column in ICEAPIDatagrid.Columns) {
                        column.SortDirection = null;
                    }
                }
            }
        }

        private void Reset_CurrFlts(object sender, RoutedEventArgs e) {
            _vm.APICollectionView.View.Filter = null;
            _vm.APICollectionView.View.Refresh();

            #region //clear previous selection
            DealID_Flt_Input.Text = "";
            OrigID_Flt_Input.Text = "";
            Memo_Flt_Input.Text = "";
            ClearingVenue_Flt_Input.Text = "";

            Checkbox_selectAll_Status.IsChecked = true;
            Checkbox_selectAll_B_S.IsChecked = true;
            Checkbox_selectAll_Source.IsChecked = true;
            Checkbox_selectAll_Product.IsChecked = true;
            Checkbox_selectAll_Hub.IsChecked = true;
            Checkbox_selectAll_Contract.IsChecked = true;
            Checkbox_selectAll_ClearingAcct.IsChecked = true;
            Checkbox_selectAll_ClearingFirm.IsChecked = true;
            Checkbox_selectAll_PriceUnit.IsChecked = true;
            Checkbox_selectAll_QtyUnits.IsChecked = true;
            Checkbox_selectAll_TrdType.IsChecked = true;
            Checkbox_selectAll_BRK.IsChecked = true;
            Checkbox_selectAll_BrokerFirm.IsChecked = true;
            Checkbox_selectAll_Trader.IsChecked = true;
            Checkbox_selectAll_UserID.IsChecked = true;

            MinNum_Price.Text = "";
            MaxNum_Price.Text = "";
            MinNum_Lots.Text = "";
            MaxNum_Lots.Text = "";

            datePicker_TrdDateFrom.SelectedDate = null;
            datePicker_TrdDateTo.SelectedDate = null;
            datePicker_BeginDateFrom.SelectedDate = null;
            datePicker_BeginDateTo.SelectedDate = null;
            datePicker_EndDateFrom.SelectedDate = null;
            datePicker_EndDateTo.SelectedDate = null;

            datePicker_TrdTimeFrom.SelectedDate = null;
            datePicker_TrdTimeTo.SelectedDate = null;
            TrdTime_From.SelectedItem = null;
            TrdTime_To.SelectedItem = null;

            #endregion //clear previous selection

            #region //clear current filter label
            _vm.FilterLabel.Clear();
            _vm.FilterDic.Clear();
            added_FilterDic_Label.Visibility = Visibility.Hidden;
            Default_Filter_Label.Visibility = Visibility.Visible;
            #endregion //clear current filter label

            #region //clear sort direction
            ICollectionView view = CollectionViewSource.GetDefaultView(ICEAPIDatagrid.ItemsSource);
            if (view != null && view.SortDescriptions.Count > 0) {
                view.SortDescriptions.Clear();
                foreach (DataGridColumn column in ICEAPIDatagrid.Columns) {
                    column.SortDirection = null;
                }
            }
            #endregion //clear sort direction

            #region // re-display all columns    
            checkbox_selectAll_CustomizeColumns.IsChecked = true;
            ICEAPIDatagrid.Columns.ToList().ForEach(x => x.Visibility = Visibility.Visible);
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
                else if (valueName == "MinNum_Lots") {
                    if (MinNum_Lots.Text == "") {
                        MinNum_Lots.Text = "0";
                    }
                    double Num;
                    bool isNumber = double.TryParse(MinNum_Lots.Text, out Num);
                    if (!isNumber) {
                        MessageBox.Show("Input must be numeric"); return;
                    }
                    Num = (int)Num;
                    MinNum_Lots.Text = (Num + 1).ToString();
                }
                else if (valueName == "MaxNum_Lots") {
                    if (MaxNum_Lots.Text == "") {
                        MaxNum_Lots.Text = "0";
                    }
                    double Num;
                    bool isNumber = double.TryParse(MaxNum_Lots.Text, out Num);
                    if (!isNumber) {
                        MessageBox.Show("Input must be numeric"); return;
                    }
                    Num = (int)Num;
                    MaxNum_Lots.Text = (Num + 1).ToString();
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
                        else if (valueName == "MinNum_Lots") {
                            if (MinNum_Lots.Text == "") {
                                MinNum_Lots.Text = "0";
                            }
                            double Num;
                            bool isNumber = double.TryParse(MinNum_Lots.Text, out Num);
                            if (!isNumber) {
                                MessageBox.Show("Input must be numeric"); return;
                            }
                            Num = (int)Num;
                            MinNum_Lots.Text = (Num - 1).ToString();
                        }
                        else if (valueName == "MaxNum_Lots") {
                            if (MaxNum_Lots.Text == "") {
                                MaxNum_Lots.Text = "0";
                            }
                            double Num;
                            bool isNumber = double.TryParse(MaxNum_Lots.Text, out Num);
                            if (!isNumber) {
                                MessageBox.Show("Input must be numeric"); return;
                            }
                            Num = (int)Num;
                            MaxNum_Lots.Text = (Num - 1).ToString();
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
            IEnumerable<TradeCaptureData> filteredValue;
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
        private void OrigID_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_OrigID.Uid;
            string input = OrigID_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeCaptureData> filteredValue;
            string filterCtrl = (string)TextFltOptions_OrigID.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.OrigID)) {
                    currentFilter.Add(new FilterObj() { Label = item.OrigID });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_OrigID.IsOpen = false;
        }
        private void Memo_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_Memo.Uid;
            string input = Memo_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeCaptureData> filteredValue;
            string filterCtrl = (string)TextFltOptions_Memo.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.Memo)) {
                    currentFilter.Add(new FilterObj() { Label = item.Memo });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_Memo.IsOpen = false;
        }
        private void ClearingVenue_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_ClearingVenue.Uid;
            string input = ClearingVenue_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeCaptureData> filteredValue;
            string filterCtrl = (string)TextFltOptions_ClearingVenue.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.ClearingVenue)) {
                    currentFilter.Add(new FilterObj() { Label = item.ClearingVenue });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_ClearingVenue.IsOpen = false;
        }
        private IEnumerable<TradeCaptureData> ComboxBoxFilter(string fltLabel, string filterCtrl, string input) {
            IEnumerable<TradeCaptureData> filteredValue;

            switch (filterCtrl) {
                case "Contains":
                    filteredValue = _vm.APIData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper().Contains(input.ToUpper()));
                    break;
                case "Starts With":
                    filteredValue = _vm.APIData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper().StartsWith(input.ToUpper()));
                    break;
                case "Ends With":
                    filteredValue = _vm.APIData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper().EndsWith(input.ToUpper()));
                    break;
                case "Equals":
                    filteredValue = _vm.APIData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper() == input.ToUpper());
                    break;
                case "Does Not Contain":
                    filteredValue = _vm.APIData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper().Contains(input.ToUpper()) == false);
                    break;
                default:
                    filteredValue = _vm.APIData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper().Contains(input.ToUpper()));
                    break;
            }
            return filteredValue;

        }
        #endregion

        #region checkbox filter
        private void Status_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_Status.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_Status.IsOpen = false;
        }
        private void B_S_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_B_S.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_B_S.IsOpen = false;
        }
        private void Source_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_Source.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_Source.IsOpen = false;
        }
        private void Product_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_Product.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_Product.IsOpen = false;
        }
        private void Hub_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_Hub.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_Hub.IsOpen = false;
        }
        private void Contract_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_Contract.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_Contract.IsOpen = false;
        }
        private void ClearingAcct_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_ClearingAcct.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_ClearingAcct.IsOpen = false;
        }
        private void ClearingFirm_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_ClearingFirm.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_ClearingFirm.IsOpen = false;
        }
        private void PriceUnit_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_PriceUnit.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_PriceUnit.IsOpen = false;
        }
        private void QtyUnits_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_QtyUnits.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_QtyUnits.IsOpen = false;
        }
        private void TrdType_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_TrdType.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_TrdType.IsOpen = false;
        }
        private void BRK_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_BRK.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_BRK.IsOpen = false;
        }
        private void BrokerFirm_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_BrokerFirm.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_BrokerFirm.IsOpen = false;
        }
        private void Trader_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_Trader.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_Trader.IsOpen = false;
        }
        private void UserID_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_UserID.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_UserID.IsOpen = false;
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
        private void Lots_Filter_Apply(object sender, RoutedEventArgs e) {
            if (MinNum_Lots.Text == "" && MaxNum_Lots.Text == "") {
                return;
            }
            string fltLabel = pop_RangeFlt_Lots.Uid;
            double parseOutput;
            if (!double.TryParse(MinNum_Lots.Text, out parseOutput) && MinNum_Lots.Text != "") {
                MessageBox.Show("Input must be numeric"); return;
            }
            if (!double.TryParse(MaxNum_Lots.Text, out parseOutput) && MaxNum_Lots.Text != "") {
                MessageBox.Show("Input must be numeric"); return;
            }
            double min = MinNum_Lots.Text != "" ? Convert.ToDouble(MinNum_Lots.Text) : MINNUM;
            double max = MaxNum_Lots.Text != "" ? Convert.ToDouble(MaxNum_Lots.Text) : MAXNUM;
            if (min > max) {
                MessageBox.Show("Quantity In Lots's Minimum value cannot be greater than Maximum value!");
                return;
            }
            RefreshWindowAfterNumRangeFilter(fltLabel, min, max);
            pop_RangeFlt_Lots.IsOpen = false;
        }
        #endregion

        #region combo and datetime filter
        private void TrdTime_Filter_Apply(object sender, RoutedEventArgs e) {
            ObservableCollection<FilterObj>? currentFilter;
            string fltLabel = pop_ComboDateTimeFlt_TrdTime.Uid;
            _vm.FilterDic.TryGetValue(fltLabel, out currentFilter);
            if (currentFilter != null) {
                string selectedValue;
                DateTime? TrdDateFrom = datePicker_TrdTimeFrom.SelectedDate;
                DateTime? TrdDateTo = datePicker_TrdTimeTo.SelectedDate;
                if (TrdDateFrom == null && TrdDateTo == null) {
                    pop_ComboDateTimeFlt_TrdTime.IsOpen = false;
                    TrdTime_From.SelectedItem = null;
                    TrdTime_To.SelectedItem = null;
                    return;
                }
                else {
                    if (TrdDateFrom != null) {
                        DateTime dateStartFrom = ((DateTime)TrdDateFrom).Date;
                        TimeSpan timeStartFrom = Convert.ToDateTime(TrdTime_From.SelectedItem).TimeOfDay;
                        TrdDateFrom = TrdDateFrom == null ? null : dateStartFrom + timeStartFrom;
                    }

                    if (TrdDateTo != null) {
                        DateTime dateEndWith = ((DateTime)TrdDateTo).Date;
                        TimeSpan timeEndWith = Convert.ToDateTime(TrdTime_To.SelectedItem).TimeOfDay;
                        TrdDateTo = TrdDateTo == null ? null : dateEndWith + timeEndWith;
                    }

                    if (TrdDateTo == null) {
                        selectedValue = "From: " + TrdDateFrom.ToString();
                    }
                    else if (TrdDateFrom == null) {
                        selectedValue = "To: " + TrdDateTo.ToString();
                    }
                    else {
                        selectedValue = "From: " + TrdDateFrom.ToString() + " To: " + TrdDateTo.ToString();
                    }
                }

                foreach (var item in currentFilter) {
                    item.DateFrom = TrdDateFrom == null ? DateTime.MinValue : TrdDateFrom;
                    item.DateTo = TrdDateTo == null ? DateTime.MaxValue : TrdDateTo;
                }
                RefreshWindowAfterDateTimeFilter(fltLabel, currentFilter, selectedValue);

                pop_ComboDateTimeFlt_TrdTime.IsOpen = false;
            }
        }
        #endregion

        #region datetime filter
        private void TrdDate_Filter_Apply(object sender, RoutedEventArgs e) {
            ObservableCollection<FilterObj>? currentFilter;
            string fltLabel = pop_DateTimeFlt_TrdDate.Uid;
            _vm.FilterDic.TryGetValue(fltLabel, out currentFilter);
            if (currentFilter != null) {
                DateTime? TrdDateFrom = datePicker_TrdDateFrom.SelectedDate;
                DateTime? TrdDateTo = datePicker_TrdDateTo.SelectedDate;
                string selectedValue;
                if (TrdDateFrom == null && TrdDateTo == null) {
                    pop_DateTimeFlt_TrdDate.IsOpen = false;
                    return;
                }
                if (TrdDateTo == null) {//TrdDateFrom has value
                    selectedValue = "From: " + ((DateTime)TrdDateFrom).ToString("dd/MM/yyyy");
                }
                else if (TrdDateFrom == null) {//TrdDateTo has value
                    selectedValue = "To: " + ((DateTime)TrdDateTo).ToString("dd/MM/yyyy");
                }
                else {//TrdDateTo and TrdDateFrom both have values
                    if (TrdDateFrom == TrdDateTo) {
                        selectedValue = " " + ((DateTime)TrdDateFrom).ToString("dd/MM/yyyy");
                    }
                    else {
                        selectedValue = "From: " + ((DateTime)TrdDateFrom).ToString("dd/MM/yyyy") + " To: " + ((DateTime)TrdDateTo).ToString("dd/MM/yyyy");
                    }
                }
                foreach (var item in currentFilter) {
                    item.DateFrom = TrdDateFrom == null ? DateTime.MinValue : TrdDateFrom;
                    item.DateTo = TrdDateTo == null ? DateTime.MaxValue : TrdDateTo;
                }
                RefreshWindowAfterDateTimeFilter(fltLabel, currentFilter, selectedValue);
                pop_DateTimeFlt_TrdDate.IsOpen = false;
            }
        }
        private void BeginDate_Filter_Apply(object sender, RoutedEventArgs e) {
            ObservableCollection<FilterObj>? currentFilter;
            string fltLabel = pop_DateTimeFlt_BeginDate.Uid;
            _vm.FilterDic.TryGetValue(fltLabel, out currentFilter);
            if (currentFilter != null) {
                DateTime? BeginDateFrom = datePicker_BeginDateFrom.SelectedDate;
                DateTime? BeginDateTo = datePicker_BeginDateTo.SelectedDate;
                string selectedValue;
                if (BeginDateFrom == null && BeginDateTo == null) {
                    pop_DateTimeFlt_BeginDate.IsOpen = false;
                    return;
                }
                if (BeginDateTo == null) {//BeginDateFrom has value
                    selectedValue = "From: " + ((DateTime)BeginDateFrom).ToString("dd/MM/yyyy");
                }
                else if (BeginDateFrom == null) {//BeginDateTo has value
                    selectedValue = "To: " + ((DateTime)BeginDateTo).ToString("dd/MM/yyyy");
                }
                else {//BeginDateTo and BeginDateFrom both have values
                    if (BeginDateFrom == BeginDateTo) {
                        selectedValue = " " + ((DateTime)BeginDateFrom).ToString("dd/MM/yyyy");
                    }
                    else {
                        selectedValue = "From: " + ((DateTime)BeginDateFrom).ToString("dd/MM/yyyy") + " To: " + ((DateTime)BeginDateTo).ToString("dd/MM/yyyy");
                    }
                }
                foreach (var item in currentFilter) {
                    item.DateFrom = BeginDateFrom == null ? DateTime.MinValue : BeginDateFrom;
                    item.DateTo = BeginDateTo == null ? DateTime.MaxValue : BeginDateTo;
                }
                RefreshWindowAfterDateTimeFilter(fltLabel, currentFilter, selectedValue);
                pop_DateTimeFlt_BeginDate.IsOpen = false;
            }
        }
        private void EndDate_Filter_Apply(object sender, RoutedEventArgs e) {
            ObservableCollection<FilterObj>? currentFilter;
            string fltLabel = pop_DateTimeFlt_EndDate.Uid;
            _vm.FilterDic.TryGetValue(fltLabel, out currentFilter);
            if (currentFilter != null) {
                DateTime? EndDateFrom = datePicker_EndDateFrom.SelectedDate;
                DateTime? EndDateTo = datePicker_EndDateTo.SelectedDate;
                string selectedValue;
                if (EndDateFrom == null && EndDateTo == null) {
                    pop_DateTimeFlt_EndDate.IsOpen = false;
                    return;
                }
                if (EndDateTo == null) {//EndDateFrom has value
                    selectedValue = "From: " + ((DateTime)EndDateFrom).ToString("dd/MM/yyyy");
                }
                else if (EndDateFrom == null) {//EndDateTo has value
                    selectedValue = "To: " + ((DateTime)EndDateTo).ToString("dd/MM/yyyy");
                }
                else {//EndDateTo and EndDateFrom both have values
                    if (EndDateFrom == EndDateTo) {
                        selectedValue = " " + ((DateTime)EndDateFrom).ToString("dd/MM/yyyy");
                    }
                    else {
                        selectedValue = "From: " + ((DateTime)EndDateFrom).ToString("dd/MM/yyyy") + " To: " + ((DateTime)EndDateTo).ToString("dd/MM/yyyy");
                    }
                }
                foreach (var item in currentFilter) {
                    item.DateFrom = EndDateFrom == null ? DateTime.MinValue : EndDateFrom;
                    item.DateTo = EndDateTo == null ? DateTime.MaxValue : EndDateTo;
                }
                RefreshWindowAfterDateTimeFilter(fltLabel, currentFilter, selectedValue);
                pop_DateTimeFlt_EndDate.IsOpen = false;
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
            if (_vm.APICollectionView.View.Filter == null) {
                _vm.APICollectionView.Filter += Data_Filter;
            }
            _vm.APICollectionView.View.Refresh();
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
            var visibleColumns = ICEAPIDatagrid.Columns.Where(x => x.Visibility == Visibility.Visible);
            if (visibleColumns.Count() == _vm.APIFeedColumns.Where(x => x.IsChecked == true).Count()) {
                return;
            }
            int debugIndex = 0;
            foreach (FilterObj item in _vm.APIFeedColumns) {
                try {
                    if (visibleColumns.Any(x => _vm.PropertyColumnMapping[x.SortMemberPath] == item.Label)) {
                        item.IsChecked = true;
                        debugIndex++;
                    }
                    else {
                        item.IsChecked = false;
                    }
                }
                catch { }
            }
            if (!_vm.APIFeedColumns.Any(x => x.IsChecked == false)) {
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
            ObservableCollection<DataGridColumn> item = ICEAPIDatagrid.Columns;
            foreach (FilterObj option in lstColumnOptions.Items) {
                if (option.IsChecked == false) {
                    try { item.First(x => _vm.PropertyColumnMapping[x.SortMemberPath] == option.Label).Visibility = Visibility.Collapsed; }
                    catch(Exception ex) { Debug.WriteLine(ex); }
                }
                else if (option.IsChecked == true) {
                    try { item.First(x => _vm.PropertyColumnMapping[x.SortMemberPath] == option.Label).Visibility = Visibility.Visible; }
                    catch { }
                }
            }
            pop_CustomizeColumn.IsOpen = false;
        }
        #endregion

        #region Column enumerator
        public enum Displaycolumn {
            DealID,
            OrigID,
            Memo,
            ClearingVenue,
            TrdDate,
            BeginDate,
            EndDate,
            Status,
            B_S,
            Source,
            Product,
            Hub,
            Contract,
            ClearingAcct,
            ClearingFirm,
            PriceUnit,
            QtyUnits,
            TrdType,
            BRK,
            BrokerFirm,
            Trader,
            UserID,
            TrdTime,
            Price,
            Lots
        }

        public enum ComboTextFilterColumn {
            DealID,
            OrigID,
            Memo,
            ClearingVenue
        }

        public enum ComboDateTimeFilterColumn {
            TrdTime
        }

        public enum CheckboxFiltercolumn {
            Status,
            B_S,
            Source,
            Product,
            Hub,
            Contract,
            ClearingAcct,
            ClearingFirm,
            PriceUnit,
            QtyUnits,
            TrdType,
            BRK,
            BrokerFirm,
            Trader,
            UserID
        }

        public enum DateTimeFilterColumn {
            TrdDate,
            BeginDate,
            EndDate
        }

        public enum NumberRangeFilterColumn {
            Price,
            Lots,
        }
        #endregion

        #region download function
        private void ExportToExcel(object sender, RoutedEventArgs e) {
            if (ICEAPIDatagrid.Items.Count > 0) {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV (*.csv)|*.csv";
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sfd.FileName = "API Feed Report_" + DateTime.Today.ToString("yyyyMMdd") + ".csv";
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
                            ICEAPIDatagrid.SelectAllCells();
                            ApplicationCommands.Copy.Execute(null, ICEAPIDatagrid);
                            ICEAPIDatagrid.UnselectAllCells();
                            string result = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);

                            string columnNames = "";
                            foreach (var column in ICEAPIDatagrid.Columns) {
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

                            MessageBox.Show("API Feed Data Exported Successfully !!!", "Info");
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
        #endregion

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count == 0) { return; }
            foreach (var item in e.AddedItems) {
                if (item is TradeCaptureData) {
                    _vm.DisplayDealDetails((TradeCaptureData)item);
                }
            }
        }
    }
}
