using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ICEChecker.ModuleDictionary.ViewModels {
    public class NewProductViewModel : BindableBase, INotifyDataErrorInfo {
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private string _securityName;
        [Required(ErrorMessage = "\"Product Name\" value is missing")]
        public string SecurityName {
            get {
                ValidateProperty(_securityName);
                return _securityName;
            }
            set {
                SetProperty(ref _securityName, value);
                ValidateProperty(value);
            }
        }

        private string _abbr;
        [Required(ErrorMessage = "\"Abbreviation\" value is missing")]
        public string Abbr {
            get {
                ValidateProperty(_abbr);
                return _abbr;
            }
            set {
                SetProperty(ref _abbr, value);
                ValidateProperty(value);
            }
        }
        public NewProductViewModel() {
            SaveCommand = new DelegateCommand(SaveProductAbbr);
            CancelCommand = new DelegateCommand(CancelSaving);
            _errorsContainer = new ErrorsContainer<string>(pn => RaiseErrorsChanged(pn));
        }

        private void CancelSaving() {
        }

        private void SaveProductAbbr() {
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
