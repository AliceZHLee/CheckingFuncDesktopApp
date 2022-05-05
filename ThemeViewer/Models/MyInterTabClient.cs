using Dragablz;
using System.Windows;
using ThemeViewer.ViewModels;
using ThemeViewer.Views.Components;

namespace ThemeViewer.Models {
    public class MyInterTabClient : IInterTabClient {
        private ThmViewWinVM _mainWindowVM;
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source) {
            _mainWindowVM = new ThmViewWinVM();
            var view = new TabHostWindow();
            view.DataContext = _mainWindowVM;
            _mainWindowVM.InterTabClient= interTabClient;
            return new NewTabHost<Window>(view, view.TabsContainer);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window) {
            if (window is TabHostWindow) {
                return TabEmptiedResponse.CloseWindowOrLayoutBranch;
            }
            return TabEmptiedResponse.DoNothing;           
        }
    }
}
