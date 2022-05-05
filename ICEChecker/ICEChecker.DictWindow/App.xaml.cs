using ICEChecker.DictWindow.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using System.Windows;

namespace ICEChecker.DictWindow {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication {
        protected override Window CreateShell() {
            return Container.Resolve<AbbrDictWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry) {

        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) {          
            moduleCatalog.AddModule<ModuleDictionary.ModuleDictionaryModule>();
        }
    }
}
