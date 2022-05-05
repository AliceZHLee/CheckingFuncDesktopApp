using ICEChecker.ModuleDictionary.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ICEChecker.ModuleDictionary.ViewModels {

    public class NewShopBrkViewModel : BindableBase, INotifyDataErrorInfo {
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private string _shop;
        [Required(ErrorMessage = "\"Shop\" value is missing")]
        public string Shop {
            get {
                ValidateProperty(_shop);
                return _shop;
            }
            set {
                SetProperty(ref _shop, value);
                ValidateProperty(value);
            }
        }

        private string _brk;
        public string BRK {
            get => _brk;
            set => SetProperty(ref _brk, value);
        }

        private string _brokerFirm;
        [Required(ErrorMessage = "\"Broker Company (from API)\" value is missing")]
        public string BrokerFirm {
            get {
                ValidateProperty(_brokerFirm);
                return _brokerFirm;
            }
            set {
                SetProperty(ref _brokerFirm, value);
                ValidateProperty(value);
            }
        }

        public NewShopBrkViewModel() {
            SaveCommand = new DelegateCommand(SaveBrkDetails);
            CancelCommand = new DelegateCommand(CancelSaving);
            _errorsContainer = new ErrorsContainer<string>(pn => RaiseErrorsChanged(pn));
        }

        private void CancelSaving() {

        }
        private void SaveBrkDetails() {

        }
        private void PopulateBrkDictionary() {
            try {
                Lookup<string, string> lookUp = (Lookup<string, string>)File.ReadLines(@"config\BrokerDict.csv").Select(line => line.Split(';')).ToLookup(line => line[0], line => line[1]);
                Lookup<string, string> lookUp2 = (Lookup<string, string>)File.ReadLines(@"config\BrokerDict.csv").Select(line => line.Split(';')).ToLookup(line => line[0], line => line[2]);

                
            }
            catch (Exception e) { }
        }

        #region INotifyDataErrorInfo        
        private ErrorsContainer<string> _errorsContainer;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public bool HasErrors {
            get { return _errorsContainer.HasErrors; }
        }
        public IEnumerable GetErrors(string propertyName) {
            return _errorsContainer.GetErrors(propertyName);
        }

        protected void RaiseErrorsChanged(string propertyName) {
            var handler = ErrorsChanged;
            if (handler != null) {
                handler(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }
        protected void ValidateProperty(object value, [CallerMemberName] string propertyName = null) {
            var context = new ValidationContext(this) { MemberName = propertyName };
            var validationErrors = new List<ValidationResult>();

            if (!Validator.TryValidateProperty(value, context, validationErrors)) {
                var errors = validationErrors.Select(error => error.ErrorMessage);
                _errorsContainer.SetErrors(propertyName, errors);
            }
            else {
                _errorsContainer.ClearErrors(propertyName);
            }
        }
        #endregion 
    }
}
