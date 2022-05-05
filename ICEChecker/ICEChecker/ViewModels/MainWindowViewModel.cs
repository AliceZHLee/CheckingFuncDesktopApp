using ICEChecker.ModuleICEData;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Threading;
using System.Windows.Input;

namespace ICEChecker.ViewModels {
    public class MainWindowViewModel : BindableBase, IDisposable {
        public bool IsToAdd { get; set; }
        public ICommand AddCommand { get; private set; }
        public MainWindowViewModel() {
            AddCommand = new DelegateCommand(AddNewRecordsToCheck);
            //OtcHelper.StartTradeData();

        }

        private void AddNewRecordsToCheck() {
            IsToAdd = true;
            Thread.Sleep(10);
            IsToAdd = false;
        }

        public void Dispose() {
            OtcHelper.Release();
        }
    }
}
