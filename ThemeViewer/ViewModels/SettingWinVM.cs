using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ThemeViewer.Models;

namespace ThemeViewer.ViewModels {
    public class SettingWinVM : BindableBase, INotifyDataErrorInfo {
        #region INotifyDataErrorInfo        
        private ErrorsContainer<string> _errorsContainer;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public bool HasErrors {
            get { return this._errorsContainer.HasErrors; }
        }
        public IEnumerable GetErrors(string propertyName) {
            return this._errorsContainer.GetErrors(propertyName);
        }

        protected void RaiseErrorsChanged(string propertyName) {
            var handler = this.ErrorsChanged;
            if (handler != null) {
                handler(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }
        protected void ValidateProperty(object value, [CallerMemberName] string propertyName = null) {
            var context = new ValidationContext(this) { MemberName = propertyName };
            var validationErrors = new List<ValidationResult>();

            if (!Validator.TryValidateProperty(value, context, validationErrors)) {
                var errors = validationErrors.Select(error => error.ErrorMessage);
                this._errorsContainer.SetErrors(propertyName, errors);
            }
            else {
                this._errorsContainer.ClearErrors(propertyName);
            }
        }
        #endregion 

        private readonly IEventAggregator _ea;
        public ICommand ApplyCommand { get; private set; }
        public ICommand CancelChangeCommand { get; private set; }

        public static bool OriginalAlertOn;
        public string OriginalQtyThreshold;
        public static bool OriginalExcludeOurs;

        #region constructor
        public SettingWinVM(IEventAggregator eventAggregator) {
            _ea = eventAggregator;
            _errorsContainer = new ErrorsContainer<string>(pn => RaiseErrorsChanged(pn));
            ApplyCommand = new DelegateCommand(UpdateSetting);
            CancelChangeCommand = new DelegateCommand(CancelChange);

            //UI display: data binding
            var alertHistorySetting = ConfigHelper.LoadAlertSetting(@"config\config.json");
            _alertOn = (bool)alertHistorySetting["AlertOn"] == null ? true : (bool)alertHistorySetting["AlertOn"];
            _alertStatus = _alertOn ? "On" : "Off";
            _qtyThreshold = alertHistorySetting["Threshold"].ToString();
            _isOursTradesExcluded = (bool)alertHistorySetting["ExcludeAlertForOurTrades"] == null ? true : (bool)alertHistorySetting["ExcludeAlertForOurTrades"];
            _excludeOurTrades = _isOursTradesExcluded ? "On" : "Off";

            OriginalAlertOn = _alertOn;
            OriginalQtyThreshold = _qtyThreshold;
            OriginalExcludeOurs = _isOursTradesExcluded;
        }
        #endregion
       
        #region AlertSetting
        private bool _alertOn;
        public bool AlertOn {
            get => _alertOn;
            set => SetProperty(ref _alertOn, value);
        }
      
        private string _alertStatus;
        public string AlertStatus {
            get => _alertStatus;
            set => SetProperty(ref _alertStatus, value);
        }       

        private string _qtyThreshold;
        [Required(ErrorMessage = "Threshold value is missing"), Range(0, long.MaxValue, ErrorMessage = "Invalid threshold input")]
        public string QtyThreshold {
            get {
                ValidateProperty(_qtyThreshold);
                return _qtyThreshold;
            }
            set {
                SetProperty(ref _qtyThreshold, value);
                ValidateProperty(value);
            }
        }

        //DISABLE OUR OWN TRADES' ALERT IF USERS JUST WANT TO OBSERVE EXTERNAL TRADES
        private bool _isOursTradesExcluded;
        public bool IsOursTradesExcluded {
            get => _isOursTradesExcluded;
            set => SetProperty(ref _isOursTradesExcluded, value);
        }

        private string _excludeOurTrades;
        public string ExcludeOurTrades {
            get => _excludeOurTrades;
            set => SetProperty(ref _excludeOurTrades, value);
        }
        #endregion

        private void UpdateSetting() {
            try {
                AlertSetting setting = new AlertSetting() { IsAlertOn = AlertOn, 
                                                            Threshold = QtyThreshold,
                                                            IsOursExcluded=IsOursTradesExcluded};
                _ea.GetEvent<AlertSettingUpdateEvent>().Publish(setting);
                OriginalAlertOn = _alertOn;
                OriginalQtyThreshold = _qtyThreshold;
                OriginalExcludeOurs = _isOursTradesExcluded;
            }
            catch (Exception ex) {
                MessageBox.Show("Invalid Input !");
                _alertOn = OriginalAlertOn;
                _qtyThreshold = OriginalQtyThreshold;
                _isOursTradesExcluded = OriginalExcludeOurs;
            }
            AlertStatus = _alertOn ? "On" : "Off";
        }
        private void CancelChange() {
            _alertOn = OriginalAlertOn;
            _qtyThreshold = OriginalQtyThreshold;
            AlertStatus = _alertOn ? "On" : "Off";
            _isOursTradesExcluded = OriginalExcludeOurs;
            ExcludeOurTrades = _isOursTradesExcluded ? "On" : "Off";
        }
    }
}
