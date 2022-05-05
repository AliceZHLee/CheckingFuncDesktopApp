using ICEChecker.ModuleCheckerOutput.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace ICEChecker.ModuleCheckerOutput {
    public class ModuleCheckerOutputModule : IModule {
        public void OnInitialized(IContainerProvider containerProvider) {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("CheckerOutputRegion", typeof(OutputView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
        }
    }
}
