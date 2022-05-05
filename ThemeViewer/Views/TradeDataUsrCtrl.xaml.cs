using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Interaction logic for TradeDataUsrCtrl.xaml
    /// </summary>    
    public partial class TradeDataUsrCtrl : UserControl {
        private readonly TradeDataVM _vm;
        private readonly int MAXNUM = int.MaxValue;
        private readonly int MINNUM = int.MinValue;
        private readonly CollectionViewSource _collectionVS;
        private bool isSelectAllOperated = false;

        public TradeDataUsrCtrl() {
            InitializeComponent();
            _vm = new TradeDataVM(ThmViewerWin.EA);
            DataContext = _vm;

            _collectionVS = _vm.TradeDataViewS;
            _collectionVS.Filter += Data_Filter;

            Loaded += TradeDataUsrCtrl_Loaded;
        }

        private void TradeDataUsrCtrl_Loaded(object sender, RoutedEventArgs e) {
            var win = Window.GetWindow(this);
            win.Closing += Win_Closing;
        }

        private void Win_Closing(object sender, CancelEventArgs e) {
            _vm.Dispose();
        }

        private void Data_Filter(object sender, FilterEventArgs e) {
            e.Accepted = true;
            if (e.Item is TradeRecord p) {
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
                                    else if (Enum.IsDefined(typeof(DateTimeFilterColumn), colName)) {
                                        if (string.IsNullOrEmpty(value)) {
                                            e.Accepted = false;
                                            return;
                                        }
                                        DateTime from = currentFilter[0].DateFrom == null ? DateTime.MinValue : (DateTime)currentFilter[0].DateFrom;
                                        DateTime to = currentFilter[0].DateTo == null ? DateTime.MaxValue : (DateTime)currentFilter[0].DateTo;
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
                //currentFilter = new ObservableCollection<FilterObj>();
                foreach (TradeRecord model in _collectionVS.View) {
                    TradeRecord p = model;
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
                    foreach (var model in (ObservableCollection<TradeRecord>)_collectionVS.Source) {
                        TradeRecord p = model;
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
            else if (Enum.IsDefined(typeof(DateTimeFilterColumn), colName)) {
                if (!_vm.FilterDic.ContainsKey(colName)) {
                    //currentFilter = new ObservableCollection<FilterObj>();
                    foreach (var model in (ObservableCollection<TradeRecord>)_collectionVS.Source) {
                        TradeRecord p = model;
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
                else if (valueName == "MinNum_QtyInUnits") {
                    if (MinNum_QtyInUnits.Text == "") {
                        MinNum_QtyInUnits.Text = "0";
                    }
                    double Num;
                    bool isNumber = double.TryParse(MinNum_QtyInUnits.Text, out Num);
                    if (!isNumber) {
                        MessageBox.Show("Input must be numeric"); return;
                    }
                    Num = (int)Num;
                    MinNum_QtyInUnits.Text = (Num + 1).ToString();
                }
                else if (valueName == "MaxNum_QtyInUnits") {
                    if (MaxNum_QtyInUnits.Text == "") {
                        MaxNum_QtyInUnits.Text = "0";
                    }
                    double Num;
                    bool isNumber = double.TryParse(MaxNum_QtyInUnits.Text, out Num);
                    if (!isNumber) {
                        MessageBox.Show("Input must be numeric"); return;
                    }
                    Num = (int)Num;
                    MaxNum_QtyInUnits.Text = (Num + 1).ToString();
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
            }
        }

        private void NUDButtonDown_Click(object sender, RoutedEventArgs e) {
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
                else if (valueName == "MinNum_QtyInUnits") {
                    if (MinNum_QtyInUnits.Text == "") {
                        MinNum_QtyInUnits.Text = "0";
                    }
                    double Num;
                    bool isNumber = double.TryParse(MinNum_QtyInUnits.Text, out Num);
                    if (!isNumber) {
                        MessageBox.Show("Input must be numeric"); return;
                    }
                    Num = (int)Num;
                    MinNum_QtyInUnits.Text = (Num - 1).ToString();
                }
                else if (valueName == "MaxNum_QtyInUnits") {
                    if (MaxNum_QtyInUnits.Text == "") {
                        MaxNum_QtyInUnits.Text = "0";
                    }
                    double Num;
                    bool isNumber = double.TryParse(MaxNum_QtyInUnits.Text, out Num);
                    if (!isNumber) {
                        MessageBox.Show("Input must be numeric"); return;
                    }
                    Num = (int)Num;
                    MaxNum_QtyInUnits.Text = (Num - 1).ToString();
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
            }
        }
        #endregion


        #region Close one filter label
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
                    case "TradeID":
                        TradeID_Flt_Input.Text = "";
                        break;
                    case "DealID":
                        DealID_Flt_Input.Text = "";
                        break;
                    case "SecurityID":
                        SecurityID_Flt_Input.Text = "";
                        break;
                    case "BuyerAccount":
                        BuyerAccount_Flt_Input.Text = "";
                        break;
                    case "BuyerComment":
                        BuyerComment_Flt_Input.Text = "";
                        break;
                    case "BuyerInternalTrdID":
                        BuyerInternalTrdID_Flt_Input.Text = "";
                        break;
                    case "BuyerTrader":
                        BuyerTrader_Flt_Input.Text = "";
                        break;
                    case "BuySource":
                        BuySource_Flt_Input.Text = "";
                        break;
                    case "BuyerSecurID":
                        BuyerSecurID_Flt_Input.Text = "";
                        break;
                    case "SellerAccount":
                        SellerAccount_Flt_Input.Text = "";
                        break;
                    case "SellerComment":
                        SellerComment_Flt_Input.Text = "";
                        break;
                    case "SellerInternalTrdID":
                        SellerInternalTrdID_Flt_Input.Text = "";
                        break;
                    case "SellerTrader":
                        SellerTrader_Flt_Input.Text = "";
                        break;
                    case "SellSource":
                        SellSource_Flt_Input.Text = "";
                        break;
                    case "SellerSecurID":
                        SellerSecurID_Flt_Input.Text = "";
                        break;
                    case "ClearMsg":
                        ClearMsg_Flt_Input.Text = "";
                        break;
                    case "USI":
                        USI_Flt_Input.Text = "";
                        break;
                    case "TradeRegistrationDateTime":
                        datePicker_TrdRegistFrom.SelectedDate = null;
                        datePicker_TrdRegistTo.SelectedDate = null;
                        break;
                    case "ApprovalDateTime":
                        datePicker_ApprovalFrom.SelectedDate = null;
                        datePicker_ApprovalTTo.SelectedDate = null;
                        break;
                    case "ClearedDate":
                        datePicker_ClearedDateFrom.SelectedDate = null;
                        datePicker_ClearedDateTo.SelectedDate = null;
                        break;
                    case "ExpireDate":
                        datePicker_ExpireDateFrom.SelectedDate = null;
                        datePicker_ExpireDateTo.SelectedDate = null;
                        break;
                    case "TrdExecDateTime":
                        datePicker_TrdExecFrom.SelectedDate = null;
                        datePicker_TrdExecTo.SelectedDate = null;
                        break;
                    case "Price":
                        MinNum_Price.Text = "";
                        MaxNum_Price.Text = "";
                        break;
                    case "QtyInLots":
                        MinNum_QtyInLots.Text = "";
                        MaxNum_QtyInLots.Text = "";
                        break;
                    case "QtyInUnits":
                        MinNum_QtyInUnits.Text = "";
                        MaxNum_QtyInUnits.Text = "";
                        break;
                    case "Strike":
                        MinNum_Strike.Text = "";
                        MaxNum_Strike.Text = "";
                        break;
                    default: break;
                }
                _collectionVS.View.Filter = null;
                _collectionVS.Filter += Data_Filter;
                _collectionVS.View.Refresh();

                //clear sort direction after remove one filter label
                ICollectionView view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
                if (view != null) {
                    foreach (DataGridColumn column in dataGrid.Columns) {
                        column.SortDirection = null;
                    }
                }
            }
        }
        #endregion

        #region Apply filter
        #region datetime filter
        private void TrdRegistDT_Filter_Apply(object sender, RoutedEventArgs e) {
            ObservableCollection<FilterObj>? currentFilter;
            string fltLabel = pop_DateTimeFlt_TrdRegistration.Uid;
            _vm.FilterDic.TryGetValue(fltLabel, out currentFilter);
            if (currentFilter != null) {
                DateTime? trdRegistFrom = datePicker_TrdRegistFrom.SelectedDate;
                DateTime? trdRegistTo = datePicker_TrdRegistTo.SelectedDate;
                string selectedValue;
                if (trdRegistFrom == null && trdRegistTo == null) {
                    pop_DateTimeFlt_TrdRegistration.IsOpen = false;
                    return;
                }
                if (trdRegistTo == null) {//trdregistFrom has value
                    selectedValue = "From: " + ((DateTime)trdRegistFrom).ToString("yyyy-MMM-dd");
                }
                else if (trdRegistFrom == null) {//trdRegistTo has value
                    selectedValue = "To: " + ((DateTime)trdRegistTo).ToString("yyyy-MMM-dd");
                }
                else {//trdRegistTo and trdRegistFrom both have values
                    if (trdRegistFrom == (DateTime)trdRegistTo) {
                        selectedValue = " " + ((DateTime)trdRegistFrom).ToString("yyyy-MMM-dd");
                    }
                    else {
                        selectedValue = "From: " + ((DateTime)trdRegistFrom).ToString("yyyy-MMM-dd") + " To: " + ((DateTime)trdRegistTo).ToString("yyyy-MMM-dd");
                    }
                }
                foreach (var item in currentFilter) {
                    item.DateFrom = trdRegistFrom == null ? DateTime.MinValue : trdRegistFrom;
                    item.DateTo = trdRegistTo == null ? DateTime.MaxValue : ((DateTime)trdRegistTo).AddDays(1);
                }
                RefreshWindowAfterDateTimeFilter(fltLabel, currentFilter, selectedValue);
                pop_DateTimeFlt_TrdRegistration.IsOpen = false;
            }
        }

        private void ClearedDate_Filter_Apply(object sender, RoutedEventArgs e) {
            ObservableCollection<FilterObj>? currentFilter;
            string fltLabel = pop_DateTimeFlt_ClearedDate.Uid;
            _vm.FilterDic.TryGetValue(fltLabel, out currentFilter);
            if (currentFilter != null) {
                DateTime? ClearedFrom = datePicker_ClearedDateFrom.SelectedDate;
                DateTime? ClearedTo = datePicker_ClearedDateTo.SelectedDate;
                string selectedValue;
                if (ClearedFrom == null && ClearedTo == null) {
                    pop_DateTimeFlt_ClearedDate.IsOpen = false;
                    return;
                }
                if (ClearedTo == null) {//ClearedFrom has value
                    selectedValue = "From: " + ((DateTime)ClearedFrom).ToString("yyyy-MMM-dd");
                }
                else if (ClearedFrom == null) {//ClearedTo has value              
                    selectedValue = "To: " + ((DateTime)ClearedTo).ToString("yyyy-MMM-dd");
                }
                else {//ClearedTo and ClearedFrom both have values
                    if (ClearedFrom == (DateTime)ClearedTo) {
                        selectedValue = " " + ((DateTime)ClearedFrom).ToString("yyyy-MMM-dd");
                    }
                    else {
                        selectedValue = "From: " + ((DateTime)ClearedFrom).ToString("yyyy-MMM-dd") + " To: " + ((DateTime)ClearedTo).ToString("yyyy-MMM-dd");
                    }
                }
                foreach (var item in currentFilter) {
                    item.DateFrom = ClearedFrom == null ? DateTime.MinValue : ClearedFrom;
                    item.DateTo = ClearedTo == null ? DateTime.MaxValue : ClearedTo;
                }
                RefreshWindowAfterDateTimeFilter(fltLabel, currentFilter, selectedValue);
                pop_DateTimeFlt_ClearedDate.IsOpen = false;
            }
        }

        private void ExpireDate_Filter_Apply(object sender, RoutedEventArgs e) {
            ObservableCollection<FilterObj>? currentFilter;
            string fltLabel = pop_DateTimeFlt_ExpireDate.Uid;
            _vm.FilterDic.TryGetValue(fltLabel, out currentFilter);
            if (currentFilter != null) {
                DateTime? ExpiredFrom = datePicker_ExpireDateFrom.SelectedDate;
                DateTime? ExpiredTo = datePicker_ExpireDateTo.SelectedDate;
                string selectedValue;
                if (ExpiredFrom == null && ExpiredTo == null) {
                    pop_DateTimeFlt_ExpireDate.IsOpen = false;
                    return;
                }
                if (ExpiredTo == null) {//ExpiredFrom has value
                    selectedValue = "From: " + ((DateTime)ExpiredFrom).ToString("yyyy-MMM-dd");
                }
                else if (ExpiredFrom == null) {//ExpiredTo has value              
                    selectedValue = "To: " + ((DateTime)ExpiredTo).ToString("yyyy-MMM-dd");
                }
                else {//ExpiredTo and ExpiredFrom both have values
                    if (ExpiredFrom == (DateTime)ExpiredTo) {
                        selectedValue = " " + ((DateTime)ExpiredFrom).ToString("yyyy-MMM-dd");
                    }
                    else {
                        selectedValue = "From: " + ((DateTime)ExpiredFrom).ToString("yyyy-MMM-dd") + " To: " + ((DateTime)ExpiredTo).ToString("yyyy-MMM-dd");
                    }
                }
                foreach (var item in currentFilter) {
                    item.DateFrom = ExpiredFrom == null ? DateTime.MinValue : ExpiredFrom;
                    item.DateTo = ExpiredTo == null ? DateTime.MaxValue : ExpiredTo;
                }
                RefreshWindowAfterDateTimeFilter(fltLabel, currentFilter, selectedValue);
                pop_DateTimeFlt_ExpireDate.IsOpen = false;
            }
        }

        private void ApprovalDT_Filter_Apply(object sender, RoutedEventArgs e) {
            ObservableCollection<FilterObj>? currentFilter;
            string fltLabel = pop_DateTimeFlt_ApprovalDateTime.Uid;
            _vm.FilterDic.TryGetValue(fltLabel, out currentFilter);
            if (currentFilter != null) {
                DateTime? ApproveFrom = datePicker_ApprovalFrom.SelectedDate;
                DateTime? ApproveTo = datePicker_ApprovalTTo.SelectedDate;
                string selectedValue;
                if (ApproveFrom == null && ApproveTo == null) {
                    pop_DateTimeFlt_ApprovalDateTime.IsOpen = false;
                    return;
                }
                if (ApproveTo == null) {//ApproveFrom has value
                    selectedValue = "From: " + ((DateTime)ApproveFrom).ToString("dd-MMM-yyyy");
                }
                else if (ApproveFrom == null) {//ApproveTo has value              
                    selectedValue = "To: " + ((DateTime)ApproveTo).ToString("dd-MMM-yyyy");
                }
                else {//ApproveTo and ApproveFrom both have values
                    if (ApproveFrom == (DateTime)ApproveTo) {
                        selectedValue = " " + ((DateTime)ApproveFrom).ToString("yyyy-MMM-dd");
                    }
                    else {
                        selectedValue = "From: " + ((DateTime)ApproveFrom).ToString("yyyy-MMM-dd") + " To: " + ((DateTime)ApproveTo).ToString("yyyy-MMM-dd");
                    }
                }
                foreach (var item in currentFilter) {
                    item.DateFrom = ApproveFrom == null ? DateTime.MinValue : ApproveFrom;
                    item.DateTo = ApproveTo == null ? DateTime.MaxValue : ((DateTime)ApproveTo).AddDays(1);
                }
                RefreshWindowAfterDateTimeFilter(fltLabel, currentFilter, selectedValue);
                pop_DateTimeFlt_ApprovalDateTime.IsOpen = false;
            }
        }

        private void TrdExecDT_Filter_Apply(object sender, RoutedEventArgs e) {
            ObservableCollection<FilterObj>? currentFilter;
            string fltLabel = pop_DateTimeFlt_TrdExecDateTime.Uid;
            _vm.FilterDic.TryGetValue(fltLabel, out currentFilter);
            if (currentFilter != null) {
                DateTime? TrdExecFrom = datePicker_TrdExecFrom.SelectedDate;
                DateTime? TrdExecTo = datePicker_TrdExecTo.SelectedDate;
                string selectedValue;
                if (TrdExecFrom == null && TrdExecTo == null) {
                    pop_DateTimeFlt_TrdExecDateTime.IsOpen = false;
                    return;
                }
                if (TrdExecTo == null) {//TrdExecFrom has value
                    selectedValue = "From: " + ((DateTime)TrdExecFrom).ToString("dd-MMM-yyyy");
                }
                else if (TrdExecFrom == null) {//TrdExecTo has value              
                    selectedValue = "To: " + ((DateTime)TrdExecTo).ToString("dd-MMM-yyyy");
                }
                else {//TrdExecTo and TrdExecFrom both have values
                    if (TrdExecFrom == (DateTime)TrdExecTo) {
                        selectedValue = " " + ((DateTime)TrdExecFrom).ToString("dd-MMM-yyyy");
                    }
                    else {
                        selectedValue = "From: " + ((DateTime)TrdExecFrom).ToString("dd-MMM-yyyy") + " To: " + ((DateTime)TrdExecTo).ToString("dd-MMM-yyyy");
                    }
                }
                foreach (var item in currentFilter) {
                    item.DateFrom = TrdExecFrom == null ? DateTime.MinValue : TrdExecFrom;
                    item.DateTo = TrdExecTo == null ? DateTime.MaxValue : ((DateTime)TrdExecTo).AddDays(1);
                }
                RefreshWindowAfterDateTimeFilter(fltLabel, currentFilter, selectedValue);
                pop_DateTimeFlt_TrdExecDateTime.IsOpen = false;
            }
        }
        #endregion datetime filter

        #region checkbox filter       
        private void MarketSegID_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_MarketSegmentID.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_MarketSegmentID.IsOpen = false;
        }

        private void Instrument_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_Instrument.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_Instrument.IsOpen = false;
        }

        private void SubmittedBy_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_SubmittedBy.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_SubmittedBy.IsOpen = false;
        }
        private void BuyerCM_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_BuyerCM.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_BuyerCM.IsOpen = false;
        }
        private void BuyerTradeRegistrant_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_BuyerTradeRegistrant.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_BuyerTradeRegistrant.IsOpen = false;
        }

        private void BuyerTrdCom_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_BuyerTrdCom.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_BuyerTrdCom.IsOpen = false;
        }

        private void BuyerBroker_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_BuyerBroker.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_BuyerBroker.IsOpen = false;
        }

        private void SellerBroker_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_SellerBroker.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_SellerBroker.IsOpen = false;
        }

        private void SellerCM_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_SellerCM.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_SellerCM.IsOpen = false;
        }

        private void SellerTrdCom_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_SellerTrdCom.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_SellerTrdCom.IsOpen = false;
        }

        private void SellerTradeRegistrant_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_SellerTradeRegistrant.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_SellerTradeRegistrant.IsOpen = false;
        }

        private void TrdSessionSubID_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_TradingSessionSubID.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_TradingSessionSubID.IsOpen = false;
        }

        private void QtyType_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_QtyType.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_QtyType.IsOpen = false;
        }

        private void TrdStatus_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_TrdStatus.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_TrdStatus.IsOpen = false;
        }

        private void TrdType_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_CheckboxFlt_TrdType.Uid;
            bool? isTopSelectAllChecked = GetTopSelectAllCheckStatus(sender);
            RefreshWindowAfterCheckBoxFilter(fltLabel, isTopSelectAllChecked);
            pop_CheckboxFlt_TrdType.IsOpen = false;
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

        #region num_range filter
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

        private void QtyInUnits_filter_Apply(object sender, RoutedEventArgs e) {
            if (MinNum_QtyInUnits.Text == "" && MaxNum_QtyInUnits.Text == "") {
                return;
            }
            string fltLabel = pop_RangeFlt_QtyInUnits.Uid;
            double parseOutput;
            if (!double.TryParse(MinNum_QtyInUnits.Text, out parseOutput) && MinNum_QtyInUnits.Text != "") {
                MessageBox.Show("Input must be numeric"); return;
            }
            if (!double.TryParse(MaxNum_QtyInUnits.Text, out parseOutput) && MaxNum_QtyInUnits.Text != "") {
                MessageBox.Show("Input must be numeric"); return;
            }
            double min = MinNum_QtyInUnits.Text != "" ? Convert.ToDouble(MinNum_QtyInUnits.Text) : MINNUM;
            double max = MaxNum_QtyInUnits.Text != "" ? Convert.ToDouble(MaxNum_QtyInUnits.Text) : MAXNUM;
            if (min > max) {
                MessageBox.Show("Quantity In Units's Minimum value cannot be greater than Maximum value!");
                return;
            }
            RefreshWindowAfterNumRangeFilter(fltLabel, min, max);
            pop_RangeFlt_QtyInUnits.IsOpen = false;
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
        #endregion num range filter

        #region combo box filter
        private IEnumerable<TradeRecord> ComboxBoxFilter(string fltLabel, string filterCtrl, string input) {
            IEnumerable<TradeRecord> filteredValue;

            switch (filterCtrl) {
                case "Contains":
                    filteredValue = _vm.AllTradeData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper().Contains(input.ToUpper()));
                    break;
                case "Starts With":
                    filteredValue = _vm.AllTradeData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper().StartsWith(input.ToUpper()));
                    break;
                case "Ends With":
                    filteredValue = _vm.AllTradeData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper().EndsWith(input.ToUpper()));
                    break;
                case "Equals":
                    filteredValue = _vm.AllTradeData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper() == input.ToUpper());
                    break;
                case "Does Not Contain":
                    filteredValue = _vm.AllTradeData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper().Contains(input.ToUpper()) == false);
                    break;
                default:
                    filteredValue = _vm.AllTradeData.Where(w => ((w.GetType().GetProperty(fltLabel).GetValue(w, null) ?? "").ToString() ?? "").ToUpper().Contains(input.ToUpper()));
                    break;
            }
            return filteredValue;

        }
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
        private void TrdID_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_TradeID.Uid;
            string input = TradeID_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeRecord> filteredValue;
            string filterCtrl = (string)TextFltOptions_TradeID.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);

            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.TradeID)) {
                    currentFilter.Add(new FilterObj() { Label = item.TradeID });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_TradeID.IsOpen = false;
        }

        private void DealID_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_DealID.Uid;
            string input = DealID_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeRecord> filteredValue;
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

        private void SecurityID_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_SecurityID.Uid;
            string input = SecurityID_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeRecord> filteredValue;
            string filterCtrl = (string)TextFltOptions_SecurityID.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.SecurityID)) {
                    currentFilter.Add(new FilterObj() { Label = item.SecurityID });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_SecurityID.IsOpen = false;
        }

        private void ClearMsg_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_ClearMsg.Uid;
            string input = ClearMsg_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeRecord> filteredValue;
            string filterCtrl = (string)TextFltOptions_ClearMsg.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.ClearMsg)) {
                    currentFilter.Add(new FilterObj() { Label = item.ClearMsg });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_ClearMsg.IsOpen = false;
        }

        private void BuyerTrader_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_BuyerTrader.Uid;
            string input = BuyerTrader_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeRecord> filteredValue;
            string filterCtrl = (string)TextFltOptions_BuyerTrader.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.BuyerTrader)) {
                    currentFilter.Add(new FilterObj() { Label = item.BuyerTrader });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_BuyerTrader.IsOpen = false;
        }

        private void BuyerAccount_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_BuyerAccount.Uid;
            string input = BuyerAccount_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeRecord> filteredValue;
            string filterCtrl = (string)TextFltOptions_BuyerAccount.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.BuyerAccount)) {
                    currentFilter.Add(new FilterObj() { Label = item.BuyerAccount });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_BuyerAccount.IsOpen = false;
        }

        private void BuyerSecurID_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_BuyerSecurID.Uid;
            string input = BuyerSecurID_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeRecord> filteredValue;
            string filterCtrl = (string)TextFltOptions_BuyerSecurID.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.BuyerSecurID)) {
                    currentFilter.Add(new FilterObj() { Label = item.BuyerSecurID });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_BuyerSecurID.IsOpen = false;
        }
        private void BuyerInternalTrdID_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_BuyerInternalTrdID.Uid;
            string input = BuyerInternalTrdID_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeRecord> filteredValue;
            string filterCtrl = (string)TextFltOptions_BuyerInternalTrdID.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.BuyerInternalTrdID)) {
                    currentFilter.Add(new FilterObj() { Label = item.BuyerInternalTrdID });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_BuyerInternalTrdID.IsOpen = false;
        }

        private void SellerTrader_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_SellerTrader.Uid;
            string input = SellerTrader_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeRecord> filteredValue;
            string filterCtrl = (string)TextFltOptions_SellerTrader.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.SellerTrader)) {
                    currentFilter.Add(new FilterObj() { Label = item.SellerTrader });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_SellerTrader.IsOpen = false;
        }

        private void SellerAccount_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_SellerAccount.Uid;
            string input = SellerAccount_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeRecord> filteredValue;
            string filterCtrl = (string)TextFltOptions_SellerAccount.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.SellerAccount)) {
                    currentFilter.Add(new FilterObj() { Label = item.SellerAccount });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_SellerAccount.IsOpen = false;
        }

        private void SellerSecurID_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_SellerSecurID.Uid;
            string input = SellerSecurID_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeRecord> filteredValue;
            string filterCtrl = (string)TextFltOptions_SellerSecurID.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.SellerSecurID)) {
                    currentFilter.Add(new FilterObj() { Label = item.SellerSecurID });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_SellerSecurID.IsOpen = false;
        }

        private void BuySource_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_BuySource.Uid;
            string input = BuySource_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeRecord> filteredValue;
            string filterCtrl = (string)TextFltOptions_BuySource.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.BuySource)) {
                    currentFilter.Add(new FilterObj() { Label = item.BuySource });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_BuySource.IsOpen = false;
        }

        private void SellerComment_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_SellerComment.Uid;
            string input = SellerComment_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeRecord> filteredValue;
            string filterCtrl = (string)TextFltOptions_SellerComment.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.SellerComment)) {
                    currentFilter.Add(new FilterObj() { Label = item.SellerComment });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_SellerComment.IsOpen = false;
        }

        private void SellerInternalTrdID_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_SellerInternalTrdID.Uid;
            string input = SellerInternalTrdID_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeRecord> filteredValue;
            string filterCtrl = (string)TextFltOptions_SellerInternalTrdID.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.SellerInternalTrdID)) {
                    currentFilter.Add(new FilterObj() { Label = item.SellerInternalTrdID });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_SellerInternalTrdID.IsOpen = false;
        }

        private void SellSource_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_SellSource.Uid;
            string input = SellSource_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeRecord> filteredValue;
            string filterCtrl = (string)TextFltOptions_SellSource.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.SellSource)) {
                    currentFilter.Add(new FilterObj() { Label = item.SellSource });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_SellSource.IsOpen = false;
        }

        private void USI_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_USI.Uid;
            string input = USI_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeRecord> filteredValue;
            string filterCtrl = (string)TextFltOptions_USI.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.USI)) {
                    currentFilter.Add(new FilterObj() { Label = item.USI });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_USI.IsOpen = false;
        }

        private void BuyerComment_Filter_Apply(object sender, RoutedEventArgs e) {
            string fltLabel = pop_ComboFlt_BuyerComment.Uid;
            string input = BuyerComment_Flt_Input.Text;
            ObservableCollection<FilterObj> currentFilter = new ObservableCollection<FilterObj>();
            IEnumerable<TradeRecord> filteredValue;
            string filterCtrl = (string)TextFltOptions_BuyerComment.SelectedItem;
            filteredValue = ComboxBoxFilter(fltLabel, filterCtrl, input);
            foreach (var item in filteredValue) {
                if (!currentFilter.Any(x => x.Label == item.BuyerComment)) {
                    currentFilter.Add(new FilterObj() { Label = item.BuyerComment });
                }
            }
            RefreshWindowAfterComboBoxFilter(fltLabel, filterCtrl, input, currentFilter);
            pop_ComboFlt_BuyerComment.IsOpen = false;
        }
        #endregion combo box filter

        #endregion

        #region checkbox 3 status
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
        #endregion

        #region Refresh Window after apply filter
        private void Refresh() {
            if (_collectionVS.View.Filter == null) {
                _collectionVS.Filter += Data_Filter;
            }
            _collectionVS.View.Refresh();
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
        #endregion

        private void Reset_CurrFlts(object sender, RoutedEventArgs e) {
            _collectionVS.View.Filter = null;
            _collectionVS.View.Refresh();

            #region //clear previous selection
            Checkbox_selectAll_SubmittedBy.IsChecked = true;
            Checkbox_selectAll_BuyerBroker.IsChecked = true;
            Checkbox_selectAll_BuyerCM.IsChecked = true;
            Checkbox_selectAll_BuyerTradeRegistrant.IsChecked = true;
            Checkbox_selectAll_BuyerTrdCom.IsChecked = true;
            Checkbox_selectAll_Instrument.IsChecked = true;
            Checkbox_selectAll_MarketSegmentID.IsChecked = true;
            Checkbox_selectAll_QtyType.IsChecked = true;
            Checkbox_selectAll_SellerBroker.IsChecked = true;
            Checkbox_selectAll_SellerCM.IsChecked = true;
            Checkbox_selectAll_SellerTradeRegistrant.IsChecked = true;
            Checkbox_selectAll_SellerTrdCom.IsChecked = true;
            Checkbox_selectAll_TradingSessionSubID.IsChecked = true;
            Checkbox_selectAll_TrdStatus.IsChecked = true;
            Checkbox_selectAll_TrdType.IsChecked = true;

            DealID_Flt_Input.Text = "";
            TradeID_Flt_Input.Text = "";
            BuyerAccount_Flt_Input.Text = "";
            BuyerComment_Flt_Input.Text = "";
            BuyerInternalTrdID_Flt_Input.Text = "";
            BuyerSecurID_Flt_Input.Text = "";
            BuyerTrader_Flt_Input.Text = "";
            BuySource_Flt_Input.Text = "";
            ClearMsg_Flt_Input.Text = "";
            SecurityID_Flt_Input.Text = "";
            SellerAccount_Flt_Input.Text = "";
            SellerComment_Flt_Input.Text = "";
            SellerInternalTrdID_Flt_Input.Text = "";
            SellerSecurID_Flt_Input.Text = "";
            SellerTrader_Flt_Input.Text = "";
            SellSource_Flt_Input.Text = "";
            USI_Flt_Input.Text = "";

            MinNum_Price.Text = "";
            MaxNum_Price.Text = "";
            MinNum_QtyInLots.Text = "";
            MaxNum_QtyInLots.Text = "";
            MinNum_QtyInUnits.Text = "";
            MaxNum_QtyInUnits.Text = "";
            MinNum_Strike.Text = "";
            MaxNum_Strike.Text = "";

            datePicker_TrdRegistFrom.SelectedDate = null;
            datePicker_TrdRegistTo.SelectedDate = null;
            datePicker_ApprovalFrom.SelectedDate = null;
            datePicker_ApprovalTTo.SelectedDate = null;
            datePicker_ClearedDateFrom.SelectedDate = null;
            datePicker_ClearedDateTo.SelectedDate = null;
            datePicker_ExpireDateFrom.SelectedDate = null;
            datePicker_ExpireDateTo.SelectedDate = null;
            datePicker_TrdExecFrom.SelectedDate = null;
            datePicker_TrdExecTo.SelectedDate = null;
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
            _vm.TradeColumns.ForEach(x => x.IsChecked = true);
            dataGrid.Columns.ToList().ForEach(x => x.Visibility = Visibility.Visible);
            #endregion // re-display all columns
        }

        private void ClosePopUp_Click(object sender, RoutedEventArgs e) {//click CANCEL button to close the popup
            Button btn = (Button)sender;
            StackPanel sp = (StackPanel)btn.Parent;
            Grid gd = (Grid)sp.Parent;
            Border bd = (Border)gd.Parent;
            Popup pop = (Popup)bd.Parent;
            pop.IsOpen = false;
        }

        private void ClosePopUp(object sender, EventArgs e) {//checkbox popup's auto-closure when clicking somewhere else
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
                                        if (m is CheckBox selectAllCheckbox) {
                                            selectAllCheckbox.IsChecked = null;
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

        private void CustomizePopUpClose(object sender, EventArgs e) {//customize button's popup's auto-closure when clicking somewhere else
            var visibleColumns = dataGrid.Columns.Where(x => x.Visibility == Visibility.Visible);
            if (visibleColumns.Count() == _vm.TradeColumns.Where(x => x.IsChecked == true).Count()) {
                return;
            }
            foreach (var item in _vm.TradeColumns) {
                if (visibleColumns.Any(x => _vm.PropertyColumnMapping[x.SortMemberPath] == item.Label)) {
                    item.IsChecked = true;
                }
                else {
                    item.IsChecked = false;
                }
            }
            if (!_vm.TradeColumns.Any(x => x.IsChecked == false)) {
                //sometimes, if UI cannot be fully loaded in screen, some code-behind event handler(like CheckBox_Update)
                //cannot be triggered
                checkbox_selectAll_CustomizeColumns.IsChecked = true;
            }
        }

        #region Customize feature
        private void Customize_Click(object sender, RoutedEventArgs e) {
            pop_CustomizeColumn.IsOpen = true;
        }

        private void UpdateColumn_Click(object sender, RoutedEventArgs e) {
            ObservableCollection<DataGridColumn> item = dataGrid.Columns;
            foreach (FilterObj option in _vm.TradeColumns) {
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
            TradeID,
            DealID,
            SecurityID,
            TrdStatus,
            TrdType,
            USI,
            BuyerAccount,
            BuyerComment,
            BuyerInternalTrdID,
            BuyerSecurID,
            BuyerTrader,
            BuySource,
            ClearMsg,
            SellerAccount,
            SellerComment,
            SellerInternalTrdID,
            SellerSecurID,
            SellerTrader,
            SellSource,
            BuyerBroker,
            BuyerCM,
            BuyerTradeRegistrant,
            BuyerTrdCom,
            Instrument,
            MarketSegmentID,
            QtyType,
            SellerBroker,
            SellerCM,
            SellerTradeRegistrant,
            SellerTrdCom,
            SubmittedBy,
            TradingSessionSubID,
            TradeRegistrationDateTime,
            ApprovalDateTime,
            TrdExecDateTime,
            ExpireDate,
            ClearedDate,
            Price,
            QtyInUnits,
            QtyInLots,
            Strike
        }
        public enum ComboTextFilterColumn {
            TradeID,
            DealID,
            SecurityID,
            USI,
            BuyerAccount,
            BuyerComment,
            BuyerInternalTrdID,
            BuyerSecurID,
            BuyerTrader,
            BuySource,
            ClearMsg,
            SellerAccount,
            SellerComment,
            SellerInternalTrdID,
            SellerSecurID,
            SellerTrader,
            SellSource
        }
        public enum CheckboxFiltercolumn {
            TrdType,
            TrdStatus,
            BuyerBroker,
            BuyerCM,
            BuyerTradeRegistrant,
            BuyerTrdCom,
            Instrument,
            MarketSegmentID,
            QtyType,
            SellerBroker,
            SellerCM,
            SellerTradeRegistrant,
            SellerTrdCom,
            SubmittedBy,
            TradingSessionSubID
        }
        public enum DateTimeFilterColumn {
            TradeRegistrationDateTime,
            ApprovalDateTime,
            TrdExecDateTime,
            ExpireDate,
            ClearedDate
        }
        public enum NumberRangeFilterColumn {
            Price,
            QtyInUnits,
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
                        foreach (var item in gd.Children) {
                            if (item is StackPanel sp) {
                                foreach (var control in sp.Children) {
                                    if (control is ListBox lb) {
                                        lb.ItemsSource = currentFilter.Where(x => x.Label != null && x.Label.IndexOf(tb.Text, System.StringComparison.OrdinalIgnoreCase) >= 0);
                                    }
                                }
                            }
                        }
                    }
                }
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
                sfd.FileName = "Trade Report_" + DateTime.Today.ToString("yyyyMMdd") + ".csv";
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

                            MessageBox.Show("Trade Data Exported Successfully !!!", "Info");
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
