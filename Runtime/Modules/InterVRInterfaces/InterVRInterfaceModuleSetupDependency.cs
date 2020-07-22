using EcsRx.Infrastructure.Dependencies;
using EcsRx.Infrastructure.Extensions;

namespace Inter.VR.Modules.InterVRInterfaces
{
    public class InterVRInterfaceModuleSetupDependency : IDependencyModule
    {
        public void Setup(IDependencyContainer container)
        {
            container.Bind<IInterVRInterface, InterVRInterface>();
        }
    }
}