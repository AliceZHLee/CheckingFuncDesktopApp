using Prism.Mvvm;
using System.Collections.Generic;

namespace ThemeViewer.ViewModels {
    public class SelectProductWinVM : BindableBase {
        #region constructor
        public SelectProductWinVM() {
            _selectedAccounts = new Dictionary<string, object>();

            //add trader accounts
            foreach (var account in ConfigHelper.LoadAccountData(@"config\config.json")) {
                AccountOptions.Add(account, account);
            }

            //add product options
            foreach (var product in ConfigHelper.LoadProductData(@"config\config.json")) {
                ProductOptions.Add(product, product);
            }
        }
        #endregion

        #region properties
        public Dictionary<string, object> AccountOptions { get; } = new();

        //[Required(ErrorMessage = "You must select accounts to view")]
        private Dictionary<string, object> _selectedAccounts;
        public Dictionary<string, object> SelectedAccounts {
            get {
                return _selectedAccounts;
            }
            set {
                SetProperty(ref _selectedAccounts, value);
            }
        }

        public Dictionary<string, object> ProductOptions { get; } = new();

        private Dictionary<string, object> _selectedProducts;
        public Dictionary<string, object> SelectedProducts {
            get {
                return _selectedProducts;
            }
            set {
                SetProperty(ref _selectedProducts, value);
            }
        }
        #endregion
    }
}
