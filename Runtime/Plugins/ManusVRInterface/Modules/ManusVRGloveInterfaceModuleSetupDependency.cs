using EcsRx.Infrastructure.Dependencies;
using EcsRx.Infrastructure.Extensions;
using Inter.VR.Modules.InterVRInterfaces;

namespace Inter.VR.Plugins.ManusVRInterface.Modules
{
    public class ManusVRGloveInterfaceModuleSetupDependency : IDependencyModule
    {
        public void Setup(IDependencyContainer container)
        {
            container.Unbind<IInterVRGloveInterface>();
            container.Bind<IInterVRGloveInterface, ManusVRGloveInterface>();
            container.Unbind<IInterVRHandGrabStatus>();
            container.Bind<IInterVRHandGrabStatus, ManusVRHandGrabStatus>();
            container.Unbind<IInterVRHandInterface>();
            container.Bind<IInterVRHandInterface, ManusVRHandInterface>();
        }
    }
}