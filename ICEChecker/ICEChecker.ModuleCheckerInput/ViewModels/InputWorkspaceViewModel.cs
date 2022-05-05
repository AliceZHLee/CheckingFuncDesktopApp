using Prism.Events;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ICEChecker.ModuleCheckerInput.ViewModels {
    public class InputWorkspaceViewModel : BindableBase {
        IEventAggregator _ea1 = new EventAggregator();
        IEventAggregator _ea2 = new EventAggregator();

        #region constructor
        public InputWorkspaceViewModel() {
            Workspaces = new ObservableCollection<InputViewModel>();
            Workspaces.Add(new InputViewModel(_ea1) { DisplayName = "Li Yong" });
            //Workspaces.Add(new InputViewModel(_ea2) { DisplayName = "Zhang Shiqi" });
        }
        #endregion

        #region wrap other ViewModels inside MainWin
        #region wrap InputWorkspaceViewModel

        public ObservableCollection<InputViewModel> Workspaces { get; set; } 
        private int _selectedTabIndex;
        public int SelectedTabIndex {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }
        #endregion wrap InputWorkspaceViewModel
        #endregion wrap other ViewModels inside MainWin
    }
}
