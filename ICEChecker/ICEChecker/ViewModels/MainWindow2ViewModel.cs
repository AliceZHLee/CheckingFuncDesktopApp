using ICEChecker.ModuleCheckerInput.Models;
using ICEChecker.ModuleICEData;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;

namespace ICEChecker.ViewModels {
    public class MainWindow2ViewModel : BindableBase, IDisposable {
        private readonly IRegionManager _regionManager;
        private string _userID;
        IEventAggregator _ea;

        public string UserID {
            get => _userID;
            set=>SetProperty(ref _userID, value);
        }
        //public DelegateCommand<string> NavigateCommand { get; private set; }

        public MainWindow2ViewModel(IEventAggregator ea) {
            _ea = ea;
            _ea.GetEvent<RetrieveUserIDEvent>().Subscribe(SyncUserID);
            //_regionManager = regionManager;
            //   NavigateCommand = new DelegateCommand<string>(Navigate);
        }

        private void SyncUserID(string id) {
            UserID = id;
        }

        //private void Navigate(string navigatePath) {
        //    if (navigatePath != null)
        //        _regionManager.RequestNavigate("ContentRegion", navigatePath);
        //}


        public void Dispose() {
            OtcHelper.Release();
        }
    }
}
