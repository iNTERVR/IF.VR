using EcsRx.Infrastructure.Dependencies;
using EcsRx.Infrastructure.Extensions;
using Inter.VR.Modules.InterVRInterfaces;

namespace Inter.VR.Plugins.SteamVRInterface.Modules
{
    public class SteamVRHandInterfaceModuleSetupDependency : IDependencyModule
    {
        public void Setup(IDependencyContainer container)
        {
            container.Unbind<IInterVRHandInterface>();
            container.Bind<IInterVRHandInterface, SteamVRHandInterface>();
            container.Unbind<IInterVRHandGrabStatus>();
            container.Bind<IInterVRHandGrabStatus, SteamVRHandGrabStatus>();
        }
    }
}