using ICEChecker.ModuleCheckerInput.Models;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ICEChecker.ModuleCheckerInput.ViewModels {
    public class Input2ViewModel : BindableBase {
        IEventAggregator _ea;
        public string DisplayName { get; set; }
        public ObservableCollection<TraderInput> InputData { get; } = new();
        public List<TraderInput> AlrCheckedData { get; } = new();
        public ICommand CheckCommand { get; private set; }
        public ICommand OpenExcelCommand { get; private set; }
        public ICommand LoadExcelCommand { get; private set; }
        private DateTime _checkTime;
        public DateTime CheckTime {
            get => _checkTime;
            set => SetProperty(ref _checkTime, value);
        }

        private int _checkRdNum;
        public int CheckRdNum {
            get => _checkRdNum;
            set => SetProperty(ref _checkRdNum, value);
        }

        private int _passedRdNum;
        public int PassedRdNum {
            get => _passedRdNum;
            set => SetProperty(ref _passedRdNum, value);
        }

        private int _failedRdNum;
        public int FailedRdNum {
            get => _failedRdNum;
            set => SetProperty(ref _failedRdNum, value);
        }

        private int _pendingRdNum;
        public int PendingRdNum {
            get => _pendingRdNum;
            set => SetProperty(ref _pendingRdNum, value);
        }

        // public ICommand RemoveCommand { get; private set; } 
        public bool IsToAdd { get; } = true;

        private string _remark = "";
        public string Remark {
            get => _remark;
            set => SetProperty(ref _remark, value);
        }

        private string _pathName;
        public string PathName {
            get => _pathName;
            set => SetProperty(ref _pathName, value);
        }
        private string _sheetName;
        public string SheetName {
            get => _sheetName;
            set => SetProperty(ref _sheetName, value);
        }

        private string _userID;
        public string UserID {
            get => _userID;
            set => SetProperty(ref _userID, value);
        }

        public Dictionary<string, string> TraderUserIDMapping { get; set; } = new Dictionary<string, string>() {
            ["liyong"] = "lyong3",
            ["Shiqi"] = "szhang23",
            ["Shaoye"] = "sysng"
        };
        public Input2ViewModel(IEventAggregator ea) {
            _ea = ea;
            _ea.GetEvent<CheckerCallBackEvent>().Subscribe(RemoveCheckedRecords);
            _ea.GetEvent<DisplayCheckedNumEvent>().Subscribe(DisplayTotalRecordsAftCheck);
            _ea.GetEvent<DisplayPassedNumEvent>().Subscribe(DisplayPassedRecordsAftCheck);
            _ea.GetEvent<DisplayFailedNumEvent>().Subscribe(DisplayFailedRecordsAftCheck);
            _ea.GetEvent<DisplayPendingNumEvent>().Subscribe(DisplayPendingRecordsAftCheck);
            _ea.GetEvent<DisplayMergedSprNumEvent>().Subscribe(DisplayMergedSprRecords);
            _ea.GetEvent<RetrieveUserIDEvent>().Subscribe(SyncUserID);

            CheckCommand = new DelegateCommand(CheckConsistency);
            OpenExcelCommand = new DelegateCommand(OpenExcelFile);
            LoadExcelCommand = new DelegateCommand(LoadExcel);
            //RemoveCommand = new DelegateCommand(RemoveRecords);

        }

        private void SyncUserID(string id) {
            UserID = id;
        }

        private void DisplayFailedRecordsAftCheck(int num) {
            FailedRdNum = num;
        }

        private void DisplayPassedRecordsAftCheck(int num) {
            PassedRdNum = num;
        }

        private void DisplayPendingRecordsAftCheck(int num) {
            PendingRdNum = num;
        }
        private void DisplayTotalRecordsAftCheck(int num) {
            CheckRdNum = num;
        }
        private void DisplayMergedSprRecords(int num) {
            Remark = "";
            if (num > 0) {
                if (num == 1) {
                    Remark += "2 inputs are merged into 1 spread\n ";
                }
                else {
                    Remark += string.Format("{0} inputs are merged into {1} spreads\n", num * 2, num);
                }
            }
        }

        private void RemoveCheckedRecords(TraderInput input) {
            InputData.Remove(input);
            AlrCheckedData.Add(input);
        }

        //private void RemoveRecords() {}

        private bool RemoveDuplicateForCurrentBatch() {// find duplicate in this batch's input 
            //duplicate inputs for current batch
            Remark = "";
            //int duplicateNumForCurrent = 0;
            List<TraderInput> noDuplicates = new List<TraderInput>();
            for (int i = 0; i < InputData.Count; i++) {
                bool haveDuplicateForCurrentInput = false;
                foreach (var item in noDuplicates) {
                    if (item.Equals(InputData[i])) {
                        haveDuplicateForCurrentInput = true;
                        break;
                    }
                }
                if (!haveDuplicateForCurrentInput) {
                    noDuplicates.Add(InputData[i]);
                }
            }
            //confirm with user whether to auto merge the duplicate one, or let them to edit manually
            MessageBoxResult result = new MessageBoxResult();
            if (noDuplicates.Count != InputData.Count) {
                result = MessageBox.Show(string.Format("{0} inputs are duplicate, do you want to auto merge these duplicate ones?", InputData.Count - noDuplicates.Count), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                Remark = string.Format("{0} inputs are duplicate, ", InputData.Count - noDuplicates.Count);
                if (result == MessageBoxResult.Yes) {
                    //combine the duplicate one
                    InputData.Clear();
                    noDuplicates.ForEach(x => InputData.Add(x));
                    Remark += "auto merge successfully!\n";
                    return true;
                }
                else {
                    Remark += "auto merge Rejected, pls edit manually!\n";
                    return false;
                }
            }
            return true;

        }
        private bool RemoveDuplicateWithFormerBatch() { //find whether input has duplicate with former batches
            //find duplicate inputs for former batches
            //int duplicateNumForFormer = 0;
            List<TraderInput> noDuplicates = new List<TraderInput>();
            for (int i = 0; i < InputData.Count; i++) {
                bool haveDuplicateForFormerBatch = false;
                foreach (var item in AlrCheckedData) {
                    //if (item.Equals(InputData[i])) {
                    //    haveDuplicateForFormerBatch = true;
                    //    break;
                    //}
                    if (item.Equals(InputData[i]) && !item.IsSpread) {
                        haveDuplicateForFormerBatch = true;
                        break;
                    }
                }
                if (!haveDuplicateForFormerBatch) {
                    noDuplicates.Add(InputData[i]);
                }
            }

            MessageBoxResult result = new MessageBoxResult();
            if (noDuplicates.Count != InputData.Count) {
                result = MessageBox.Show(string.Format("{0} inputs are duplicate with previous checked records, they will be auto merged when checking!", InputData.Count - noDuplicates.Count), "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                Remark += string.Format("{0} inputs have been compared previously, ", InputData.Count - noDuplicates.Count);
                if (result == MessageBoxResult.OK) {
                    //remove the duplicate one
                    InputData.Clear();
                    noDuplicates.ForEach(x => InputData.Add(x));
                    Remark += "auto merge successfully!\n";
                    return true;
                }
                else {
                    return false;
                }
            }
            return true;
        }

        private bool RemoveEmptyRow() {
            List<TraderInput> emptyInputs = new List<TraderInput>();
            foreach (var rowdata in InputData) {
                if (rowdata is null) {
                    //InputData.Remove(rowdata);
                    emptyInputs.Add(rowdata);
                    continue;
                }
                if (string.IsNullOrEmpty(rowdata.Contract) && string.IsNullOrEmpty(rowdata.Month) &&
                    rowdata.Price == null && rowdata.Qty == null && rowdata.Time == null &&
                    string.IsNullOrEmpty(rowdata.Shop) && string.IsNullOrEmpty(rowdata.PxQ)) {
                    //InputData.Remove(rowdata);
                    emptyInputs.Add(rowdata);
                    continue;
                }
            }
            foreach (var input in emptyInputs) {
                InputData.Remove(input);
            }
            return true;
        }
        private void CheckConsistency() {
            bool IsEmptyRowReoved = RemoveEmptyRow();
            if (IsEmptyRowReoved) {
                foreach (TraderInput input in InputData) {
                    if (string.IsNullOrEmpty(input.Contract) && string.IsNullOrEmpty(input.Month) && input.Price == null && input.Qty == null && string.IsNullOrEmpty(input.Shop) && input.Time == null && string.IsNullOrEmpty(input.PxQ)) {
                        InputData.Remove(input);
                        continue;
                    }
                    input.TradeDate = DateTime.Now.Date;
                    input.Trader = UserID.ToLower().Replace(" ", "");
                    #region keep this field for possible future usage
                    //if (string.IsNullOrEmpty(input.IsSpreadValue)) {
                    //    input.IsSpread = false;
                    //}
                    //else {
                    //    input.IsSpread = input.IsSpreadValue.Trim().ToLower().Contains("y") ? true : false;
                    //}
                    #endregion
                }

                //bool IsCurrentBatchUnique = RemoveDuplicateForCurrentBatch();
                bool IsCurrentBatchUnique = true;
                if (IsCurrentBatchUnique) {
                    //bool IsAllBatchUnique = RemoveDuplicateWithFormerBatch();
                    bool IsAllBatchUnique = true;
                    if (IsAllBatchUnique) {
                        List<TraderInput> inputs = new List<TraderInput>();
                        foreach (TraderInput input in InputData) {
                            inputs.Add(input);
                        }
                        string EmptyColumnNames = "";
                        bool ContainsEmptyCol = false;
                        int EmptyColNum = 0;
                        TraderInput inputField = new TraderInput();
                        foreach (var item in inputField.GetType().GetProperties()) {
                            string colName = item.Name;
                            if (colName != "TradeDate" && colName != "Trader" && colName != "IsConsistent") {
                                if (InputData.All(w => (w.GetType().GetProperty(colName).GetValue(w, null) == null || w.GetType().GetProperty(colName).GetValue(w, null).ToString() == ""))) {
                                    ContainsEmptyCol = true;
                                    EmptyColNum++;
                                    EmptyColumnNames = string.IsNullOrEmpty(EmptyColumnNames) ? colName : EmptyColumnNames + " & " + colName;
                                }
                            }
                        }
                        if (ContainsEmptyCol) {
                            MessageBoxResult result = new MessageBoxResult();
                            MessageBoxResult confirmSprInputResult = new MessageBoxResult();
                            if (EmptyColNum == 1) {
                                result = MessageBox.Show("There is one column empty: \"" + EmptyColumnNames + "\", are you sure to continue checking?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            }
                            else if (EmptyColNum > 1) {
                                result = MessageBox.Show("There are " + EmptyColNum + " columns empty: \"" + EmptyColumnNames + "\", are you sure to continue checking?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            }
                            //if (EmptyColumnNames.Contains("Spr")) {
                            //    confirmSprInputResult = MessageBox.Show("The \"Is Spread\" column is empty, if spread deals are not assigned with \"Y\", it will be processed as non-spread. Do you want to continue?", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                            //}
                            if (result == MessageBoxResult.Yes && confirmSprInputResult == MessageBoxResult.OK) {
                                _ea.GetEvent<SendCheckInputEvent>().Publish(inputs);
                            }
                        }
                        else {
                            _ea.GetEvent<SendCheckInputEvent>().Publish(inputs);
                        }
                        CheckTime = DateTime.Now;
                    }
                }
            }
        }

        private void OpenExcelFile() {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";

            if (openFileDialog.ShowDialog() == true)
                PathName = openFileDialog.FileName;
        }

        private void LoadExcel() {
            InputData.Clear();
            string PathCpnn = " Provider = Microsoft.ACE.OLEDB.12.0; Data Source = " + PathName + ";Extended Properties='Excel 12.0 XML;HDR=YES;';";
            try {
                OleDbConnection conn = new OleDbConnection(PathCpnn);
                OleDbCommand comm = new OleDbCommand("Select * from [" + SheetName + "$]", conn);
                conn.Open();

                OleDbDataAdapter sda = new OleDbDataAdapter(comm);
                DataTable dt = new DataTable();
                try { sda.Fill(dt); }
                catch (Exception e) {
                    MessageBox.Show(e.Message);
                }
                foreach (DataColumn column in dt.Columns) {
                    column.ColumnName = column.ColumnName.ToLower().Trim();
                }
                var headers = dt.Rows[0].ItemArray;

                try {
                    if (dt.Rows.Count > 0) {
                        List<TraderInput> lists = new List<TraderInput>();
                        for (int i = 0; i < dt.Rows.Count; i++) {
                            int EmptyRowNo = 0;
                            TraderInput input = new TraderInput();
                            if (!string.IsNullOrEmpty(dt.Rows[i]["con"].ToString())) {
                                input.Contract = dt.Rows[i]["con"].ToString();
                            }
                            else {
                                EmptyRowNo++;
                            }
                            if (!string.IsNullOrEmpty(dt.Rows[i]["mon"].ToString())) {
                                input.Month = dt.Rows[i]["mon"].ToString();
                            }
                            else {
                                EmptyRowNo++;
                            }
                            if (!string.IsNullOrEmpty(dt.Rows[i]["price"].ToString())) {
                                //input.Price = (double)Convert.ToDecimal(dt.Rows[i][2]);
                                input.Price = (double)Convert.ToDecimal(dt.Rows[i]["Price"]);
                            }
                            else {
                                EmptyRowNo++;
                            }
                            if (!string.IsNullOrEmpty(dt.Rows[i]["qty"].ToString())) {
                                input.Qty = (double)Convert.ToDecimal(dt.Rows[i]["qty"]);
                            }
                            else {
                                EmptyRowNo++;
                            }
                            if (!string.IsNullOrEmpty(dt.Rows[i]["shop"].ToString())) {
                                input.Shop = dt.Rows[i]["shop"].ToString();
                            }
                            else {
                                EmptyRowNo++;
                            }
                            if (!string.IsNullOrEmpty(dt.Rows[i]["time"].ToString())) {
                                input.Time = Convert.ToDateTime(dt.Rows[i]["time"]);
                            }
                            else {
                                EmptyRowNo++;
                            }
                            if (!string.IsNullOrEmpty(dt.Rows[i]["p*q"].ToString())) {
                                input.PxQ = Convert.ToDecimal(dt.Rows[i]["p*q"]).ToString();
                            }
                            else {
                                EmptyRowNo++;
                            }
                            //if (!string.IsNullOrEmpty(dt.Rows[i]["sprd?"].ToString())) {
                            //    input.IsSpreadValue = dt.Rows[i]["sprd?"].ToString();
                            //}
                            //else {
                            //    EmptyRowNo++;
                            //}
                            if (EmptyRowNo != 7) {
                                lists.Add(input);
                            }
                        }
                        foreach (var item in lists) {

                            InputData.Add(item);
                        }
                        MessageBox.Show(string.Format("Load Sucessfully! {0} total records", lists.Count));
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                    SheetName = "";
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
            }
        }
    }
}
