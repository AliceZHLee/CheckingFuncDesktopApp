using ICEChecker.ModuleICEData.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace ICEChecker.ModuleICEData {
    public class ModuleICEDataModule:IModule {
        public void OnInitialized(IContainerProvider containerProvider) {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("TCAPIDataRegion", typeof(ICETCApiView));

        }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
            //containerRegistry.RegisterForNavigation<ICETCApiView>();
        }
    }
}
