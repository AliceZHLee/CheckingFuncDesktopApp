using ICEChecker.ModuleCheckerInput.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;

namespace ICEChecker.ModuleCheckerInput {
    public class ModuleCheckerInputModule : IModule {
        public void OnInitialized(IContainerProvider containerProvider) {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            //regionManager.RegisterViewWithRegion("CheckerInputRegion", typeof(InputView));
            regionManager.RegisterViewWithRegion("CheckerInputRegion", typeof(Input2View));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
        }
    }
}
