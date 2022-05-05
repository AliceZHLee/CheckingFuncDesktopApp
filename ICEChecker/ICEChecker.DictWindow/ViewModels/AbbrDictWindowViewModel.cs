using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Windows.Input;

namespace ICEChecker.DictWindow.ViewModels {
    public class AbbrDictWindowViewModel : BindableBase {
        private readonly IRegionManager _regionManager;
        public ICommand ApplyCommand { get; private set; }
        public ICommand CancelChangeCommand { get; private set; }

        public DelegateCommand<string> NavigateCommand { get; private set; }

        public AbbrDictWindowViewModel(IRegionManager regionManager) {
            _regionManager = regionManager;
            ApplyCommand = new DelegateCommand(UpdateSetting);
            CancelChangeCommand = new DelegateCommand(CancelChange);
            NavigateCommand = new DelegateCommand<string>(Navigate);
        }

        private void CancelChange() {

        }

        private void UpdateSetting() {

        }

        private void Navigate(string navigatePath) {
            if (navigatePath != null)
                _regionManager.RequestNavigate("ContentRegion", navigatePath);
        }
    }
}
