using ICEChecker.ModuleDictionary.Views;
using Prism.Ioc;
using Prism.Modularity;
using System;

namespace ICEChecker.ModuleDictionary {
    public class ModuleDictionaryModule:IModule {
        public void OnInitialized(IContainerProvider containerProvider) {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterForNavigation<ProductAbbrView>();
            containerRegistry.RegisterForNavigation<ShopBRKView>();
        }
    }
}
