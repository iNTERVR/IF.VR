using EcsRx.Entities;
using Inter.VR.Defines;
using Inter.VR.Modules.InterVRInterfaces;
using ManusVR.Hands;
using Valve.VR;

namespace Inter.VR.Plugins.ManusVRInterface.Extensions
{
    public static class IInterVRInterfaceExtensions
    {
        public static bool ManusVRInitialized(this IInterVRInterface vrInterface)
        {
            return HandDataManager.IsInitialised;
        }
    }
}