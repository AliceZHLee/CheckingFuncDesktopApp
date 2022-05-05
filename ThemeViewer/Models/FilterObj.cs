using Prism.Mvvm;
using System;

namespace ThemeViewer.Models {
    public class FilterObj : BindableBase {     
        public string? Label { get; set; }
        public double? MinLimit { get; set; }
        public double? MaxLimit { get; set; }

        private bool _isChecked;
        public bool IsChecked {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        private DateTime? _dateFrom;
        public DateTime? DateFrom {
            get => _dateFrom;
            set => SetProperty(ref _dateFrom, value);
        }

        private DateTime? _dateTo;
        public DateTime? DateTo {
            get => _dateTo;
            set => SetProperty(ref _dateTo, value);
        }

        private string _selectedFilterDic="";
        public string SelectedFilterDic {
            get => _selectedFilterDic;
            set => SetProperty(ref _selectedFilterDic, value);
        }

        public FilterObj(bool isChecked = false, string label = "", string selectedFilterDic = "") {
            IsChecked = isChecked;
            Label = label;
            SelectedFilterDic = selectedFilterDic;
        }
    }
}
