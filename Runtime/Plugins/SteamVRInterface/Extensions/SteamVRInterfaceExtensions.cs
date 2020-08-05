using EcsRx.Entities;
using Inter.VR.Defines;
using Inter.VR.Modules.InterVRInterfaces;
using Valve.VR;

namespace Inter.VR.Plugins.SteamVRInterface.Extensions
{
    public static class IInterVRInterfaceExtensions
    {
        public static bool SteamVRInitialized(this IInterVRInterface vrInterface)
        {
            if (SteamVR.initializedState == SteamVR.InitializedStates.None || SteamVR.initializedState == SteamVR.InitializedStates.Initializing)
                return false;

            return true;
        }

        public static bool SteamVRValid(this IInterVRInterface vrInterface)
        {
            if (!vrInterface.SteamVRInitialized())
                return false;

            return SteamVR.instance != null;
        }

        public static bool SteamVRInitializedAndValid(this IInterVRInterface vrInterface)
        {
            return vrInterface.SteamVRInitialized() && vrInterface.SteamVRValid();
        }
    }
}