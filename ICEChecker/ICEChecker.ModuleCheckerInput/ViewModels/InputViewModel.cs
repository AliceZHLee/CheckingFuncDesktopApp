using ICEChecker.ModuleCheckerInput.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ICEChecker.ModuleCheckerInput.ViewModels {
    public class InputViewModel : BindableBase {
        IEventAggregator _ea;
        public string UserName;
        public string DisplayName { get; set; }
        public ObservableCollection<TraderInput> InputData { get; } = new();
        public List<TraderInput> AlrCheckedData { get; } = new();
        public ICommand CheckCommand { get; private set; }
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

        public Dictionary<string, string> TraderUserIDMapping { get; set; } = new Dictionary<string, string>() {
            ["liyong"] = "lyong3",
            ["Shiqi"] = "szhang23",
            ["Shaoye"] = "sysng"
        };
        public InputViewModel(IEventAggregator ea) {
            _ea = ea;
            _ea.GetEvent<CheckerCallBackEvent>().Subscribe(RemoveCheckedRecords);
            _ea.GetEvent<DisplayCheckedNumEvent>().Subscribe(DisplayTotalRecordsAftCheck);
            _ea.GetEvent<DisplayPassedNumEvent>().Subscribe(DisplayPassedRecordsAftCheck);
            _ea.GetEvent<DisplayFailedNumEvent>().Subscribe(DisplayFailedRecordsAftCheck);
            _ea.GetEvent<DisplayPendingNumEvent>().Subscribe(DisplayPendingRecordsAftCheck);
            _ea.GetEvent<DisplayMergedSprNumEvent>().Subscribe(DisplayMergedSprRecords);

            CheckCommand = new DelegateCommand<string>(CheckConsistency);
            //RemoveCommand = new DelegateCommand(RemoveRecords);
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

        private void CheckConsistency(string traderName) {
            foreach (TraderInput input in InputData) {
                input.TradeDate = DateTime.Now.Date;
                input.Trader = TraderUserIDMapping[traderName.ToLower().Replace(" ", "")];
                //if (string.IsNullOrEmpty(input.IsSpreadValue)) {
                //    input.IsSpread = false;
                //}
                //else {
                //    if (input.IsSpreadValue.Trim().ToLower().Contains("y")) {
                //        input.IsSpread = true;
                //    }
                //    else {
                //        input.IsSpread = false;
                //    }
                //}
            }
            bool IsCurrentBatchUnique = RemoveDuplicateForCurrentBatch();
            if (IsCurrentBatchUnique) {
                bool IsAllBatchUnique = RemoveDuplicateWithFormerBatch();
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
                        if (EmptyColumnNames.Contains("Spr")) {
                            confirmSprInputResult = MessageBox.Show("The \"Is Spread\" column is empty, if spread deals are not assigned with \"Y\", it will be processed as non-spread. Do you want to continue?", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                        }
                        if (result == MessageBoxResult.Yes && confirmSprInputResult==MessageBoxResult.OK) {
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
}
