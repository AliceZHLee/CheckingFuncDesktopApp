using ICEChecker.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using System.Windows;

namespace ICEChecker {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication {
        protected override Window CreateShell() {
            //return Container.Resolve<MainWindow>();
            return Container.Resolve<MainWindow2>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry) {

        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) {
            moduleCatalog.AddModule<ModuleICEData.ModuleICEDataModule>();
            moduleCatalog.AddModule<ModuleCheckerInput.ModuleCheckerInputModule>();
            moduleCatalog.AddModule<ModuleCheckerOutput.ModuleCheckerOutputModule>();
            moduleCatalog.AddModule<ModuleDictionary.ModuleDictionaryModule>();
        }
       
    }
}
