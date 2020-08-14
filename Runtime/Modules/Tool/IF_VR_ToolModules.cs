using EcsRx.Infrastructure.Dependencies;
using EcsRx.Infrastructure.Extensions;

namespace InterVR.IF.VR.Modules
{
    public class IF_VR_ToolModules : IDependencyModule
    {
        public void Setup(IDependencyContainer container)
        {
            container.Bind<IF_VR_IInterface, IF_VR_Interface>();
        }
    }
}