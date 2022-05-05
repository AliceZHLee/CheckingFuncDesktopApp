using Dragablz;
using Prism.Mvvm;
using System;
using ThemeViewer.Models;

namespace ThemeViewer.ViewModels {
    public class ThmViewWinVM : BindableBase, IDisposable {
        private DateTime _startDate = DateTime.Today;
        private DateTime _endDate = DateTime.Today;

        public DateTime StartDate {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime EndDate {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        private string _product = "";
        public string Product {
            get => _product;
            set => SetProperty(ref _product, value);
        }

        private IInterTabClient _interTabClient;
        public IInterTabClient InterTabClient {
            get => _interTabClient;
            set => SetProperty(ref _interTabClient, value);
        }

        public ThmViewWinVM() {
            _interTabClient = new MyInterTabClient();
        }

        //public void Start(string product) {
        public void Start() {
            //Product = product;
            OtcHelper.StartSecurityData();
            OtcHelper.StartPartyData();
            OtcHelper.StartTradeData();
            OtcHelper.StartMarketData();
        }

        public void Dispose() {
            OtcHelper.Release();
        }
    }
}
