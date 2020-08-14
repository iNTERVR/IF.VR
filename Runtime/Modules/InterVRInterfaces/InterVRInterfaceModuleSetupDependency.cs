using EcsRx.Infrastructure.Dependencies;
using EcsRx.Infrastructure.Extensions;

namespace InterVR.Unity.SDK.SteamVR.Modules.InterVRInterfaces
{
    public class InterVRInterfaceModuleSetupDependency : IDependencyModule
    {
        public void Setup(IDependencyContainer container)
        {
            container.Bind<IInterVRInterface, InterVRInterface>();
        }
    }
}